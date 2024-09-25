using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMainComp : MonoBehaviour
{

    private void OnGUI()
    {
        GUILayout.Box("[TestComp] Test done");
    }

    /// <summary>
    /// DLLReceiver will call 
    /// </summary>
    static void Main()
    {
        var go = new GameObject("TestMainComp GO");
        go.AddComponent<TestMainComp>();
    }
}
