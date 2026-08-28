<div align="center">

# SUI

### Small UI. Sharp Experience.

轻量、原生、可组合的 WPF UI 基础组件库

`WPF` · `.NET 10` · `XAML` · `MVVM-friendly`

[使用说明](doc/使用手册.md) · [许可证](LICENSE)

</div>

---

> 用小而锋利的组件，拼出大而顺滑的桌面体验。

## ✦ SUI 是什么

SUI 是一套为 Windows 桌面应用准备的 WPF UI 基础设施。它把常用控件、主题资源、样式字典和转换器收拢到一个简洁的项目中，让界面可以快速成型，也让视觉规范能够持续沉淀。

SUI 不接管你的业务。页面路由、数据状态、文件流程和应用结构仍然属于宿主项目；SUI 专注于把控件做好、把边界留清楚、把组合变简单。

## ◈ 组件矩阵

| 模块 | 组件 | 关键词 |
| --- | --- | --- |
| 内容表达 | `SUICard` | 图标 · 标题 · 描述 · 依赖属性 |
| 应用导航 | `SUINavigationView` / `SUINavigationItem` | 绑定 · 选中 · 路由解耦 · 自定义模板 |
| 视觉资源 | `SUIStyles.xaml` / `Styles/` / `Themes/` | DataGrid · GridSplitter · Switch |
| 数据转换 | `Converters/` | 可见性 · 颜色 · 多值比较 |
| 交互基础 | `FileUpload` / `TimePicker` | WPF 控件骨架 · 持续迭代 |

## ⟡ 设计语言

> **Native First**  
> 使用 WPF 原生控件、依赖属性、模板和资源字典，保持熟悉的开发体验。

> **Low Coupling**  
> 控件只关心自身的状态和表达，不绑定任何业务页面或产品流程。

> **Composable**  
> 控件、样式和主题各自独立，可以从一个小卡片逐步扩展成完整桌面界面。

> **Built to Evolve**  
> 先把基础体验做稳，再以清晰 API 持续扩展组件能力。

## 🚀 开始使用

SUI 支持通过项目引用接入现有 WPF 应用。具体的引用方式、命名空间、XAML 示例、属性表、样式资源和常见问题，统一放在：

<div align="center">

### [→ 阅读 SUI 使用说明](doc/使用手册.md)

</div>

如果你只想快速浏览组件能力，可以先看上面的组件矩阵；如果要开始接入，请直接进入使用说明。

## 🧭 文档导航

- [使用说明](doc/使用手册.md) —— 完整接入流程与控件 API。
- [许可证](LICENSE) —— SUI UI License 1.0 自定义授权条款。

## 🗂️ 项目结构

```text
SUI/
├─ Assets/              # 图片和其他资源
├─ Converters/          # WPF 值转换器
├─ Navigation/          # 侧边导航控件
├─ Styles/              # 可选样式资源
├─ Themes/              # 默认主题资源
├─ doc/                 # 使用说明
├─ SUICard.xaml         # 卡片控件
├─ FileUpload.xaml      # 文件上传控件基础结构
├─ TimePicker.xaml      # 时间选择控件基础结构
├─ SUIStyles.xaml       # 样式资源总入口
└─ SUI.csproj
```

## ⚙️ 工程信息

| 项目 | 配置 |
| --- | --- |
| 平台 | Windows |
| UI 框架 | WPF |
| Target Framework | `net10.0-windows7.0` |
| 构建工具 | .NET SDK / MSBuild `10.0.302` |

构建项目时使用固定版本的 MSBuild：

```powershell
& 'C:\Program Files\dotnet\sdk\10.0.302\MSBuild.exe' .\SUI.csproj /t:Build /p:Configuration=Debug
```

## 📍 项目状态

SUI 目前处于持续迭代阶段。`SUICard` 和 `SUINavigationView` 已可作为通用基础组件使用；`FileUpload` 和 `TimePicker` 当前仍是基础控件骨架，具体交互能力会继续完善。

## ⚖️ 许可证

SUI 使用 [SUI UI License 1.0](LICENSE)。个人、学习、研究和非商业使用可免费进行；商业项目可以使用、修改、集成和分发。当某个商业项目或产品累计取得人民币 10,000 元或以上的等值商业收入时，需要向作者支付一次性人民币 500 元授权费。完整条款以 `LICENSE` 为准。

<div align="center">

**SUI · Keep it small. Make it sharp.**

</div>
