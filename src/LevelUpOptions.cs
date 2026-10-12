using System;
using System.IO;
using PeterHan.PLib.Options;
using UnityEngine;

namespace FastLevelUp
{
    /// <summary>
    /// 玩家可调选项，保存在 mod 目录下 config.json。
    /// 默认值与 v1（硬编码版）完全一致，保证旧存档行为不变。
    /// </summary>
    /// <remarks>
    /// 注意：只能用 Int/Float 滑块与 bool 勾选这几类选项。
    /// PLib 的枚举下拉（SelectOneOptionsEntry）会用 PComboBox.SetMinWidthInCharacters，
    /// 而该方法依赖的 TMPro.FaceInfo 在当前版游戏的 Unity.TextMeshPro 里已不存在，
    /// 一打开选项面板就会抛 TypeLoadException 直接崩游戏。
    /// </remarks>
    public class LevelUpOptions
    {
        /// <summary>配置重扫间隔（毫秒）：在选项面板保存后约 1 秒内生效，无需重启游戏。</summary>
        private const int RESCAN_INTERVAL_MS = 1000;

        private const int MIN_LEVEL = 1;
        private const int MAX_LEVEL = 500;
        private const int MIN_CYCLE = 5;
        private const int MAX_CYCLE = 2000;
        private const float MIN_SPEED = 0.1f;
        private const float MAX_SPEED = 20f;
        private const float MIN_POWER = 0.6f;
        private const float MAX_POWER = 2.5f;

        /// <summary>一个游戏周期的秒数。</summary>
        private const float SECONDS_PER_CYCLE = 600f;

        /// <summary>
        /// <summary>
        /// 「随上限缩放」模式的基准上限：在这个上限下总预算正好等于上限在 v1 时的设定，
        /// 因此调到 50 级时两个模式结果相同，行为不会因为新增模式而改变。
        /// </summary>
        private const int BUDGET_REFERENCE_LEVEL = 50;

        /// <summary>达到等级上限时返回的「不可能达到」经验值（沿用 v1 做法）。</summary>
        public const float BLOCKED_REQUIREMENT = 99999999f;

        private static LevelUpOptions cached;
        private static string configPath;
        private static System.DateTime lastStamp;
        private static long lastCheckMs;

        private float[] requirements;

        /// <summary>当前生效的配置（带缓存，永不返回 null）。</summary>
        public static LevelUpOptions Instance
        {
            get
            {
                long now = (long)(Time.realtimeSinceStartup * 1000.0);
                if (cached != null && now - lastCheckMs < RESCAN_INTERVAL_MS)
                    return cached;
                lastCheckMs = now;

                try
                {
                    if (configPath == null)
                        configPath = POptions.GetConfigFilePath(typeof(LevelUpOptions));

                    System.DateTime stamp = System.IO.File.GetLastWriteTimeUtc(configPath);
                    if (cached == null || stamp != lastStamp)
                    {
                        lastStamp = stamp;
                        LevelUpOptions read = POptions.ReadSettings<LevelUpOptions>();
                        cached = read != null ? read.Sanitize() : new LevelUpOptions();
                    }
                }
                catch (Exception ex)
                {
                    if (cached == null)
                        cached = new LevelUpOptions();
                    Debug.LogWarning("[FastLevelUp] 读取配置失败，使用当前值: " + ex.Message);
                }
                return cached;
            }
        }

        // 下面 Option 特性里的三个字符串都是「翻译键」，不是显示文本。
        // 英文默认值在 STRINGS.cs，中文在 translations/zh.po。
        // 键的拼法固定为「命名空间.类名.字段名」，见 STRINGS.cs 的注释。

        [Option("FastLevelUp.STRINGS.OPTIONS.MAX_LEVEL.NAME",
            "FastLevelUp.STRINGS.OPTIONS.MAX_LEVEL.TOOLTIP",
            "FastLevelUp.STRINGS.OPTIONS.CATEGORY_LEVEL.NAME")]
        [Limit(MIN_LEVEL, MAX_LEVEL)]
        public int MaxLevel { get; set; } = 50;

        [Option("FastLevelUp.STRINGS.OPTIONS.SCALE_BUDGET_WITH_CAP.NAME",
            "FastLevelUp.STRINGS.OPTIONS.SCALE_BUDGET_WITH_CAP.TOOLTIP",
            "FastLevelUp.STRINGS.OPTIONS.CATEGORY_LEVEL.NAME")]
        public bool ScaleBudgetWithCap { get; set; } = false;

        [Option("FastLevelUp.STRINGS.OPTIONS.TARGET_CYCLE.NAME",
            "FastLevelUp.STRINGS.OPTIONS.TARGET_CYCLE.TOOLTIP",
            "FastLevelUp.STRINGS.OPTIONS.CATEGORY_SPEED.NAME")]
        [Limit(MIN_CYCLE, MAX_CYCLE)]
        public int TargetCycle { get; set; } = 150;

        [Option("FastLevelUp.STRINGS.OPTIONS.SPEED_MULTIPLIER.NAME",
            "FastLevelUp.STRINGS.OPTIONS.SPEED_MULTIPLIER.TOOLTIP",
            "FastLevelUp.STRINGS.OPTIONS.CATEGORY_SPEED.NAME")]
        [Limit(MIN_SPEED, MAX_SPEED)]
        public float SpeedMultiplier { get; set; } = 1f;

        [Option("FastLevelUp.STRINGS.OPTIONS.EXP_CURVE_POWER.NAME",
            "FastLevelUp.STRINGS.OPTIONS.EXP_CURVE_POWER.TOOLTIP",
            "FastLevelUp.STRINGS.OPTIONS.CATEGORY_SPEED.NAME")]
        [Limit(MIN_POWER, MAX_POWER)]
        public float ExpCurvePower { get; set; } = 1.8f;

        [Option("FastLevelUp.STRINGS.OPTIONS.CARRY_OVER_SURPLUS.NAME",
            "FastLevelUp.STRINGS.OPTIONS.CARRY_OVER_SURPLUS.TOOLTIP",
            "FastLevelUp.STRINGS.OPTIONS.CATEGORY_SPEED.NAME")]
        public bool CarryOverSurplus { get; set; } = true;

        /// <summary>把越界值夹回安全范围，避免手改 config.json 导致异常。</summary>
        public LevelUpOptions Sanitize()
        {
            MaxLevel = Mathf.Clamp(MaxLevel, MIN_LEVEL, MAX_LEVEL);
            TargetCycle = Mathf.Clamp(TargetCycle, MIN_CYCLE, MAX_CYCLE);

            //手改 config.json 可能塞进 NaN / Infinity（比如 1e999），Clamp 挡不住它们，
            //一进来就会污染整条经验公式，所以显式兜底。
            if (!IsFinite(SpeedMultiplier))
                SpeedMultiplier = 1f;
            else
                SpeedMultiplier = Mathf.Clamp(SpeedMultiplier, MIN_SPEED, MAX_SPEED);

            if (!IsFinite(ExpCurvePower))
                ExpCurvePower = 1.8f;
            else
                ExpCurvePower = Mathf.Clamp(ExpCurvePower, MIN_POWER, MAX_POWER);

            return this;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        /// <summary>
        /// 升到指定等级所需的累计练习秒数。
        /// 默认参数下与 v1 公式完全等价：((level / MaxLevel) ^ Power) * TargetCycle * 600
        /// </summary>
        public float GetTotalExperienceForLevel(float level)
        {
            float budget = TargetCycle * SECONDS_PER_CYCLE;
            if (ScaleBudgetWithCap)
                budget *= MaxLevel / (float)BUDGET_REFERENCE_LEVEL;
            return Mathf.Pow(level / MaxLevel, ExpCurvePower) * budget;
        }

        /// <summary>
        /// 从 level 升到 level+1 所需经验，走预计算表（这是每个复制人每次获得经验都会调用的热路径）。
        /// </summary>
        public float GetRequirementForLevel(int level)
        {
            if (requirements == null)
                BuildRequirementTable();
            if (level < 0)
                level = 0;
            if (level >= MaxLevel)
                return BLOCKED_REQUIREMENT;
            return requirements[level];
        }

        /// <summary>按当前配置预生成 0 ~ MaxLevel-1 每级的经验需求。</summary>
        private void BuildRequirementTable()
        {
            int count = Mathf.Max(1, MaxLevel);
            requirements = new float[count + 1];
            for (int i = 0; i < count; i++)
            {
                float current = GetTotalExperienceForLevel(i);
                float next = GetTotalExperienceForLevel(i + 1f);
                float need = (next - current) / SpeedMultiplier;
                if (!IsFinite(need))
                    need = BLOCKED_REQUIREMENT; // 兜底：宁可卡住也不要产出 NaN
                requirements[i] = need < 1f ? 1f : need;
            }
            requirements[count] = requirements[count - 1]; // 越界兜底，正常不会取到
        }
    }
}
