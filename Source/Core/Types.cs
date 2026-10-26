using RimWorld;
using Verse;
using Verse.AI;

namespace Diary.Core;

public enum ExportFormat
{
    Text,
    RTF,
    HTML
}

public enum DefaultMessage
{
    NoEntryFound,
    Empty
}

public enum LogFilter
{
    All,
    Events,
    Chats
}

public enum LogWriterFilter
{
    All,
    Events,
    Chats,
    None
}

public enum AutomaticExportPeriod
{
    Day,
    Week,
    Month
}

public class DiaryTypeTools
{
    public static string GetFormatName(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Text => "Diary_Format_Text".Translate(),
            ExportFormat.RTF => "Diary_Format_RTF".Translate(),
            ExportFormat.HTML => "Diary_Format_HTML".Translate(),
            _ => ""
        };
    }

    public static string GetDefaultMessageName(DefaultMessage m)
    {
        return m switch
        {
            DefaultMessage.Empty => "Diary_Empty".Translate(),
            DefaultMessage.NoEntryFound => "Diary_No_Entry_Found_Message".Translate(),
            _ => ""
        };
    }

    public static string GetLogFilterName(LogFilter f)
    {
        return f switch
        {
            LogFilter.All => "Diary_All".Translate(),
            LogFilter.Chats => "Diary_Chats".Translate(),
            LogFilter.Events => "Diary_Events".Translate(),
            _ => ""
        };
    }

    public static string GetLogWriterFilterName(LogWriterFilter f)
    {
        return f switch
        {
            LogWriterFilter.All => "Diary_All".Translate(),
            LogWriterFilter.Chats => "Diary_Chats".Translate(),
            LogWriterFilter.Events => "Diary_Events".Translate(),
            LogWriterFilter.None => "Diary_None".Translate(),
            _ => ""
        };
    }

    public static string GetAutomaticExportPeriodName(AutomaticExportPeriod f)
    {
        return f switch
        {
            AutomaticExportPeriod.Day => "Diary_Every_Day".Translate(),
            AutomaticExportPeriod.Week => "Diary_Every_Week".Translate(),
            AutomaticExportPeriod.Month => "Diary_Every_Month".Translate(),
            _ => ""
        };
    }
}

public class DiaryImageEntry : IExposable
{
    public int days;
    public int hours;
    public string path;
    public Quadrum quadrum;
    public int year;

    public DiaryImageEntry()
    {
    }

    public DiaryImageEntry(string path, int hours, int day, Quadrum quadrum, int year)
    {
        this.path = path;
        this.hours = hours;
        days = day;
        this.quadrum = quadrum;
        this.year = year;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref path, "Path");
        Scribe_Values.Look(ref hours, "Hours");
        Scribe_Values.Look(ref days, "Days");
        Scribe_Values.Look(ref quadrum, "Quadrum");
        Scribe_Values.Look(ref year, "Year");
    }
}

public class PawnStatus
{
    private readonly bool _dead;
    private readonly bool _downed;
    private Job _currentJob;
    private JobDriver _currentJobDriver;
    private Pawn _pawn;

    public PawnStatus(bool downed, bool dead)
    {
        _downed = downed;
        _dead = dead;
    }

    public bool Downed { get; }
    public bool Dead { get; }
    public Pawn Pawn { get; }
    public Job CurrentJob { get; }
    public JobDriver CurrentJobDriver { get; }

    public void SavePawnStatus(Pawn p)
    {
        _pawn = p;
        _currentJob = p.CurJob;
        _currentJobDriver = p.CurJob.GetCachedDriver(p);
    }

    public override string ToString()
    {
        return
            $"PawnStatus dead:{_dead} downed:{_downed} _pawn:{_pawn} currentJob:{_currentJob} currentJobDriver:{_currentJobDriver}";
    }
}