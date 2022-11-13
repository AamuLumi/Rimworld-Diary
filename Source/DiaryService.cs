using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using RimWorld;
using Verse;

namespace Diary
{
    public class DiaryService : GameComponent
    {
        private Dictionary<string, string> entries;

        public DiaryService(Game game)
        {
            entries = new Dictionary<string, string>();
        }

        private string[] GetEntriesToExport()
        {
            int currentItem = 0;
            string[] entriesToExport = new string[entries.Count];

            foreach (KeyValuePair<string, string> item in entries)
            {
                string[] strings = item.Key.Split('-');
                int day = int.Parse(strings[0]);
                Quadrum quadrum = (Quadrum)Quadrum.Parse(typeof(Quadrum), strings[1]);
                int year = int.Parse(strings[2]);

                entriesToExport[currentItem] = $"{Find.ActiveLanguageWorker.OrdinalNumber(day + 1)} {QuadrumUtility.Label(quadrum)} {year}\n\n{item.Value}\n";

                currentItem++;
            }

            return entriesToExport;
        }

        private string GetDictionaryKey(int day, Quadrum quadrum, int year)
        {
            return day.ToString() + '-' + quadrum.ToString() + '-' + year.ToString();
        }

        public string ReadEntry(int day, Quadrum quadrum, int year)
        {
            return entries.TryGetValue(GetDictionaryKey(day, quadrum, year), "Diary_No_Entry_Found".Translate());
        }
        public void WriteEntry(string data, int day, Quadrum quadrum, int year)
        {
            entries.SetOrAdd(GetDictionaryKey(day, quadrum, year), data);
        }

        public void Export()
        {
            string savename = Faction.OfPlayer.Name;
            string folder = LoadedModManager.GetMod<Diary>().GetSettings<DiarySettings>().FolderPath;

            try
            {
                File.WriteAllLines(Path.Combine(folder, "diary-" + savename + "-" + DateTime.Now.Ticks + ".txt"), GetEntriesToExport());

                Find.WindowStack.Add(new Dialog_ExportDiary("Diary_Export_Successful".Translate()));
            }
            catch (DirectoryNotFoundException)
            {
                Find.WindowStack.Add(new Dialog_ExportDiary("Diary_Directory_Not_Found".Translate()));
            }
            catch (Exception)
            {
                Find.WindowStack.Add(new Dialog_ExportDiary("Diary_Error_During_Export".Translate()));
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref entries, "entries", LookMode.Value, LookMode.Value);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }
    }
}
