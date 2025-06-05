using UnityEngine;
 using UnityEngine.UI; 

public class Tooltip : MonoBehaviour
{
     public Text tooltipTextComponent; 

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (tooltipTextComponent == null)
        {
             tooltipTextComponent = GetComponentInChildren<Text>();// 自动查找
        }
        HideTooltip(); // 默认隐藏
    }

    // --- In Tooltip.cs ---

    public void ShowTooltip(string text, Vector2 mouseScreenPosition) // Renamed parameter for clarity
    {
        // --- 初始检查 ---
        if (rectTransform == null) // rectTransform 应该在 Awake 中获取
        {
            Debug.LogError("Tooltip's RectTransform is null! Was Awake called?");
            gameObject.SetActive(false);
            return;
        }
        if (tooltipTextComponent == null)
        {
            Debug.LogError("TooltipTextComponent is null in ShowTooltip! Check Inspector reference and Awake.");
            gameObject.SetActive(false);
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("Tooltip text is null or empty. Hiding tooltip.");
            HideTooltip(); // 调用 HideTooltip 而不是直接SetActive(false) 来确保状态一致
            return;
        }

        // --- 步骤 1: 设置文本内容 ---
        tooltipTextComponent.text = text;
        Debug.Log($"[Tooltip Debug] Text content set to: '{text}'");

        // --- 步骤 2: 确保 GameObject 激活 (如果之前是隐藏的) ---
        // 这一步很重要，因为非激活状态下，UI布局计算可能不会发生或不正确
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log("[Tooltip Debug] Tooltip GameObject was inactive, now activated.");
        }

        // --- 步骤 3: 强制 UI 布局重建 ---
        // 这是关键步骤，以确保ContentSizeFitter根据新的文本内容更新TooltipPanel的大小
        // 首先强制更新子文本组件的布局
        if (tooltipTextComponent.gameObject.activeInHierarchy) // 仅当子对象激活时
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipTextComponent.rectTransform);
            Debug.Log("[Tooltip Debug] Forced layout rebuild on TooltipTextComponent.");
        }
        else
        {
            Debug.LogWarning("[Tooltip Debug] TooltipTextComponent is not active in hierarchy during layout rebuild attempt.");
        }

        // 然后强制更新TooltipPanel (父对象，包含ContentSizeFitter)的布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        Debug.Log("[Tooltip Debug] Forced layout rebuild on TooltipPanel (rectTransform).");

        // --- 步骤 4: 打印调试信息，获取尺寸 ---
        // 获取文本组件的首选尺寸
        float preferredW = tooltipTextComponent.preferredWidth;
        float preferredH = tooltipTextComponent.preferredHeight;
        Debug.Log($"[Tooltip Debug] Text Preferred Size: Width={preferredW}, Height={preferredH}");

        // 获取文本组件实际的RectTransform尺寸 (这可能与preferred size不同，取决于其锚点和父布局)
        Rect textRect = tooltipTextComponent.rectTransform.rect;
        Debug.Log($"[Tooltip Debug] Text Actual Rect Size: Width={textRect.width}, Height={textRect.height}");

        // 获取TooltipPanel (rectTransform) 的实际尺寸，这是ContentSizeFitter作用后的结果
        Rect panelRect = rectTransform.rect;
        Debug.Log($"[Tooltip Debug] TooltipPanel Size (after ContentSizeFitter): Width={panelRect.width}, Height={panelRect.height}");

        // 如果Panel的尺寸仍然非常小 (例如16x16)，这是一个强烈的信号，表明ContentSizeFitter没有按预期工作
        if (panelRect.width <= 20 || panelRect.height <= 20) // 用一个略大于16的阈值
        {
            Debug.LogWarning($"[Tooltip Debug] TooltipPanel size ({panelRect.width}x{panelRect.height}) is very small! Check Text Preferred Size and ContentSizeFitter setup.");
            // 在这种情况下，后续的UpdatePosition可能基于错误的尺寸进行定位
        }

        // --- 步骤 5: 获取Canvas RectTransform ---
        RectTransform canvasRectTransform = null;
        if (transform.parent != null)
        {
            canvasRectTransform = transform.parent.GetComponent<RectTransform>();
        }
        if (canvasRectTransform == null) // 如果不是Canvas的直接子对象，尝试向上查找
        {
            Canvas canvas = GetComponentInParent<Canvas>(); // 更稳妥地找到父Canvas
            if (canvas != null)
            {
                canvasRectTransform = canvas.GetComponent<RectTransform>();
                Debug.Log("[Tooltip Debug] Found Canvas via GetComponentInParent.");
            }
        }

        // --- 步骤 6: 调用UpdatePosition来定位Tooltip ---
        if (canvasRectTransform != null)
        {
            UpdatePosition(mouseScreenPosition, canvasRectTransform); // UpdatePosition 内部会使用 panelRect.width/height
                                                                      // 可以在UpdatePosition内部或之后再次打印最终的panelRect，但通常在UpdatePosition之前的值更关键
        }
        else
        {
            Debug.LogError("[Tooltip Debug] Tooltip could not find a parent Canvas RectTransform to position itself correctly.");
            // 作为备用方案，可以尝试将其放置在屏幕上的某个默认位置，或者干脆隐藏
            // HideTooltip();
        }
    }

    // --- In Tooltip.cs ---
    public void UpdatePosition(Vector2 screenPosition, RectTransform canvasRect)
    {
        if (canvasRect == null)
        {
            Debug.LogError("CanvasRect is null in UpdatePosition. Cannot position tooltip.");
            return;
        }
        if (rectTransform == null) // rectTransform 是 TooltipPanel(Clone) 的 RectTransform
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // 确保Tooltip的锚点是中心 (0.5, 0.5)，这样anchoredPosition才有意义
        // 这一步也可以在EnsureTooltipInstance时设置一次，或者在Prefab上固定好
        // if (rectTransform.anchorMin != new Vector2(0.5f, 0.5f) || rectTransform.anchorMax != new Vector2(0.5f, 0.5f))
        // {
        //     rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        //     rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        // }


        Vector2 localPointInCanvas;
        // 对于 Screen Space - Overlay，第三个参数（相机）为 null
        // screenPosition 是鼠标的屏幕坐标 (左下角0,0)
        // localPointInCanvas 将是鼠标位置相对于 canvasRect 枢轴点的本地坐标
        // 因为 canvasRect 的枢轴是 (0.5,0.5)，所以 localPointInCanvas 是相对于 Canvas 中心的
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out localPointInCanvas);

        // Tooltip的本地尺寸 (由ContentSizeFitter决定，在它设置为Preferred Size后)
        float tooltipLocalWidth = rectTransform.rect.width;
        float tooltipLocalHeight = rectTransform.rect.height;

        // Tooltip在屏幕上的实际显示尺寸 (考虑Canvas的Transform Scale)
        // canvasRect.localScale.x 和 canvasRect.localScale.y 来自Canvas自身的Transform组件
        float canvasActualScaleX = canvasRect.localScale.x;
        float canvasActualScaleY = canvasRect.localScale.y;

        // 视觉偏移量 (以屏幕像素为单位)
        float xScreenOffset = 15f;
        float yScreenOffset = 10f;

        // 将屏幕偏移量转换为Canvas本地坐标系下的偏移量 (因为Canvas被缩放了)
        float xLocalOffset = xScreenOffset / canvasActualScaleX;
        float yLocalOffset = yScreenOffset / canvasActualScaleY;

        Vector2 newPivot = Vector2.zero; // Tooltip的枢轴点，(0,0)是左下
        Vector2 targetAnchoredPosition = localPointInCanvas; // Tooltip的枢轴点将对齐这个转换后的鼠标位置（相对于Canvas中心）

        // 1. 尝试将Tooltip放在鼠标指针的右边 (Tooltip的左边缘在鼠标右侧)
        float rightTargetPivotX = localPointInCanvas.x + xLocalOffset;
        float leftTargetPivotX = localPointInCanvas.x - xLocalOffset - tooltipLocalWidth; // 如果放左边，这是其左枢轴的目标X
        float canvasHalfLocalWidth = canvasRect.rect.width / 2f;

        // 检查如果放在右边 (枢轴在左，pivot.x=0)，其右边缘 (rightTargetPivotX + tooltipLocalWidth) 是否超出
        if (rightTargetPivotX + tooltipLocalWidth <= canvasHalfLocalWidth)
        {
            targetAnchoredPosition.x = rightTargetPivotX;
            newPivot.x = 0; // 枢轴在左
        }
        // 否则，尝试放在左边 (枢轴在右，pivot.x=1)，其左边缘 (leftTargetPivotX + tooltipLocalWidth) 是否在界内
        // (leftTargetPivotX + tooltipLocalWidth) 是其右枢轴的目标X
        // 或者更简单：它的右枢轴点 (localPointInCanvas.x - xLocalOffset) 的左边是 (localPointInCanvas.x - xLocalOffset - tooltipLocalWidth)
        else if (localPointInCanvas.x - xLocalOffset - tooltipLocalWidth >= -canvasHalfLocalWidth)
        {
            targetAnchoredPosition.x = localPointInCanvas.x - xLocalOffset; // Tooltip的右边缘将在鼠标左边一点
            newPivot.x = 1; // 枢轴在右
        }
        else // 如果两边都放不下，就让它贴着右/左边缘 (或者居中等其他策略)
        {
            // 简单处理：默认放右边，可能会部分出界，或者你可以选择一个固定位置
            targetAnchoredPosition.x = rightTargetPivotX;
            newPivot.x = 0;
            // Debug.LogWarning("Tooltip too wide for current mouse position and screen width.");
        }


        // 2. 尝试将Tooltip放在鼠标指针的上边 (Tooltip的下边缘在鼠标上侧)
        float topTargetPivotY = localPointInCanvas.y + yLocalOffset;
        float bottomTargetPivotY = localPointInCanvas.y - yLocalOffset - tooltipLocalHeight; // 如果放下边，这是其下枢轴的目标Y
        float canvasHalfLocalHeight = canvasRect.rect.height / 2f;
        // 检查如果放在上边 (枢轴在下，pivot.y=0)，其上边缘 (topTargetPivotY + tooltipLocalHeight) 是否超出
        if (topTargetPivotY + tooltipLocalHeight <= canvasHalfLocalHeight)
        {
            targetAnchoredPosition.y = topTargetPivotY;
            newPivot.y = 0; // 枢轴在下
        }
        // 否则，尝试放在下边 (枢轴在上，pivot.y=1)
        else if (localPointInCanvas.y - yLocalOffset - tooltipLocalHeight >= -canvasHalfLocalHeight)
        {
            targetAnchoredPosition.y = localPointInCanvas.y - yLocalOffset; // Tooltip的上边缘将在鼠标下边一点
            newPivot.y = 1; // 枢轴在上
        }
        else // 如果两边都放不下
        {
            targetAnchoredPosition.y = topTargetPivotY;
            newPivot.y = 0;
            // Debug.LogWarning("Tooltip too tall for current mouse position and screen height.");
        }

        rectTransform.pivot = newPivot;
        rectTransform.anchoredPosition = targetAnchoredPosition;
        // (恢复你的Debug.Log)
        string debugMsg = $"Tooltip Update: screenPos={screenPosition}\n" +
                          $"localPointInCanvas={localPointInCanvas}\n" +
                          $"canvasActualScale=({canvasActualScaleX},{canvasActualScaleY})\n" +
                          $"tooltipLocalSize=({tooltipLocalWidth},{tooltipLocalHeight})\n" +
                          $"targetAnchoredPos={targetAnchoredPosition}, newPivot={newPivot}\n" +
                          $"Canvas local rect width/2: {canvasHalfLocalWidth}, height/2: {canvasHalfLocalHeight}";
        Debug.Log(debugMsg);
    }


    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    // (可选) 如果希望tooltip跟随鼠标平滑移动，可以在这里更新位置
    // void Update()
    // {
    //     if (gameObject.activeSelf)
    //     {
    //         // Vector2 position = Input.mousePosition;
    //         // UpdatePosition(position, transform.parent.GetComponent<RectTransform>());
    //     }
    // }
}