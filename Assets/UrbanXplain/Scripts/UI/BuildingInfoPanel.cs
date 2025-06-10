using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // 引入DOTween命名空间

public class BuildingInfoPanel : MonoBehaviour
{
    // 在Inspector中拖拽赋值
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI strategyText;
    public TextMeshProUGUI materialsText;
    public TextMeshProUGUI rationaleText;
    public Button closeButton;
    public CanvasGroup canvasGroup;

    void Start()
    {
        // 绑定关闭按钮的点击事件
        closeButton.onClick.AddListener(Hide);

        // 初始时隐藏面板
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // 公开方法，用于外部调用来显示并填充数据
    public void Show(BuildingData data)
    {
        // 1. 填充文本
        titleText.text = $"LOT ID: {data.id}";
        typeText.text = data.type;
        strategyText.text = data.strategy;
        materialsText.text = data.materials;
        rationaleText.text = data.rationale;

        // 2. 播放动画显示面板
        gameObject.SetActive(true); // 确保GameObject是激活的
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // 使用DOTween实现0.3秒的淡入动画
        canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);
    }

    // 关闭面板的方法
    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // 使用DOTween实现0.3秒的淡出动画，并在动画结束后禁用GameObject（可选，为了性能）
        canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad).OnComplete(() => {
            // gameObject.SetActive(false); 
        });
    }
}

// 一个简单的数据结构来存储建筑信息
[System.Serializable]
public class BuildingData
{
    public string id;
    public string type;
    public string strategy;
    public string materials;
    public string rationale;
}