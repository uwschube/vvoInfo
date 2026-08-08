using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VVOInfo.Models;

namespace VVOInfo.Services
{
    public class DepartureService
    {
        private static readonly ILog log = LogManager.GetLogger("DefaultLogger");
        private static readonly ILog dataLogger = LogManager.GetLogger("DataLogger");

        private static readonly HttpClient _client = new HttpClient();
        // private const string ApiUrl = "https://webapi.vvo-online.de/dm?format=json";
        private const string ApiUrl = "https://vvo-online.de";

        public async Task<DepartureResponse> GetDeparturesAsync3(string stopId)
        {
            string url = "https://webapi.vvo-online.de/dm?format=json";

            // 1. Request-Daten vorbereiten
            var requestData = new DepartureRequest
            {
                StopId = "33000028", // Beispiel: Dresden Hauptbahnhof
                Limit = 40
            };
            requestData.StopId = stopId;
            string jsonPayload = JsonSerializer.Serialize(requestData);
            string jsonResponse = "";
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            dataLogger.Info($"Request: {jsonPayload}");

            // 2. HTTP-Anfrage konfigurieren (User-Agent ist PFLICHT)
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) CSharpVVOClient/1.0");

            try
            {
                // 3. Request senden
                HttpResponseMessage response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                jsonResponse = await response.Content.ReadAsStringAsync();
                dataLogger.Info($"Response: {jsonResponse}");

                // 4. JSON deserialisieren
                var result = JsonSerializer.Deserialize<DepartureResponse>(jsonResponse);
                if (result == null)
                {
                    throw new Exception("Fehler beim Deserialisieren der Antwort. (result == null)");
                }

               // Console.WriteLine($"--- Nächste Abfahrten (ID: {requestData.StopId}) ---");
                foreach (var departure in result.Departures)
                {
                    departure.IsMissingInDataResponse = false;
                    try
                    {
                        departure.RealTimeDateTime = departure.ScheduledTimeDateTime = ParseVvoToDateTime(departure.ScheduledTime);
                        departure.RealTimeDateTimeOffset = departure.ScheduledTimeDateTimeOffset = ParseVvoTimestampOffset(departure.ScheduledTime);
                        departure.RealDepartureTimeInMinutes = departure.ScheduledDepartureTimeInMinutes = DepartureInMinutes(departure.ScheduledTimeDateTime);
                        if (!String.IsNullOrEmpty(departure.RealTime))
                        {
                            departure.RealTimeDateTime = ParseVvoToDateTime(departure.RealTime);
                            departure.RealTimeDateTimeOffset = ParseVvoTimestampOffset(departure.RealTime);
                            departure.RealDepartureTimeInMinutes = DepartureInMinutes(departure.RealTimeDateTime);
                        }

                    }
                    catch (Exception ex)
                    {
                        log.Error($"Fehler beim Parsen: {ex.Message} {ex.StackTrace} ");
                    }

                    //Console.WriteLine($"Linie {departure.LineName} Richtung: {departure.Direction}");

                }
                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler bei der Abfrage: {ex.Message} + request:{jsonPayload} jsonResponse:{jsonResponse}");
                log.Error($"Fehler bei der Abfrage: {ex.Message} + request:{jsonPayload} jsonResponse:{jsonResponse}");
                throw;
            }
        }


        public async Task<string> GetDeparturesAsync2(string stopId, string directionFilter)
        {
            // 1. Die URL für den Abfahrtsmonitor
            string url = "https://webapi.vvo-online.de/dm?format=json";

            // 2. Die Anfrage-Parameter als reiner JSON-String zusammengestellt
            // Ersetze "33000028" bei Bedarf durch eine andere Haltestellen-ID (z. B. "33000001" für Postplatz)
            string jsonRequestString = @"
        {
            ""stopid"": ""33000028"",
            ""limit"": 3,
            ""mot"": [""Tram"", ""CityBus"", ""IntercityBus"", ""SuburbanRailway"", ""Train""],
            ""format"": ""json""
        }";

            // 3. HTTP-Anfrage vorbereiten und Content-Typ auf JSON setzen
            var content = new StringContent(jsonRequestString, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;

            // WICHTIG: Ohne einen User-Agent antwortet der Server mit einem Fehler (z.B. 403 Forbidden)
            request.Headers.UserAgent.ParseAdd("HttpClient/1.0 (Windows NT 10.0; Win64; x64)");
            string jsonResponseString = string.Empty;
            try
            {
                // 4. Anfrage absenden
                HttpResponseMessage response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // 5. Die Antwort als reinen JSON-String auslesen
                jsonResponseString = await response.Content.ReadAsStringAsync();

                // Auswertung auf der Konsole
                Console.WriteLine("--- Rohe JSON-Antwort vom VVO-Server ---");
                Console.WriteLine(jsonResponseString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abruf: {ex.Message}");
            }
            return jsonResponseString;
        }

        public static DateTime ParseVvoToDateTime(string input)
        {
            if (string.IsNullOrEmpty(input) || !input.StartsWith("/Date(") || !input.EndsWith(")/"))
            {
                throw new ArgumentException($@"Ungültiges VVO-Datumsformat. Input:'{input}'");
            }

            // 1. Umschließenden Text abschneiden -> "1718974500000+0200"
            string cleanContent = input.Substring(6, input.Length - 8);

            // 2. Trennung an der Zeitzone (+ oder -)
            int signIndex = cleanContent.IndexOf('+');
            if (signIndex == -1) signIndex = cleanContent.IndexOf('-');

            if (signIndex == -1)
            {
                throw new ArgumentException("Zeitzonen-Indikator (+/-) nicht gefunden.");
            }

            string msStr = cleanContent.Substring(0, signIndex);
            string offsetStr = cleanContent.Substring(signIndex);

            // 3. Millisekunden und Zeitzone parsen
            long milliseconds = long.Parse(msStr);
            int hours = int.Parse(offsetStr.Substring(1, 2));
            int minutes = int.Parse(offsetStr.Substring(3, 2));

            if (offsetStr.StartsWith("-"))
            {
                hours = -hours;
                minutes = -minutes;
            }
            TimeSpan timeZoneOffset = new TimeSpan(hours, minutes, 0);

            // 4. Erst als globales DateTimeOffset einlesen
            DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);

            // 5. Auf die Ziel-Zeitzone des VVO umrechnen (+02:00 oder +01:00)
            DateTimeOffset localizedDto = dto.ToOffset(timeZoneOffset);

            // 6. Konvertierung in ein absolutes DateTime (ortsübliche Zeit im VVO)
            return localizedDto.LocalDateTime;
        }


        public int DepartureInMinutes(DateTime departureTime)
        {
            TimeSpan timeUntilDeparture = departureTime - DateTime.Now;
            return (int)timeUntilDeparture.TotalMinutes;   
        }


        public DateTimeOffset ParseVvoTimestampOffset(string input)
        {
            // 1. Schutzklausel gegen leere oder ungültige Strings
            if (string.IsNullOrEmpty(input) || !input.StartsWith("/Date(") || !input.EndsWith(")/"))
            {
                throw new ArgumentException($@"Ungültiges VVO-Datumsformat. Input:'{input}'");
            }

            // 2. Den reinen Zahlenbereich herausschneiden
            // Schneidet "/Date(" (6 Zeichen) am Anfang und ")/" (2 Zeichen) am Ende ab.
            string cleanContent = input.Substring(6, input.Length - 8);

            // 3. Trennung von Unix-Millisekunden und Zeitzone (am '+' oder '-' Zeichen)
            int signIndex = cleanContent.IndexOf('+');
            if (signIndex == -1)
            {
                signIndex = cleanContent.IndexOf('-');
            }

            if (signIndex == -1)
            {
                throw new ArgumentException("Zeitzonen-Indikator (+/-) im Zeitstempel nicht gefunden.");
            }

            // Split in Millisekunden und Offset-String (z. B. "1718974500000" und "+0200")
            string msStr = cleanContent.Substring(0, signIndex);
            string offsetStr = cleanContent.Substring(signIndex);

            // 4. Unix-Millisekunden in long parsen
            long milliseconds = long.Parse(msStr);

            // 5. Zeitzonen-Offset parsen (Format "+HHmm" oder "-HHmm")
            // Beispiel: "+0200" -> Stunden: 02, Minuten: 00
            int hours = int.Parse(offsetStr.Substring(1, 2));
            int minutes = int.Parse(offsetStr.Substring(3, 2));

            if (offsetStr.StartsWith("-"))
            {
                hours = -hours;
                minutes = -minutes;
            }
            TimeSpan timeZoneOffset = new TimeSpan(hours, minutes, 0);

            // 6. Aus den Unix-Millisekunden ein DateTimeOffset-Objekt erzeugen
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);

            // 7. Das korrekte lokale VVO-Offset (z. B. +02:00 für Sommerzeit) zuweisen
            return dateTimeOffset.ToOffset(timeZoneOffset);
        }

    }

}
