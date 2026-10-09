using System.Collections.Generic;
using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace FastLevelUp.Patches
{
    /// <summary>
    /// 抬高 AttributeLevel 的等级上限。
    /// 兼容旧存档：v1 存档里 maxGainedLevel 已为 50，这里只在需要时抬高，不做下调。
    /// </summary>
    [HarmonyPatch(typeof(AttributeLevel), "AddExperience")]
    public static class MaxGainedLevelPatch
    {
        /// <summary>已提示过的属性等级，避免每个复制人每帧刷屏。</summary>
        private static readonly HashSet<AttributeLevel> warned = new HashSet<AttributeLevel>();

        /// <summary>上次检查时的上限；上限改过之后要重新允许提示一次。</summary>
        private static int lastCap = -1;

        public static void Prefix(AttributeLevel __instance)
        {
            int cap = LevelUpOptions.Instance.MaxLevel;
            int level = __instance.level;

            if (cap != lastCap)
            {
                lastCap = cap;
                warned.Clear();
            }

            if (level >= cap && warned.Add(__instance))
            {
                // 玩家把上限调到了比现有等级更低。
                // 不会掉级、属性加成保留，但 AddExperience 会因为 level >= maxGainedLevel 直接返回 false，
                // 小人从此完全吃不到经验。这里提示一次，免得被当成 mod 坏了。
                Debug.LogWarning(string.Format(
                    "[FastLevelUp] {0} 的 {1} 已是 {2} 级，高于当前设置的等级上限 {3}，该属性不会再获得经验。" +
                    "把「等级上限」调回 {4} 以上即可继续升级（等级与属性加成都没有损失）。",
                    GetOwnerName(__instance), GetAttributeName(__instance), level, cap, level));
            }

            if (__instance.maxGainedLevel < cap)
                __instance.maxGainedLevel = cap;
        }

        private static string GetAttributeName(AttributeLevel instance)
        {
            return instance.attribute != null && instance.attribute.Attribute != null
                ? instance.attribute.Attribute.Id
                : "未知属性";
        }

        private static string GetOwnerName(AttributeLevel instance)
        {
            GameObject go = instance.attribute != null ? instance.attribute.gameObject : null;
            return go != null ? go.name : "未知单位";
        }
    }
}
