# MiniHttpServer

[English](README.md) | [简体中文](README.zh-CN.md)

## 简介

MiniHttpServer 是一个可在 Unity（编辑器、真机/玩家端、Android）中运行的轻量级 HTTP 服务器与客户端，用于在运行时热更新资源包（AssetBundle）、C# DLL 与 C# 源码。服务器端基于 System.Net.HttpListener：通过 NetTools 把所有 IPv4 地址与 localhost/127.0.0.1 绑定到指定端口（默认 8000），每帧调用一次 TryAccept() 拉取连接并触发 OnReceived 事件；客户端基于 System.Net.Http 实现，通过 filename / filetype 等自定义请求头把文件字节 POST 给服务器。

文件接收流程：MiniHttpServerComponent 收到文件 POST 后（要求请求头中带 filename），把原始字节保存到 Application.temporaryCachePath/<resourceFolder>/<fileName>，回复 “[server] File Received : <path>”，随后触发静态事件 OnFileReceived(fileName, fileType, filePath, headers)。所有接收器（Receiver）订阅该事件，按文件类型分别处理资源包、DLL 与 C# 源码。

文档：https://ti4z0mosnbo.feishu.cn/wiki/HNFswBFhei2EAQkkswIcWa0bnjd

## 功能特性（Features）

- **MiniHttpServer**（Core/MiniHttpServer.cs）—— 对 System.Net.HttpListener 的轻量封装。
  - NetTools.AddHttpPrefixes 把每个 IPv4 地址 + localhost + 127.0.0.1 绑定到指定端口（默认 8000）。
  - StartListen() / StopListen() 控制监听启停；TryAccept()（每帧调用一次）通过 OnReceived 事件把每个完成的 HttpListenerContext 抛出来。
  - 可选的调试日志（isShowDebugInfo）：启动时打印 URL 前缀，每次接收时打印请求信息。
- **NetTools**（Core/NetTools.cs，PowerUtilities 命名空间）—— GetHostName()、GetIPv4s()、AddHttpPrefixes(listener, port)。
- **MiniHttpServerComponent**（Components/MiniHttpServerComponent.cs）—— 开箱即用的服务器 MonoBehaviour。
  - 默认只在 Development Build（开发构建）中启动（isDebugBuildOnly）；可选的 DontDestroyOnLoad；通过 OnGUI 在界面角落绘制 miniHttpIcon 图标。
  - 接收文件 POST：要求请求头带 “filename”，把原始字节保存到 Application.temporaryCachePath/<resourceFolder>/<fileName>，回复 “[server] File Received : <path>”，然后触发静态事件 OnFileReceived(fileName, fileType, filePath, headers)。
  - 在 Update() 中每帧调用 httpServer.TryAccept()。
- **MiniHttpClient**（Core/MiniHttpClient.cs，静态类）—— 共享的 HttpClient；PostFile(url, fileName, fileType, bytes, isShowDebugInfo, headersList) 以 filename / filetype / Content-Type: application/file 请求头加上额外的键值对上传字节数据；另有 multipart/form-data 的 PostMultipart。
- **MiniHttpClientComponent**（Components/MiniHttpClientComponent.cs）—— 由检视面板驱动的客户端：url、fileType（AssetBundle 或 ByExtensionName）、额外请求头（headerPairList）、bundleAbsPath；PostFile() 读取文件并上传。编辑器检视面板提供 “Test Post” 按钮。
- **MiniHttpFileType**（Core/MiniHttpFileType.cs）—— AssetBundle | ByExtensionName；**MiniHttpKeyValuePair** —— 可序列化的键值对请求头，带 IsValid()。
- **接收器（Receivers）**（订阅 MiniHttpServerComponent.OnFileReceived，通过 [RuntimeInitializeOnLoadMethod] 注册）：
  - **AssetBundleReceiver** —— 当 fileType == "AssetBundle"：先卸载映射到同一路径的旧资源包，再（异步）加载新资源包，然后用 ReplaceShaderExisted 把所有已加载 Renderer 上同名的着色器替换为包内着色器、用 ReplaceMaterialExisted 替换同名材质球，并实例化包内所有 GameObject。
  - **DLLReceiver** —— 当 fileType == ".dll"（iPhone 上跳过）：通过 Assembly.Load 从文件字节加载程序集，然后对程序集内所有已定义类型调用其 Main 方法（CallAllMain）。
  - **CSReceiver** —— 当文件类型包含 ".cs"：读取源码文本，创建一个以文件名命名的 GameObject，并通过 CSharpScript.CSriptComponent.Run（SlowSharp）执行。由 CSHARP_SCRIPT 宏开关控制（在 CSReceiver.cs 文件顶部定义）。

## 目录结构（Folder structure）

- Core/ —— 核心逻辑：MiniHttpServer.cs、MiniHttpClient.cs、NetTools.cs、MiniHttpFileType.cs、MiniHttpKeyValuePair.cs、Receivers/（AssetBundleReceiver.cs、DLLReceiver.cs、CSReceiver.cs）
- Components/ —— 组件：MiniHttpServerComponent.cs、MiniHttpClientComponent.cs
- Test/ —— 测试：TestServer.unity（测试场景）、MiniHttpServer.prefab（预设体）、miniHttp.png（图标）、TestDll/（TestDll.asmdef + 带静态 Main 的 TestMainComp.cs 示例）

## 使用说明（Usage）

1. 把 MiniHttpServer 拖到 Hierarchy（层级）窗口中，
2. 开始 Play（或部署到设备上运行），
3. 使用 MiniHttpClientComponent
   3.1 设置 File Type（文件类型：bundle、file）
   3.2 设置 bundleAbsPath，并点击 TestPost

接收器（Receiver）说明：
1. 着色器资源包（shader asset bundle）：会用包内同名着色器替换材质球上的着色器
2. C# DLL（Android）：遍历每个 C# 类型，若存在静态 Main 则调用它
3. C# 源码：使用 CSharpScript（SlowSharp）运行 C# 代码
   在 Unity 工程中定义 CSHARP_SCRIPT 宏，或修改 CSReceiver.cs

## 参考仓库 / 依赖（Reference Gits）

- CSharpScript（供 CSReceiver 执行 C# 源码使用）：https://github.com/redcool/CSharpScript.git
- 本包仓库：https://github.com/redcool/MiniHttpServer.git

## 备注 / 更新记录（Notes / Changelog）

- 可在编辑器 / 真机 / Android 中运行；服务器默认只在 Development Build（开发构建）中启动（isDebugBuildOnly）。
- 接收到的文件保存于 Application.temporaryCachePath/<resourceFolder>/ 目录下，OnFileReceived 会把文件的绝对路径传给接收器。
- 测试 DLL 示例：Test/TestDll/TestMainComp.cs —— 它的静态 Main 会创建一个挂有 TestMainComp 组件的 GameObject，该组件显示一个 GUILayout 提示框（[TestComp] Test done）。
