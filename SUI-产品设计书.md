# SUI 产品设计与开发路线图（个人精简版）

**Small UI. Sharp Experience.**

> 界面好看，交互顺滑；保持简单、轻量、够用、容易维护。不为未来可能的需求增加今天的复杂度。

本文基于当前源码（`Styles/*`、`Themes/Generic.xaml`、`SUICard.*`、`Navigation/*`、`SUIStyles.xaml`、`SUI.csproj` 等）审查得出。性能相关判断未经测量，标注为「待测」。

---

## 1. SUI 是什么、不是什么

**是**：一个自用的轻量 WPF 样式库。把 WPF 默认控件整理成现代、简洁、统一的 Fluent 浅色界面，让界面耐看、日常交互顺滑。

SUI 的视觉质量主要来自排版、留白、色彩层次、控件状态和细节一致性。动画用于说明状态变化、衔接操作反馈；要求短、顺、自然，不以吸睛或炫技为目标。

- 只支持 .NET 10 WPF、Windows。
- 以 `Style` / `ControlTemplate` / `ResourceDictionary` 为主，零第三方依赖。
- 引入后，普通 `Button`、`TextBox` 等无需指定 Style 即自动获得 SUI 外观。
- 只保留少量自定义组件：NavigationView、ToggleSwitch、Card、Toast。

**不是**：

- 不是商业框架，不与 HandyControl、WPF UI 竞争，不追求控件数量。
- 不做深色主题、主题切换、ThemeManager、跟随系统、动态 Accent、多套密度。
- 不做 Mica / Acrylic、自定义窗口、Docking、Ribbon、日历、图表、Markdown、编辑器。
- 不含任何业务（树节点的 `OI`、文件上传流程等不进库）。
- 暂不发布 NuGet，不做多 TFM，不做插件或复杂分层。

---

## 2. 控件清单

### 2.1 原生控件美化（只写样式，不写新控件）

| 控件 | 说明 |
| :--- | :--- |
| Button | 默认样式 + 显式变体 `SUI.Button.Primary`（强调色）、`SUI.Button.Ghost`（无边框）、`SUI.Button.Danger` |
| TextBox / PasswordBox | 含只读、错误（`Validation.HasError`）状态 |
| ComboBox / ComboBoxItem | 模板较复杂，单独验证 |
| CheckBox / RadioButton | |
| TabControl / TabItem | |
| ListBox / ListBoxItem | 导航与普通列表共用 |
| DataGrid | 修复现有冲突后保留 |
| TreeView / TreeViewItem | 去除业务耦合 |
| ScrollBar / ScrollViewer | 影响所有滚动区，视觉统一的关键 |
| Menu / ContextMenu / MenuItem | |
| ToolTip | |
| ProgressBar | |
| GridSplitter | 补横向 |
| Expander、Slider | 仅在真正用到时再加 |

### 2.2 自定义组件（只保留这四个）

| 组件 | 现状 | 处理 |
| :--- | :--- | :--- |
| `SUINavigationView` | 已有，设计合理（模板可换、`SelectedItem` 双向、列表已虚拟化） | 保留，颜色改用公共资源 |
| ToggleSwitch | 现为 `SwitchCheckBoxStyle`（CheckBox 样式） | 继续用样式实现，不新建控件类；补焦点/禁用 |
| `SUICard` | 已有，`UserControl` | 保留，去硬编码，不扩展 |
| Toast | 暂无 | 需要时再做，最小实现（见 §6） |

### 2.3 不做（需要时再说）

NumberBox、SearchBox、LogView、Dialog、PropertyGrid、DatePicker/TimePicker 重做、Calendar、FileUpload。
`FileUpload`、`TimePicker` 现在只有占位文字（已核实），建议移出 README 组件表，或直接删除文件。

---

## 3. 视觉规范

只维护**一个颜色文件**和一组固定数值，不建 Token 体系。

### 3.1 颜色：`Themes/Colors.xaml`

用 Fluent 浅色，一个强调色，几个中性色，四个状态色。画刷直接命名，控件直接引用：

```text
SUI.Brush.Accent / AccentHover / AccentPressed    强调色（统一一种蓝）
SUI.Brush.Background        窗口背景
SUI.Brush.Surface           卡片、面板、输入框底色
SUI.Brush.Text / TextSecondary / TextDisabled
SUI.Brush.Border / BorderHover
SUI.Brush.Hover / Pressed   控件悬停、按下底色
SUI.Brush.Selected          选中底色
SUI.Brush.Focus             焦点环
SUI.Brush.Success / Warning / Danger / Info
```

不做原始色层、不做控件级 Token。具体色值在 Gallery 中调整后定。

引用方式：全部用 `StaticResource`（不切主题，无需 `DynamicResource`）。`Colors.xaml` 必须先于样式合并。画刷在字典里定义时加 `PresentationOptions:Freeze="True"`。

### 3.2 固定数值（写在 `Themes/Metrics.xaml`，或并入 Colors.xaml）

| 项 | 值 |
| :--- | :--- |
| 字体 | `Segoe UI Variable, Segoe UI, Microsoft YaHei UI` |
| 等宽字体 | `Cascadia Mono, Consolas` |
| 字号 | 正文 13，辅助 12，标题 16 |
| 圆角 | 控件 4，面板/弹出层 8 |
| 边框 | 1 |
| 控件高度 | 32（只有一档） |
| 间距 | 4 / 8 / 12 / 16 |

### 3.3 状态表现（所有控件一致）

- Hover：底色变为 `Hover`；Pressed：`Pressed`；Disabled：文字用 `TextDisabled`，不再各控件各设 `Opacity`。
- 选中与悬停分别表达，悬停不覆盖选中。
- 键盘焦点必须可见（用 `Focus` 色边框）。
- 阴影只用于 Popup/ToolTip/Toast；常驻控件和卡片用边框，不用 `DropShadowEffect`。
- 动画：用于有明确反馈价值的状态变化与轻量过渡，保持短、顺、自然；使用适度缓动，避免生硬启停，不做无意义循环、弹跳或炫技效果。常见控件状态过渡可从约 120–200ms 起步，再在 Gallery 中调整；动效不能拖慢操作，也不能影响滚动和高频刷新。

### 3.4 图标

沿用 `Segoe Fluent Icons`，字体链加上 `Segoe MDL2 Assets` 回退（Win10 无前者）。简单图标优先用 `Geometry`（树已采用）。

---

## 4. 代码结构

保持现有结构，只增不拆：

```text
SUI/
├─ Themes/
│  ├─ Colors.xaml          新增：唯一颜色/数值文件
│  └─ Generic.xaml         保留：仅自定义控件的默认样式
├─ Styles/                 每个控件一个文件
│  ├─ Button.xaml  TextBox.xaml  ComboBox.xaml  ...（新增）
│  ├─ DataGridStyles.xaml  TreeStyles.xaml  GridSplitterStyles.xaml  SwitchCheckBoxStyle.xaml（现有）
├─ Navigation/             保留
├─ Converters/             保留
├─ SUICard.xaml(.cs)       保留
└─ SUIStyles.xaml          唯一入口：合并 Colors + 所有样式
samples/SUI.Gallery/       新增：展示项目
```

### 4.1 开箱即用的接入方式

宿主 `App.xaml` 只需一行合并：

```xml
<ResourceDictionary Source="/SUI;component/SUIStyles.xaml" />
```

`SUIStyles.xaml` 内合并全部样式。

- **隐式样式**：每个原生控件写一个无 key 的 `Style TargetType="Button"`，自动生效。
- **显式变体**：用 `BasedOn` 派生并加 key，例如 `SUI.Button.Primary`。
- **旧 key 保留**：`CommonDataGridStyle`、`CustomGridSplitterStyle`、`SwitchCheckBoxStyle` 等继续可用；DataGrid 等同时提供隐式样式。因为是个人项目，旧 key 不需要兼容别名，可直接改名并同步修改自己的项目。
- 注意：隐式样式不会作用于派生类型；`ControlTemplate` 一旦自写，需自己覆盖所有状态。

### 4.2 命名与组织

- 显式样式 key：`SUI.{控件}.{变体}`；画刷：`SUI.Brush.*`。
- 自定义控件继续用 `SUI` 前缀（`SUINavigationView`）。
- 控件内不写颜色字面量；颜色只在 `Colors.xaml`。
- Switch、Splitter 这类原生行为够用的，只写 Style；只有 NavigationView 是真正的 CustomControl。

### 4.3 性能：只做低成本的规范

- 常驻控件不用 `DropShadowEffect`；`SUICard` 当前每个实例都带 `BlurRadius=16` 阴影，改为边框。
- 模板层级尽量浅；颜色画刷在字典内冻结。
- DataGrid / TreeView 在使用文档里注明：大数据量时开启虚拟化（`VirtualizingPanel.IsVirtualizing`、`EnableRowVirtualization`）。
- 1 万行 DataGrid、1 万节点树是否流畅：**待测**，在 Gallery 里手动试一次即可，不建基线体系。

---

## 5. Gallery 示例项目

一个简单 WPF 项目 `samples/SUI.Gallery`：

- 引用 `SUI.csproj`，`App.xaml` 合并 `SUIStyles.xaml`。
- 左侧用 `SUINavigationView`，每个控件一页，展示 Normal / Hover / Disabled / 错误等状态。
- 一页「综合示例」：导航 + 表单 + DataGrid + 状态栏，用来检查整体观感。
- 手动检查即可：键盘 Tab 焦点、中文输入、100% / 150% 缩放。
- 不做截图回归、不做自动化测试。

---

## 6. 现有代码：先改什么、后做什么

### 第 1 步：修复已发现的问题（不改结构）

| 问题 | 依据 | 做法 |
| :--- | :--- | :--- |
| DataGrid `CellStyle` 重复 | `CommonDataGridStyle` 中有两个 `CellStyle` Setter，后者覆盖前者，`CommonDataGridCellStyle` 实际不生效 | 只保留一个 `CellStyle` |
| DataGrid 选中不可见 | 行和单元格 `IsSelected` 触发器都设为 `Transparent`；悬停色与表头同为 `#F5F5F5` | 使用 `Selected` 选中色，悬停改为比表头更浅的色 |
| Switch 无焦点 | `FocusVisualStyle=Null`，无禁用/悬停状态 | 恢复焦点，补 Hover、Disabled；圆角改为全圆 |
| GridSplitter 只有竖向 | `Cursor=SizeWE`、宽 1 的 `Rectangle`；悬停时宽度变 3 引起布局 | 补横向样式；悬停只变色 |
| 导航图标字体 | `Generic.xaml` 写死 `Segoe Fluent Icons` | 加 MDL2 回退 |
| `SUICard` 硬编码 | 固定 `MaxWidth=300`、`White`、阴影、文字色 | 改引用 `Colors.xaml`，移除固定宽度与阴影 |

### 第 2 步：统一颜色

- 新建 `Themes/Colors.xaml`，统一一种强调色（目前至少有导航 `#1684F8`、树 `#3478D4`、开关 `#1976c6`、分隔条 `#1E90FF` 四种蓝）。
- 把 DataGrid、Switch、GridSplitter、Navigation、SUICard、Tree 的颜色字面量改为引用该文件。

### 第 3 步：TreeView 解耦

- `TreeStyles.cs` 现有约 20 个颜色附加属性、默认值写在代码里；节点模板每个节点有十余处 `RelativeSource` 绑定。
- 做法：颜色改为引用 `Colors.xaml`；只保留确有用的布局类属性（如图标尺寸、圆角），其余删除，个人项目无需过渡期。
- `SUITreeNodeTemplate` 中的 `Title`/`OI`/`Children` 约定移到 Gallery 示例里，库内只保留通用 `TreeViewItem` 样式。
- 补键盘焦点和禁用状态。

### 第 4 步：补基础控件样式（MVP 主体）

按使用频率依次：Button → TextBox → ScrollBar → ComboBox → CheckBox / RadioButton → TabControl → ListBox → Menu/ContextMenu/ToolTip → ProgressBar。
每做完一个，在 Gallery 加一页并合并到 `SUIStyles.xaml`，同时提供隐式样式。

### 第 5 步：Gallery

可在第 4 步开始前先建好空项目，之后每个样式随做随看。DataGrid 与树的虚拟化手动试一次。

### 第 6 步：按需

Toast（最小实现：一个静态 `Toast.Show(message, kind)` + 右下角自动消失的小面板，可用简短、平顺的出现/消失反馈；不做队列与动画框架）。NumberBox 等其他组件等真的在项目里需要时再说。

### 清理

- README 组件矩阵移除 `FileUpload`/`TimePicker`（或删除文件及未引用的 `Assets/upload.svg`）。
- README 与使用手册补充：一行接入、隐式样式、显式变体。

---

## 7. MVP 清单（够用即可）

1. `Colors.xaml` + 固定字体/圆角/间距。
2. 隐式样式：Button、TextBox、ComboBox、CheckBox、RadioButton、TabControl、ListBox、ScrollBar、Menu、ToolTip、ProgressBar。
3. 修好并沿用：DataGrid、TreeView、Switch、GridSplitter、NavigationView、SUICard。
4. Gallery 能展示以上所有控件。

**MVP 完成标准**：在自己的一个真实项目里，只合并 `SUIStyles.xaml`，不改控件 XAML，界面即呈现统一的 Fluent 浅色外观，键盘焦点可见。

---

## 8. 维护原则

1. **先用原生**：能 Style 就不写控件，能复制样式就不抽象。
2. **颜色只在一个文件**：新控件需要新颜色就加到 `Colors.xaml`。
3. **用到才做**：新控件在真实项目里需要了再加，不预先实现。
4. **改动小步可回退**：一次只改一个控件，在 Gallery 里看一遍。
5. **删除优于保留**：长期没用的东西直接删。
