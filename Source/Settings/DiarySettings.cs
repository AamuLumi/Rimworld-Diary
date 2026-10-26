using Diary.Core;
using RimWorld;
using UnityEngine;
using Verse;

namespace Diary.Settings;

public class DiarySettings : ModSettings
{
    private readonly List<Type> _ignoredArchivableClasses;
    private bool _areDescriptionExportedWithEvents;
    private bool _automaticExportEnabled;
    private AutomaticExportPeriod _automaticExportPeriod;
    private LogFilter _defaultLogFilter;
    private DefaultMessage _defaultMessage;
    private ExportFormat _exportFormat;
    private string _folderPath;
    private bool _isEventAddedWithCurrentHour;
    private LogWriterFilter _logWriterFilter;
    private bool _previousAutomaticExportEnabled;

    public bool isConnectedToProgressRenderer = false;

    public DiarySettings()
    {
        _ignoredArchivableClasses = new List<Type>();
    }

    public string FolderPath => _folderPath;

    public ExportFormat ExportFormat => _exportFormat;

    public DefaultMessage DefaultMessage => _defaultMessage;

    public LogWriterFilter LogWriterFilter => _logWriterFilter;

    public LogFilter DefaultLogFilter => _defaultLogFilter;

    public bool AutomaticExportEnabled => _automaticExportEnabled;

    public AutomaticExportPeriod AutomaticExportPeriod => _automaticExportPeriod;

    public bool AreDescriptionExportedWithEvents => _areDescriptionExportedWithEvents;

    public bool IsEventAddedWithCurrentHour => _isEventAddedWithCurrentHour;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref _folderPath, "folderPath", Application.dataPath);
        Scribe_Values.Look(ref _exportFormat, "exportFormat");
        Scribe_Values.Look(ref _defaultMessage, "defaultMessage", DefaultMessage.Empty);
        Scribe_Values.Look(ref _logWriterFilter, "logWriterFilter", LogWriterFilter.None);
        Scribe_Values.Look(ref _defaultLogFilter, "defaultLogFilter", LogFilter.Events);
        Scribe_Values.Look(ref _automaticExportEnabled, "automaticExportEnabled");
        Scribe_Values.Look(ref _automaticExportPeriod, "automaticExportPeriod");
        Scribe_Values.Look(ref _areDescriptionExportedWithEvents, "areDescriptionExportedWithEvents");
        Scribe_Values.Look(ref _isEventAddedWithCurrentHour, "isEventAddedWithCurrentHour", true);
    }

    public void AddIgnoreArchivableClass(Type t)
    {
        _ignoredArchivableClasses.Add(t);
    }

    public bool ArchivableShouldBeIgnored(IArchivable a)
    {
        if (_ignoredArchivableClasses == null) return false;

        foreach (var type in _ignoredArchivableClasses)
            if (a.GetType() == type)
                return true;

        return false;
    }

    public void SetFormat(ExportFormat f)
    {
        _exportFormat = f;
    }

    public void SetDefaultMessage(DefaultMessage m)
    {
        _defaultMessage = m;
    }

    public void SetDefaultLogFilter(LogFilter f)
    {
        _defaultLogFilter = f;
    }

    public void SetLogWriterFilter(LogWriterFilter f)
    {
        _logWriterFilter = f;
    }

    public void SetAutomaticExportEnabled(bool b)
    {
        _automaticExportEnabled = b;

        UpdateGameComponent();
    }

    public void SetIsEventAddedWithCurrentHour(bool b)
    {
        _isEventAddedWithCurrentHour = b;
    }

    private void UpdateGameComponent()
    {
        try
        {
            var gameComponent = Current.Game.GetComponent<GameComponent_AutomaticExport>();

            gameComponent?.OnSettingsUpdate(_automaticExportEnabled, _automaticExportPeriod);
        }
        catch (NullReferenceException)
        {
            // Do nothing, gameComponent is not loaded
        }
    }

    public void SetAutomaticExportPeriod(AutomaticExportPeriod p)
    {
        _automaticExportPeriod = p;

        UpdateGameComponent();
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        if (_automaticExportEnabled != _previousAutomaticExportEnabled) UpdateGameComponent();

        _previousAutomaticExportEnabled = _automaticExportEnabled;


        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);

        listingStandard.Label("Diary_Export_Folder_Path".Translate());
        _folderPath = listingStandard.TextEntry(_folderPath);

        if (listingStandard.ButtonText("Diary_Reset_To_Default_Path".Translate()))
            _folderPath = Application.dataPath;

        listingStandard.GapLine();

        listingStandard.Label("Diary_Export_Format".Translate());
        if (listingStandard.ButtonText(DiaryTypeTools.GetFormatName(_exportFormat)))
        {
            var list = new List<FloatMenuOption>();
            foreach (int i in Enum.GetValues(typeof(ExportFormat)))
            {
                var current = i;
                list.Add(new FloatMenuOption(DiaryTypeTools.GetFormatName((ExportFormat)i),
                    delegate { SetFormat((ExportFormat)current); }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        listingStandard.GapLine();

        listingStandard.Label("Diary_Automatic_Export".Translate());

        listingStandard.CheckboxLabeled("Diary_Automatic_Export_Enabled".Translate(), ref _automaticExportEnabled);

        listingStandard.Label("Diary_Automatic_Export_Period".Translate());

        if (listingStandard.ButtonText(DiaryTypeTools.GetAutomaticExportPeriodName(_automaticExportPeriod)))
        {
            var list = new List<FloatMenuOption>();
            foreach (int i in Enum.GetValues(typeof(AutomaticExportPeriod)))
            {
                var current = i;
                list.Add(new FloatMenuOption(DiaryTypeTools.GetAutomaticExportPeriodName((AutomaticExportPeriod)i),
                    delegate { SetAutomaticExportPeriod((AutomaticExportPeriod)current); }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        listingStandard.GapLine();

        listingStandard.Label("Diary_Default_Log_Filter".Translate());
        if (listingStandard.ButtonText(DiaryTypeTools.GetLogFilterName(_defaultLogFilter)))
        {
            var list = new List<FloatMenuOption>();
            foreach (int i in Enum.GetValues(typeof(LogFilter)))
            {
                var current = i;
                list.Add(new FloatMenuOption(DiaryTypeTools.GetLogFilterName((LogFilter)i),
                    delegate { SetDefaultLogFilter((LogFilter)current); }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        listingStandard.CheckboxLabeled("Diary_Export_Description_With_Events".Translate(),
            ref _areDescriptionExportedWithEvents);

        listingStandard.Gap();

        listingStandard.CheckboxLabeled("Diary_Use_Current_Hour_When_Adding_Event_In_Diary".Translate(),
            ref _isEventAddedWithCurrentHour);

        listingStandard.Label("Diary_Log_Writer_Filter".Translate());
        listingStandard.Label("Diary_Log_Writer_Filter_Explanation".Translate());
        if (listingStandard.ButtonText(DiaryTypeTools.GetLogWriterFilterName(_logWriterFilter)))
        {
            var list = new List<FloatMenuOption>();
            foreach (int i in Enum.GetValues(typeof(LogWriterFilter)))
            {
                var current = i;
                list.Add(new FloatMenuOption(DiaryTypeTools.GetLogWriterFilterName((LogWriterFilter)i),
                    delegate { SetLogWriterFilter((LogWriterFilter)current); }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        listingStandard.GapLine();

        if (isConnectedToProgressRenderer)
            listingStandard.Label("Diary_Connected_To_Progress_Renderer".Translate());
        else
            listingStandard.Label("Diary_Not_Connected_To_Progress_Renderer".Translate());

        listingStandard.End();
    }
}