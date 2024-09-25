
//use CSHARP_SCRIPT open this
#define CSHARP_SCRIPT 

#if CSHARP_SCRIPT
using CSharpScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PowerUtilities.Net
{
    /// <summary>
    /// Receive *.cs
    /// </summary>
    public class CSReceiver
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            MiniHttpServerComponent.OnFileReceived -= OnReceived;
            MiniHttpServerComponent.OnFileReceived += OnReceived;
        }

        public static void OnReceived(string fileName, string fileType, string filePath, List<MiniHttpKeyValuePair> headers)
        {
            if(fileType.ToLower().Contains(".cs"))
            {
                var codeStr = File.ReadAllText(filePath);

                var go = new GameObject(fileName);
                CScriptComponent.Run(go, codeStr);
            }
        }
    }
}
#endif