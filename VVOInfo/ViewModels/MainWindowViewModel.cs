using Avalonia.Controls.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVOInfo.Models;
using VVOInfo.Services;

namespace VVOInfo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly ILog log = LogManager.GetLogger("DefaultLogger");

    private int MaxLines = 9;

    private DepartureService _departureService { get; set; }
    public string Greeting { get; } = "Welcome to Avalonia!";

    private List<String> BlackList = new List<string>() { "Cottbus", "Leipzig", "Meißen", "Großenhain", "Weinböhla", "Ruhland", "Glaubitz", "Risa", "Radeburg", "Niederau",
        "Elsterwerda", "Priestewitz", "Großdobritz", "Steinbach", "Hoyerswerda" };

    private List<String> BlackList2 = new List<string>() { };

    public ObservableCollection<DepartureItem> WeinboehlaDepartures { get; set; } = new ObservableCollection<DepartureItem>();
    public ObservableCollection<DepartureItem> NeusoernewitzDepartures { get; set; } = new ObservableCollection<DepartureItem>();
    public ObservableCollection<DepartureItem> WeinboehlaTramDepartures { get; set; } = new ObservableCollection<DepartureItem>();

    [ObservableProperty]
    private string? _TimeAsStr;

    [ObservableProperty]
    private string? _LastUpadateAsStr;


    [ObservableProperty]
    private int _IconSize;//Größe icon Bus Bahn usw...

    [ObservableProperty]
    private int _LineNameFontSize;//RE XX, 26

    [ObservableProperty]
    private int _LineInfoFontSize;//Berlin, Neustadt,  in 5 min, 12:45

    [ObservableProperty]
    private int _HeaderFontSize;//Name der Haltestelle

    [ObservableProperty]
    private String _CancelReasonsText;

    [ObservableProperty]
    private double _ColWidth0;

    [ObservableProperty]
    private double _ColWidth1;

    [ObservableProperty]
    private double _ColWidth2;

    [ObservableProperty]
    private double _ColWidth3;

    [ObservableProperty]
    private double _ColWidth4;


    public MainWindowViewModel()
    {
        IconSize= 30;
        LineInfoFontSize = 23;
        ColWidth0 = 40;
        ColWidth1 = 70;
        ColWidth3 = 200;
        ColWidth4 = 65;


        LineNameFontSize = LineInfoFontSize + 1;
        HeaderFontSize = LineInfoFontSize + 5;


        CancelReasonsText = "";

        _departureService = new DepartureService();
        _ = Run();
        _ = UpdateTime();
    }

    private async Task UpdateTime()
    {
        while (true)
        {
            try
            {
                //TimeAsStr = DateTime.Now.ToString("HH:mm:ss");
                TimeAsStr = DateTime.Now.ToString("HH:mm");
                await Task.Delay(TimeSpan.FromSeconds(20));
              //  return;
            }
            catch (Exception ex)
            {
                log.Error($"Fehler: {ex.Message} {ex.StackTrace}");
            }
        }
    }



    StringBuilder CancelReasonsStringBuilder = new StringBuilder();
    protected Dictionary<string, Dictionary<string, DepartureItem>> DataCache = new Dictionary<string, Dictionary<string, DepartureItem>>();


    private async Task<List<DepartureItem>> GetDepartures(String vvoStationId)
    {
        log.Info("vvoStationId:" + vvoStationId);

        if (!DataCache.TryGetValue(vvoStationId, out var cachedStops))
        {
            cachedStops = new Dictionary<string, DepartureItem>();
            DataCache.Add(vvoStationId, cachedStops);
        }

        foreach (var departure in cachedStops.Values)
        {
            departure.IsMissingInDataResponse = true;
        }

        List <DepartureItem> departures = new List<DepartureItem>(20);
        try
        {
            DepartureResponse departureResponse = await _departureService.GetDeparturesAsync3(vvoStationId);
            foreach (var departure in departureResponse.Departures)
            {
                String cancelReason = "";// departure.CancelReasons != null ? string.Join(", ", departure.CancelReasons) : "";
                CancelReasonsStringBuilder.Append(cancelReason);
              //  Debug.WriteLine($@"Mot:{departure.Mot} Line:{departure.LineName}, p1:{departure.Platform?.Name} p1:{departure.Platform?.PlatformType} State:{departure.State} Occupancy:{departure.Occupancy} CancelReasons:{cancelReason}");

                if (departure.CancelReasons != null && departure.CancelReasons.Count > 0)
                {
                    cancelReason = $"Reason: {departure.CancelReasons[0].Reason} AdditionalText: {departure.CancelReasons[0].AdditionalText}";
                }
                departure.IsMissingInDataResponse = false;

                log.Info($@"Mot:{departure.Mot} Line:{departure.LineName}, p1:{departure.Platform?.Name} p1:{departure.Platform?.PlatformType} State:{departure.State} Occupancy:{departure.Occupancy} CancelReasons:{cancelReason} DlId:{departure.DlId} Id:{departure.Id}");
                if (!BlackList.Any(bl => departure.Direction.Contains(bl)))
                {
                    cachedStops[departure.Key] = departure;
                }
            }
            var now = DateTime.Now.AddMinutes(1);
            var keysToRemove = cachedStops.Values.Where(o => o.RealTimeDateTime < now).Select(o => o.Key);
            foreach(var keyToRemove in keysToRemove)
            {
                cachedStops.Remove(keyToRemove);
            }
            foreach (var departure in cachedStops.Values.ToList().OrderBy(o => o.RealTimeDateTime))
            {
                if (departures.Count < MaxLines)
                {
                    departures.Add(departure);
                }
            }


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
        }
        return departures;
    }

    private async Task LoadStationData()
    {
        var datetime1 = DateTime.MinValue;
        var datetime2 = DateTime.MinValue;
        var datetime3 = DateTime.MinValue;
        CancelReasonsStringBuilder.Clear();
        //"33000028" Dreden Hauptbahnhof, "33004147" Dresden Tram, "8013232" Weinböhla
        try
        {
            var departureResponseWeinboehla = await GetDepartures("33004401");//Haltepunk weinböhla
            WeinboehlaDepartures.Clear();
            foreach (var departure in departureResponseWeinboehla)
            {
                WeinboehlaDepartures.Add(departure);
            }
            datetime1 = DateTime.Now;
        }
        catch (Exception ex)
        {
            WeinboehlaDepartures.Clear();
            log.Error($"Fehler: {ex.Message} {ex.StackTrace}");
        }

        try
        {
            var neusoernewitzDepartures = await GetDepartures("33004194");//Haltepunkt Neusörnewitz
            NeusoernewitzDepartures.Clear();
            foreach (var departure in neusoernewitzDepartures)
            {
                NeusoernewitzDepartures.Add(departure);
            }
            datetime2 = DateTime.Now;
        }
        catch (Exception ex)
        {
            NeusoernewitzDepartures.Clear();
            log.Error($"Fehler: {ex.Message} {ex.StackTrace}");
        }

        try
        {
            var weinboehlaTramDepartures = await GetDepartures("33004147");//Köhlerstraße Wenböhla
            WeinboehlaTramDepartures.Clear();
            foreach (var departure in weinboehlaTramDepartures)
            {
                WeinboehlaTramDepartures.Add(departure);
            }
            datetime3 = DateTime.Now;
        }
        catch (Exception ex)
        {
            WeinboehlaTramDepartures.Clear();
            log.Error($"Fehler: {ex.Message} {ex.StackTrace}");
        }
        DateTime min = datetime1;
        if (datetime2 < min) min = datetime2;
        if(datetime3 < min) min = datetime3;
        StringBuilder sb = new StringBuilder();
        sb.Append("Letzte Aktualisierung: ");
        sb.Append(min.ToString("HH:mm"));
        LastUpadateAsStr = sb.ToString();

        CancelReasonsText = CancelReasonsStringBuilder.ToString();
    }

    private async Task Run()
    {
        while (true) {
            try
            {
                await LoadStationData();
                await Task.Delay(TimeSpan.FromSeconds(50));
            } catch (Exception ex)
            {
                log.Error($"Fehler: {ex.Message} {ex.StackTrace}");
            }
        }
    }
 


}
