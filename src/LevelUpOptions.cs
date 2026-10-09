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

        [Option("等级上限", "复制人技能可升到的最高等级（默认 50，与旧版一致）", "等级")]
        [Limit(MIN_LEVEL, MAX_LEVEL)]
        public int MaxLevel { get; set; } = 50;

        [Option("预算随上限缩放", "关闭（默认）：升到上限需要的练习总量与上限无关，即旧版行为。开启：上限相对 50 级翻 N 倍，总练习量也翻 N 倍，更高的上限意味着更强的投入。", "等级")]
        public bool ScaleBudgetWithCap { get; set; } = false;

        [Option("满级练习周期", "从 0 级升到上限所需的「练习周期」数，越小越快。注意这不是日历周期：经验只在复制人干活时累积，且乘工作类型倍率（1~4）与训练速度修正，实际日历时间约为它的 1.5~2 倍。（默认 150，与旧版一致）", "升级速度")]
        [Limit(MIN_CYCLE, MAX_CYCLE)]
        public int TargetCycle { get; set; } = 150;

        [Option("升级速度倍率", "整体调整升级快慢：2 = 所需经验减半，0.5 = 所需经验翻倍（默认 1，与旧版一致）", "升级速度")]
        [Limit(MIN_SPEED, MAX_SPEED)]
        public float SpeedMultiplier { get; set; } = 1f;

        [Option("经验曲线陡峭度", "= 1 时每级所需练习时间相同（线性推进）；> 1 越往后越贵（原版手感）；< 1 前重后轻、后期越升越快。（默认 1.8，与旧版一致）", "升级速度")]
        [Limit(MIN_POWER, MAX_POWER)]
        public float ExpCurvePower { get; set; } = 1.8f;

        [Option("溢出经验结转", "升级时把超出部分的经验留到下一级，而不是清零。原版会丢弃溢出经验并限制每次只升一级，导致高速倍率调到很高后不再变快。默认开启。", "升级速度")]
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
