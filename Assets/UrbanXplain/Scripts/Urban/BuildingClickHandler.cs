using UnityEngine;
using UnityEngine.EventSystems; // 引入 EventSystem
using UnityEngine.UI;         // 引入 UI 以便检查 InputField

namespace UrbanXplain
{
    public class BuildingClickHandler : MonoBehaviour
    {
        [Header("Settings")]
        public Color highlightColor = Color.magenta;
        public LayerMask clickableLayers;
        public float maxRaycastDistance = 1000f;

        [Header("References")]
        [Tooltip("Drag the GameObject containing the BuildingColorChanger script here.")]
        public BuildingColorChanger buildingColorChanger;

        [Header("UI References")]
        [Tooltip("Drag the GameObject containing the InfoPopupManager script here.")]
        public InfoPopupManager infoPopupManager;
        // 注意：不需要直接引用 InputField，我们将通过 EventSystem 判断

        private Camera mainCamera;
        private ChildColorToggler currentlyHighlightedToggler = null;
        private const string EmptyLandsParentName = "EmptyLands000";
        public UIControl uIControl; // 确保这个引用在 Inspector 中设置了

        void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("BuildingClickHandler: Main Camera not found! Script will be disabled.");
                enabled = false;
                return;
            }
            // Debug.Log("BuildingClickHandler: Main Camera found and cached."); // 可选调试日志

            if (buildingColorChanger == null)
            {
                Debug.LogWarning("BuildingClickHandler: BuildingColorChanger reference not set.");
            }
            if (infoPopupManager == null)
            {
                Debug.LogWarning("BuildingClickHandler: InfoPopupManager reference not set.");
            }
            if (uIControl == null)
            {
                Debug.LogWarning("BuildingClickHandler: UIControl reference not set. Mode detection will fail.");
            }
        }

        void Update()
        {
            if (InputManager.GetGameMouseButtonDown(0)) // 检测鼠标左键点击
            {
                // Debug.Log("BuildingClickHandler: Mouse button 0 (left-click) detected.");

                // 检查 UIControl 是否已设置，并且当前是否处于输入模式
                if (uIControl != null && uIControl.IsInputMode())
                {
                    // === 当前处于输入模式 (鼠标可见) ===
                    // Debug.Log("BuildingClickHandler: In Input Mode.");

                    // 检查点击是否在任何 UI 元素上
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    {
                        // 点击发生在 UI 元素上
                        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
                        if (selectedObject != null && selectedObject.GetComponent<InputField>() != null)
                        {
                            // 当前选中的是一个 InputField，说明用户正在与输入框交互
                            Debug.Log("BuildingClickHandler: Input Mode - Clicked while an InputField is active or was clicked. Ignoring world click.");
                        }
                        else
                        {
                            // 点击了其他类型的 UI 元素，或者只是在 UI 面板上但没有特定选中的可交互对象
                            Debug.Log("BuildingClickHandler: Input Mode - Click was on a UI element (not necessarily an active InputField). Ignoring world click.");
                        }
                        // 在输入模式下，只要点击的是 UI (IsPointerOverGameObject() 为 true)，就不处理场景点击
                        return;
                    }
                    else
                    {
                        // 点击不在任何 UI 元素上，此时允许点击场景中的建筑
                        // Debug.Log("BuildingClickHandler: Input Mode - Click was NOT on any UI element. Processing world click.");
                        HandleClick(); // 处理场景物体点击
                    }
                }
                else if (uIControl != null && !uIControl.IsInputMode())
                {
                    // === 当前处于游玩模式 (鼠标隐藏) ===
                    // Debug.Log("BuildingClickHandler: In Gameplay Mode.");
                    Debug.Log("BuildingClickHandler: Gameplay Mode - World object clicking is disabled.");
                    // 在游玩模式下，禁止点击变色，并取消任何现有高亮
                    if (currentlyHighlightedToggler != null)
                    {
                        RestorePreviousColor(currentlyHighlightedToggler);
                        currentlyHighlightedToggler = null;
                        if (infoPopupManager != null) infoPopupManager.HidePopup();
                    }
                    // 不需要执行 HandleClick()
                }
                else if (uIControl == null)
                {
                    Debug.LogError("BuildingClickHandler: UIControl reference is null in Update. Cannot determine mode.");
                    // 在这种情况下，可以决定是默认允许还是禁止，或者什么都不做
                    // 为安全起见，可以默认不处理点击
                    return;
                }
            }
        }

        void HandleClick()
        {
            // Debug.Log("BuildingClickHandler: HandleClick() processing world click.");
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxRaycastDistance, clickableLayers))
            {
                // Debug.Log($"BuildingClickHandler: Raycast hit object: '{hit.collider.gameObject.name}' on layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                GameObject hitObject = hit.collider.gameObject;
                Transform currentTransform = hitObject.transform;
                ChildColorToggler clickedPlotToggler = null;
                GameObject landPlotRootGameObjectForID = null;

                while (currentTransform != null)
                {
                    if (currentTransform.parent != null && currentTransform.parent.name == EmptyLandsParentName)
                    {
                        clickedPlotToggler = currentTransform.GetComponent<ChildColorToggler>();
                        if (clickedPlotToggler != null)
                        {
                            landPlotRootGameObjectForID = currentTransform.gameObject;
                            break;
                        }
                    }
                    if (currentTransform.parent == null) break;
                    currentTransform = currentTransform.parent;
                }

                if (clickedPlotToggler != null && landPlotRootGameObjectForID != null)
                {
                    // Debug.Log($"BuildingClickHandler: Clicked on a recognized land plot: '{landPlotRootGameObjectForID.name}'");
                    if (currentlyHighlightedToggler == clickedPlotToggler)
                    {
                        // Debug.Log("BuildingClickHandler: Clicked the same highlighted plot. Deselecting.");
                        RestorePreviousColor(currentlyHighlightedToggler);
                        currentlyHighlightedToggler = null;
                        if (infoPopupManager != null) infoPopupManager.HidePopup();
                    }
                    else
                    {
                        // Debug.Log($"BuildingClickHandler: New plot clicked: '{landPlotRootGameObjectForID.name}'.");
                        if (currentlyHighlightedToggler != null)
                        {
                            RestorePreviousColor(currentlyHighlightedToggler);
                        }
                        currentlyHighlightedToggler = clickedPlotToggler;
                        currentlyHighlightedToggler.SetChildrenColor(highlightColor, false);

                        if (infoPopupManager != null &&
                            buildingColorChanger != null &&
                            buildingColorChanger.deepSeekAPI != null &&
                            buildingColorChanger.deepSeekAPI.buildingSpawnerJson != null)
                        {
                            BuildingSpawnerJson spawner = buildingColorChanger.deepSeekAPI.buildingSpawnerJson;
                            if (spawner.landArray == null) { Debug.LogError("BuildingClickHandler: spawner.landArray is null!"); return; }

                            int landId = -1;
                            for (int i = 0; i < spawner.landArray.Length; i++)
                            {
                                if (spawner.landArray[i] == landPlotRootGameObjectForID)
                                {
                                    landId = i + 1;
                                    break;
                                }
                            }

                            if (landId != -1)
                            {
                                string summary = spawner.GetLandSummary(landId);
                                BuildingInfo info = new BuildingInfo(landId.ToString(), summary);
                                infoPopupManager.ShowPopup(info);
                            }
                            else
                            {
                                Debug.LogWarning($"BuildingClickHandler: Could not find LandID for clicked plot '{landPlotRootGameObjectForID.name}'.");
                                if (infoPopupManager != null) infoPopupManager.HidePopup();
                            }
                        }
                        else
                        {
                            Debug.LogWarning("BuildingClickHandler: Missing references for popup (InfoPopupManager, BuildingColorChanger, etc.).");
                            if (infoPopupManager != null) infoPopupManager.HidePopup();
                        }
                    }
                }
                else
                {
                    // Debug.Log("BuildingClickHandler: Clicked object was on a clickable layer but not recognized as part of a land plot. Deselecting.");
                    if (currentlyHighlightedToggler != null)
                    {
                        RestorePreviousColor(currentlyHighlightedToggler);
                        currentlyHighlightedToggler = null;
                    }
                    if (infoPopupManager != null) infoPopupManager.HidePopup();
                }
            }
            else
            {
                // Debug.Log("BuildingClickHandler: Raycast did not hit any object on clickable layers. Deselecting.");
                if (currentlyHighlightedToggler != null)
                {
                    RestorePreviousColor(currentlyHighlightedToggler);
                    currentlyHighlightedToggler = null;
                }
                if (infoPopupManager != null) infoPopupManager.HidePopup();
            }
        }

        private void RestorePreviousColor(ChildColorToggler togglerToRestore)
        {
            if (togglerToRestore == null) return;
            // Debug.Log($"BuildingClickHandler: Restoring color for toggler on '{togglerToRestore.gameObject.name}'.");
            togglerToRestore.RestoreToPreviousState(buildingColorChanger);
        }
    }
}