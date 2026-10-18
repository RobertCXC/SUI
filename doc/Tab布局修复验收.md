# Tab 默认内容对齐修复验收

日期：2026-10-18。范围仅为 Tab 布局，不涉及主题能力。

## 根因与兼容性

WPF `TabControl.UpdateSelectedContent()` 在模板应用、容器生成及选中页更新时，把选中 TabItem 的 `HorizontalContentAlignment`、`VerticalContentAlignment` 直接赋给 `PART_SelectedContentHost.HorizontalAlignment`、`VerticalAlignment`。

源码：https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Controls/TabControl.cs

因此原先 TabItem 默认 Center 同时让业务页宿主居中；内部 Grid、DataGrid 为 Stretch 无法弥补。修复将 TabItem 的两个默认值改为 Stretch，标题 ContentPresenter 独立 Center，未增加或移除公开属性、资源键、控件类型。用户显式页面对齐继续有效。运行时改变已选中项后的即时刷新未扩展，未将该观察认定为 SUI 的另一缺陷。

## 包与自动验证

- 生成 `artifacts/SUI.0.0.7.nupkg` 及符号包，包含 net48、net8.0-windows、net10.0-windows。
- nupkg SHA256：`3F735DA78027E37118DBF12FF07CDE8052A97E7D4758E603BD105C21D547BE59`。
- `dotnet build SUI.slnx -c Release -p:Version=0.0.7`：0 警告、0 错误。
- 说明书已同步，`Build-Docs.ps1` 生成及 `-Check` 检查通过。网页交互代码没有变化。
- `samples/SUI.TabLayoutChecks` 以 PackageReference 安装本地 nupkg；在真实 STA/WPF 窗口执行，连同 SerialLink 宿主共 53 条断言通过。运行方法见该目录 README。
- 最小复现无内部固定高度、无显式 Stretch：900×760、650×500、1100×900 窗口下，表格约 613.67、353.67、753.67 DIP；最终宿主两个方向均 Stretch，底部按钮位置与星号行分配正确。
- 普通 Grid 填满，简单字符串内容宿主 Stretch；显式 Center 页面仍居中；切换后返回表格、缩放后重新选择均正确。
- 标题独立居中、焦点边框、选中指示线、Home/End 跳过禁用页断言通过。

## SerialLink NuGet 回归

SerialLink `PackageReference` 与锁文件已升级 0.0.7，三个业务 TabItem 的显式 Stretch 适配已移除。未使用 ProjectReference。测试加载真实 `SerialLink.App.MainWindow`，并断言业务 TabItem 没有本地内容对齐值。

1500×900 时内容宿主约 663.67 DIP、表格 534.38 DIP；1150×700 时内容宿主约 463.67 DIP、表格 334.38 DIP；放大恢复 534.38 DIP。三页多次切换均保持 Stretch。

SerialLink 既有 `Validate.ps1` 回归另通过 26 条断言，结果在 SerialLink 的 `artifacts/sui-0.0.7-regression`。旧验证程序中人为设置 Center 的基线及重新应用 Stretch 的逻辑已移除，改为检查默认填充。

本轮验证对象是本地打包的新 NuGet，不是 nuget.org 已发布版本；尚未发布 Git 标签或上传 nuget.org。公开发布后需从 nuget.org 干净还原同版包再回归，不能将本地包验收称为公开发布验收完成。

## Gallery 实际窗口检查

运行 `samples/SUI.Gallery` 的 net10.0-windows Release。容器与反馈页增加实际 WPF 的星号行表格与底部操作栏，另含普通 Grid、字符串、显式 Center、图标标题和禁用标签。展示页位于 ScrollViewer 内，因此仅为示例容器提供有限高度；业务页面及表格没有固定高度规避。

实际窗口确认两行数据的表格仍填满内容区，操作按钮位于底部；普通 Grid 的左上与右下标记正确；方向键切换文本与 Center 页，焦点边框可见；鼠标停留在未选中标签时显示悬停底色；点击禁用标签不切换。未改动原有状态触发器与 WPF 键盘处理逻辑。
