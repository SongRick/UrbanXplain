using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // 需要 System.IO 来读取文件
using System.Linq; // 需要 Linq 来方便地处理字符串数组
using UnityEngine.Networking; // 确保引入了 Networking

namespace UrbanXplain
{
    public class BuildingSpawnerJson : MonoBehaviour // 类名可以保留，或者改成更合适的如 BuildingSpawnerCsv
    {
        [Header("CSV File Names (in StreamingAssets)")]
        public string buildingPrefabCsvFileName = "buildingprefab.csv";
        public string emptyLandCsvFileName = "emptyland.csv";

        [Header("Building Prefabs")]
        [SerializeField] public GameObject[] buildings;

        [Header("Empty Lands")]
        public GameObject emptyLandsParent;
        [SerializeField] public GameObject[] landArray;

        private float minBuildingDistance = 10f;

        private Dictionary<int, BuildingPrefabData> buildingPrefabCache = new Dictionary<int, BuildingPrefabData>();
        private Dictionary<int, EmptyLandData> emptyLandCache = new Dictionary<int, EmptyLandData>();

        private static HashSet<int> usedSpecialBuildings = new HashSet<int>();
        private static HashSet<int> usedCulturalBuildings = new HashSet<int>();
        public bool IsCsvDataLoaded { get; private set; } = false; // 修改变量名

        void Start()
        {
            StartCoroutine(LoadDataFromCsv());
        }

        public IEnumerator LoadDataFromCsv()
        {
            buildingPrefabCache.Clear();
            emptyLandCache.Clear();

            // Debug.Log($"Current platform: {Application.platform}"); // 可以添加这行来确认平台

            yield return StartCoroutine(LoadBuildingPrefabsFromCsv());
            yield return StartCoroutine(LoadEmptyLandsFromCsv());

            if (buildingPrefabCache.Count > 0 && emptyLandCache.Count > 0)
            {
                IsCsvDataLoaded = true;
                Debug.Log("CSV 数据加载完成。");
            }
            else
            {
                IsCsvDataLoaded = false;
                Debug.LogError("CSV 数据加载失败或未获取到任何数据。请确保 CSV 文件在 StreamingAssets 文件夹中且格式正确，并检查网络请求（针对WebGL/Android）。");
            }
        }

        IEnumerator LoadBuildingPrefabsFromCsv()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, buildingPrefabCsvFileName);
            // 对于 WebGL 和 Android，路径实际上是 URL，必须用 UnityWebRequest
            // 对于其他平台，UnityWebRequest 也能处理 file:// URL，所以可以统一使用
            // 但要注意 file:// URL 的格式和权限问题，特别是 macOS 和 Linux
            // 更安全的做法是明确区分平台

            string csvText = "";
            bool loadSuccess = false;

            Debug.Log($"Attempting to load Building Prefabs CSV from: {filePath}");

            // WebGL 和 Android 平台必须使用 UnityWebRequest
            if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android)
            {
                UnityWebRequest www = UnityWebRequest.Get(filePath);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    csvText = www.downloadHandler.text;
                    loadSuccess = true;
                    Debug.Log($"成功加载 {buildingPrefabCsvFileName} (WebGL/Android)");
                }
                else
                {
                    Debug.LogError($"加载 {buildingPrefabCsvFileName} 失败 (WebGL/Android): {www.error} at path {filePath}");
                }
                www.Dispose(); // 及时释放资源
            }
            else // 其他平台 (PC, Mac, Linux Editor/Standalone, iOS Editor/Standalone)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        csvText = File.ReadAllText(filePath);
                        loadSuccess = true;
                        Debug.Log($"成功加载 {buildingPrefabCsvFileName} (Desktop/iOS)");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"读取 {buildingPrefabCsvFileName} 时发生错误: {ex.Message} at path {filePath}");
                    }
                }
                else
                {
                    Debug.LogError($"找不到 {buildingPrefabCsvFileName} 文件于: {filePath}");
                }
            }

            if (loadSuccess && !string.IsNullOrEmpty(csvText))
            {
                ParseBuildingPrefabCsv(csvText);
                Debug.Log($"已处理 {buildingPrefabCsvFileName}。缓存数量: {buildingPrefabCache.Count}");
            }
            else if (loadSuccess && string.IsNullOrEmpty(csvText))
            {
                Debug.LogWarning($"{buildingPrefabCsvFileName} 文件已加载但内容为空。");
            }
        }

        IEnumerator LoadEmptyLandsFromCsv()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, emptyLandCsvFileName);
            string csvText = "";
            bool loadSuccess = false;

            Debug.Log($"Attempting to load Empty Lands CSV from: {filePath}");

            if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android)
            {
                UnityWebRequest www = UnityWebRequest.Get(filePath);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    csvText = www.downloadHandler.text;
                    loadSuccess = true;
                    Debug.Log($"成功加载 {emptyLandCsvFileName} (WebGL/Android)");
                }
                else
                {
                    Debug.LogError($"加载 {emptyLandCsvFileName} 失败 (WebGL/Android): {www.error} at path {filePath}");
                }
                www.Dispose();
            }
            else
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        csvText = File.ReadAllText(filePath);
                        loadSuccess = true;
                        Debug.Log($"成功加载 {emptyLandCsvFileName} (Desktop/iOS)");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"读取 {emptyLandCsvFileName} 时发生错误: {ex.Message} at path {filePath}");
                    }
                }
                else
                {
                    Debug.LogError($"找不到 {emptyLandCsvFileName} 文件于: {filePath}");
                }
            }

            if (loadSuccess && !string.IsNullOrEmpty(csvText))
            {
                ParseEmptyLandCsv(csvText);
                Debug.Log($"已处理 {emptyLandCsvFileName}。缓存数量: {emptyLandCache.Count}");
            }
            else if (loadSuccess && string.IsNullOrEmpty(csvText))
            {
                Debug.LogWarning($"{emptyLandCsvFileName} 文件已加载但内容为空。");
            }
        }

        void ParseBuildingPrefabCsv(string csvText)
        {
            StringReader reader = new StringReader(csvText);
            string line;
            bool isFirstLine = true; // 跳过标题行
            Dictionary<string, int> headerMap = new Dictionary<string, int>();

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = ParseCsvLine(line); // 使用辅助函数处理带引号的逗号

                if (isFirstLine)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        headerMap[values[i].Trim('"')] = i; // 移除引号并存储列名和索引
                    }
                    isFirstLine = false;
                    continue;
                }

                try
                {
                    BuildingPrefabData data = new BuildingPrefabData
                    {
                        // 使用列名映射来获取数据，更健壮
                        ID = int.Parse(values[headerMap["ID"]].Trim('"')),
                        Length = float.Parse(values[headerMap["Length"]].Trim('"')),
                        Width = float.Parse(values[headerMap["Width"]].Trim('"')),
                        Function = int.Parse(values[headerMap["Function"]].Trim('"')),
                        FloorType = int.Parse(values[headerMap["FloorType"]].Trim('"')),
                        Material = int.Parse(values[headerMap["Material"]].Trim('"'))
                        // 注意：您的 buildingprefab.csv 还有 "Name", "Classification", "Height", "PivotPoint" 列
                        // 如果需要这些数据，请在 BuildingPrefabData 结构体中添加相应字段，并在这里解析
                    };
                    buildingPrefabCache[data.ID] = data;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"解析 buildingprefab.csv 行数据时出错: '{line}'. 错误: {ex.Message}");
                }
            }
        }

        void ParseEmptyLandCsv(string csvText)
        {
            StringReader reader = new StringReader(csvText);
            string line;
            bool isFirstLine = true;
            Dictionary<string, int> headerMap = new Dictionary<string, int>();

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = ParseCsvLine(line);

                if (isFirstLine)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        headerMap[values[i].Trim('"')] = i;
                    }
                    isFirstLine = false;
                    continue;
                }

                try
                {
                    EmptyLandData data = new EmptyLandData
                    {
                        ID = int.Parse(values[headerMap["ID"]].Trim('"')),
                        Length = float.Parse(values[headerMap["Length"]].Trim('"')),
                        Width = float.Parse(values[headerMap["Width"]].Trim('"')),
                        Position = new Vector3(
                            float.Parse(values[headerMap["StartPosX"]].Trim('"')),
                            float.Parse(values[headerMap["StartPosY"]].Trim('"')),
                            float.Parse(values[headerMap["StartPosZ"]].Trim('"'))
                        ),
                        RotationY = float.Parse(values[headerMap["RotationY"]].Trim('"')),
                        T = int.Parse(values[headerMap["T"]].Trim('"')),
                        S = int.Parse(values[headerMap["S"]].Trim('"')),
                        BuildingUsageCount = new Dictionary<int, int>(),
                        Function = "",
                        FloorType = "",
                        Material = "",
                        Summary = ""
                    };
                    emptyLandCache[data.ID] = data;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"解析 emptyland.csv 行数据时出错: '{line}'. 错误: {ex.Message}");
                }
            }
        }

        // 辅助函数：解析包含引号和逗号的CSV行
        // 这个解析器比较基础，对于非常复杂的CSV可能不够健壮
        private string[] ParseCsvLine(string line)
        {
            List<string> result = new List<string>();
            System.Text.StringBuilder currentField = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 处理 "" -> "
                        currentField.Append('"');
                        i++; // 跳过下一个引号
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
            result.Add(currentField.ToString()); // 添加最后一个字段
            return result.ToArray();
        }

        // --- 以下是您原有的建筑生成逻辑，基本保持不变 ---
        // 它们现在将使用从 CSV 加载并填充到缓存中的数据

        public void SpawnBuilding(string emptyID, string function, string floorType, string material)
        {
            if (!IsCsvDataLoaded)
            {
                Debug.LogError("CSV 数据尚未加载。无法生成建筑。");
                return;
            }

            if (landArray == null || landArray.Length == 0)
            {
                Debug.LogError("Land array has not been initialized! Cannot spawn buildings.");
                return;
            }

            if (!int.TryParse(emptyID, out int landId) || landId < 1 || landId > landArray.Length)
            {
                Debug.LogError($"Invalid land ID: {emptyID}. Current registered land ID range: 1-{landArray.Length}.");
                return;
            }

            if (!emptyLandCache.TryGetValue(landId, out EmptyLandData landData))
            {
                Debug.LogError($"Land configuration not found for ID: {landId}.");
                return;
            }

            GameObject landObject = landArray[landId - 1];

            if (landData.S == 1)
            {
                GenerateS1Building(landData, landObject);
                return;
            }

            int targetFunction = int.Parse(function);
            int targetFloor = int.Parse(floorType);
            int targetMaterial = int.Parse(material);

            if (targetFunction == 4)
            {
                GenerateCulturalBuilding(landData, landObject);
            }
            else
            {
                GenerateStandardBuildings(landData, landObject, targetFunction, targetFloor, targetMaterial);
            }

            if (landData.T == 1 && targetFunction != 4)
            {
                GenerateAdditionalBuildings(landData, landObject, targetFunction, targetFloor, targetMaterial);
            }
        }

        void GenerateS1Building(EmptyLandData landData, GameObject landObject)
        {
            List<int> availableIds = new List<int> { 235, 236 };
            availableIds.RemoveAll(id => usedSpecialBuildings.Contains(id) || GetUsageCount(landData, id) >= 2);

            if (availableIds.Count == 0)
            {
                Debug.LogError("Special buildings for S=1 plots (IDs 235/236) are all used or have reached their usage limit on this plot!");
                return;
            }

            int selectedId = availableIds[Random.Range(0, availableIds.Count)];
            usedSpecialBuildings.Add(selectedId);
            IncrementUsageCount(landData, selectedId);

            if (!buildingPrefabCache.ContainsKey(selectedId))
            {
                Debug.LogError($"Building configuration does not exist for ID: {selectedId}. Cannot spawn S1 building.");
                return;
            }

            Vector3 offset = landData.RotationY switch
            {
                90f => new Vector3(86f, 0f, -86f),
                180f => new Vector3(-86f, 0f, -86f),
                270f => new Vector3(-86f, 0f, 86f),
                _ => new Vector3(86f, 0f, 86f)
            };

            Vector3 spawnPos = landData.Position + offset;
            Quaternion rotation = Quaternion.Euler(0f, landObject.transform.eulerAngles.y, 0f);
            InstantiateBuilding(selectedId, spawnPos, rotation, landObject.transform);
        }

        void GenerateCulturalBuilding(EmptyLandData landData, GameObject landObject)
        {
            if (Mathf.Approximately(landData.Width, 72f) == false)
            {
                Debug.LogError($"Cultural building plot {landData.ID} (Position: {landData.Position}) width ({landData.Width}) is not 72. Cannot generate cultural building.");
                return;
            }

            Vector3 basePos = landData.Position;
            Quaternion buildingWorldRotation = Quaternion.Euler(0, landObject.transform.eulerAngles.y, 0);
            List<Vector3> relativeSpawnOffsets = new List<Vector3>();

            if (landData.T == 0)
            {
                int maxBuildings = Mathf.FloorToInt(landData.Length / 72f);

                if (maxBuildings == 0)
                {
                    Debug.LogWarning($"Plot {landData.ID} (T=0, Position: {basePos}, Length: {landData.Length}) does not have enough length to place any 72x72 cultural buildings.");
                    return;
                }

                for (int i = 0; i < maxBuildings; i++)
                {
                    Vector3 offset;
                    float spacing = 72f + minBuildingDistance;

                    switch (landData.RotationY)
                    {
                        case 0f:
                            offset = new Vector3(36f + i * spacing, 0f, 36f);
                            break;
                        case 90f:
                            offset = new Vector3(36f, 0f, -36f - i * spacing);
                            break;
                        case 180f:
                            offset = new Vector3(-36f - i * spacing, 0f, -36f);
                            break;
                        case 270f:
                            offset = new Vector3(-36f, 0f, 36f + i * spacing);
                            break;
                        default:
                            Debug.LogError($"Plot {landData.ID} (T=0) has an unsupported RotationY value ({landData.RotationY}). Skipping this building index.");
                            continue;
                    }

                    relativeSpawnOffsets.Add(offset);
                }
            }
            else if (landData.T == 1)
            {
                switch (landData.RotationY)
                {
                    case 0f:
                        relativeSpawnOffsets.Add(new Vector3(36f, 0f, 36f));
                        relativeSpawnOffsets.Add(new Vector3(136f, 0f, 36f));
                        relativeSpawnOffsets.Add(new Vector3(236f, 0f, 36f));
                        relativeSpawnOffsets.Add(new Vector3(136f, 0f, -64f));
                        break;
                    case 90f:
                        relativeSpawnOffsets.Add(new Vector3(36f, 0f, -36f));
                        relativeSpawnOffsets.Add(new Vector3(36f, 0f, -136f));
                        relativeSpawnOffsets.Add(new Vector3(36f, 0f, -236f));
                        relativeSpawnOffsets.Add(new Vector3(-64f, 0f, -136f));
                        break;
                    case 180f:
                        relativeSpawnOffsets.Add(new Vector3(-36f, 0f, -36f));
                        relativeSpawnOffsets.Add(new Vector3(-136f, 0f, -36f));
                        relativeSpawnOffsets.Add(new Vector3(-236f, 0f, -36f));
                        relativeSpawnOffsets.Add(new Vector3(-136f, 0f, 64f));
                        break;
                    case 270f:
                        relativeSpawnOffsets.Add(new Vector3(-36f, 0f, 36f));
                        relativeSpawnOffsets.Add(new Vector3(-36f, 0f, 136f));
                        relativeSpawnOffsets.Add(new Vector3(-36f, 0f, 236f));
                        relativeSpawnOffsets.Add(new Vector3(64f, 0f, 136f));
                        break;
                    default:
                        Debug.LogError($"Plot {landData.ID} (T=1) has an unsupported RotationY value ({landData.RotationY}). Cannot determine fixed positions.");
                        return;
                }
            }

            if (relativeSpawnOffsets.Count == 0)
            {
                Debug.LogWarning($"Plot {landData.ID} (Position {basePos}) could not calculate any valid spawn positions for cultural buildings.");
                return;
            }

            int spawnedCount = 0;

            foreach (Vector3 relativeOffset in relativeSpawnOffsets)
            {
                List<BuildingPrefabData> validPrefabs = new List<BuildingPrefabData>();

                foreach (BuildingPrefabData prefab_iter in buildingPrefabCache.Values)
                {
                    if (prefab_iter.Function == 4)
                        validPrefabs.Add(prefab_iter);
                }

                if (validPrefabs.Count == 0)
                    continue;

                BuildingPrefabData selectedPrefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
                usedCulturalBuildings.Add(selectedPrefab.ID);
                IncrementUsageCount(landData, selectedPrefab.ID);

                Vector3 spawnPosition = basePos + relativeOffset;
                InstantiateBuilding(selectedPrefab.ID, spawnPosition, buildingWorldRotation, landObject.transform);
                spawnedCount++;
            }

            if (spawnedCount == 0 && relativeSpawnOffsets.Count > 0)
            {
                Debug.LogWarning($"Plot {landData.ID} (Position {basePos}): Planned {relativeSpawnOffsets.Count} cultural building positions, but failed to generate any.");
            }
        }

        void GenerateStandardBuildings(EmptyLandData landData, GameObject landObject, int function, int floorType, int material)
        {
            float L = landData.Length;
            Vector3 basePos = landData.Position;
            float parentRotationY = landObject.transform.eulerAngles.y;

            var (row1Start, row2Start, direction) = GetGenerationParameters(parentRotationY, basePos);
            GenerateBuildingRow(L, row1Start, direction, function, floorType, material, landObject.transform, parentRotationY, landData);

            if (Mathf.Approximately(landData.Width, 72f))
            {
                GenerateBuildingRow(L, row2Start, direction, function, floorType, material, landObject.transform, parentRotationY, landData);
            }
        }

        void GenerateAdditionalBuildings(EmptyLandData landData, GameObject landObject, int function, int floorType, int material)
        {
            float newLength = 100f;
            float newWidth = 72f;
            float newSceneRotationY = landObject.transform.eulerAngles.y + 90f;

            Vector3 newBasePosForRow1 = CalculateNewPosition(36, landData.Position, landData.RotationY);
            Vector3 newBasePosForRow2 = CalculateNewPosition(72, landData.Position, landData.RotationY);

            EmptyLandData newLandForAdditional = new EmptyLandData
            {
                Length = newLength,
                Width = newWidth,
                Position = newBasePosForRow1,
                RotationY = landData.RotationY + 90f,
                BuildingUsageCount = landData.BuildingUsageCount
            };

            Vector3 placementDirection = GetNewDirection(newLandForAdditional.RotationY);

            GenerateBuildingRow(newLength, newBasePosForRow1, placementDirection, function, floorType, material, landObject.transform, newSceneRotationY, newLandForAdditional);
            GenerateBuildingRow(newLength, newBasePosForRow2, placementDirection, function, floorType, material, landObject.transform, newSceneRotationY, newLandForAdditional);
        }

        Vector3 CalculateNewPosition(float offsetWithinNewArea, Vector3 originalPos, float originalDbRotation)
        {
            float totalOffsetFromOriginalEdge = 100f + offsetWithinNewArea;

            return originalDbRotation switch
            {
                90f => new Vector3(originalPos.x, originalPos.y, originalPos.z - totalOffsetFromOriginalEdge),
                180f => new Vector3(originalPos.x - totalOffsetFromOriginalEdge, originalPos.y, originalPos.z),
                270f => new Vector3(originalPos.x, originalPos.y, originalPos.z + totalOffsetFromOriginalEdge),
                _ => new Vector3(originalPos.x + totalOffsetFromOriginalEdge, originalPos.y, originalPos.z)
            };
        }

        Vector3 GetNewDirection(float rotationY)
        {
            float normalizedRotation = Mathf.Repeat(rotationY, 360f);

            if (Mathf.Abs(normalizedRotation - 90f) < 1f)
                return new Vector3(0, 0, -1);
            if (Mathf.Abs(normalizedRotation - 180f) < 1f)
                return new Vector3(-1, 0, 0);
            if (Mathf.Abs(normalizedRotation - 270f) < 1f)
                return new Vector3(0, 0, 1);

            return new Vector3(1, 0, 0);
        }

        (Vector3, Vector3, Vector3) GetGenerationParameters(float sceneRotationY, Vector3 basePos)
        {
            float normalizedRotation = Mathf.Repeat(sceneRotationY, 360f);

            if (Mathf.Abs(normalizedRotation - 90f) < 1f)
                return (new Vector3(basePos.x + 36f, basePos.y, basePos.z),
                       new Vector3(basePos.x + 72f, basePos.y, basePos.z),
                       new Vector3(0, 0, -1));

            if (Mathf.Abs(normalizedRotation - 180f) < 1f)
                return (new Vector3(basePos.x, basePos.y, basePos.z - 36f),
                       new Vector3(basePos.x, basePos.y, basePos.z - 72f),
                       new Vector3(-1, 0, 0));

            if (Mathf.Abs(normalizedRotation - 270f) < 1f)
                return (new Vector3(basePos.x - 36f, basePos.y, basePos.z),
                       new Vector3(basePos.x - 72f, basePos.y, basePos.z),
                       new Vector3(0, 0, 1));

            return (new Vector3(basePos.x, basePos.y, basePos.z + 36f),
                   new Vector3(basePos.x, basePos.y, basePos.z + 72f),
                   new Vector3(1, 0, 0));
        }

        void GenerateBuildingRow(float maxLength, Vector3 startPos, Vector3 direction, int function, int floorType, int material, Transform parent, float buildingRotationY, EmptyLandData landData)
        {
            Vector3 currentPos = startPos;
            float remainingLength = maxLength;

            while (remainingLength > 20f)
            {
                var candidates = GetValidBuildings(function, floorType, remainingLength - (currentPos == startPos ? 0 : minBuildingDistance), landData);

                if (candidates.Count == 0)
                    break;

                BuildingPrefabData selected = candidates[Random.Range(0, candidates.Count)];
                Vector3 spawnOffsetDueToLength = direction * (selected.Length / 2f);
                Vector3 spawnPos = currentPos + spawnOffsetDueToLength;

                if (currentPos != startPos)
                {
                    spawnPos = currentPos + direction * (minBuildingDistance + selected.Length / 2f);
                }

                InstantiateBuilding(selected.ID, spawnPos, Quaternion.Euler(0, buildingRotationY, 0), parent);
                IncrementUsageCount(landData, selected.ID);

                float consumedLength = selected.Length;
                if (currentPos != startPos || (currentPos == startPos && remainingLength < maxLength))
                {
                    consumedLength += minBuildingDistance;
                }

                currentPos += direction * consumedLength;
                remainingLength -= consumedLength;
            }
        }

        List<BuildingPrefabData> GetValidBuildings(int function, int floorType, float maxLength, EmptyLandData landData)
        {
            List<BuildingPrefabData> result = new List<BuildingPrefabData>();

            foreach (var prefab_iter in buildingPrefabCache.Values)
            {
                if (prefab_iter.Function == function &&
                    prefab_iter.FloorType == floorType &&
                    prefab_iter.Length <= maxLength &&
                    GetUsageCount(landData, prefab_iter.ID) < 3)
                {
                    result.Add(prefab_iter);
                }
            }

            return result;
        }

        GameObject InstantiateBuilding(int prefabId, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (prefabId < 0 || prefabId >= buildings.Length)
            {
                Debug.LogError($"Invalid prefab ID: {prefabId}. Array size is {buildings.Length}.");
                return null;
            }

            GameObject prefab = buildings[prefabId];

            if (prefab == null)
            {
                Debug.LogError($"Prefab not assigned for ID: {prefabId} in the 'buildings' array.");
                return null;
            }

            return Instantiate(prefab, position, rotation, parent);
        }

        public void RemoveAllBuildings()
        {
            foreach (var landGameObject in landArray)
            {
                if (landGameObject != null)
                {
                    for (int i = landGameObject.transform.childCount - 1; i >= 0; i--)
                    {
                        if (landGameObject.transform.GetChild(i) != landGameObject.transform)
                        {
                            Destroy(landGameObject.transform.GetChild(i).gameObject);
                        }
                    }
                }
            }

            // --- MODIFIED: 重置所有缓存数据 ---
            List<int> keys = new List<int>(emptyLandCache.Keys);
            foreach (int key in keys)
            {
                EmptyLandData landData = emptyLandCache[key];
                landData.BuildingUsageCount.Clear();
                landData.Summary = "";
                landData.Function = "";
                landData.FloorType = "";
                landData.Material = "";
                emptyLandCache[key] = landData;
            }

            usedSpecialBuildings.Clear();
            usedCulturalBuildings.Clear();
            Debug.Log("All spawned buildings and plot data cleared.");
        }

        private int GetUsageCount(EmptyLandData landData, int buildingId)
        {
            if (landData.BuildingUsageCount.TryGetValue(buildingId, out int count))
                return count;

            return 0;
        }

        private void IncrementUsageCount(EmptyLandData landData, int buildingId)
        {
            if (landData.BuildingUsageCount.ContainsKey(buildingId))
                landData.BuildingUsageCount[buildingId]++;
            else
                landData.BuildingUsageCount[buildingId] = 1;
        }


        
        // --- NEW METHOD: 用于存储LLM返回的完整地块属性 ---
        public void StoreLandProperties(string emptyID, string function, string floorType, string material, string summary)
        {
            if (!int.TryParse(emptyID, out int landId))
            {
                Debug.LogError($"StoreLandProperties: Invalid land ID format: {emptyID}");
                return;
            }

            if (emptyLandCache.TryGetValue(landId, out EmptyLandData landData))
            {
                landData.Function = function;
                landData.FloorType = floorType;
                landData.Material = material;
                landData.Summary = summary;
                emptyLandCache[landId] = landData; // 因为是 struct，必须写回
            }
            else
            {
                Debug.LogError($"StoreLandProperties: Land configuration not found for ID: {landId}.");
            }
        }

        // --- NEW METHOD: 用于获取地块的完整数据 ---
        // 返回可空类型，以便调用者检查是否找到了数据
        public EmptyLandData? GetLandData(int landId)
        {
            if (emptyLandCache.TryGetValue(landId, out EmptyLandData landData))
            {
                return landData;
            }
            return null; // 表示未找到
        }

        // 内部数据结构
        public struct BuildingPrefabData
        {
            public int ID;
            public float Length;
            public float Width;
            public int Function;
            public int FloorType;
            public int Material;
            // 如果您需要CSV中的 "Name", "Classification", "Height", "PivotPoint" 字段，请在这里添加
            // public string Name;
            // public string Classification;
            // public float Height;
            // public int PivotPoint;
        }

        public struct EmptyLandData
        {
            public int ID;
            public float Length;
            public float Width;
            public Vector3 Position;
            public float RotationY;
            public int T;
            public int S;
            public Dictionary<int, int> BuildingUsageCount;
            // --- NEW FIELDS ---
            public string Function;
            public string FloorType;
            public string Material;
            public string Summary; // Rationale
        }
    }
}