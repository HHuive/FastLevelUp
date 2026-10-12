using STRINGS;

namespace FastLevelUp
{
    /// <summary>
    /// 选项面板的文案。这里的英文是「默认值」，也就是任何语言下找不到
    /// <c>translations/&lt;lang&gt;.po</c> 条目时的兜底内容。
    /// </summary>
    /// <remarks>
    /// 命名空间必须正好是 <c>FastLevelUp</c>、类名必须正好是 <c>STRINGS</c>：
    /// 游戏会用「命名空间.类名.字段名」拼出翻译键，PLib 的
    /// <c>OptionsEntry.LookInStrings</c> 再拿这个键去全局 Strings 表里查。
    /// 所以 Option 特性里写的是键名而不是明文，见 LevelUpOptions.cs。
    /// </remarks>
    public static class STRINGS
    {
        /// <summary>选项面板的分组标题与每个选项的名称/说明。</summary>
        public static class OPTIONS
        {
            /// <summary>选项分组：等级相关。</summary>
            public static class CATEGORY_LEVEL
            {
                public static LocString NAME = "Level";
            }

            /// <summary>选项分组：升级速度相关。</summary>
            public static class CATEGORY_SPEED
            {
                public static LocString NAME = "Leveling Speed";
            }

            public static class MAX_LEVEL
            {
                public static LocString NAME = "Max Level";

                public static LocString TOOLTIP =
                    "Highest skill level a duplicant can reach. Default: 50.\n\n" +
                    "The cap only limits further leveling; duplicants already above it keep " +
                    "their levels and attribute bonuses.";
            }

            public static class SCALE_BUDGET_WITH_CAP
            {
                public static LocString NAME = "Scale Cost With Cap";

                public static LocString TOOLTIP =
                    "Off (default): reaching the cap always takes the same total practice, " +
                    "no matter how high the cap is.\n" +
                    "On: raising the cap multiplies the total practice needed by the same " +
                    "amount, so a higher cap costs proportionally more.\n\n" +
                    "Only matters when Max Level is above 50. Default: off.";
            }

            public static class TARGET_CYCLE
            {
                public static LocString NAME = "Cycles to Max Level";

                public static LocString TOOLTIP =
                    "Practice cycles needed to go from level 0 to the cap. Lower is faster.\n\n" +
                    "This is not calendar time: experience only builds while a duplicant " +
                    "works, and is multiplied by the job type (1-4x) and training speed. " +
                    "Real calendar time usually ends up around 1.5-2x this number.\n\n" +
                    "Default: 150.";
            }

            public static class SPEED_MULTIPLIER
            {
                public static LocString NAME = "Leveling Speed";

                public static LocString TOOLTIP =
                    "Global multiplier on how fast duplicants level up:\n" +
                    "2 = experience needed is halved, 1 = unchanged, 0.5 = doubled.\n\n" +
                    "Default: 1.";
            }

            public static class EXP_CURVE_POWER
            {
                public static LocString NAME = "Curve Steepness";

                public static LocString TOOLTIP =
                    "Shape of the experience curve:\n" +
                    "1 = every level costs the same practice (linear).\n" +
                    "Above 1 = each level costs more than the last (vanilla feel).\n" +
                    "Below 1 = early levels are slow, later ones speed up.\n\n" +
                    "Default: 1.8.";
            }

            public static class CARRY_OVER_SURPLUS
            {
                public static LocString NAME = "Carry Over Surplus XP";

                public static LocString TOOLTIP =
                    "On (default): experience beyond the level requirement is kept toward " +
                    "the next level.\n" +
                    "Off: surplus experience is discarded and only one level can be gained " +
                    "per tick, so very high speed multipliers stop helping.\n\n" +
                    "Default: on.";
            }
        }
    }
}
