using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld.Planet;

namespace Diary
{
    public class Diary : Mod
    {
        DiarySettings settings;

        public Diary(ModContentPack content) : base(content)
        {
            settings = GetSettings<DiarySettings>();

            new Harmony("aamulumi.diary").PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Diary";
        }
    }
}
