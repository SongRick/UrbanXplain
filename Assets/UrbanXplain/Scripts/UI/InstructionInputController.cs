using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System; // **新增**: 为了使用 Action 事件

public class InstructionInputController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    // ---- 事件 ----
    // **新增**: 当指令被提交时触发的事件。外部脚本可以订阅这个事件。
    public event Action<string> OnInstructionSubmitted;

    // ---- UI 引用 ----
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI promptIndicator;
    [SerializeField] private Image underlineImage;
    [SerializeField] private RectTransform executeButtonIcon;
    [SerializeField] private Image containerBorder;
    // **新增**: 直接引用按钮的Image组件来控制颜色
    [SerializeField] private Image executeButtonImage;
    private TextMeshProUGUI placeholderText;

    // ---- 配置 ----
    [Header("Color Settings")]
    [SerializeField] private Color defaultColor = new Color(0.62f, 0.65f, 0.72f);
    [SerializeField] private Color highlightColor = new Color(0, 0.75f, 1f);
    [SerializeField] private Color submitFlashColor = new Color(0, 0.75f, 1f);

    [Header("Text Settings")]
    [SerializeField] private string defaultPlaceholder = "Enter urban planning instructions...";
    [SerializeField] private string loadingPlaceholder = "Processing instruction...";

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.2f;
    [SerializeField] private float submitAnimDuration = 0.15f;

    // ---- 内部状态 ----
    private Sequence promptPulseSequence;
    private Vector3 executeButtonInitialPos;
    private bool isLocked = false; // **新增**: 标记输入框是否因加载而被锁定

    private void Awake()
    {
        if (inputField == null) inputField = GetComponent<TMP_InputField>();

        // **新增**: 获取Placeholder组件
        placeholderText = inputField.placeholder as TextMeshProUGUI;
        placeholderText.text = defaultPlaceholder;

        if (executeButtonIcon != null) executeButtonInitialPos = executeButtonIcon.localPosition;
        if (containerBorder != null) containerBorder.color = Color.clear;

        // **新增**: 监听输入框文本变化事件，用于更新按钮状态
        inputField.onValueChanged.AddListener(OnInputTextChanged);
        // 监听提交事件
        inputField.onEndEdit.AddListener(OnInputSubmit);
    }

    private void Start()
    {
        SetDeselectedState();
        UpdateExecuteButtonState(); // 初始化按钮状态
    }

    // ---- 公共方法：由外部逻辑控制器调用 ----
    /// <summary>
    /// 设置输入框的加载状态 (由外部逻辑控制器调用)
    /// </summary>
    /// <param name="isProcessing">是否正在处理指令</param>
    public void SetProcessingState(bool isProcessing)
    {
        isLocked = isProcessing;
        inputField.interactable = !isProcessing;

        if (isProcessing)
        {
            placeholderText.text = loadingPlaceholder;
        }
        else
        {
            placeholderText.text = defaultPlaceholder;
            inputField.ActivateInputField(); // 处理完成后自动聚焦
        }
        UpdateExecuteButtonState(); // 更新按钮状态
    }

    // ---- 事件回调 ----
    private void OnInputTextChanged(string text)
    {
        if (!isLocked) // 如果不在加载中，才更新按钮状态
        {
            UpdateExecuteButtonState();
        }
    }

    private void OnInputSubmit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitInstruction();
        }
    }

    // ---- 内部逻辑 ----
    private void UpdateExecuteButtonState()
    {
        if (executeButtonImage == null) return;

        // 核心逻辑: 当输入框可交互(未锁定)、被聚焦、且内容不为空时，按钮高亮
        bool shouldHighlight = inputField.interactable && inputField.isFocused && !string.IsNullOrEmpty(inputField.text);

        executeButtonImage.DOColor(shouldHighlight ? highlightColor : defaultColor, transitionDuration);
    }

    public void SubmitInstruction()
    {
        if (isLocked || string.IsNullOrWhiteSpace(inputField.text)) return;

        string instructionText = inputField.text;

        // --- 触发事件，通知外部监听者 ---
        OnInstructionSubmitted?.Invoke(instructionText);

        // --- 播放动画 ---
        PlaySubmitAnimations();

        inputField.text = ""; // 清空输入框
    }

    // ---- 视觉状态和动画 ----
    public void OnSelect(BaseEventData eventData)
    {
        if (isLocked) return;

        if (promptPulseSequence != null && promptPulseSequence.IsActive()) promptPulseSequence.Kill();

        promptIndicator.color = highlightColor;
        underlineImage.DOColor(highlightColor, transitionDuration);
        underlineImage.rectTransform.DOScaleY(2f, transitionDuration);

        UpdateExecuteButtonState(); // 聚焦时更新按钮状态
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (isLocked) return;

        SetDeselectedState();
        UpdateExecuteButtonState(); // 失焦时更新按钮状态
    }

    private void SetDeselectedState()
    {
        underlineImage.DOColor(defaultColor, transitionDuration);
        underlineImage.rectTransform.DOScaleY(1f, transitionDuration);

        if (promptPulseSequence != null) promptPulseSequence.Kill();
        promptPulseSequence = DOTween.Sequence();
        promptPulseSequence.Append(promptIndicator.DOColor(defaultColor, 1.5f))
                         .Append(promptIndicator.DOColor(highlightColor, 1.5f))
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
                .Append(containerBorder.DOColor(submitFlashColor, submitAnimDuration / 2))
                .Append(containerBorder.DOColor(Color.clear, submitAnimDuration / 2));
        }
    }

    private void OnDestroy()
    {
        if (promptPulseSequence != null) promptPulseSequence.Kill();

        // 移除所有监听器
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnInputTextChanged);
            inputField.onEndEdit.RemoveListener(OnInputSubmit);
        }
    }
}