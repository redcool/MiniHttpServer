using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities.Net
{
    /// <summary>
    /// Receive *.dll
    /// </summary>
    public class DLLReceiver
    {
        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            MiniHttpServerComponent.OnFileReceived -= OnReceived;
            MiniHttpServerComponent.OnFileReceived += OnReceived;
        }

        private static void OnReceived(string fileName, string fileType, string filePath, List<MiniHttpKeyValuePair> headers)
        {
            if (fileType.ToLower() != ".dll" || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return;
            }

            var asmBytes = File.ReadAllBytes(filePath);
            var asm = Assembly.Load(asmBytes);
            CallAllMain(asm);
        }

         static void CallAllMain(Assembly asm)
        {
            if (asm == null)
            {
                Debug.Log($"[DLLReceiver], dll not found,{asm.FullName}");
                return;
            }

            var defTypes = asm.DefinedTypes;
            foreach (var defType in defTypes)
            {
                //defType.InvokeMethod("Main", null, null, null);
                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
                defType.GetMethod("Main", flags)?.Invoke(null, null);
            }
        }
    }
}
