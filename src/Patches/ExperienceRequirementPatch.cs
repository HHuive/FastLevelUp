using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace FastLevelUp.Patches
{
    /// <summary>
    /// 重写「升到下一级所需经验」，直接查预计算表。
    /// 默认情况下公式与 v1 完全一致：(next - current) / 倍率。
    /// </summary>
    [HarmonyPatch(typeof(AttributeLevel), "GetExperienceForNextLevel")]
    public static class ExperienceRequirementPatch
    {
        public static void Postfix(AttributeLevel __instance, ref float __result)
        {
            int level = __instance.level;
            if (level < 0)
                return;

            __result = LevelUpOptions.Instance.GetRequirementForLevel(level);
            if (__result < 1f)
                __result = 1f;
        }
    }
}
