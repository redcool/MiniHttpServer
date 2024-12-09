using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PowerUtilities.Net
{
    /// <summary>
    /// Receive shader assetBundle, no extName 
    /// </summary>
    public static class ShaderBundleReceiver
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            MiniHttpServerComponent.OnFileReceived -= OnReceived;
            MiniHttpServerComponent.OnFileReceived += OnReceived;
        }
        /// <summary>
        /// error in unity editor, device is ok
        /// </summary>
        /// <param name="shaderObjs"></param>
        public static void ReplaceShaderExisted(Shader[] shaderObjs)
        {
            //var renderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var renderers = Resources.FindObjectsOfTypeAll<Renderer>();
            foreach (var shaderObj in shaderObjs)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];
                    //Debug.Log(renderer.sharedMaterial.shader?.name + " -> " + shaderObj.name);
                    if (renderer.sharedMaterial.shader?.name == shaderObj.name)
                    {
                        renderer.sharedMaterial.shader = shaderObj;
                    }
                }
            }
        }

        public static void OnReceived(string fileName, string fileType, string filePath, List<MiniHttpKeyValuePair> headers)
        {
            if (fileType == typeof(AssetBundle).Name)
            {
                AsyncRead(filePath);
                //SyncRead(filePath);
            }

            static void AsyncRead(string filePath)
            {
                var req = AssetBundle.LoadFromFileAsync(filePath);
                req.completed += OnComplete;

                void OnComplete(AsyncOperation op)
                {
                    req.completed -= OnComplete;

                    var ab = req.assetBundle;
                    ReadAssetBundle(ab, filePath);
                };
            }

            static void ReadAssetBundle(AssetBundle ab,string filePath)
            {
                if (!ab)
                {
                    Debug.Log($"[{nameof(ShaderBundleReceiver)}] can't read from : {filePath}");
                    return;
                }
                var shaderObjs = ab.LoadAllAssets<Shader>();
                ReplaceShaderExisted(shaderObjs);
                ab.Unload(false);
            }

            static void SyncRead(string filePath)
            {
                var ab = AssetBundle.LoadFromFile(filePath);
                ReadAssetBundle(ab, filePath);
            }
        }
    }
}