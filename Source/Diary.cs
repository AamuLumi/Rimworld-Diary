using Diary.Patches;
using Diary.Settings;
using Diary.UI;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Diary;

public class Diary : Mod
{
    private DiarySettings _settings;

    public Diary(ModContentPack content)
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
        _settings = GetSettings<DiarySettings>();

        GUIScrollableTextArea.Patch(harmony);
        // harmony.PatchAll();
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

            _settings.isConnectedToProgressRenderer = true;
        }
    }

    public void InitWithRimWar(Harmony harmony)
    {
        var RW_Letter = Type.GetType(
            "RimWar.History.RW_Letter, RimWar");

        if (RW_Letter != null) _settings.AddIgnoreArchivableClass(RW_Letter);
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        _settings.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Diary";
    }
}