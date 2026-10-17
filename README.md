<div align="center">

# SUI

### Small UI. Sharp Experience.

轻量、原生、可组合的 WPF UI 基础组件库

`WPF` · `.NET 10` · `XAML` · `MVVM-friendly`

[GitHub 项目](https://github.com/RobertCXC/SUI) · [在线网页说明书](https://robertcxc.github.io/SUI/) · [NuGet 包](https://www.nuget.org/packages/SUI)

[Markdown 手册](https://github.com/RobertCXC/SUI/blob/main/doc/使用手册.md) · [许可证](https://github.com/RobertCXC/SUI/blob/main/LICENSE)

</div>

---

> 用小而可靠的组件，做出好看、顺手、动效流畅的桌面体验。

## ✦ SUI 是什么

SUI 是一套为 Windows 桌面应用准备的 WPF UI 基础设施。它把常用控件、主题资源、样式字典和转换器收拢到一个简洁的项目中，让界面可以快速成型，也让视觉规范能够持续沉淀。

SUI 不接管你的业务。页面路由、数据状态、文件流程和应用结构仍然属于宿主项目；SUI 专注于把控件做好、把边界留清楚、把组合变简单。

SUI 的体验重点是**界面好看、动画流畅**。通过排版、留白、颜色层次和控件细节形成清爽耐看的界面；动效用来辅助状态反馈和操作衔接，保持自然、连贯、克制，不追求炫技效果。

## ◈ 组件矩阵

| 模块 | 组件 | 关键词 |
| --- | --- | --- |
| 内容表达 | `SUICard` | 图标 · 标题 · 描述 · 依赖属性 |
| 应用导航 | `SUINavigationView` / `SUINavigationItem` | 绑定 · 选中 · 路由解耦 · 自定义模板 |
| 视觉资源 | `SUIStyles.xaml` / `Styles/` / `Themes/` | 主题令牌 · Button · TextBox · ComboBox · CheckBox · Tab · ListBox · Menu · ToolTip · ProgressBar · DataGrid · GridSplitter · Switch · TreeView |
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

> **Polished, not flashy:** 美感来自统一的视觉细节，动效服务于理解与反馈；不添加只为吸睛的装饰。

## 🚀 开始使用

推荐通过 [NuGet 包 SUI](https://www.nuget.org/packages/SUI) 接入现有 WPF 应用。需要修改库源码或联调时，也可以使用本地项目引用。

SUI 面向 Windows 上的 WPF 应用，项目目标框架为 `net10.0-windows7.0`。当前仓库示例通过 `ProjectReference` 引用 SUI。

### 1. 通过 NuGet 安装

在你的 WPF 项目 `.csproj` 所在目录执行，安装最新稳定版：

```powershell
dotnet add package SUI
```

需要固定版本时，可指定已发布版本，例如：

```powershell
dotnet add package SUI --version 0.0.3
```

NuGet 会通过 `PackageReference` 管理包内的 `SUI.dll` 引用。安装后继续合并样式、添加 XAML 命名空间即可。

源码开发时，可改用本地项目引用，并按实际目录调整相对路径：

```xml
<ItemGroup>
    <ProjectReference Include="..\SUI\SUI.csproj" />
</ItemGroup>
```

同一宿主项目选择 NuGet 包引用或本地项目引用中的一种。完整 API 说明按仓库当前源码编写；使用 NuGet 时请核对安装版本是否包含所用功能。

### 2. 在应用中加载样式

将 SUI 样式字典合并到 `App.xaml`。合并后，常用 WPF 原生控件会自动使用 SUI 样式：

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/SUI;component/SUIStyles.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 3. 在 XAML 中使用控件

```xml
<Window
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:sui="clr-namespace:SUI;assembly=SUI">
    <StackPanel Margin="24">
        <Button Content="保存" Style="{StaticResource SUI.Button.Primary}" />
        <sui:SUICard
            Margin="0,16,0,0"
            Title="项目状态"
            Description="SUI 控件可以和 WPF 原生控件一起组合使用。"
            IconText="S" />
    </StackPanel>
</Window>
```

想先查看完整控件效果，可以在仓库根目录运行示例程序：

```powershell
dotnet run --project .\samples\SUI.Gallery\SUI.Gallery.csproj
```

<div align="center">

### [→ 在线阅读完整使用手册](https://robertcxc.github.io/SUI/)

</div>

手册包含命名空间、控件属性、导航接入、主题资源和常见问题。Markdown 正文位于 [GitHub 上的 `doc/使用手册.md`](https://github.com/RobertCXC/SUI/blob/main/doc/使用手册.md)，在线网页由默认分支上的 GitHub Actions 自动部署。

网页说明书支持章节目录、本地全文搜索、代码复制和文档明暗主题，无需安装前端依赖。克隆仓库后也可以直接打开 `docs/index.html` 离线阅读；该文件由 Markdown 手册生成，请勿手工编辑。

修改手册后，在仓库根目录用 PowerShell 7 重新生成并检查网页：

```powershell
pwsh -File scripts/Build-Docs.ps1
pwsh -File scripts/Build-Docs.ps1 -Check
```

## 🧭 文档导航

- [在线网页说明书](https://robertcxc.github.io/SUI/) —— GitHub Pages 上的在线版本；克隆后也可打开 `docs/index.html` 离线阅读。
- [使用说明](https://github.com/RobertCXC/SUI/blob/main/doc/使用手册.md) —— 完整接入流程与控件 API。
- [网页说明书维护规范](https://github.com/RobertCXC/SUI/blob/main/doc/网页说明书维护规范.md) —— 内容来源、生成流程与验收规则。
- [产品设计书](https://github.com/RobertCXC/SUI/blob/main/SUI-产品设计书.md) —— SUI 的核心体验方向与设计约定。
- [产品定位与控件路线图](https://github.com/RobertCXC/SUI/blob/main/SUI-产品定位与精简控件路线图.md) —— 面向场景的产品范围、视觉系统与实施阶段。
- [样式开发任务单](https://github.com/RobertCXC/SUI/blob/main/doc/样式开发任务单.md) —— 控件样式实现与验收约定。
- [许可证](https://github.com/RobertCXC/SUI/blob/main/LICENSE) —— SUI UI License 1.0 自定义授权条款。

## 🗂️ 项目结构

```text
SUI/
├─ .github/workflows/   # 文档站部署与版本发布流程
├─ Assets/              # 图片和其他资源
├─ Converters/          # WPF 值转换器
├─ Navigation/          # 导航控件及相关代码
├─ Styles/              # WPF 原生控件样式资源
├─ Themes/              # 颜色令牌与控件默认主题资源
├─ doc/                 # Markdown 使用手册与开发规范
├─ docs/                # 生成的网页说明书及站点资源
├─ samples/
│  └─ SUI.Gallery/      # 可运行的控件 Gallery
│     ├─ Pages/         # Gallery 分类页面
│     └─ Examples/      # 页面使用的交互示例
├─ scripts/             # 文档生成脚本与网页模板
├─ SUICard.xaml         # 卡片控件
├─ FileUpload.xaml      # 文件上传控件骨架
├─ TimePicker.xaml      # 时间选择控件骨架
├─ SUIStyles.xaml       # 样式资源总入口
├─ SUI.csproj           # WPF 控件库项目
└─ SUI.slnx             # 控件库与 Gallery 解决方案
```

## 📦 发布与交付

- **源码**：在 GitHub 仓库中持续维护。
- **NuGet**：推送 `v*.*.*` 版本标签后，Release 工作流构建并发布 SUI 包。
- **Gallery**：同一版本流程会编译 Gallery，并将 ZIP 附加到 GitHub Release。
- **网页说明书**：文档相关改动合并到默认分支并通过检查后，自动部署到 [GitHub Pages](https://robertcxc.github.io/SUI/)。

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

SUI 使用 [SUI UI License 1.0](https://github.com/RobertCXC/SUI/blob/main/LICENSE)。个人、学习、研究和非商业使用可免费进行；商业项目可以使用、修改、集成和分发。当某个商业项目或产品累计取得人民币 10,000 元或以上的等值商业收入时，需要向作者支付一次性人民币 500 元授权费。完整条款以 `LICENSE` 为准。

<div align="center">

**SUI · Keep it small. Make it sharp.**

</div>
