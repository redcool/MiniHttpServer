using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities.Net
{
    /// <summary>
    /// Receive assetBundle,include shader,material,gameObjects
    /// </summary>
    public static class AssetBundleReceiver
    {
        static Dictionary<string, AssetBundle> bundleDict = new();

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            MiniHttpServerComponent.OnFileReceived -= OnReceived;
            MiniHttpServerComponent.OnFileReceived += OnReceived;
        }


        public static void OnReceived(string fileName, string fileType, string filePath, List<MiniHttpKeyValuePair> headers)
        {
            if (fileType == typeof(AssetBundle).Name)
            {
                RemoveExistedBundle(filePath);

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
                    Debug.Log($"[{nameof(AssetBundleReceiver)}] can't read from : {filePath}");
                    return;
                }

                bundleDict[filePath] = ab;

                HandleAssetBundle(ab);

                ab.Unload(false);


                //=====================
                static void HandleAssetBundle(AssetBundle ab)
                {
                    var renderers = Resources.FindObjectsOfTypeAll<Renderer>();

                    var shaderObjs = ab.LoadAllAssets<Shader>();
                    ReplaceShaderExisted(shaderObjs, renderers);

                    var matObjs = ab.LoadAllAssets<Material>();
                    ReplaceMaterialExisted(matObjs, renderers);

                    var gos = ab.LoadAllAssets<GameObject>();
                    InstantiateGameObjects(gos);

                }

                static void InstantiateGameObjects(GameObject[] gos)
                {
                    if (gos != null)
                    {
                        foreach (var go in gos)
                        {
                            if (go)
                                Object.Instantiate(go);
                        }
                    }
                }
            }

            static void SyncRead(string filePath)
            {
                var ab = AssetBundle.LoadFromFile(filePath);
                ReadAssetBundle(ab, filePath);
            }
        }
        /// <summary>
        /// error in unity editor, device is ok
        /// </summary>
        /// <param name="shaderObjs"></param>
        public static void ReplaceShaderExisted(Shader[] shaderObjs, Renderer[] renderers)
        {
            //var renderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            
            foreach (var shaderObj in shaderObjs)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];

                    if (!renderer || !renderer.sharedMaterial || !renderer.sharedMaterial.shader)
                        continue;

                    //Debug.Log(renderer.sharedMaterial.shader?.name + " -> " + shaderObj.name);
                    if (renderer.sharedMaterial.shader?.name == shaderObj.name)
                    {
                        renderer.sharedMaterial.shader = shaderObj;
                    }
                }
            }
        }


        private static void ReplaceMaterialExisted(Material[] matObjs, Renderer[] renderers)
        {
            foreach (var mat in matObjs)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];

                    if (!renderer || !renderer.sharedMaterial || !renderer.sharedMaterial.shader)
                        continue;

                    if (renderer.sharedMaterial.name == mat.name)
                    {
                        renderer.sharedMaterial = mat;
                    }
                }
            }
        }

        private static void RemoveExistedBundle(string filePath)
        {
            bundleDict.Remove(filePath, out var ab);
            if (ab)
                ab.Unload(true);
        }
    }
}