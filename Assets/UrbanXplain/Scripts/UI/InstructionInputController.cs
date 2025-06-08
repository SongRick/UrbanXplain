using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 引入EventSystems命名空间
using TMPro;
using DG.Tweening;

// **修改1: 实现接口**
public class InstructionInputController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI promptIndicator;
    [SerializeField] private Image TopBorderImage;
    [SerializeField] private Image underlineImage;
    [SerializeField] private RectTransform executeButtonIcon;
    [SerializeField] private Image containerBorder;

    [Header("Color Settings")]
    [SerializeField] private Color defaultColor = new Color(0.62f, 0.65f, 0.72f);
    [SerializeField] private Color highlightColor = new Color(0, 0.75f, 1f);
    [SerializeField] private Color submitFlashColor = new Color(0, 0.75f, 1f);

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.2f;
    [SerializeField] private float submitAnimDuration = 0.15f;

    private Sequence promptPulseSequence;
    private Vector3 executeButtonInitialPos;

    private void Awake()
    {
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
        }

        if (executeButtonIcon != null)
        {
            executeButtonInitialPos = executeButtonIcon.localPosition;
        }

        if (containerBorder != null)
        {
            containerBorder.color = Color.clear;
        }

        // **修改2: 在Awake中添加监听器，并简化逻辑**
        inputField.onEndEdit.AddListener((text) => {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SubmitInstruction();
            }
        });
    }

    private void Start()
    {
        // 初始状态为失焦状态 (因为游戏开始时它默认不是选中的)
        // 我们直接手动调用一次Deselect的逻辑
        SetDeselectedState();
    }

    // **修改3: 这是 ISelectHandler 接口的实现方法**
    // 当InputField被选中时，Unity会自动调用这个方法
    public void OnSelect(BaseEventData eventData)
    {
        if (promptPulseSequence != null && promptPulseSequence.IsActive())
        {
            promptPulseSequence.Kill();
        }

        promptIndicator.color = highlightColor;
        TopBorderImage.DOColor(highlightColor, transitionDuration);
        underlineImage.DOColor(highlightColor, transitionDuration);

        TopBorderImage.rectTransform.DOScaleY(3f, transitionDuration);
        underlineImage.rectTransform.DOScaleY(3f, transitionDuration);
        Debug.Log("OnSelect called!");
    }

    // **修改4: 这是 IDeselectHandler 接口的实现方法**
    // 当InputField失去焦点时，Unity会自动调用这个方法
    public void OnDeselect(BaseEventData eventData)
    {
        SetDeselectedState();
        Debug.Log("OnDeselect called!");
    }

    // **新增**: 将Deselect的逻辑提取到一个单独的方法中，方便Start调用
    private void SetDeselectedState()
    {
        TopBorderImage.DOColor(defaultColor, transitionDuration);
        TopBorderImage.rectTransform.DOScaleY(1f, transitionDuration);
        underlineImage.DOColor(defaultColor, transitionDuration);
        underlineImage.rectTransform.DOScaleY(1f, transitionDuration);

        // 杀死旧的动画序列以防万一
        if (promptPulseSequence != null) promptPulseSequence.Kill();

        promptPulseSequence = DOTween.Sequence();
        promptPulseSequence.Append(promptIndicator.DOColor(defaultColor, 1.5f))
                         .Append(promptIndicator.DOColor(highlightColor, 1.5f))
                         .SetLoops(-1, LoopType.Yoyo);
    }

    public void SubmitInstruction()
    {
        string instructionText = inputField.text;
        if (string.IsNullOrWhiteSpace(instructionText)) return;

        // --- 动画部分 ---
        if (executeButtonIcon != null)
        {
            // 确保动画不会重复播放
            DOTween.Kill(executeButtonIcon);
            DOTween.Sequence()
                .Append(executeButtonIcon.DOLocalMoveX(executeButtonInitialPos.x + 20f, submitAnimDuration / 2).SetEase(Ease.OutCubic))
                .Append(executeButtonIcon.DOLocalMoveX(executeButtonInitialPos.x, submitAnimDuration / 2).SetEase(Ease.InCubic));
        }
        inputField.textComponent.DOFade(0, submitAnimDuration).OnComplete(() =>
        {
            inputField.text = "";
            inputField.textComponent.color = new Color(inputField.textComponent.color.r, inputField.textComponent.color.g, inputField.textComponent.color.b, 1f);
        });
        if (containerBorder != null)
        {
            DOTween.Kill(containerBorder);
            DOTween.Sequence()
                .Append(containerBorder.DOColor(submitFlashColor, submitAnimDuration / 2))
                .Append(containerBorder.DOColor(Color.clear, submitAnimDuration / 2));
        }

        // --- 核心逻辑 ---
        Debug.Log("Executing Instruction: " + instructionText);
    }

    private void OnDestroy()
    {
        if (promptPulseSequence != null)
        {
            promptPulseSequence.Kill();
        }
    }
}