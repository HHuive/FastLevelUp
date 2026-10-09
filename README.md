# FastLevelUp（快速升级）

缺氧（Oxygen Not Included）本地 mod：提升复制人技能等级上限并加速升级，数值全部可在游戏内调整，改完即时生效。

## 安装

1. 下载 Release 里的压缩包，解压到
   `Documents\Klei\OxygenNotIncluded\mods\local\FastLevelUp\`
   （目录里应包含 `FastLevelUp.dll`、`PLib.dll`、`mod.yaml`、`mod_info.yaml`）
2. 游戏主菜单 → 模组 → 勾选 **快速升级** → 重启游戏
3. 依赖 [PLib](https://github.com/peterhanel/PLib) 提供选项面板，已随包附带

## 从源码构建

```bash
dotnet build
```

编译后自动部署到 `Documents\Klei\OxygenNotIncluded\mods\local\FastLevelUp`。
游戏不在默认位置时指定路径：

```bash
dotnet build -p:OniDir="E:\SteamLibrary\steamapps\common\OxygenNotIncluded"
```

要求：.NET SDK（目标框架 `net48`）、缺氧本体（构建需要引用游戏程序集）。

## 使用

模组列表里点击该 mod 的 **选项** 按钮，调整：

| 选项 | 默认 | 说明 |
| --- | --- | --- |
| 等级上限 | 50 | 1–500，可升到的最高等级 |
| 预算随上限缩放 | 关 | 关：总量与上限无关（旧版行为）；开：上限相对 50 级翻 N 倍，总量也翻 N 倍 |
| 满级练习周期 | 150 | 升到上限需要的练习周期数，越小越快。**不是日历周期**，实际日历时间约为其 1.5–2 倍 |
| 升级速度倍率 | 1.0 | 0.1–20；2 = 所需经验减半 |
| 经验曲线陡峭度 | 1.8 | 0.6–2.5。=1 每级等成本；>1 越往后越贵（原版手感）；<1 前重后轻、后期越升越快 |
| 溢出经验结转 | 开 | 升级时把多出来的经验留到下一级，而不是像原版那样清零 |

保存在选项面板后约 1 秒生效，**无需重启**。
配置文件：`mods/local/FastLevelUp/config.json`（也可直接手改，非法的数值会被自动夹回范围）。

## 目录结构

```
FastLevelUp/
├── FastLevelUp.csproj      # 构建 + 自动部署目标
├── mod.yaml                # 模组标题/描述
├── mod_info.yaml           # 支持的版本/API
└── src/
    ├── FastLevelUpMod.cs                     # 入口：注册 PLib 选项 + PatchAll
    ├── LevelUpOptions.cs                     # 可调选项 + 配置读取 + 需求表预计算
    └── Patches/
        ├── ExperienceRequirementPatch.cs     # 重写升级所需经验（查预计算表）
        ├── MaxGainedLevelPatch.cs            # 抬高 maxGainedLevel
        └── SurplusExperiencePatch.cs         # 升级时结转溢出经验
```

### 曲线公式

```
累计经验(level) = (level / MaxLevel) ^ Power × Cycle × 600 × 缩放系数
每级需求      = 累计(level+1) − 累计(level)，再除以速度倍率，最低 1
```

Klei 原版公式形状完全一致，只是常量不同（原版：上限 20 / 幂 1.7 / 400 周期）。
所有选项取默认值时，本 mod 的每级需求与 v1（硬编码版）逐项相同。

## 实现要点

- **选项 UI** 由 [PLib](https://github.com/peterhanel/PLib) 4.25 提供（`POptions`）；NuGet 包名就叫 `PLib`
- 经验公式：`((level+1)/MaxLevel)^Power × TargetCycle × 600` 减去当前级累计值，再除以速度倍率
- 游戏 DLL 引用全部 `<Private>false</Private>`，bin 里只留本 mod DLL + PLib.dll + 两个 yaml
- `TargetFramework` 为 `net48`（游戏程序集版本）而不是 net472
- **⚠ 不要用枚举选项（下拉框）**：PLib 的 `SelectOneOptionsEntry` 构建时会调 `PComboBox.SetMinWidthInCharacters`，后者依赖 `TMPro.FaceInfo`，而新版 Unity.TextMeshPro 已移除该类型——一打开选项面板就 `TypeLoadException` 直接崩游戏（PLib 4.25 也一样）。滑块（int/float）与勾选框是安全的，所以预算模式用勾选框表达。

## 边界行为

### 卸载 / 停用 mod 后读档

`AttributeLevels` 只往存档写 `attributeId`、`experience`、`level` 三项，**本 mod 不写自己的任何数据**。因此：

- 等级与属性加成**完整保留**（卸载不会掉级）
- 上限回到原版 20：`maxGainedLevel` 不落盘，每次进游戏都由 `OnPrefabInit` 重置
- 已超过 20 级的小人**不能再吃经验**（原版 `AddExperience` 开头就 return false），无报错、无崩溃
- 重新启用本 mod 后上限被抬回设定值，可继续升级

### 把上限调得比现有等级更低

与上一种情况同构：不掉级、加成不丢，只是不再获得经验。
此时游戏日志会一次性写明是哪个单位、哪个属性、多少级、上限多少以及如何恢复；把上限调回现有等级以上即可，无需其它操作。

### 极端设置的边界

- 倍率 20 + 上限 500 + 周期 5 时，单次需求被 `1f` 下限托住，配合溢出结转可以在短时间内连跳多级——这是设置意图内的结果，不是 bug
- 手改 `config.json` 塞入 NaN / Infinity / 越界值会被 `Sanitize()` 夹回安全范围，不会污染经验公式

## 发布打包

```powershell
.\tools\package.ps1 -Version 1.1.0
```

产出 `dist\FastLevelUp-v1.1.0.zip`，内含 `FastLevelUp.dll`、`PLib.dll`、`mod.yaml`、`mod_info.yaml` 四个文件，
解压到 `mods\local\FastLevelUp\` 即可使用。

CI 无法自动构建：本项目编译时必须引用游戏本体的 `Assembly-CSharp.dll` 等程序集，
而这些文件不便于随仓库分发，也不在 GitHub Runner 上，因此 Release 由维护者本地打包后上传。

## 变更记录

- **v1.1.0**：加入可调选项面板（原版依赖 PLib）；经验预算模式、曲线陡峭度放宽到 0.6–2.5、溢出经验结转、需求表预计算；结构重命名为 FastLevelUp
- **v1.0.0**：硬编码 50 级上限 + 150 周期曲线（旧存档不受影响：默认值与其完全一致）
