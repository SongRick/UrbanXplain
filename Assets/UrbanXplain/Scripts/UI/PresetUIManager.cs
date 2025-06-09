// --- START OF FILE: PresetUIManager.cs ---
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UrbanXplain;

public class PresetUIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("拖拽场景中的PresetManager对象到这里")]
    public PresetManager presetManager;

    [Header("UI Configuration")]
    [Tooltip("用于动态放置按钮的父容器 (带Vertical Layout Group)")]
    [SerializeField] private GameObject presetButtonContainer;
    [Tooltip("按钮的预制件")]
    [SerializeField] private GameObject presetButtonPrefab;

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
            button.SetLabel(name); // 设置按钮显示的文本
            button.Initialize(this);
            allButtons.Add(button);
        }
    }

    // 清空所有已创建的按钮
    public void ClearButtons()
    {
        foreach (var button in allButtons)
        {
            if (button != null) Destroy(button.gameObject);
        }
        allButtons.Clear();
        currentActiveButton = null;
    }

    // --- 事件处理 ---

    public void OnButtonEnter(PresetButton button)
    {
        if (hidePreviewCoroutine != null)
        {
            StopCoroutine(hidePreviewCoroutine);
            hidePreviewCoroutine = null;
        }

        var canvasGroup = instructionPreviewPanel.GetComponent<CanvasGroup>();
        canvasGroup.DOKill();

        textPreviewContent.text = button.instructionText;

        // 精准定位
        var buttonRect = button.GetComponent<RectTransform>();
        var panelRect = instructionPreviewPanel.GetComponent<RectTransform>();
        instructionPreviewPanel.transform.position = button.transform.position;
        panelRect.anchoredPosition += new Vector2((panelRect.rect.width + buttonRect.rect.width) / 2f + panelOffsetX, -50f);

        instructionPreviewPanel.SetActive(true);
        canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutCubic);
    }

    public void OnButtonExit(PresetButton button)
    {
        hidePreviewCoroutine = StartCoroutine(HidePreviewPanelRoutine());
    }

    private IEnumerator HidePreviewPanelRoutine()
    {
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
        if (currentActiveButton == clickedButton) return;

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
}
// --- END OF FILE: PresetUIManager.cs ---