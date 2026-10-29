using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using STRINGS;
using UnityEngine;

namespace FastLevelUp
{
    /// <summary>mod 入口：注册选项面板、加载语言包、挂 Patch。</summary>
    public sealed class FastLevelUpMod : UserMod2
    {
        public const string VERSION = "1.1.2";

        /// <summary>
        /// 本 mod 所有文案键的前缀，「命名空间.类名」，与 Option 特性里写的一致。
        /// </summary>
        private const string STRINGS_KEY_PREFIX = "FastLevelUp.STRINGS.";

        public override void OnLoad(Harmony harmony)
        {
            try
            {
                // ★ base.OnLoad 内部就是 harmony.PatchAll(assembly)（U59-744825 反编译确认）。
                //   这里不要再手动 PatchAll，否则每个补丁会被挂两遍。
                base.OnLoad(harmony);

                PUtil.InitLibrary(); // 初始化 PLib 核心（其它带选项面板的 mod 都这么做）

                // 顺序很重要：先把自己的 STRINGS 注册进全局表并灌入翻译，
                // 再注册选项面板，这样面板第一次打开就是翻译后的文字。
                SetupLocalization();

                new POptions().RegisterOptions(this, typeof(LevelUpOptions));
                Debug.Log("[FastLevelUp] v" + VERSION + " 已加载，选项文件: " +
                    POptions.GetConfigFilePath(typeof(LevelUpOptions)));
            }
            catch (Exception ex)
            {
                Debug.LogError("[FastLevelUp] 加载失败: " + ex);
            }
        }

        /// <summary>
        /// 注册选项文案，并用 <c>translations/&lt;语言&gt;.po</c> 覆盖成当前语言。
        /// </summary>
        /// <remarks>
        /// 三步走：
        /// 1. <c>Localization.RegisterForTranslation</c> 把 STRINGS 类里的字段名拼成完整键
        ///    （FastLevelUp.STRINGS.OPTIONS.MAX_LEVEL.NAME）写入全局 Strings 表，值是英文默认文本。
        ///    同时把本程序集登记为「可翻译」，否则第 2 步遍历不到我们的字段。
        /// 2. <c>Localization.OverloadStrings</c> 用 PO 里的译文替换 STRINGS 类的 LocString 字段。
        /// 3. 关键一步：OverloadStrings 只改了字段，没改全局 Strings 表，而 PLib 的
        ///    <c>OptionsEntry.LookInStrings</c> 正是拿键去查那张表。所以要把译文再刷进表里，
        ///    否则面板上永远是英文默认值。
        /// </remarks>
        private static void SetupLocalization()
        {
            // 传 null 让游戏按 STRINGS.cs 所在命名空间（FastLevelUp）自动拼键，
            // 结果与 Option 特性里写的键完全一致，不要手写前缀以免对不上。
            Localization.RegisterForTranslation(typeof(STRINGS));

            string lang = Localization.GetCurrentLanguageCode();
            if (string.IsNullOrEmpty(lang))
            {
                Debug.Log("[FastLevelUp] 未取得语言码，使用内置英文默认值");
                return;
            }

            string modDir = PUtil.GetModPath(typeof(FastLevelUpMod).Assembly);
            if (string.IsNullOrEmpty(modDir))
            {
                Debug.LogWarning("[FastLevelUp] 无法确定 mod 目录，跳过语言包加载");
                return;
            }

            string translationsDir = Path.Combine(modDir, "translations");
            foreach (string code in LanguageCandidates(lang))
            {
                string path = Path.Combine(translationsDir, code + ".po");
                if (!File.Exists(path))
                    continue;

                Dictionary<string, string> translated =
                    Localization.ExtractTranslatedStrings(File.ReadAllLines(path, Encoding.UTF8));
                Localization.OverloadStrings(translated);
                PublishToStringsTable(translated);
                Debug.Log("[FastLevelUp] 已加载语言包: " + path);
                return;
            }

            Debug.Log("[FastLevelUp] 没有匹配 " + lang + " 的语言包，使用内置英文默认值");
        }

        /// <summary>
        /// 把译文写回全局 Strings 表，让 PLib 查键时能取到当前语言。
        /// </summary>
        /// <remarks>
        /// PO 文件里的键已经是完整键（命名空间.类名.嵌套类名.字段名），直接用即可。
        /// 只筛选本 mod 前缀，避免误改其它 mod 的同名键。
        /// </remarks>
        private static void PublishToStringsTable(Dictionary<string, string> translated)
        {
            foreach (KeyValuePair<string, string> pair in translated)
            {
                // 只有属于本 mod 的键才需要写，避免和其他 mod 撞键。
                if (pair.Key.StartsWith(STRINGS_KEY_PREFIX, StringComparison.Ordinal))
                    Strings.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// 把游戏的语言码映射成 PO 文件名候选，按优先级从精确到宽松。
        /// </summary>
        /// <remarks>
        /// 游戏内置语言码是 "zh"/"ja"/"ko"/"ru"/"en"，但装了 Klei 官方中文包时
        /// 会变成 "zh_klei"，Steam 语言名还可能是 "schinese"。逐个试，命中即止。
        /// </remarks>
        private static IEnumerable<string> LanguageCandidates(string lang)
        {
            string lower = lang.ToLowerInvariant();
            yield return lower;

            // zh_klei -> zh
            int underscore = lower.IndexOf('_');
            if (underscore > 0)
                yield return lower.Substring(0, underscore);

            switch (lower)
            {
                case "schinese":
                case "tchinese":
                case "chinese":
                    yield return "zh";
                    break;
                case "japanese":
                    yield return "ja";
                    break;
                case "korean":
                    yield return "ko";
                    break;
                case "russian":
                    yield return "ru";
                    break;
                case "english":
                    yield return "en";
                    break;
            }
        }
    }
}
