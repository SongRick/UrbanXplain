using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening; // 引入DOTween

public enum ButtonState { Normal, Hover, Active }

public class PresetButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // --- 在Inspector中设置 ---
    public int presetID;
    [TextArea(3, 10)]
    public string instructionText;

    // --- 内部引用 ---
    [SerializeField] private Image imageMainShape;
    [SerializeField] private Image imageMainShapeStroke;
    [SerializeField] private Image imageAccentLine;
    [SerializeField] private TextMeshProUGUI textLabel;

    private PresetUIManager uiManager;
    private ButtonState currentState = ButtonState.Normal;

    // --- 颜色定义 ---
    private readonly Color colorMainNormal = new Color(0.17f, 0.20f, 0.27f); // #2C3344
    private readonly Color colorMainActive = new Color(0.00f, 0.75f, 1.00f); // #00BFFF
    private readonly Color colorAccentNormal = new Color(0.63f, 0.66f, 0.72f); // #A0A8B8
    private readonly Color colorAccentActive = new Color(0.00f, 0.75f, 1.00f); // #00BFFF
    private readonly Color colorTextNormal = new Color(0.88f, 0.90f, 0.94f); // #E0E5F0
    private readonly Color colorTextActive = new Color(0.12f, 0.14f, 0.18f); // #1F242E

    public void Initialize(PresetUIManager manager)
    {
        uiManager = manager;
        SetState(ButtonState.Normal, true); // 强制初始化状态
    }

    public void SetState(ButtonState newState, bool immediate = false)
    {
        currentState = newState;
        float duration = immediate ? 0f : 0.2f;

        // 控制描边辉光
        imageMainShapeStroke.gameObject.SetActive(newState == ButtonState.Hover);

        // 根据状态设置颜色
        switch (newState)
        {
            case ButtonState.Normal:
                imageMainShape.DOColor(colorMainNormal, duration);
                imageAccentLine.DOColor(colorAccentNormal, duration);
                textLabel.DOColor(colorTextNormal, duration);
                break;
            case ButtonState.Hover:
                // 主体颜色不变，描边出现
                imageMainShapeStroke.DOColor(colorMainActive, duration);
                imageAccentLine.DOColor(colorAccentActive, duration);
                textLabel.DOColor(Color.white, duration); // 悬停时文本更亮
                break;
            case ButtonState.Active:
                imageMainShape.DOColor(colorMainActive, duration);
                imageAccentLine.DOColor(colorAccentActive, duration);
                textLabel.DOColor(colorTextActive, duration);
                break;
        }
    }

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
        transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0), 0.2f, 5, 0.5f);
        // 核心逻辑调用
        // GameManager.Instance.LoadPreset(presetID);
    }
}