# Tab 布局 NuGet 回归

此程序是 .NET 10 STA/WPF 实际窗口回归，使用 `PackageReference`，不引用 SUI 源码项目。先构建和打包，再从本地源安装生成的包：

```powershell
dotnet build SUI.slnx -c Release -p:Version=0.0.7
dotnet pack SUI.csproj -c Release --no-build -p:Version=0.0.7 -o artifacts
dotnet restore samples/SUI.TabLayoutChecks --source ./artifacts
dotnet run --project samples/SUI.TabLayoutChecks --no-restore
```

可附加已升级同版 SUI NuGet、移除 TabItem Stretch 适配并构建好的 SerialLink 程序集路径：

```powershell
dotnet run --project samples/SUI.TabLayoutChecks --no-restore -- ../SerialLink/src/SerialLink.App/bin/Release/net10.0-windows/SerialLink.App.dll
```

结果输出至当前目录 `artifacts/tab-layout-results.json`，失败返回非零。包含实际选中内容宿主的最终对齐值、星号行与底部按钮几何关系、普通 Grid、文本、显式 Center、多页切换和窗口缩放。键盘事件断言用于检查 WPF 路由行为，不能替代 Gallery 中的真实输入检查。宿主模式额外断言所有业务 TabItem 没有本地内容对齐属性，并记录业务表格高度。

本地 nupkg 安装验证不代表已发布到 nuget.org。公开发布后，仍应在干净 NuGet 缓存中从 nuget.org 锁定同版包重跑。
