// --- START OF FILE: PresetButton.cs ---
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public enum ButtonState { Normal, Hover, Active }

public class PresetButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Data (Set by Manager)")]
    [HideInInspector] public int presetID;
    [HideInInspector] public string instructionText;

    [Header("Internal References")]
    [SerializeField] private Image imageMainShape;
    [SerializeField] private Image imageMainShapeStroke;
    [SerializeField] private Image imageAccentLine;
    [SerializeField] private TextMeshProUGUI textLabel;

    private PresetUIManager uiManager;
    private ButtonState currentState = ButtonState.Normal;

    // --- 设计规范: 颜色定义 ---
    private readonly Color colorMainNormal = new Color(0.17f, 0.20f, 0.27f, 1f); // #2C3344
    private readonly Color colorMainActive = new Color(0.00f, 0.75f, 1.00f, 1f); // #00BFFF
    private readonly Color colorAccentNormal = new Color(0.63f, 0.66f, 0.72f, 1f); // #A0A8B8
    private readonly Color colorAccentActive = new Color(0.00f, 0.75f, 1.00f, 1f); // #00BFFF
    private readonly Color colorTextNormal = new Color(0.88f, 0.90f, 0.94f, 1f); // #E0E5F0
    private readonly Color colorTextActive = new Color(0.12f, 0.14f, 0.18f, 1f); // #1F242E
    private readonly Color colorTextHover = Color.white;

    // 由UI管理器在创建时调用
    public void Initialize(PresetUIManager manager)
    {
        uiManager = manager;
        SetState(ButtonState.Normal, true); // 强制初始化状态
    }

    // 由UI管理器调用，设置按钮上显示的文本
    public void SetLabel(string labelText)
    {
        if (textLabel != null)
        {
            textLabel.text = labelText;
        }
    }

    // 核心方法：改变按钮的视觉状态
    public void SetState(ButtonState newState, bool immediate = false)
    {
        if (currentState == newState && !immediate) return; // 避免不必要的动画

        currentState = newState;
        float duration = immediate ? 0f : 0.2f;

        // 停止所有正在进行的颜色动画以防冲突
        imageMainShape.DOKill();
        imageMainShapeStroke.DOKill();
        imageAccentLine.DOKill();
        textLabel.DOKill();

        imageMainShapeStroke.gameObject.SetActive(newState == ButtonState.Hover);

        switch (newState)
        {
            case ButtonState.Normal:
                imageMainShape.DOColor(colorMainNormal, duration);
                imageAccentLine.DOColor(colorAccentNormal, duration);
                textLabel.DOColor(colorTextNormal, duration);
                break;
            case ButtonState.Hover:
                imageMainShape.color = colorMainNormal; // 悬停时主体颜色不变
                imageMainShapeStroke.color = colorMainActive; // 描边直接变色
                imageAccentLine.DOColor(colorAccentActive, duration);
                textLabel.DOColor(colorTextHover, duration);
                break;
            case ButtonState.Active:
                imageMainShape.DOColor(colorMainActive, duration);
                imageAccentLine.DOColor(colorAccentActive, duration);
                textLabel.DOColor(colorTextActive, duration);
                break;
        }
    }

    // --- 接口实现 ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        uiManager.OnButtonEnter(this);
        if (currentState != ButtonState.Active)
        {
            SetState(ButtonState.Hover);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        uiManager.OnButtonExit(this);
        if (currentState != ButtonState.Active)
        {
            SetState(ButtonState.Normal);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        uiManager.OnButtonClick(this);
        transform.DOPunchScale(new Vector3(-0.05f, -0.05f, 0), 0.2f, 5, 0.5f);
    }
}
// --- END OF FILE: PresetButton.cs ---