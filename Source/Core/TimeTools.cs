﻿using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Diary
{
    public class TimeTools
    {
        public static Vector2 GetCurrentLocation()
        {
            Vector2 vector;

            if (WorldRendererUtility.WorldRendered && Find.WorldSelector.SelectedTile >= 0)
            {
                vector = Find.WorldGrid.LongLatOf(Find.WorldSelector.SelectedTile);
            }
            else if (WorldRendererUtility.WorldRendered && Find.WorldSelector.NumSelectedObjects > 0)
            {
                vector = Find.WorldGrid.LongLatOf(Find.WorldSelector.FirstSelectedObject.Tile);
            }
            else
            {
                if (Find.CurrentMap == null) return new Vector2();
                vector = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
            }

            return vector;
        }

        public static int GetCurrentTicks()
        {
            return Find.TickManager.TicksAbs;
        }

        public static int GetCurrentHour()
        {
            return GenDate.HourOfDay(Find.TickManager.TicksAbs, GetCurrentLocation().x);
        }

        public static int GetCurrentDay()
        {
            return GenDate.DayOfQuadrum(Find.TickManager.TicksAbs, GetCurrentLocation().x);
        }

        public static Quadrum GetCurrentQuadrum()
        {
            return GenDate.Quadrum(Find.TickManager.TicksAbs, GetCurrentLocation().x);
        }

        public static int GetCurrentYear()
        {
            return GenDate.Year(Find.TickManager.TicksAbs, GetCurrentLocation().x);
        }

        public static int GetMinimumDay(Quadrum quadrum, int year)
        {
            if (quadrum != GenDate.Quadrum(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x) ||
                year != GenDate.Year(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x)) return 0;

            return GenDate.DayOfQuadrum(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x);
        }

        public static Quadrum GetMinimumQuadrum(int year)
        {
            if (year != GenDate.Year(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x)) return Quadrum.Aprimay;

            return GenDate.Quadrum(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x);
        }

        public static int GetMinimumYear()
        {
            return GenDate.Year(Find.TickManager.gameStartAbsTick, GetCurrentLocation().x);
        }

        public static int GetMaximumDay(Quadrum quadrum, int year)
        {
            if (quadrum != GetCurrentQuadrum() || year != GetCurrentYear()) return GenDate.DaysPerQuadrum - 1;

            return GetCurrentDay();
        }

        public static Quadrum GetMaximumQuadrum(int year)
        {
            if (year != GetCurrentYear()) return Quadrum.Undefined - 1;

            return GetCurrentQuadrum();
        }

        public static int GetMaximumYear()
        {
            return GetCurrentYear();
        }

        public static int GetHourFromTicks(int ticks, bool startingFromStartHourOfDay = false)
        {
            if (startingFromStartHourOfDay)
                return (GenDate.GameStartHourOfDay +
                        GenDate.HourOfDay(ticks, GetCurrentLocation().x)) % GenDate.HoursPerDay;

            return GenDate.HourOfDay(ticks, GetCurrentLocation().x) % GenDate.HoursPerDay;
        }
    }
}