using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using VVOInfo.Views;

namespace VVOInfo.Models
{



    // Modell für die POST-Anfrage
    public class DepartureRequest
    {
        [JsonPropertyName("stopid")]
        public string StopId { get; set; } = String.Empty; // Die ID der Haltestelle (z.B. "33000028" für Dresden Hauptbahnhof)

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 5; // Anzahl der gewünschten Abfahrten

        [JsonPropertyName("mot")]
        public List<string> Mot { get; set; } = new List<string>
        {
            "Tram", "CityBus", "IntercityBus", "SuburbanRailway", "Train"
        }; // "Modes of Transport" (Verkehrsmittel)

        [JsonPropertyName("format")]
        public string Format { get; set; } = "json";

        [JsonPropertyName("shorttermchanges")]
        public Boolean ShortTermChanges { get; set; } = true;

        [JsonPropertyName("isarrival")]
        public Boolean IsArrival { get; set; } = false;
    }

    // Minimal-Modell für die API-Antwort
    public class DepartureResponse
    {
        public DepartureResponse()
        {
            Status = new StatusInfo();
            Place = string.Empty;
            ExpirationTime = string.Empty;
            Departures = new List<DepartureItem>();
        }

        [JsonPropertyName("Status")]
        public StatusInfo Status { get; set; }
        
        
        [JsonPropertyName("Place")]
        public String Place { get; set; }

        [JsonPropertyName("ExpirationTime")]
        public string ExpirationTime { get; set; }


        [JsonPropertyName("Departures")]
        public List<DepartureItem> Departures { get; set; }
    }

    public class StatusInfo
    {
        public StatusInfo()
        {
            Code = string.Empty;
        }

        [JsonPropertyName("Code")]
        public string Code { get; set; }
    }

    public class CancelReason {

        public CancelReason()
        {
            Reason = string.Empty;
            AdditionalText = string.Empty;
        }

        [JsonPropertyName("Reason")]
        public string Reason { get; set; }

        [JsonPropertyName("AdditionalText")]
        public string AdditionalText { get; set; }
    }

    public class DepartureItem
    {
        public string Key
        {
            get {
                return $"{ScheduledTime}_{LineName}_{Direction}";
            }
        }

        public Boolean IsMissingInDataResponse { get; set;  }

        public DepartureItem()
        {
            IsMissingInDataResponse = true;
            Id = string.Empty;
            DlId = string.Empty;
            LineName = string.Empty;
            Direction = string.Empty;
            Platform = new PlatformInfo();
            Mot = string.Empty;
            RealTime = string.Empty;
            ScheduledTime = string.Empty;
            State = string.Empty;
            RouteChanges = new List<string>();
            CancelReasons = new List<CancelReason>();
            Occupancy = string.Empty;
        }

        [JsonPropertyName("Id")]
        public string Id { get; set; } // Z.B. "voe:11007: :R:j26"

        [JsonPropertyName("DlId")]
        public string DlId { get; set; } // Z.B. "de:vvo:11-7"

        [JsonPropertyName("LineName")]
        public string LineName { get; set; } // Z.B. "3" oder "EV11"

        [JsonPropertyName("Direction")]
        public string Direction { get; set; } // Zielrichtung der Linie

        [JsonPropertyName("Platform")]
        public PlatformInfo Platform { get; set; } // Informationen zum Bahnsteig
        [JsonPropertyName("Mot")]
        public string Mot { get; set; } // Fahrmittelstyp, z.B. "Tram", "CityBus", "IntercityBus", "SuburbanRailway", "Train"

        [JsonPropertyName("RealTime")]
        public string RealTime { get; set; } // VVO nutzt das MS-Date-Format: "/Date(timestamp+timezone)/"
        public DateTime RealTimeDateTime { get; set; }
        public DateTimeOffset RealTimeDateTimeOffset { get; set; }
        public int RealDepartureTimeInMinutes { get; set; } // Berechnet aus RealTime und aktuellem Zeitpunkt


        [JsonPropertyName("ScheduledTime")]
        public string ScheduledTime { get; set; } // VVO nutzt das MS-Date-Format: "/Date(timestamp+timezone)/"
        public DateTime ScheduledTimeDateTime { get; set; }
        public DateTimeOffset ScheduledTimeDateTimeOffset { get; set; }
        public int ScheduledDepartureTimeInMinutes { get; set; } // Berechnet aus RealTime und aktuellem Zeitpunkt

        public string DirectionAsString {
            get
            {
                var sb = new StringBuilder();
                sb.Append(Direction);
                if (CancelReasons != null && CancelReasons.Count > 0)
                {
                    sb.AppendLine("");
                    sb.Append(CancelReasons[0].Reason).Append(" ").Append(CancelReasons[0].AdditionalText);
                }
                return sb.ToString();
            }
        }
        public string ScheduledTimeAsString {
            get
            {
                try
                {
                    return ScheduledTimeDateTime.ToString("HH:mm");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler in ScheduledTimeAsString: {ex.Message}");
                    return "Error";
                }
            }
        } 

        public string DepartureTimeAsString
        {
            get
            {
                try
                {
                    if (RealTimeDateTime > DateTime.Now.AddDays(-1))
                    {
                        return RealTimeDateTime.ToString("HH:mm");
                    }
                    if (ScheduledTimeDateTime > DateTime.Now.AddDays(-1))
                    {
                        return ScheduledTimeDateTime.ToString("HH:mm");
                    }
                    return String.Empty;
                }
                catch (Exception ex)
                {
                    if (ScheduledTimeDateTime > DateTime.Now.AddDays(-1))
                    {
                        return ScheduledTimeDateTime.ToString("HH:mm");
                    }
                    Console.WriteLine($"Fehler in DepartureTimeAsString: {ex.Message}");
                    return "Error";
                }
            }
        }

        public string DepartureAsString
        {
            get
            {
                try
                {
                    if (State == "Cancelled")
                    {
                        return "Cancelled";
                    }
                    if (IsMissingInDataResponse)
                    {
                        return "?Missing in Data Response?";
                    }
                    var sb = new StringBuilder();
                    sb.Append("in ");
                    int h = RealDepartureTimeInMinutes / 60;
                    if (h > 0)
                    {
                        sb.Append(h).Append(" h ");
                    }
                    int min = RealDepartureTimeInMinutes - (h * 60);

                    sb.Append(min.ToString());
                    if (RealDepartureTimeInMinutes != ScheduledDepartureTimeInMinutes)
                    {
                        var diff = RealDepartureTimeInMinutes - ScheduledDepartureTimeInMinutes;
                        sb.Append(" (");
                        if (diff > 0)
                        {
                            sb.Append("+");
                        }
                        sb.Append(diff).Append(")");
                    }
                    sb.Append(" min");
                    return sb.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler in DepartureAsString: {ex.Message}");
                    return "Error";
                }
            }

        }

        public Boolean IsInTime
        {
            get
            {
                try
                {
                    if (State == "Cancelled")
                    {
                        return false;
                    }
                    if (IsMissingInDataResponse)
                    {
                        return false;
                    }

                    var diff = RealDepartureTimeInMinutes - ScheduledDepartureTimeInMinutes;
                    return diff <= 1;
                }catch(Exception ex)
                {
                    Console.WriteLine($"Fehler in IsInTime: {ex.Message}");
                    return false;
                }

            }
        }

        public Boolean IsDelayed
        {
            get
            {
                return !IsInTime;
                /*
                try
                {
                    if (State == "Cancelled")
                    {
                        return true;
                    }
                    if (IsMissingInDataResponse)
                    {
                        return true;
                    }
                    var diff = RealDepartureTimeInMinutes - ScheduledDepartureTimeInMinutes;
                    return diff > 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler in IsDelayed: {ex.Message}");
                    return true;
                }*/

            }
        }

        public VvoIconType MeansOfTransport
        {
            get
            {
                switch(Mot)
                {
                    case "Tram":
                        return VvoIconType.Tram;
                    case "Bus":
                    case "Bus+":
                    case "PlusBus":
                    case "CityBus":
                    case "IntercityBus":
                    case var s when s.Contains("Bus"):
                        return VvoIconType.Bus;
                    case "Train":
                    case "Railtrack":
                        return VvoIconType.Zug;
                    case "SuburbanRailway":
                        return VvoIconType.SBahn;
                    default:
                        return VvoIconType.Tram; // Default-Icon
                }
            }
        }


        [JsonPropertyName("State")]
        public string State { get; set; } //State "Delayed", "InTime"


        [JsonPropertyName("RouteChanges")]
        public List<string> RouteChanges { get; set; } = new List<string>();

        [JsonPropertyName("CancelReasons")]
        public List<CancelReason> CancelReasons { get; set; } = new List<CancelReason>();

        [JsonPropertyName("Occupancy")]
        public string Occupancy { get; set; }
    }


    public class PlatformInfo
    {
        public PlatformInfo()
        {
            Name = string.Empty;
            PlatformType = string.Empty;
        }


        [JsonPropertyName("Name")]
        public string Name { get; set; } // Z.B. "1"

        [JsonPropertyName("Type")]
        public string PlatformType { get; set; } // Z.B. "Platform"
    }

}
