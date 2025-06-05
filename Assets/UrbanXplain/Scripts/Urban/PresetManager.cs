// --- START OF FILE PresetManager.cs ---
using UnityEngine;
using UnityEngine.UI; // 如果你准备动态创建按钮或需要引用Button组件
using System.Collections.Generic; // 如果你使用列表管理预设

namespace UrbanXplain
{
    public class PresetManager : MonoBehaviour
    {
        [System.Serializable]
        public class CityPreset
        {
            public string presetName; // 用于在UI上显示或识别
            public TextAsset jsonFile; // 预设的JSON文件
        }

        [Header("Preset Configuration")]
        public List<CityPreset> presets = new List<CityPreset>(); // 预设列表

        [Header("Dependencies")]
        public DeepSeekAPI deepSeekAPI; // 拖拽场景中的DeepSeekAPI对象到这里

        // 可选: 如果你想动态生成预设按钮
        [Header("UI (Optional for Dynamic Buttons)")]
        public Transform presetButtonContainer; // 用于放置预设按钮的UI父对象
        public GameObject presetButtonPrefab;   // 预设按钮的Prefab

        void Start()
        {
            if (deepSeekAPI == null)
            {
                Debug.LogError("PresetManager: DeepSeekAPI 实例未分配！");
                return;
            }

            // 检查BuildingSpawnerJson是否已加载其数据
            // 因为DeepSeekAPI的ProcessLandData依赖它
            StartCoroutine(WaitForDependenciesAndSetup());
        }

        private System.Collections.IEnumerator WaitForDependenciesAndSetup()
        {
            if (deepSeekAPI.buildingSpawnerJson == null)
            {
                Debug.LogError("PresetManager: DeepSeekAPI中的buildingSpawnerJson未设置!");
                yield break;
            }
            // 等待BuildingSpawnerJson加载完成
            yield return new WaitUntil(() => deepSeekAPI.buildingSpawnerJson.IsCsvDataLoaded);

            Debug.Log("PresetManager: Dependencies ready.");
            // 可选：如果选择动态创建按钮
            // CreatePresetButtons();
        }


        // 公共方法，由UI按钮调用
        // 参数 presetIndex 对应 presets 列表中的索引
        public void LoadPresetByIndex(int presetIndex)
        {
            if (deepSeekAPI == null)
            {
                Debug.LogError("PresetManager: DeepSeekAPI 实例未找到。");
                return;
            }
            if (!deepSeekAPI.buildingSpawnerJson.IsCsvDataLoaded)
            {
                Debug.LogWarning("PresetManager: BuildingSpawnerJson data is not loaded yet. Cannot load preset.");
                // 可以选择在这里提示用户或稍后重试
                return;
            }

            if (presetIndex < 0 || presetIndex >= presets.Count)
            {
                Debug.LogError($"PresetManager: 预设索引 {presetIndex} 超出范围！");
                return;
            }

            CityPreset selectedPreset = presets[presetIndex];
            if (selectedPreset.jsonFile != null)
            {
                Debug.Log($"PresetManager: 正在加载预设 '{selectedPreset.presetName}'...");
                string jsonContent = selectedPreset.jsonFile.text;

                // 调用DeepSeekAPI中新增的公共方法来处理这个JSON
                deepSeekAPI.ApplyJsonLayout(jsonContent);
            }
            else
            {
                Debug.LogError($"PresetManager: 预设 '{selectedPreset.presetName}' (索引 {presetIndex}) 的JSON文件未分配！");
            }
        }

        // 可选: 动态创建按钮的方法
        void CreatePresetButtons()
        {
            if (presetButtonContainer == null || presetButtonPrefab == null)
            {
                Debug.LogWarning("PresetManager: 未设置用于动态创建按钮的父容器或按钮预制件。");
                return;
            }

            // 清理旧按钮（如果需要）
            foreach (Transform child in presetButtonContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < presets.Count; i++)
            {
                GameObject buttonGO = Instantiate(presetButtonPrefab, presetButtonContainer);
                Button button = buttonGO.GetComponent<Button>();
                // 假设你的按钮Prefab有一个Text组件作为子对象来显示名称
                Text buttonText = buttonGO.GetComponentInChildren<Text>();

                if (buttonText != null)
                {
                    buttonText.text = presets[i].presetName;
                }
                else
                {
                    buttonGO.name = presets[i].presetName; // 备用，设置GameObject名称
                }

                // 捕获循环变量以供闭包使用
                int currentIndex = i;
                button.onClick.AddListener(() => LoadPresetByIndex(currentIndex));
            }
        }
    }
}
// --- END OF FILE PresetManager.cs ---