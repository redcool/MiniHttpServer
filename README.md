# MiniHttpServer

[English](README.md) | [简体中文](README.zh-CN.md)


A lightweight HTTP server & client that runs inside Unity (editor, player, Android), used to hot-load asset bundles, C# DLLs and C# source at runtime.

Docs: https://ti4z0mosnbo.feishu.cn/wiki/HNFswBFhei2EAQkkswIcWa0bnjd

## Features

- **MiniHttpServer** (Core/MiniHttpServer.cs) — thin wrapper over System.Net.HttpListener.
  - NetTools.AddHttpPrefixes binds every IPv4 address + localhost + 127.0.0.1 on the given port (default 8000).
  - StartListen() / StopListen(); TryAccept() (call once per frame) surfaces each completed HttpListenerContext via the OnReceived event.
  - Optional debug logging (isShowDebugInfo): logs URL prefixes on start and request info per accept.
- **NetTools** (Core/NetTools.cs, PowerUtilities namespace) — GetHostName(), GetIPv4s(), AddHttpPrefixes(listener, port).
- **MiniHttpServerComponent** (Components/MiniHttpServerComponent.cs) — drop-in server MonoBehaviour.
  - Only starts in Development Builds by default (isDebugBuildOnly); DontDestroyOnLoad optional; draws miniHttpIcon in the GUI corner via OnGUI.
  - Receives file POSTs: requires a "filename" header, saves raw bytes to Application.temporaryCachePath/<resourceFolder>/<fileName>, replies "[server] File Received : <path>", then fires the static event OnFileReceived(fileName, fileType, filePath, headers).
  - httpServer.TryAccept() is pumped from Update() every frame.
- **MiniHttpClient** (Core/MiniHttpClient.cs, static) — shared HttpClient; PostFile(url, fileName, fileType, bytes, isShowDebugInfo, headersList) uploads bytes with filename / filetype / Content-Type: application/file headers plus extra key/value pairs; also a multipart/form-data PostMultipart.
- **MiniHttpClientComponent** (Components/MiniHttpClientComponent.cs) — inspector-driven client: url, fileType (AssetBundle or ByExtensionName), extra headers (headerPairList), bundleAbsPath; PostFile() reads the file and posts it. Editor inspector adds a "Test Post" button.
- **MiniHttpFileType** (Core/MiniHttpFileType.cs) — AssetBundle | ByExtensionName; **MiniHttpKeyValuePair** — serializable key/value header pair with IsValid().
- **Receivers** (subscribe to MiniHttpServerComponent.OnFileReceived, registered via [RuntimeInitializeOnLoadMethod]):
  - **AssetBundleReceiver** — for fileType == "AssetBundle": removes any earlier bundle mapped to the same path, loads the bundle (async), then remaps same-name shaders on all loaded Renderers (ReplaceShaderExisted), remaps same-name materials (ReplaceMaterialExisted), and instantiates all GameObjects in the bundle.
  - **DLLReceiver** — for fileType == ".dll" (skipped on iPhone): Assembly.Load from the file bytes and invokes every Main method found on all defined types (CallAllMain).
  - **CSReceiver** — for file types containing ".cs": reads the source text, creates a GameObject named after the file, and runs it via CSharpScript.CSriptComponent.Run (SlowSharp). Gated by the CSHARP_SCRIPT define (defined at the top of CSReceiver.cs).

## Folder structure

- Core/ — MiniHttpServer.cs, MiniHttpClient.cs, NetTools.cs, MiniHttpFileType.cs, MiniHttpKeyValuePair.cs, Receivers/ (AssetBundleReceiver.cs, DLLReceiver.cs, CSReceiver.cs)
- Components/ — MiniHttpServerComponent.cs, MiniHttpClientComponent.cs
- Test/ — TestServer.unity (test scene), MiniHttpServer.prefab, miniHttp.png (icon), TestDll/ (TestDll.asmdef + TestMainComp.cs example with a static Main)

## Usage

1. Drag MiniHttpServer to Hierarchy,
2. start Play (or deploy to device),
3. use MiniHttpClientComponent
   3.1 set File Type (bundle, file)
   3.2 set bundleAbsPath, and click TestPost

Receiver:
1. shader asset bundle, will change material's same name shader
2. c# dll (android), iterate every cs type, invoke static Main if exists
3. c# src, use CSharpScript (SlowSharp), run cs code
   unity prj define CSHARP_SCRIPT or change CSReceiver.cs

## Reference Gits / Dependencies

- CSharpScript (used by CSReceiver for C# source execution): https://github.com/redcool/CSharpScript.git
- Package git: https://github.com/redcool/MiniHttpServer.git

## Notes

- Runs in editor / player / Android; server only starts in Development Builds by default (isDebugBuildOnly).
- Received files are saved under Application.temporaryCachePath/<resourceFolder>/, and OnFileReceived passes the absolute file path to receivers.
- Test DLL example: Test/TestDll/TestMainComp.cs — its static Main creates a GameObject with TestMainComp, which shows a GUILayout box ([TestComp] Test done).