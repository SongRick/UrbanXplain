// --- START OF FILE: PauseMenuController.cs (修改后) ---
using UnityEngine;
using TMPro; // 如果你用的是TextMeshPro的InputField

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public TMP_InputField commandInputField; // 强烈建议使用TextMeshPro的InputField

    private bool isMenuVisible = false;

    void Start()
    {
        // 初始时菜单应该是关闭的
        pauseMenuPanel.SetActive(true);
        isMenuVisible = true;
        GlobalInputManager.SetState(InputState.UIOnly); // 确保初始状态正确
        Time.timeScale = 0f;
    }

    void Update()
    {
        // P键的检测现在不受InputManager状态的影响，因为它本身就是UI控制键
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 如果输入框正在输入，P键是打字，不触发菜单
            if (commandInputField != null && commandInputField.isFocused)
            {
                return;
            }

            ToggleControlsMenu();
        }
    }

    public void ToggleControlsMenu()
    {
        isMenuVisible = !isMenuVisible;
        pauseMenuPanel.SetActive(isMenuVisible);

        if (isMenuVisible)
        {
            // 打开菜单：切换到UIOnly模式，暂停时间
            GlobalInputManager.SetState(InputState.UIOnly);
            Time.timeScale = 0f;
        }
        else
        {
            // 关闭菜单：恢复Gameplay模式，恢复时间
            GlobalInputManager.SetState(InputState.Gameplay);
            Time.timeScale = 1f;
        }
    }

    // 提供给菜单上的"关闭"或"继续"按钮调用
    public void ResumeGame()
    {
        if (isMenuVisible)
        {
            ToggleControlsMenu();
        }
    }
}
// --- END OF FILE: PauseMenuController.cs (修改后) ---