using UnityEngine;
using UnityEngine.UI; // **新增**: 为了引用Button组件
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System;

public class InstructionInputController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    // ---- 事件 ----
    public event Action<string> OnInstructionSubmitted;

    // ---- UI 引用 ----
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI promptIndicator;
    [SerializeField] private Image underlineImage;
    [SerializeField] private RectTransform executeButtonIcon;
    [SerializeField] private Image containerBorder;
    // **修改1: 将Image引用改为Button引用**
    [SerializeField] private Button executeButton;
    private TextMeshProUGUI placeholderText;

    // ---- 配置 ----
    // **修改2: 移除了脚本中的颜色变量**
    [Header("Text Settings")]
    [SerializeField] private string defaultPlaceholder = "Enter urban planning instructions...";
    [SerializeField] private string loadingPlaceholder = "Processing instruction...";

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.2f;
    [SerializeField] private float submitAnimDuration = 0.15f;

    // ---- 内部状态 ----
    private Sequence promptPulseSequence;
    private Vector3 executeButtonInitialPos;
    private bool isLocked = false;

    private void Awake()
    {
        if (inputField == null) inputField = GetComponent<TMP_InputField>();

        placeholderText = inputField.placeholder as TextMeshProUGUI;
        placeholderText.text = defaultPlaceholder;

        if (executeButtonIcon != null) executeButtonInitialPos = executeButtonIcon.localPosition;
        if (containerBorder != null) containerBorder.color = Color.clear;

        inputField.onValueChanged.AddListener(OnInputTextChanged);
        inputField.onEndEdit.AddListener(OnInputSubmit);

        // **新增**: 如果按钮被点击，也触发提交逻辑
        if (executeButton != null)
        {
            executeButton.onClick.AddListener(SubmitInstruction);
        }
    }

    private void Start()
    {
        SetDeselectedState();
        UpdateExecuteButtonState();
    }

    // ---- 公共方法 ----
    public void SetProcessingState(bool isProcessing)
    {
        isLocked = isProcessing;
        inputField.interactable = !isProcessing;

        placeholderText.text = isProcessing ? loadingPlaceholder : defaultPlaceholder;

        if (!isProcessing)
        {
            inputField.ActivateInputField();
        }

        UpdateExecuteButtonState();
    }

    // ---- 事件回调 ----
    private void OnInputTextChanged(string text)
    {
        UpdateExecuteButtonState();
    }

    private void OnInputSubmit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitInstruction();
        }
    }

    // ---- 内部核心逻辑 ----
    private void UpdateExecuteButtonState()
    {
        if (executeButton == null) return;

        // **修改3: 核心逻辑更新 - 只控制interactable属性**
        // 按钮可交互的条件：未被锁定 且 ( (聚焦 且 有文本) 或 (不聚焦 但 有文本) ) -> 简化为：未被锁定 且 有文本
        bool canSubmit = !isLocked && !string.IsNullOrEmpty(inputField.text);

        executeButton.interactable = canSubmit;
    }

    public void SubmitInstruction()
    {
        // 现在由Button的interactable状态和这里的isLocked共同决定是否能提交
        if (!executeButton.interactable || isLocked) return;

        string instructionText = inputField.text;
        OnInstructionSubmitted?.Invoke(instructionText);
        PlaySubmitAnimations();
        inputField.text = "";
    }

    // ---- 视觉状态和动画 ----
    public void OnSelect(BaseEventData eventData)
    {
        if (isLocked) return;
        if (promptPulseSequence != null && promptPulseSequence.IsActive()) promptPulseSequence.Kill();

        // **修改4: 聚焦时只改变下划线和提示符，按钮状态由UpdateExecuteButtonState管理**
        promptIndicator.color = new Color(0, 0.75f, 1f); // 直接使用颜色值
        underlineImage.DOColor(new Color(0, 0.75f, 1f), transitionDuration);
        underlineImage.rectTransform.DOScaleY(2f, transitionDuration);

        UpdateExecuteButtonState();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (isLocked) return;
        SetDeselectedState();
        UpdateExecuteButtonState();
    }

    private void SetDeselectedState()
    {
        underlineImage.DOColor(new Color(0.62f, 0.65f, 0.72f), transitionDuration);
        underlineImage.rectTransform.DOScaleY(1f, transitionDuration);

        if (promptPulseSequence != null) promptPulseSequence.Kill();
        promptPulseSequence = DOTween.Sequence();
        promptPulseSequence.Append(promptIndicator.DOColor(new Color(0.62f, 0.65f, 0.72f), 1.5f))
                         .Append(promptIndicator.DOColor(new Color(0, 0.75f, 1f), 1.5f))
                         .SetLoops(-1, LoopType.Yoyo);
    }

    private void PlaySubmitAnimations()
    {
        if (executeButtonIcon != null)
        {
            DOTween.Kill(executeButtonIcon);
            DOTween.Sequence()
                .Append(executeButtonIcon.DOLocalMoveX(executeButtonInitialPos.x + 20f, submitAnimDuration / 2).SetEase(Ease.OutCubic))
                .Append(executeButtonIcon.DOLocalMoveX(executeButtonInitialPos.x, submitAnimDuration / 2).SetEase(Ease.InCubic));
        }
        if (containerBorder != null)
        {
            DOTween.Kill(containerBorder);
            DOTween.Sequence()
                .Append(containerBorder.DOColor(new Color(0, 0.75f, 1f), submitAnimDuration / 2))
                .Append(containerBorder.DOColor(Color.clear, submitAnimDuration / 2));
        }
    }

    private void OnDestroy()
    {
        if (promptPulseSequence != null) promptPulseSequence.Kill();

        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnInputTextChanged);
            inputField.onEndEdit.RemoveListener(OnInputSubmit);
        }
        if (executeButton != null)
        {
            executeButton.onClick.RemoveListener(SubmitInstruction);
        }
    }
}