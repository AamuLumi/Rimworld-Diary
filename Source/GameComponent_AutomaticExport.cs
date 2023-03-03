using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using UnityEngine.PlayerLoop;
using Verse;

namespace Diary
{
    class GameComponent_AutomaticExport : GameComponent
    {
        private bool enabled;
        private AutomaticExportPeriod period;
        private int nbTicksForCurrentPeriod;

        public GameComponent_AutomaticExport(Game g)
        {
            enabled = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().AutomaticExportEnabled;
            period = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().AutomaticExportPeriod;
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
            if (!enabled)
            {
                return;
            }

            if (Find.TickManager.TicksGame % nbTicksForCurrentPeriod == 0)
            {
                Current.Game.GetComponent<DiaryService>().Export(true);
            }
        }
    }
}
