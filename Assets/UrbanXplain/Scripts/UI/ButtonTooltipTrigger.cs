using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [TextArea(2, 5)]
    public string tooltipMessage = "这是提示信息";
    public float delayToShow = 0.5f; // 延迟显示的时间（秒）

    private static Tooltip currentTooltipInstance; // 静态实例，确保只有一个Tooltip
    private static GameObject tooltipPrefabStatic; // 静态引用预制件，避免多次加载

    [Tooltip("将步骤1中创建的Tooltip Prefab拖到这里")]
    public GameObject tooltipPrefab; // 在Inspector中指定Tooltip Prefab

    private float hoverTimer;
    private bool isPointerOver = false;
    private bool isTooltipVisible = false;

    void Awake()
    {
        if (tooltipPrefabStatic == null && tooltipPrefab != null)
        {
            tooltipPrefabStatic = tooltipPrefab;
        }
        else if (tooltipPrefab != null && tooltipPrefabStatic != tooltipPrefab)
        {
            // 如果场景中有多个Trigger指定了不同的Prefab，以第一个为准或给出警告
            Debug.LogWarning("多个ButtonTooltipTrigger指定了不同的Tooltip Prefab。将使用第一个加载的Prefab。建议所有Trigger共享同一个Prefab引用。");
        }

        if (tooltipPrefabStatic == null)
        {
            Debug.LogError("Tooltip Prefab 未在 ButtonTooltipTrigger 中指定！");
            enabled = false; // 禁用此脚本
        }
    }

    // 确保Tooltip实例存在
    private void EnsureTooltipInstance()
    {
        if (currentTooltipInstance == null && tooltipPrefabStatic != null)
        {
            // 查找场景中是否已存在Tooltip实例，避免重复创建
            currentTooltipInstance = FindObjectOfType<Tooltip>();
            if (currentTooltipInstance == null)
            {
                GameObject tooltipGO = Instantiate(tooltipPrefabStatic);
                // 确保Tooltip在Canvas下，并且在最上层渲染
                Canvas canvas = FindObjectOfType<Canvas>(); // 找到场景中的主Canvas
                if (canvas != null)
                {
                    tooltipGO.transform.SetParent(canvas.transform, false);
                    tooltipGO.transform.SetAsLastSibling(); // 确保在UI最上层
                }
                else
                {
                    Debug.LogError("场景中找不到Canvas来放置Tooltip！");
                }
                currentTooltipInstance = tooltipGO.GetComponent<Tooltip>();
            }
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        hoverTimer = 0f;
        // Debug.Log("Pointer Enter: " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        Hide();
        // Debug.Log("Pointer Exit: " + gameObject.name);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
    }


    void Update()
    {
        if (isPointerOver && !isTooltipVisible)
        {
            hoverTimer += Time.unscaledDeltaTime; // 使用 unscaledDeltaTime 以免受 Time.timeScale 影响
            if (hoverTimer >= delayToShow)
            {
                Show();
            }
        }
    }

    private void Show()
    {
        if (string.IsNullOrEmpty(tooltipMessage)) return;

        EnsureTooltipInstance();
        if (currentTooltipInstance != null)
        {
            currentTooltipInstance.ShowTooltip(tooltipMessage, Input.mousePosition); // Input.mousePosition 是屏幕坐标
            isTooltipVisible = true;
        }
    }

    private void Hide()
    {
        if (currentTooltipInstance != null)
        {
            currentTooltipInstance.HideTooltip();
        }
        isTooltipVisible = false;
        hoverTimer = 0f; // 重置计时器
    }

    // 当对象被禁用或销毁时，也隐藏Tooltip
    void OnDisable()
    {
        if (isPointerOver || isTooltipVisible) // 如果当前鼠标正悬浮在此对象上，或者tooltip正因此对象而显示
        {
            // 检查是否是当前这个trigger导致的tooltip显示
            if (currentTooltipInstance != null && currentTooltipInstance.gameObject.activeSelf)
            {
                // 简单的判断：如果这个trigger隐藏时，Tooltip的文字和它匹配，就隐藏
                // 更稳妥的方式是让ShowTooltip返回一个ID，Hide时传递ID
                // 但为了简单，我们假设只有一个tooltip，它总是为最后一个触发它的trigger服务
                Hide(); // 强制隐藏，因为这个trigger不再活动
            }
        }
        isPointerOver = false;
        isTooltipVisible = false;
    }
}