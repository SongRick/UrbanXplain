using System.Collections; // --- 新增 ---, 用于协程
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class PresetUIManager : MonoBehaviour
{
    [SerializeField] private GameObject presetButtonContainer;
    [SerializeField] private GameObject presetButtonPrefab;

    [Header("Preview Panel")]
    [SerializeField] private GameObject instructionPreviewPanel;
    [SerializeField] private TextMeshProUGUI textPreviewContent;
    [SerializeField] private float panelOffsetX = 20f;
    [SerializeField] private float hideDelay = 0.1f; // --- 新增 ---: 延迟隐藏的时间

    private List<PresetButton> allButtons = new List<PresetButton>();
    private PresetButton currentActiveButton;
    private Coroutine hidePreviewCoroutine; // --- 新增 ---: 用于持有隐藏协程的引用

    void Start()
    {
        foreach (Transform child in presetButtonContainer.transform)
        {
            PresetButton button = child.GetComponent<PresetButton>();
            if (button != null)
            {
                allButtons.Add(button);
                button.Initialize(this);
            }
        }
        // --- 修改 ---: 确保初始时CanvasGroup的alpha为0
        instructionPreviewPanel.GetComponent<CanvasGroup>().alpha = 0;
        instructionPreviewPanel.SetActive(false);
    }

    public void OnButtonEnter(PresetButton button)
    {
        // --- 新增 ---: 如果有正在执行的“隐藏”计划，立刻取消它！
        if (hidePreviewCoroutine != null)
        {
            StopCoroutine(hidePreviewCoroutine);
            hidePreviewCoroutine = null;
        }

        // --- 以下为修改后的逻辑 ---
        // 1. 杀死所有正在对CanvasGroup进行的动画，防止冲突
        instructionPreviewPanel.GetComponent<CanvasGroup>().DOKill();

        // 2. 更新并定位预览面板
        textPreviewContent.text = button.instructionText;
        Vector3 buttonPos = button.transform.position;
        // 使用屏幕坐标系来计算，避免Canvas缩放带来的问题
        var buttonRect = button.GetComponent<RectTransform>();
        var panelRect = instructionPreviewPanel.GetComponent<RectTransform>();
        instructionPreviewPanel.transform.position = buttonPos + new Vector3((panelRect.rect.width / 2 + buttonRect.rect.width / 2 + panelOffsetX) * transform.root.localScale.x, -5, 0);

        // 3. 播放动画
        instructionPreviewPanel.SetActive(true);
        instructionPreviewPanel.GetComponent<CanvasGroup>().DOFade(1f, 0.3f).SetEase(Ease.OutCubic);
    }

    public void OnButtonExit(PresetButton button)
    {
        // --- 修改 ---: 不要立即隐藏，而是启动一个延迟隐藏的协程
        hidePreviewCoroutine = StartCoroutine(HidePreviewPanelRoutine());
    }

    // --- 新增 ---: 延迟隐藏的协程
    private IEnumerator HidePreviewPanelRoutine()
    {
        yield return new WaitForSeconds(hideDelay);

        instructionPreviewPanel.GetComponent<CanvasGroup>().DOKill(); // 杀死动画，以防万一
        instructionPreviewPanel.GetComponent<CanvasGroup>().DOFade(0f, 0.2f).OnComplete(() =>
        {
            instructionPreviewPanel.SetActive(false);
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
    }
}