using RimWorld;
using Verse;

namespace DiaryMod
{
    internal class GameComponent_AutomaticExport : GameComponent
    {
        private bool enabled;
        private int nbTicksForCurrentPeriod;
        private AutomaticExportPeriod period;

        public GameComponent_AutomaticExport(Game g)
        {
            enabled = LoadedModManager.GetMod<DiaryMod>().GetSettings<DiarySettings>().AutomaticExportEnabled;
            period = LoadedModManager.GetMod<DiaryMod>().GetSettings<DiarySettings>().AutomaticExportPeriod;
            SetTicksNumberForCurrentPeriod();
        }

        private void SetTicksNumberForCurrentPeriod()
        {
            switch (period)
            {
                case AutomaticExportPeriod.Day:
                    nbTicksForCurrentPeriod = GenDate.TicksPerDay;
                    break;
                case AutomaticExportPeriod.Month:
                    nbTicksForCurrentPeriod = GenDate.TicksPerQuadrum;
                    break;
                case AutomaticExportPeriod.Week:
                    nbTicksForCurrentPeriod = GenDate.TicksPerDay * 7;
                    break;
            }
        }

        public void OnSettingsUpdate(bool enabled, AutomaticExportPeriod period)
        {
            this.enabled = enabled;
            this.period = period;
            SetTicksNumberForCurrentPeriod();
        }

        public override void GameComponentTick()
        {
            if (!enabled) return;

            if (Find.TickManager.TicksGame % nbTicksForCurrentPeriod == 0)
                Current.Game.GetComponent<DiaryService>().Export(true);
        }
    }
}