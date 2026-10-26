using Diary.Core;
using Diary.Settings;
using RimWorld;
using Verse;

// Don't change the namespace for compatibility with old versions
namespace Diary;

internal class GameComponent_AutomaticExport : GameComponent
{
    private bool _enabled;
    private int _nbTicksForCurrentPeriod;
    private AutomaticExportPeriod _period;

    public GameComponent_AutomaticExport(Game g)
    {
        _enabled = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().AutomaticExportEnabled;
        _period = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().AutomaticExportPeriod;
        SetTicksNumberForCurrentPeriod();
    }

    private void SetTicksNumberForCurrentPeriod()
    {
        switch (_period)
        {
            case AutomaticExportPeriod.Day:
                _nbTicksForCurrentPeriod = GenDate.TicksPerDay;
                break;
            case AutomaticExportPeriod.Month:
                _nbTicksForCurrentPeriod = GenDate.TicksPerQuadrum;
                break;
            case AutomaticExportPeriod.Week:
                _nbTicksForCurrentPeriod = GenDate.TicksPerDay * 7;
                break;
        }
    }

    public void OnSettingsUpdate(bool enabled, AutomaticExportPeriod period)
    {
        _enabled = enabled;
        _period = period;
        SetTicksNumberForCurrentPeriod();
    }

    public override void GameComponentTick()
    {
        if (!_enabled) return;

        if (Find.TickManager.TicksGame % _nbTicksForCurrentPeriod == 0)
            Current.Game.GetComponent<DiaryService>().Export(true);
    }
}