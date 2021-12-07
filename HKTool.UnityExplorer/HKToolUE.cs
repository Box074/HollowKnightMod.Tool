using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Modding;
using HKTool.DebugTools;
using UnityEngine;
using UnityExplorer;
using Language;

namespace HKTool.UnityExplorer
{
    public class HKToolUE : ModBase , IDebugViewBase
    {
        protected override (LanguageCode, string)[] Languages => new (LanguageCode, string)[]
        {
            (LanguageCode.ZH, "HKTool.UnityExplorer.Lang.zh.txt")
        };
        protected override LanguageCode DefaultLanguageCode => LanguageCode.ZH;
        public HKToolUE() : base("HKTool UnityExplorer")
        {
            
        }
        public override void Initialize()
        {
            UnityExplorerLoader.Init();
        }

        public override void OnDebugDraw()
        {
            if (GUILayout.Button("HKTool.UE.EditPlayerData".Get()))
            {
                InspectorManager.Inspect(PlayerData.instance);
            }
            if (GUILayout.Button("HKTool.UE.EditHeroController".Get()))
            {
                InspectorManager.Inspect(HeroController.instance);
            }
            if (GUILayout.Button("HKTool.UE.EditBossSceneController".Get()))
            {
                InspectorManager.Inspect(BossSceneController.Instance);
            }
        }
    }
}
