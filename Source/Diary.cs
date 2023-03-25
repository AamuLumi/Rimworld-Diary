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
using static UnityEngine.UI.Image;

namespace Diary
{
    public class Diary : Mod
    {
        DiarySettings settings;

        public Diary(ModContentPack content)
            : base(content)
        {
            var harmony = new Harmony("aamulumi.diary");

            Init(harmony);

            if (ModLister.GetActiveModWithIdentifier("neptimus7.progressrenderer") != null)
            {
                InitWithProgressRenderer(harmony);
            }
        }

        public void Init(Harmony harmony)
        {
            settings = GetSettings<DiarySettings>();

            harmony.PatchAll();
        }

        public void InitWithProgressRenderer(Harmony harmony)
        {
            var PR_RenderManager = System.Type.GetType(
                "ProgressRenderer.MapComponent_RenderManager, Progress-Renderer"
            );

            if (PR_RenderManager != null)
            {
                var createFilePath = PR_RenderManager.GetMethod("CreateFilePath", AccessTools.all);

                harmony.Patch(
                    createFilePath,
                    postfix: new HarmonyMethod(
                        typeof(ListenProgressRenderer_CreateFilePath).GetMethod(
                            "Postfix",
                            AccessTools.all
                        )
                    )
                );

                settings.ConnectedToProgressRenderer = true;
            }
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
