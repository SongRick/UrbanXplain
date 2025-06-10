// --- START OF NEW FILE: BuildingInfoPanel.cs ---

using UnityEngine;
using UnityEngine.UI;
using TMPro; // 必须使用 TextMeshPro
using DG.Tweening; // 引入 DOTween 命名空间

namespace UrbanXplain
{
    // 这个数据结构用于从其他脚本向UI面板传递信息
    // 它是一个“表现层”的数据模型
    public class BuildingDisplayData
    {
        public string LotID;
        public string Function;
        public string FloorType;
        public string Material;
        public string Rationale; // 就是之前的 Description/Summary
    }

    public class BuildingInfoPanel : MonoBehaviour
    {
        [Header("UI Elements (Assign in Inspector)")]
        [Tooltip("面板的根CanvasGroup，用于控制整体透明度和交互")]
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [Tooltip("显示地块ID的TextMeshPro文本")]
        [SerializeField] private TextMeshProUGUI lotIdText;
        [Tooltip("显示功能分区的TextMeshPro文本")]
        [SerializeField] private TextMeshProUGUI functionText;
        [Tooltip("显示楼层类型的TextMeshPro文本")]
        [SerializeField] private TextMeshProUGUI floorTypeText;
        [Tooltip("显示建筑材质的TextMeshPro文本")]
        [SerializeField] private TextMeshProUGUI materialText;
        [Tooltip("显示设计理念的TextMeshPro文本")]
        [SerializeField] private TextMeshProUGUI rationaleText;
        [Tooltip("关闭面板的按钮")]
        [SerializeField] private Button closeButton;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.3f;

        void Awake()
        {
            // 确保所有引用都已设置
            if (panelCanvasGroup == null || lotIdText == null || functionText == null ||
                floorTypeText == null || materialText == null || rationaleText == null || closeButton == null)
            {
                Debug.LogError("BuildingInfoPanel: One or more UI elements are not assigned in the Inspector! The panel will not work.", this);
                enabled = false;
                return;
            }

            // 初始时隐藏面板并禁用交互
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;

            // 绑定关闭按钮事件
            closeButton.onClick.AddListener(Hide);
        }

        // 公开方法：显示面板并填充数据
        public void Show(BuildingDisplayData data)
        {
            if (data == null)
            {
                Debug.LogError("BuildingInfoPanel: Received null data. Cannot show panel.");
                return;
            }

            // 填充所有文本字段
            lotIdText.text = $"LOT ID: {data.LotID}";
            functionText.text = data.Function;
            floorTypeText.text = data.FloorType;
            materialText.text = data.Material;
            rationaleText.text = data.Rationale;

            // 激活交互性并使用DOTween播放淡入动画
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
        }

        // 公开方法：隐藏面板
        public void Hide()
        {
            // 禁用交互性并使用DOTween播放淡出动画
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad);
        }

        // 可选的测试功能，方便在编辑器中预览
        [ContextMenu("Test Show Panel")]
        private void TestShow()
        {
            BuildingDisplayData testData = new BuildingDisplayData
            {
                LotID = "99",
                Function = "Residential",
                FloorType = "High-rise",
                Material = "Glass Curtain Wall",
                Rationale = "This is a test rationale showing how the panel displays long-form text content with proper wrapping and formatting. It's designed to be a central hub for the community."
            };
            Show(testData);
        }
    }
}