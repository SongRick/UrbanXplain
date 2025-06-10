using System.Runtime.InteropServices;
using UnityEngine;

public class WebGLInputController : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    // 从 WebGLInput.jslib 导入 WebGLInputInit 函数
    [DllImport("__Internal")]
    private static extern void WebGLInputInit();
#endif

    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // 在游戏开始时调用JavaScript初始化函数
        WebGLInputInit();
        DontDestroyOnLoad(gameObject); // 确保这个对象在场景切换时不会被销毁
#endif
    }
}