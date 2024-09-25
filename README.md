# MiniHttpServer
HttpServer and Client, run in unity ,unity android ,...

Usage:
  1 Drag MiniHttpServer to Hierarchy,
  2 start Play(or deploy to device)
  3 use MiniHttpClientComponent 
    3.1 set File Type(bundle, file)
    3.2 set bundleAbsPath, and click TestPost

Receiver:
  1 shader asset bundle, will change material's same name shader 
  2 c# dll(android), iterate every cs type,invoke static Main if exists
  3 c# src, use CSharpScript(SlowSharp) ,run cs code
	unity prj define CSHARP_SCRIPT or change CSReceiver.cs