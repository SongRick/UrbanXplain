// --- START OF FILE: PresetManager.cs ---
using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Required for IEnumerator

namespace UrbanXplain
{
    public class PresetManager : MonoBehaviour
    {
        // 数据结构：定义一个预设包含的所有信息
        [System.Serializable]
        public class CityPreset
        {
            public string presetName; // 用于在UI上显示的名称
            [TextArea(3, 10)]
            public string instruction; // 用于在悬停预览面板中显示的文本
            public TextAsset jsonFile; // 预设的JSON数据文件
        }

        [Header("Preset Configuration")]
        [Tooltip("在这里配置所有可用的城市预设")]
        public List<CityPreset> presets = new List<CityPreset>();

        [Header("Dependencies")]
        [Tooltip("拖拽场景中的DeepSeekAPI对象到这里")]
        public DeepSeekAPI deepSeekAPI;
        [Tooltip("拖拽场景中的PresetUIManager对象到这里")]
        public PresetUIManager presetUIManager;

        void Start()
        {
            if (deepSeekAPI == null || presetUIManager == null)
            {
                Debug.LogError("PresetManager: DeepSeekAPI或PresetUIManager实例未分配！请在Inspector中拖拽赋值。", this);
                return;
            }
            StartCoroutine(WaitForDependenciesAndSetup());
        }

        private IEnumerator WaitForDependenciesAndSetup()
        {
            if (deepSeekAPI.buildingSpawnerJson == null)
            {
                Debug.LogError("PresetManager: DeepSeekAPI中的buildingSpawnerJson未设置!", this);
                yield break;
            }

            // 等待核心数据加载完成
            yield return new WaitUntil(() => deepSeekAPI.buildingSpawnerJson.IsCsvDataLoaded);

            Debug.Log("PresetManager: Dependencies ready. Creating UI buttons...");
            CreatePresetButtonsViaManager();
        }

        // 命令UI管理器创建所有按钮
        void CreatePresetButtonsViaManager()
        {
            presetUIManager.ClearButtons(); // 先清空旧按钮
            for (int i = 0; i < presets.Count; i++)
            {
                presetUIManager.CreateButtonForPreset(
                    i,                          // 预设的唯一ID (就是它在列表中的索引)
                    presets[i].presetName,      // 按钮上显示的名称
                    presets[i].instruction      // 悬停时预览的文本
                );
            }
        }

        // 公共方法，由UI管理器在按钮被点击时调用
        public void LoadPresetByIndex(int presetIndex)
        {
            if (presetIndex < 0 || presetIndex >= presets.Count)
            {
                Debug.LogError($"PresetManager: 预设索引 {presetIndex} 超出范围！", this);
                return;
            }

            if (!deepSeekAPI.buildingSpawnerJson.IsCsvDataLoaded)
            {
                Debug.LogWarning("PresetManager: 核心数据尚未加载，无法应用预设。", this);
                return;
            }

            CityPreset selectedPreset = presets[presetIndex];
            if (selectedPreset.jsonFile != null)
            {
                Debug.Log($"PresetManager: 正在加载预设 '{selectedPreset.presetName}' (ID: {presetIndex})...", this);
                string jsonContent = selectedPreset.jsonFile.text;
                deepSeekAPI.ApplyJsonLayout(jsonContent);
            }
            else
            {
                Debug.LogError($"PresetManager: 预设 '{selectedPreset.presetName}' (索引 {presetIndex}) 的JSON文件未分配！", this);
            }
        }
    }
}
// --- END OF FILE: PresetManager.cs ---