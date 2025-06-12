// --- START OF FILE: PresetUIManager.cs (MODIFIED) ---
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UrbanXplain; // 确保这个命名空间存在
using UnityEngine.UI;

public class PresetUIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("拖拽场景中的PresetManager对象到这里")]
    public PresetManager presetManager;
    // --- NEW: Add direct reference to BuildingSpawnerJson ---
    [Tooltip("拖拽场景中包含BuildingSpawnerJson脚本的对象到这里")]
    public BuildingSpawnerJson buildingSpawner; // 为了直接调用清空方法

    [Header("UI Configuration")]
    [Tooltip("用于动态放置按钮的父容器 (带Vertical Layout Group)")]
    [SerializeField] private GameObject presetButtonContainer;
    [Tooltip("按钮的预制件")]
    [SerializeField] private GameObject presetButtonPrefab;
    // --- NEW: Optional prefab for the clear button ---
    [Tooltip("（可选）为清空按钮指定一个不同的预制件，如果留空则使用与预设按钮相同的预制件")]
    [SerializeField] private GameObject clearButtonPrefab;


    [Header("Preview Panel")]
    [Tooltip("指令预览面板的根对象")]
    [SerializeField] private GameObject instructionPreviewPanel;
    [Tooltip("预览面板中用于显示指令的文本组件")]
    [SerializeField] private TextMeshProUGUI textPreviewContent;
    [Tooltip("预览面板与按钮的水平间距")]
    [SerializeField] private float panelOffsetX = 20f;
    [Tooltip("鼠标离开后，面板延迟消失的时间")]
    [SerializeField] private float hideDelay = 0.1f;

    private List<PresetButton> allButtons = new List<PresetButton>();
    private PresetButton currentActiveButton;
    private Coroutine hidePreviewCoroutine;

    void Start()
    {
        // 初始化预览面板状态
        if (instructionPreviewPanel != null)
        {
            instructionPreviewPanel.GetComponent<CanvasGroup>().alpha = 0;
            instructionPreviewPanel.SetActive(false);
        }
        // --- NEW: Check for BuildingSpawnerJson reference ---
        if (buildingSpawner == null)
        {
            Debug.LogError("PresetUIManager: BuildingSpawnerJson reference is not set! The clear button will not work.", this);
        }
    }

    // 由PresetManager调用，动态创建一个按钮
    public void CreateButtonForPreset(int id, string name, string instruction)
    {
        if (presetButtonPrefab == null || presetButtonContainer == null)
        {
            Debug.LogError("PresetUIManager: 按钮预制件或容器未设置！", this);
            return;
        }

        GameObject buttonGO = Instantiate(presetButtonPrefab, presetButtonContainer.transform);
        PresetButton button = buttonGO.GetComponent<PresetButton>();

        if (button != null)
        {
            button.presetID = id;
            button.instructionText = instruction;
            button.SetLabel(name);
            button.Initialize(this);
            allButtons.Add(button);
        }
    }

    // --- NEW METHOD: Add the special "Clear Layout" button ---
    public void AddClearButton()
    {
        // Use the specific clear button prefab if it's assigned, otherwise fallback to the standard preset button prefab
        GameObject prefabToUse = clearButtonPrefab != null ? clearButtonPrefab : presetButtonPrefab;

        if (prefabToUse == null || presetButtonContainer == null)
        {
            Debug.LogError("PresetUIManager: Cannot create Clear Button because no valid prefab is assigned.", this);
            return;
        }

        GameObject buttonGO = Instantiate(prefabToUse, presetButtonContainer.transform);
        // We get the PresetButton component to use its visual style, but we'll override its click behavior.
        PresetButton buttonComponent = buttonGO.GetComponent<PresetButton>();

        if (buttonComponent != null)
        {
            buttonComponent.presetID = -1; // Use a special ID to signify it's not a standard preset
            buttonComponent.instructionText = "Remove all buildings and clear the current layout from the map."; // Tooltip for the clear button
            buttonComponent.SetLabel("Clear Layout");
            buttonComponent.Initialize(this); // Initialize its visual state
            // We don't add it to the 'allButtons' list if we want to handle it separately
        }

        // --- Add a listener to its Button component to call our specific clear method ---
        // This is a more robust way to handle the click than relying on the ID in OnButtonClick
        var unityButton = buttonGO.GetComponent<Button>();
        if (unityButton == null)
        {
            // If the prefab doesn't have a Unity Button, we add one
            // This assumes the PresetButton script is designed to work with a Unity Button
            unityButton = buttonGO.AddComponent<Button>();
        }
        unityButton.onClick.RemoveAllListeners(); // Clear any existing listeners from the prefab
        unityButton.onClick.AddListener(OnClearButtonClick);
    }


    // 清空所有已创建的按钮
    public void ClearButtons()
    {
        // This should now only clear preset buttons, not the programmatically added clear button
        // if it's not in the `allButtons` list.
        foreach (Transform child in presetButtonContainer.transform)
        {
            Destroy(child.gameObject);
        }
        allButtons.Clear();
        currentActiveButton = null;
    }

    // --- 事件处理 ---

    public void OnButtonEnter(PresetButton button)
    {
        // ... (this method remains unchanged)
        if (hidePreviewCoroutine != null)
        {
            StopCoroutine(hidePreviewCoroutine);
            hidePreviewCoroutine = null;
        }

        var canvasGroup = instructionPreviewPanel.GetComponent<CanvasGroup>();
        canvasGroup.DOKill();

        textPreviewContent.text = button.instructionText;

        var buttonRect = button.GetComponent<RectTransform>();
        var panelRect = instructionPreviewPanel.GetComponent<RectTransform>();
        instructionPreviewPanel.transform.position = button.transform.position;
        panelRect.anchoredPosition += new Vector2((panelRect.rect.width + buttonRect.rect.width) / 2f + panelOffsetX, -50f);

        instructionPreviewPanel.SetActive(true);
        canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutCubic);
    }

    public void OnButtonExit(PresetButton button)
    {
        // ... (this method remains unchanged)
        hidePreviewCoroutine = StartCoroutine(HidePreviewPanelRoutine());
    }

    private IEnumerator HidePreviewPanelRoutine()
    {
        // ... (this method remains unchanged)
        yield return new WaitForSeconds(hideDelay);
        var canvasGroup = instructionPreviewPanel.GetComponent<CanvasGroup>();
        canvasGroup.DOKill();
        canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
        {
            if (instructionPreviewPanel != null) instructionPreviewPanel.SetActive(false);
        });
        hidePreviewCoroutine = null;
    }

    public void OnButtonClick(PresetButton clickedButton)
    {
        // This method now only handles preset buttons. The clear button has its own listener.
        if (clickedButton.presetID == -1) // Double safety check, though direct listener is better
        {
            OnClearButtonClick();
            return;
        }

        if (currentActiveButton == clickedButton) return;

        // When a new preset is selected, deselect the "Clear Layout" button visually if it was active
        if (currentActiveButton != null)
        {
            currentActiveButton.SetState(ButtonState.Normal);
        }

        currentActiveButton = clickedButton;
        currentActiveButton.SetState(ButtonState.Active);

        if (presetManager != null)
        {
            presetManager.LoadPresetByIndex(clickedButton.presetID);
        }
    }

    // --- NEW METHOD: Called directly when the "Clear Layout" button is clicked ---
    private void OnClearButtonClick()
    {
        Debug.Log("Clear Layout button clicked.");
        if (buildingSpawner != null)
        {
            // Call the function to remove all buildings
            buildingSpawner.RemoveAllBuildings();

            // Visually deselect any active preset button
            if (currentActiveButton != null)
            {
                currentActiveButton.SetState(ButtonState.Normal);
                currentActiveButton = null;
            }
        }
        else
        {
            Debug.LogError("BuildingSpawner reference is not set in PresetUIManager. Cannot clear layout.");
        }
    }
}
// --- END OF FILE: PresetUIManager.cs ---