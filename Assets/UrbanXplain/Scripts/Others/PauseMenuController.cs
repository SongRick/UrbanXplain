using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel; // 在Inspector中指定你的暂停菜单Panel
    public InputField commandInputField; // 在Inspector中指定你的城市规划指令输入框

    private bool isPaused = false;
    private void Start()
    {
        TogglePauseMenu();
    }

    void Update()
    {
        // 检测P键按下
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 检查指令输入框是否被选中且正在输入
            if (commandInputField != null && commandInputField.isFocused)
            {
                // 如果输入框是聚焦状态，则P键是输入内容，不打开暂停菜单
                // InputField/TMP_InputField组件会自动处理字符'P'的输入
                // 所以这里我们什么都不做，或者可以加个Debug.Log("P pressed while input field focused");
                return;
            }

            // 如果输入框未聚焦，则切换暂停菜单的显示状态
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }

        // 暂停/恢复游戏时间 (根据你的游戏需求决定是否需要)
        if (isPaused)
        {
            Time.timeScale = 0f; // 游戏暂停
            // 可以考虑在这里解锁并显示鼠标光标，如果你的游玩模式锁定了光标
            // Cursor.lockState = CursorLockMode.None;
            // Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f; // 游戏恢复
            // 如果游玩模式锁定了光标，在这里恢复锁定状态
            // if (currentMode == PlayMode) { // 假设你有模式状态变量
            //    Cursor.lockState = CursorLockMode.Locked;
            //    Cursor.visible = false;
            // }
        }
    }

    // (可选) 如果暂停菜单上有"继续游戏"按钮，可以调用这个
    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePauseMenu();
        }
    }
}