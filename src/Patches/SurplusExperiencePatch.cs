using HarmonyLib;
using Klei.AI;

namespace FastLevelUp.Patches
{
    /// <summary>
    /// 升级时保留溢出经验。
    /// 原版 LevelUp 会把 experience 清零且每次 AddExperience 只升 1 级，
    /// 所以一次结算多出来的经验会被丢弃，把速度倍率调到很高后也不再变快。
    /// 这里用 __state 把溢出部分带到 LevelUp 之后写回。
    /// </summary>
    [HarmonyPatch(typeof(AttributeLevel), "LevelUp")]
    public static class SurplusExperiencePatch
    {
        public static void Prefix(AttributeLevel __instance, out float __state)
        {
            __state = 0f;
            if (!LevelUpOptions.Instance.CarryOverSurplus)
                return;

            // 此时 level 尚未自增，GetExperienceForNextLevel 拿到的正是刚越过的需求值
            float surplus = __instance.experience - __instance.GetExperienceForNextLevel();
            if (surplus > 0f)
                __state = surplus;
        }

        public static void Postfix(AttributeLevel __instance, float __state)
        {
            if (__state > 0f)
                __instance.experience = __state;
        }
    }
}
