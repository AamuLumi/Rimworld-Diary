using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace DiaryMod
{
    public class DiaryMod : Mod
    {
        private DiarySettings settings;

        public DiaryMod(ModContentPack content)
            : base(content)
        {
            var harmony = new Harmony("aamulumi.diary");

            Init(harmony);

            if (ModLister.GetActiveModWithIdentifier("neptimus7.progressrenderer") != null)
                InitWithProgressRenderer(harmony);

            if (ModLister.GetActiveModWithIdentifier("Torann.RimWar") != null) InitWithRimWar(harmony);
        }

        public void Init(Harmony harmony)
        {
            settings = GetSettings<DiarySettings>();

            harmony.PatchAll();
        }

        public void InitWithProgressRenderer(Harmony harmony)
        {
            var PR_RenderManager = Type.GetType(
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

        public void InitWithRimWar(Harmony harmony)
        {
            var RW_Letter = Type.GetType(
                "RimWar.History.RW_Letter, RimWar");

            if (RW_Letter != null) settings.AddIgnoreArchivableClass(RW_Letter);
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