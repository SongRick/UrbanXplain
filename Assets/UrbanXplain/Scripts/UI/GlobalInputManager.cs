// --- START OF FILE: InputManager.cs (完整更新版) ---
using UnityEngine;

/// <summary>
/// 定义了全局的输入状态，用于区分游戏逻辑输入和UI交互输入。
/// </summary>
public enum InputState
{
    Gameplay, // 正常游戏模式，所有游戏性输入都有效。
    UIOnly    // UI模式，仅允许UI交互，所有游戏性输入都被禁用。
}

/// <summary>
/// 一个静态的输入管理器，作为所有游戏性输入的唯一来源。
/// 它根据当前的InputState来决定是否响应输入请求。
/// </summary>
public static class GlobalInputManager
{
    private static InputState currentState = InputState.Gameplay;

    /// <summary>
    /// 获取当前的输入状态。
    /// </summary>
    public static InputState CurrentState => currentState;

    /// <summary>
    /// 设置当前的输入状态。
    /// </summary>
    /// <param name="newState">要设置的新状态。</param>
    public static void SetState(InputState newState)
    {
        // 可以在这里加一个判断，避免不必要的重复设置
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"InputManager state set to: {newState}");
    }

    // --- 按键检测 ---

    /// <summary>
    /// 检查一个特定的游戏性按键是否在本帧被按下。
    /// 仅在Gameplay状态下返回true。
    /// </summary>
    public static bool GetGameKeyDown(KeyCode key)
    {
        if (currentState != InputState.Gameplay) return false;
        return Input.GetKeyDown(key);
    }

    /// <summary>
    /// [新增] 检查一个特定的游戏性按键是否在本帧被抬起。
    /// 仅在Gameplay状态下返回true。
    /// </summary>
    public static bool GetGameKeyUp(KeyCode key)
    {
        if (currentState != InputState.Gameplay) return false;
        return Input.GetKeyUp(key);
    }

    /// <summary>
    /// [新增] 检查一个特定的游戏性按键是否被持续按住。
    /// 仅在Gameplay状态下返回true。
    /// </summary>
    public static bool GetGameKey(KeyCode key)
    {
        if (currentState != InputState.Gameplay) return false;
        return Input.GetKey(key);
    }


    // --- 鼠标检测 ---

    /// <summary>
    /// [已完善] 检查一个特定的游戏性鼠标按键是否在本帧被按下。
    /// (0=左键, 1=右键, 2=中键)
    /// 仅在Gameplay状态下返回true。
    /// </summary>
    public static bool GetGameMouseButtonDown(int button)
    {
        if (currentState != InputState.Gameplay) return false;
        return Input.GetMouseButtonDown(button);
    }

    /// <summary>
    /// [新增] 检查一个特定的游戏性鼠标按键是否在本帧被抬起。
    /// (0=左键, 1=右键, 2=中键)
    /// 仅在Gameplay状态下返回true。
    /// </summary>
    public static bool GetGameMouseButtonUp(int button)
    {
        if (currentState != InputState.Gameplay) return false;
        return Input.GetMouseButtonUp(button);
    }

    /// <summary>
    /// [新增] 检查一个特定的游戏性鼠标按键是否被持续按住。
    /// (0=左键, 1=右键, 2=中键)
    /// 仅在Gameplay状态下返回true。
    /// </summary>
    public static bool GetGameMouseButton(int button)
    {
        if (currentState != InputState.Gameplay) return false;
        return Input.GetMouseButton(button);
    }


    // --- 轴向输入 ---

    /// <summary>
    /// 检查一个游戏性轴向输入（如"Horizontal"或"Vertical"）。
    /// 仅在Gameplay状态下返回非零值。
    /// </summary>
    public static float GetGameAxis(string axisName)
    {
        if (currentState != InputState.Gameplay) return 0f;
        return Input.GetAxis(axisName);
    }

    /// <summary>
    /// 检查一个原始的游戏性轴向输入（如"Mouse X"或"Mouse Y"），不受平滑处理影响。
    /// 仅在Gameplay状态下返回非零值。
    /// </summary>
    public static float GetGameAxisRaw(string axisName)
    {
        if (currentState != InputState.Gameplay) return 0f;
        return Input.GetAxisRaw(axisName);
    }
}
// --- END OF FILE: InputManager.cs (完整更新版) ---