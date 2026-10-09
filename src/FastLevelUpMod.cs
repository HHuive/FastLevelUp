using System;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace FastLevelUp
{
    /// <summary>mod 入口：注册 PLib 选项面板并挂 Patch。</summary>
    public sealed class FastLevelUpMod : UserMod2
    {
        public const string VERSION = "1.1.0";

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony); // 必须调用，保持游戏本地化流程正常
            try
            {
                PUtil.InitLibrary(); // 初始化 PLib 核心（其它带选项面板的 mod 都这么做）
                new POptions().RegisterOptions(this, typeof(LevelUpOptions));
                harmony.PatchAll(typeof(FastLevelUpMod).Assembly);
                Debug.Log("[FastLevelUp] v" + VERSION + " 已加载，选项文件: " +
                    POptions.GetConfigFilePath(typeof(LevelUpOptions)));
            }
            catch (Exception ex)
            {
                Debug.LogError("[FastLevelUp] 加载失败: " + ex);
            }
        }
    }
}
