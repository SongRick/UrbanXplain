using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System;

public class InstructionInputController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    // ... (所有字段声明保持不变) ...
    public event Action<string> OnInstructionSubmitted;

    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI promptIndicator;
    [SerializeField] private Image underlineImage;
    [SerializeField] private RectTransform executeButtonIcon;
    [SerializeField] private Image containerBorder;
    [SerializeField] private Button executeButton;
    private TextMeshProUGUI placeholderText;

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

    private Sequence promptPulseSequence;
    private Vector3 executeButtonInitialPos;
    private bool isLocked = false;

    // ... (Awake, Start, OnInputTextChanged, OnInputSubmit, UpdateExecuteButtonState, SubmitInstruction, PlaySubmitAnimations 方法保持不变) ...

    private void Awake()
    {
        if (inputField == null) inputField = GetComponent<TMP_InputField>();
        placeholderText = inputField.placeholder as TextMeshProUGUI;
        placeholderText.text = defaultPlaceholder;
        if (executeButtonIcon != null) executeButtonInitialPos = executeButtonIcon.localPosition;
        if (containerBorder != null) containerBorder.color = Color.clear;

        inputField.onValueChanged.AddListener(OnInputTextChanged);
        inputField.onEndEdit.AddListener(OnInputSubmit);
        if (executeButton != null) executeButton.onClick.AddListener(SubmitInstruction);
    }

    private void Start()
    {
        StartBreathingAnimation(); // 游戏开始时，默认是失焦状态，所以启动呼吸灯
        UpdateExecuteButtonState();
    }

    // ===================================================================
    // **核心修改区域**
    // ===================================================================

    /// <summary>
    /// 设置输入框的加载状态 (由外部逻辑控制器调用)
    /// </summary>
    /// <param name="isProcessing">是否正在处理指令</param>
    public void SetProcessingState(bool isProcessing)
    {
        isLocked = isProcessing;
        inputField.interactable = !isProcessing;
        placeholderText.text = isProcessing ? loadingPlaceholder : defaultPlaceholder;

        // **新增逻辑: 根据处理状态控制提示符**
        if (isProcessing)
        {
            // 正在处理中 -> 停止所有动画，并设置为常暗状态
            StopBreathingAnimation();
            promptIndicator.DOColor(defaultColor, transitionDuration); // 平滑过渡到默认(暗)颜色
        }
        else
        {
            // 处理完成 -> 恢复到正常的聚焦/失焦逻辑
            inputField.ActivateInputField(); // 自动聚焦
            // 因为ActivateInputField()会触发OnSelect，所以我们不需要在这里手动设置状态
            // OnSelect会自动处理高亮
        }

        UpdateExecuteButtonState();
    }

    // ---- 视觉状态切换 ----

    public void OnSelect(BaseEventData eventData)
    {
        if (isLocked) return;

        StopBreathingAnimation();
        SetHighlightState();
        UpdateExecuteButtonState();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (isLocked) return;

        SetDefaultState();
        StartBreathingAnimation();
        UpdateExecuteButtonState();
    }

    // ---- 动画和状态设置 ----

    /// <summary>
    /// 将UI元素设置为高亮状态
    /// </summary>
    private void SetHighlightState()
    {
        promptIndicator.DOColor(highlightColor, transitionDuration);
        underlineImage.DOColor(highlightColor, transitionDuration);
        underlineImage.rectTransform.DOScaleY(2f, transitionDuration);
    }

    /// <summary>
    /// 将UI元素设置为默认(失焦)状态
    /// </summary>
    private void SetDefaultState()
    {
        // 注意：这里不再控制promptIndicator的颜色，交给呼吸动画
        underlineImage.DOColor(defaultColor, transitionDuration);
        underlineImage.rectTransform.DOScaleY(1f, transitionDuration);
    }

    /// <summary>
    /// 停止所有与提示符相关的动画
    /// </summary>
    private void StopBreathingAnimation()
    {
        if (promptPulseSequence != null && promptPulseSequence.IsActive())
        {
            promptPulseSequence.Kill();
        }
        DOTween.Kill(promptIndicator);
    }

    /// <summary>
    /// 启动提示符的呼吸灯效果
    /// </summary>
    private void StartBreathingAnimation()
    {
        StopBreathingAnimation();
        promptPulseSequence = DOTween.Sequence();
        promptPulseSequence.Append(promptIndicator.DOColor(defaultColor, 1.5f))
                         .Append(promptIndicator.DOColor(highlightColor, 1.5f))
                         .SetLoops(-1, LoopType.Yoyo);
    }

    // ... (SubmitInstruction, PlaySubmitAnimations, UpdateExecuteButtonState 等方法保持不变) ...

    public void SubmitInstruction()
    {
        if (!executeButton.interactable || isLocked) return;
        string instructionText = inputField.text;
        OnInstructionSubmitted?.Invoke(instructionText);
        PlaySubmitAnimations();
        inputField.text = "";
    }

    private void OnInputTextChanged(string text) { UpdateExecuteButtonState(); }

    private void OnInputSubmit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) SubmitInstruction();
    }

    private void UpdateExecuteButtonState()
    {
        if (executeButton == null) return;
        bool canSubmit = !isLocked && !string.IsNullOrEmpty(inputField.text);
        executeButton.interactable = canSubmit;
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
        DOTween.Kill(this.gameObject);
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