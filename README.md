# FastLevelUp（快速升级）

缺氧（Oxygen Not Included）mod：提高复制人技能等级上限，并调整升级速度与经验曲线。所有数值都能在游戏内修改，保存后约 1 秒生效。

**当前版本：v1.1.2** · 已验证游戏构建：U59-744825 · 依赖：PLib 4.25（已随包附带） · 许可证：MIT

[下载最新 Release](https://github.com/HHuive/FastLevelUp/releases/latest) · [问题反馈](https://github.com/HHuive/FastLevelUp/issues) · [PLib](https://github.com/peterhanel/PLib)

---

## 功能

- 等级上限 1–500，默认 50；已超过上限的小人不会掉级，属性加成保留。
- 可调整升满所需周期、全局速度倍率、经验曲线陡峭度。
- “升满所需练习量随上限缩放”可切换：关（默认）保持旧版总预算，开则上限越高、总练习量越大。
- 溢出经验结转：升级时多出来的经验留到下一级，避免高速倍率被原版清零吃掉。
- 选项面板支持中文 / 英文，跟随游戏语言。
- 默认参数与 v1.0 硬编码版逐项一致，旧存档行为不变。

## 安装

1. 从 [Releases](https://github.com/HHuive/FastLevelUp/releases/latest) 下载 `FastLevelUp-v1.1.2.zip`。
2. 解压到：

   ```
   Documents\Klei\OxygenNotIncluded\mods\local\FastLevelUp\
   ```

   目录应包含：

   ```
   FastLevelUp\
   ├── FastLevelUp.dll
   ├── PLib.dll
   ├── mod.yaml
   ├── mod_info.yaml
   └── translations\
       ├── zh.po
       └── en.po
   ```

3. 启动游戏 → 主菜单「模组」→ 勾选 **快速升级** → **重启游戏**。

> **只启用一个 FastLevelUp。** 如果你同时订阅了创意工坊版本，请禁用其中一个，避免两个同名 mod 同时挂补丁。

## 使用

在游戏主菜单「模组」中点击本 mod 的 **选项** 按钮：

| 选项（中文 / 英文） | 默认 | 说明 |
|---|---|---|
| 等级上限 / Max Level | 50 | 1–500，可升到的最高等级 |
| 升满所需练习量随上限缩放 / Scale Cost With Cap | 关 | 关：总量与上限无关（旧版行为）；开：上限越高，升满所需总量按比例增加 |
| 升满所需周期 / Cycles to Max Level | 150 | 从 0 级升到上限所需练习周期数，越小越快。**不是日历周期**，实际日历时间约为其 1.5–2 倍 |
| 升级速度倍率 / Leveling Speed | 1.0 | 0.1–20；2 = 所需经验减半，0.5 = 翻倍 |
| 经验曲线陡峭度 / Curve Steepness | 1.8 | 0.6–2.5；=1 每级等成本，>1 越往后越贵，<1 前期慢后期快 |
| 溢出经验结转 / Carry Over Surplus XP | 开 | 升级时把多余经验留到下一级，而不是像原版那样清零 |

保存后约 1 秒生效，**无需重启**。配置文件：

```
mods\local\FastLevelUp\config.json
```

也可以直接手改；越界值、`NaN` / `Infinity` 会被自动夹回安全范围。

## 兼容性

| 项 | 说明 |
|---|---|
| 已验证游戏构建 | U59-744825（Unity 6000.3.5f2） |
| `minimumSupportedBuild` | 489681 |
| `supportedContent` | ALL（本体 + DLC） |
| 目标框架 | `net48` |
| 外部依赖 | PLib 4.25，已随包附带 |

### 停用 / 卸载 mod 后

- 等级与属性加成**完整保留**，不会掉级。
- 等级上限回到原版 20；已经超过 20 级的小人不会掉级，但暂时不能再获得经验。
- 重新启用本 mod 后上限恢复为设置值，可继续升级。

### 把上限调得比现有等级更低

不会掉级、不会损失属性加成，但会立即停止获得新经验；日志会提示一次是哪个单位、哪个属性。把上限调回现有等级以上即可恢复。

## 多语言

- 英文默认文案在 `src/STRINGS.cs`，作为兜底。
- 其余语言放在 `translations/<语言代码>.po`，用 `msgstr` 覆盖默认值。
- 游戏语言为中文（含 `zh_klei`）时读取 `zh.po`，其它语言回退英文。
- 新增语言：复制 `translations/en.po`，修改文件头 `Language: xx` 与各条 `msgstr`，命名为 `xx.po` 放入 `translations/` 即可，不需要改代码。

## 经验曲线

```
累计经验(level) = (level / MaxLevel) ^ Power × Cycle × 600 × 缩放系数
每级需求       = 累计(level+1) − 累计(level)，再除以速度倍率，最低 1
```

原版公式形状一致，只是常量不同（原版：上限 20 / 幂 1.7 / 400 周期）。默认参数下，本 mod 的每级需求与 v1.0 逐项相同。

## 常见问题

**看不到“选项”按钮 / 面板打不开？**
确认 `PLib.dll` 和 `FastLevelUp.dll` 在同一个 mod 目录，然后重启游戏。PLib 选项面板需要 PLib 正确初始化。

**调低上限后小人还会升级吗？**
不会。调低上限只影响继续升级，不会掉级；把上限调回去即可恢复。

**停用 mod 后为什么 20 级以上不能获得经验？**
原版 `maxGainedLevel` 不落盘，停用 mod 后回到 20。等级和属性不会丢，重新启用即可继续。

**为什么高倍率下会连续升级？**
“溢出经验结转”会把超出当前等级的经验留到下一级。关闭该选项后，每次只能升一级，溢出部分会被丢弃。

**本地版和创意工坊版可以同时开吗？**
不建议。两者是同一个 mod，同时启用会重复挂补丁；只保留一个。

## 目录结构

```
FastLevelUp/
├── FastLevelUp.csproj      # 构建 + 自动部署目标
├── mod.yaml                # 模组标题 / 描述
├── mod_info.yaml           # 兼容性 / 最低构建号 / API 版本
├── translations/           # 选项面板文案（PO 格式）
│   ├── zh.po
│   └── en.po
└── src/
    ├── FastLevelUpMod.cs                     # 入口：PLib 选项 + 语言包（补丁由 base.OnLoad 挂载）
    ├── STRINGS.cs                            # 英文默认文案与翻译键
    ├── LevelUpOptions.cs                     # 选项、配置读取、需求表预计算
    └── Patches/
        ├── ExperienceRequirementPatch.cs     # 重写升级所需经验（查表）
        ├── MaxGainedLevelPatch.cs            # 同步等级上限
        └── SurplusExperiencePatch.cs         # 升级时结转溢出经验
```

## 从源码构建

要求：.NET SDK、缺氧本体（编译需要引用游戏程序集）。

```bash
dotnet build
```

编译后自动部署到：

```
Documents\Klei\OxygenNotIncluded\mods\local\FastLevelUp
```

游戏不在默认位置时指定路径：

```bash
dotnet build -p:OniDir="E:\SteamLibrary\steamapps\common\OxygenNotIncluded"
```

## 打包发布

维护者本地打包：

```powershell
.\tools\package.ps1 -Version 1.1.2
```

产出：

```
dist\FastLevelUp-v1.1.2.zip
```

内含 `FastLevelUp.dll`、`PLib.dll`、`mod.yaml`、`mod_info.yaml` 和 `translations\`。
解压到 `mods\local\FastLevelUp\` 即可使用。

> CI 无法自动构建：编译必须引用游戏本体的 `Assembly-CSharp.dll` 等程序集，这些文件不随仓库分发。

## 变更记录

- **v1.1.2**：修复 `base.OnLoad` 与手动 `PatchAll` 重复挂载补丁；等级上限改为双向同步，调低后不会继续偷偷累积经验。
- **v1.1.1**：选项说明文案重写得更精简准确；新增 `translations/` 多语言支持（中文 / 英文），面板文案跟随游戏语言。
- **v1.1.0**：加入可调选项面板（依赖 PLib）；经验预算模式、曲线陡峭度放宽到 0.6–2.5、溢出经验结转、需求表预计算；结构重命名为 FastLevelUp。
- **v1.0.0**：硬编码 50 级上限 + 150 周期曲线（旧存档不受影响：默认值与其完全一致）。

## 许可证

[MIT License](LICENSE)

## 致谢

- [PLib](https://github.com/peterhanel/PLib) 提供选项面板。
- 感谢缺氧社区对 mod API 的公开研究。
