using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySqlConnector;

namespace UrbanXplain
{
    // Manages loading building and land plot data from a MySQL database.
    // It handles spawning building prefabs onto designated land plots based on various criteria,
    // specific plot types (S=1, cultural), and placement rules.
    public class BuildingSpawnerJson : MonoBehaviour
    {
        [Header("Database Settings")]
        // Connection string used to connect to the MySQL database containing building and land data.
        private string connectionString = "Server=localhost;Database=simcity;Uid=llm;Pwd=123456;";

        [Header("Building Prefabs")]
        // Array of GameObjects representing the building prefabs.
        // The ID from the database is expected to map directly to an index in this array.
        [SerializeField] public GameObject[] buildings;

        [Header("Empty Lands")]
        // Parent GameObject in the scene hierarchy that holds all individual empty land plot GameObjects.
        public GameObject emptyLandsParent;
        // Array of GameObjects representing the actual empty land plots in the scene.
        // These are linked to the data loaded from the 'emptyland' database table.
        [SerializeField] public GameObject[] landArray;

        // Defines the minimum distance to maintain between adjacent spawned buildings in a row.
        private float minBuildingDistance = 5f;

        // Cache for building prefab metadata loaded from the 'BuildingPrefab' database table, keyed by building ID.
        private Dictionary<int, BuildingPrefabData> buildingPrefabCache = new Dictionary<int, BuildingPrefabData>();
        // Cache for empty land plot data loaded from the 'emptyland' database table, keyed by land ID.
        private Dictionary<int, EmptyLandData> emptyLandCache = new Dictionary<int, EmptyLandData>();
        // Tracks IDs of special 'S=1' type buildings that have been used globally to ensure uniqueness or limited use.
        private static HashSet<int> usedSpecialBuildings = new HashSet<int>();
        // Tracks IDs of cultural building types (Function ID 4) that have been spawned at least once.
        private static HashSet<int> usedCulturalBuildings = new HashSet<int>();
        // Flag indicating whether the initial data loading from the database has completed successfully.
        public bool IsDatabaseLoaded { get; private set; } = false;

        void Start()
        {
            // Initiate the process of loading all necessary data from the database.
            StartCoroutine(LoadDatabaseData());
        }

        // Coroutine to orchestrate the sequential loading of building prefabs and empty land data.
        public IEnumerator LoadDatabaseData()
        {
            yield return StartCoroutine(LoadBuildingPrefabs());
            yield return StartCoroutine(LoadEmptyLands());
            IsDatabaseLoaded = true; // Set flag once all data is loaded.
            Debug.Log("Database data loading complete.");
        }

        // Coroutine to load building prefab metadata from the 'BuildingPrefab' table in the database.
        IEnumerator LoadBuildingPrefabs()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Debug.Log("Starting to load building prefab data...");

                    using (MySqlCommand cmd = new MySqlCommand(
                        "SELECT ID,Length,Width,`Function`,FloorType,Material FROM BuildingPrefab", connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BuildingPrefabData data = new BuildingPrefabData
                                {
                                    ID = reader.GetInt32("ID"),
                                    Length = reader.GetFloat("Length"),
                                    Width = reader.GetFloat("Width"),
                                    Function = reader.GetInt32("Function"),
                                    FloorType = reader.GetInt32("FloorType"),
                                    Material = reader.GetInt32("Material")
                                };
                                buildingPrefabCache[data.ID] = data; // Cache the loaded data.
                            }
                        }
                    }
                    Debug.Log($"Successfully loaded {buildingPrefabCache.Count} building configurations.");
                }
                catch (MySqlException ex)
                {
                    Debug.LogError($"Database error while loading building prefabs: {ex.Message}");
                }
            }
            yield return null; // Wait for the next frame.
        }

        // Coroutine to load empty land plot data from the 'emptyland' table in the database.
        IEnumerator LoadEmptyLands()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Debug.Log("Starting to load empty land data...");
                    using (MySqlCommand cmd = new MySqlCommand(
                        "SELECT ID,Length,Width,StartPosX,StartPosY,StartPosZ,RotationY,T,S FROM emptyland", connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                EmptyLandData data = new EmptyLandData
                                {
                                    ID = reader.GetInt32("ID"),
                                    Length = reader.GetFloat("Length"),
                                    Width = reader.GetFloat("Width"),
                                    Position = new Vector3(
                                        reader.GetFloat("StartPosX"),
                                        reader.GetFloat("StartPosY"),
                                        reader.GetFloat("StartPosZ")),
                                    RotationY = reader.GetFloat("RotationY"),
                                    T = reader.GetInt32("T"), // Type flag 'T' from database.
                                    S = reader.GetInt32("S"), // Type flag 'S' from database.
                                    BuildingUsageCount = new Dictionary<int, int>(), // Initialize usage count for this plot.
                                    Summary = "" // Initialize summary string for this plot.
                                };
                                emptyLandCache[data.ID] = data; // Cache the loaded data.
                            }
                        }
                    }
                    Debug.Log($"Successfully loaded {emptyLandCache.Count} empty land configurations.");
                }
                catch (MySqlException ex)
                {
                    Debug.LogError($"Database error while loading empty lands: {ex.Message}");
                }
            }
            yield return null; // Wait for the next frame.
        }

        // Main public method to initiate spawning of buildings on a specified empty land plot.
        // Parameters define the target criteria (function, floor type, material) for building selection.
        public void SpawnBuilding(string emptyID, string function, string floorType, string material)
        {
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

            GameObject landObject = landArray[landId - 1]; // Land IDs are 1-based, array is 0-based.

            // Special handling for S=1 type plots, which have unique generation logic.
            if (landData.S == 1)
            {
                GenerateS1Building(landData, landObject);
                return; // S=1 plots do not proceed to standard/additional generation.
            }

            // Parse target criteria from string inputs.
            int targetFunction = int.Parse(function);
            int targetFloor = int.Parse(floorType);
            int targetMaterial = int.Parse(material); // Note: material parameter is parsed but not explicitly used in GetValidBuildings logic below.

            // Handle cultural buildings (Function ID 4) with their specific generation logic.
            if (targetFunction == 4)
            {
                GenerateCulturalBuilding(landData, landObject);
            }
            else // Handle standard (non-cultural) buildings.
            {
                GenerateStandardBuildings(landData, landObject, targetFunction, targetFloor, targetMaterial);
            }

            // If the plot is type T=1 and not a cultural plot, generate additional buildings.
            if (landData.T == 1 && targetFunction != 4)
            {
                GenerateAdditionalBuildings(landData, landObject, targetFunction, targetFloor, targetMaterial);
            }
        }

        // Generates buildings for special S=1 type land plots.
        // These plots use specific building IDs (e.g., 237, 238) with unique global usage constraints.
        void GenerateS1Building(EmptyLandData landData, GameObject landObject)
        {
            List<int> availableIds = new List<int> { 237, 238 }; // Predefined IDs for S=1 plots.
            // Filter out IDs already used globally or if used more than once on this specific S1 plot.
            availableIds.RemoveAll(id => usedSpecialBuildings.Contains(id) || GetUsageCount(landData, id) >= 2);

            if (availableIds.Count == 0)
            {
                Debug.LogError("Special buildings for S=1 plots (IDs 237/238) are all used or have reached their usage limit on this plot!");
                return;
            }

            int selectedId = availableIds[Random.Range(0, availableIds.Count)];
            usedSpecialBuildings.Add(selectedId); // Mark as globally used.
            IncrementUsageCount(landData, selectedId); // Increment usage on this specific plot.

            if (!buildingPrefabCache.ContainsKey(selectedId))
            {
                Debug.LogError($"Building configuration does not exist for ID: {selectedId}. Cannot spawn S1 building.");
                return;
            }

            // Specific spawn offsets for S=1 buildings based on the land plot's rotation (from database).
            Vector3 offset = landData.RotationY switch
            {
                90f => new Vector3(86f, 0f, -86f),
                180f => new Vector3(-86f, 0f, -86f),
                270f => new Vector3(-86f, 0f, 86f),
                _ => new Vector3(86f, 0f, 86f) // Default for 0f or other unhandled rotations.
            };

            Vector3 spawnPos = landData.Position + offset;
            // Building rotation should match the land plot's orientation in the scene.
            Quaternion rotation = Quaternion.Euler(0f, landObject.transform.eulerAngles.y, 0f);

            InstantiateBuilding(selectedId, spawnPos, rotation, landObject.transform);
        }

        // Generates cultural buildings (Function ID 4) on a land plot.
        // Placement logic depends on landData.T (type) and landData.RotationY (database rotation).
        // Requires plot width to be approximately 72 units.
        void GenerateCulturalBuilding(EmptyLandData landData, GameObject landObject)
        {
            if (Mathf.Approximately(landData.Width, 72f) == false)
            {
                Debug.LogError($"Cultural building plot {landData.ID} (Position: {landData.Position}) width ({landData.Width}) is not 72. Cannot generate cultural building.");
                return;
            }

            Vector3 basePos = landData.Position; // Base position of the plot from database.
            Quaternion buildingWorldRotation = Quaternion.Euler(0, landObject.transform.eulerAngles.y, 0); // Use scene object's rotation.
            List<Vector3> relativeSpawnOffsets = new List<Vector3>(); // Stores calculated spawn points relative to basePos.

            // Determine spawn locations based on T flag and database RotationY.
            if (landData.T == 0) // T=0: fill buildings along the length of the plot.
            {
                int maxBuildings = Mathf.FloorToInt(landData.Length / 72f); // Assumes 72x72 cultural buildings.
                if (maxBuildings == 0)
                {
                    Debug.LogWarning($"Plot {landData.ID} (T=0, Position: {basePos}, Length: {landData.Length}) does not have enough length to place any 72x72 cultural buildings.");
                    return;
                }

                for (int i = 0; i < maxBuildings; i++)
                {
                    Vector3 offset;
                    // Spacing includes building size (72) + minBuildingDistance (e.g., 10, making it 82).
                    float spacing = 72f + minBuildingDistance;
                    switch (landData.RotationY) // Database RotationY determines layout direction.
                    {
                        case 0f: offset = new Vector3(36f + i * spacing, 0f, 36f); break;
                        case 90f: offset = new Vector3(36f, 0f, -36f - i * spacing); break;
                        case 180f: offset = new Vector3(-36f - i * spacing, 0f, -36f); break;
                        case 270f: offset = new Vector3(-36f, 0f, 36f + i * spacing); break;
                        default:
                            Debug.LogError($"Plot {landData.ID} (T=0) has an unsupported RotationY value ({landData.RotationY}). Skipping this building index.");
                            continue;
                    }
                    relativeSpawnOffsets.Add(offset);
                }
            }
            else if (landData.T == 1) // T=1: place buildings at 4 predefined specific locations.
            {
                // These are fixed offsets relative to landData.Position, forming a specific pattern.
                // The values (e.g., 136, 236, -64) are derived from assumed building sizes and relative placements.
                switch (landData.RotationY) // Database RotationY orients the pattern.
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
                // Select from any available cultural building type (Function ID 4).
                // No per-plot usage limit check here, unlike standard buildings, but global 'usedCulturalBuildings' tracks types.
                foreach (BuildingPrefabData prefab in buildingPrefabCache.Values)
                {
                    if (prefab.Function == 4) // Is a cultural building.
                    {
                        validPrefabs.Add(prefab);
                    }
                }

                if (validPrefabs.Count == 0)
                {
                    // Debug.LogWarning($"Plot {landData.ID}: No suitable cultural prefabs found for offset {relativeOffset}.");
                    continue; // Skip this spawn point if no valid prefabs.
                }

                BuildingPrefabData selectedPrefab = validPrefabs[Random.Range(0, validPrefabs.Count)];

                usedCulturalBuildings.Add(selectedPrefab.ID); // Track that this type of cultural building has been used at least once globally.
                IncrementUsageCount(landData, selectedPrefab.ID); // Increment usage count on this specific plot.

                Vector3 spawnPosition = basePos + relativeOffset; // Calculate absolute world spawn position.
                InstantiateBuilding(selectedPrefab.ID, spawnPosition, buildingWorldRotation, landObject.transform);
                spawnedCount++;
            }

            if (spawnedCount > 0)
            {
                // Debug.Log($"Plot {landData.ID} (Position {basePos}): Successfully spawned {spawnedCount} cultural buildings out of {relativeSpawnOffsets.Count} planned spots.");
            }
            else if (relativeSpawnOffsets.Count > 0) // Planned spots but nothing spawned.
            {
                Debug.LogWarning($"Plot {landData.ID} (Position {basePos}): Planned {relativeSpawnOffsets.Count} cultural building positions, but failed to generate any (possibly no available prefabs or all met usage limits).");
            }
        }

        // Generates standard (non-cultural, non-S1) buildings on a land plot.
        // Placement involves filling rows based on plot length and width.
        void GenerateStandardBuildings(EmptyLandData landData, GameObject landObject, int function, int floorType, int material)
        {
            float L = landData.Length;
            Vector3 basePos = landData.Position;
            float parentRotationY = landObject.transform.eulerAngles.y; // Use scene object's Y rotation for spawned building orientation.

            // Get starting positions for rows and the direction of placement along the plot's length.
            var (row1Start, row2Start, direction) = GetGenerationParameters(parentRotationY, basePos);

            // Generate the first row of buildings.
            GenerateBuildingRow(L, row1Start, direction, function, floorType, material, landObject.transform, parentRotationY, landData);

            // If the plot width is approximately 72 units, it can accommodate a second row.
            if (Mathf.Approximately(landData.Width, 72f))
            {
                GenerateBuildingRow(L, row2Start, direction, function, floorType, material, landObject.transform, parentRotationY, landData);
            }
        }

        // Generates additional buildings for T=1 type plots, typically in an area perpendicular to the main plot.
        void GenerateAdditionalBuildings(EmptyLandData landData, GameObject landObject, int function, int floorType, int material)
        {
            float newLength = 100f; // Predefined length for the additional building area.
            float newWidth = 72f;   // Predefined width for the additional building area.
            // New orientation is perpendicular to the original plot's scene orientation.
            float newSceneRotationY = landObject.transform.eulerAngles.y + 90f;

            // Calculate base positions for the two new rows in the additional area, offset from the original plot's edge.
            // The '36' and '72' here likely represent centerline offsets for rows within the new area.
            // 'landData.RotationY' (database rotation) is used to determine the direction of this offset.
            Vector3 newBasePosForRow1 = CalculateNewPosition(36, landData.Position, landData.RotationY);
            Vector3 newBasePosForRow2 = CalculateNewPosition(72, landData.Position, landData.RotationY);

            // Create a temporary EmptyLandData structure for these additional rows.
            // Crucially, it shares the BuildingUsageCount dictionary with the original landData to maintain consistent usage tracking.
            EmptyLandData newLandForAdditional = new EmptyLandData
            {
                Length = newLength,
                Width = newWidth,
                Position = newBasePosForRow1, // This will be the start for the first row.
                RotationY = landData.RotationY + 90f, // This is a conceptual DB-style rotation for GetNewDirection.
                BuildingUsageCount = landData.BuildingUsageCount
            };

            // Determine placement direction based on the conceptual new perpendicular rotation.
            Vector3 placementDirection = GetNewDirection(newLandForAdditional.RotationY);

            // Generate rows. Note: parent is still the original landObject. 'newSceneRotationY' is used for building orientation.
            GenerateBuildingRow(newLength, newBasePosForRow1, placementDirection, function, floorType, material, landObject.transform, newSceneRotationY, newLandForAdditional);
            GenerateBuildingRow(newLength, newBasePosForRow2, placementDirection, function, floorType, material, landObject.transform, newSceneRotationY, newLandForAdditional);
        }

        // Helper to calculate a new base position for additional building areas.
        // 'offsetWithinNewArea' is how far into the new area the row starts (e.g., 36 for first row centerline).
        // 'originalDbRotation' orients the direction of the 100-unit offset from the original plot.
        Vector3 CalculateNewPosition(float offsetWithinNewArea, Vector3 originalPos, float originalDbRotation)
        {
            // The additional area is assumed to start 100 units away from the original plot's edge,
            // plus the offset for the specific row within that new area.
            float totalOffsetFromOriginalEdge = 100f + offsetWithinNewArea;
            return originalDbRotation switch // Uses database rotation to determine offset direction.
            {
                90f => new Vector3(originalPos.x, originalPos.y, originalPos.z - totalOffsetFromOriginalEdge),
                180f => new Vector3(originalPos.x - totalOffsetFromOriginalEdge, originalPos.y, originalPos.z),
                270f => new Vector3(originalPos.x, originalPos.y, originalPos.z + totalOffsetFromOriginalEdge),
                _ => new Vector3(originalPos.x + totalOffsetFromOriginalEdge, originalPos.y, originalPos.z) // Default for 0 degrees.
            };
        }

        // Helper to get the normalized placement direction vector based on a Y rotation value (typically DB-style).
        Vector3 GetNewDirection(float rotationY)
        {
            float normalizedRotation = Mathf.Repeat(rotationY, 360f);
            if (Mathf.Abs(normalizedRotation - 90f) < 1f) return new Vector3(0, 0, -1);  // Rotated to face +X, length along -Z
            if (Mathf.Abs(normalizedRotation - 180f) < 1f) return new Vector3(-1, 0, 0); // Rotated to face -Z, length along -X
            if (Mathf.Abs(normalizedRotation - 270f) < 1f) return new Vector3(0, 0, 1);   // Rotated to face -X, length along +Z
            return new Vector3(1, 0, 0);    // Default (0 deg), facing +Z, length along +X
        }


        // Helper to determine starting positions for building rows and the placement direction.
        // 'sceneRotationY' is the Y rotation of the land plot GameObject in the scene.
        // 'basePos' is the database StartPos of the land plot.
        // Returns: (start position for row 1, start position for row 2, placement direction vector)
        (Vector3, Vector3, Vector3) GetGenerationParameters(float sceneRotationY, Vector3 basePos)
        {
            float normalizedRotation = Mathf.Repeat(sceneRotationY, 360f);

            // Values 36 and 72 are likely centerline offsets for rows, assuming buildings are placed side-by-side.
            if (Mathf.Abs(normalizedRotation - 90f) < 1f) // Plot rotated 90 deg (e.g., facing +X, length extends along -Z)
            {
                return (new Vector3(basePos.x + 36f, basePos.y, basePos.z),      // Row 1 centerline
                        new Vector3(basePos.x + 72f, basePos.y, basePos.z),      // Row 2 centerline
                        new Vector3(0, 0, -1));                                 // Placement direction
            }
            if (Mathf.Abs(normalizedRotation - 180f) < 1f) // Plot rotated 180 deg (e.g., facing -Z, length extends along -X)
            {
                return (new Vector3(basePos.x, basePos.y, basePos.z - 36f),
                        new Vector3(basePos.x, basePos.y, basePos.z - 72f),
                        new Vector3(-1, 0, 0));
            }
            if (Mathf.Abs(normalizedRotation - 270f) < 1f) // Plot rotated 270 deg (e.g., facing -X, length extends along +Z)
            {
                return (new Vector3(basePos.x - 36f, basePos.y, basePos.z),
                        new Vector3(basePos.x - 72f, basePos.y, basePos.z),
                        new Vector3(0, 0, 1));
            }
            // Default for 0 deg (or near 0/360) (e.g., facing +Z, length extends along +X)
            return (new Vector3(basePos.x, basePos.y, basePos.z + 36f),
                    new Vector3(basePos.x, basePos.y, basePos.z + 72f),
                    new Vector3(1, 0, 0));
        }

        // Generates a single row of buildings along a specified maximum length and direction.
        // 'buildingRotationY' is the Y rotation to apply to each spawned building.
        // 'landData' is used for tracking building usage counts on this specific plot.
        void GenerateBuildingRow(float maxLength, Vector3 startPos, Vector3 direction, int function, int floorType, int material, Transform parent, float buildingRotationY, EmptyLandData landData)
        {
            Vector3 currentPos = startPos; // Current position to place the center of the next building.
            float remainingLength = maxLength;

            // Continue placing buildings as long as there's enough space (e.g., more than 20 units).
            while (remainingLength > 20f)
            {
                // Get candidate buildings fitting criteria, remaining length, and usage limits on this plot.
                // Subtract minBuildingDistance from remainingLength for candidate search to ensure space for the next building *and* its gap.
                var candidates = GetValidBuildings(function, floorType, remainingLength - (currentPos == startPos ? 0 : minBuildingDistance), landData);
                if (candidates.Count == 0)
                {
                    // Debug.LogError($"No suitable buildings found for row with function {function}, floorType {floorType}, remaining length {remainingLength}.");
                    break; // No more buildings can be placed in this row.
                }
                BuildingPrefabData selected = candidates[Random.Range(0, candidates.Count)];

                // Calculate spawn position for the center of the building.
                // The building is placed such that its "start" edge aligns with currentPos (after the first building).
                Vector3 spawnOffsetDueToLength = direction * (selected.Length / 2f);
                Vector3 spawnPos = currentPos + spawnOffsetDueToLength;
                if (currentPos != startPos) // For subsequent buildings, shift by half length + min distance.
                {
                    spawnPos = currentPos + direction * (minBuildingDistance + selected.Length / 2f);
                }


                InstantiateBuilding(selected.ID, spawnPos, Quaternion.Euler(0, buildingRotationY, 0), parent);
                IncrementUsageCount(landData, selected.ID); // Update usage count for this plot.

                // Advance currentPos for the next building.
                float consumedLength = selected.Length;
                if (currentPos != startPos || (currentPos == startPos && remainingLength < maxLength))
                { // If it's not the very first placement in an empty row.
                    consumedLength += minBuildingDistance;
                }
                currentPos += direction * consumedLength;
                remainingLength -= consumedLength;

            }
        }

        // Retrieves a list of valid building prefabs based on function, floor type, maximum length,
        // and per-plot usage count (max 3 for standard buildings).
        List<BuildingPrefabData> GetValidBuildings(int function, int floorType, float maxLength, EmptyLandData landData)
        {
            List<BuildingPrefabData> result = new List<BuildingPrefabData>();
            foreach (var prefab in buildingPrefabCache.Values)
            {
                // Filter by function, floor type, fitting within maxLength, and not exceeding usage limit on this plot.
                // Note: 'material' parameter is not used in this filter logic.
                if (prefab.Function == function &&
                    prefab.FloorType == floorType &&
                    prefab.Length <= maxLength &&
                    GetUsageCount(landData, prefab.ID) < 3) // Max 3 instances of the same building type per plot.
                {
                    result.Add(prefab);
                }
            }
            return result;
        }

        // Instantiates a building prefab by its ID at the given position, rotation, and under the specified parent transform.
        GameObject InstantiateBuilding(int prefabId, Vector3 position, Quaternion rotation, Transform parent)
        {
            // Validate prefabId against the bounds of the 'buildings' array.
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

        // Removes all spawned buildings from all land plots in 'landArray'.
        // Also resets building usage counts and summaries for all cached land data,
        // and clears global tracking sets for special and cultural buildings.
        public void RemoveAllBuildings()
        {
            foreach (var landGameObject in landArray)
            {
                if (landGameObject != null)
                {
                    // Destroy all child GameObjects of the land plot, which are the spawned buildings.
                    for (int i = landGameObject.transform.childCount - 1; i >= 0; i--)
                    {
                        // Ensure not to destroy the land plot itself if it's somehow a child of itself (unlikely).
                        if (landGameObject.transform.GetChild(i) != landGameObject.transform)
                        {
                            Destroy(landGameObject.transform.GetChild(i).gameObject);
                        }
                    }
                }
            }

            // Clear usage counts and summaries from the cached land data.
            List<int> keys = new List<int>(emptyLandCache.Keys);
            foreach (int key in keys)
            {
                EmptyLandData landData = emptyLandCache[key];
                landData.BuildingUsageCount.Clear();
                landData.Summary = ""; // Clear the summary.
                emptyLandCache[key] = landData; // Re-assign because EmptyLandData is a struct.
            }

            // Reset global tracking sets for special (S=1) and cultural buildings.
            usedSpecialBuildings.Clear();
            usedCulturalBuildings.Clear();
            Debug.Log("All spawned buildings and plot summaries cleared. All building usage counts reset.");
        }

        // Helper to get the current usage count of a specific building ID on a given land plot.
        private int GetUsageCount(EmptyLandData landData, int buildingId)
        {
            if (landData.BuildingUsageCount.TryGetValue(buildingId, out int count))
            {
                return count;
            }
            return 0; // If not found, it means it hasn't been used on this plot yet.
        }

        // Helper to increment the usage count of a specific building ID on a given land plot.
        private void IncrementUsageCount(EmptyLandData landData, int buildingId)
        {
            if (landData.BuildingUsageCount.ContainsKey(buildingId))
            {
                landData.BuildingUsageCount[buildingId]++;
            }
            else
            {
                landData.BuildingUsageCount[buildingId] = 1; // First use on this plot.
            }
        }
        // Stores a design summary string for a specific land plot, identified by its ID.
        public void StoreLandSummary(string emptyID, string summary)
        {
            if (!int.TryParse(emptyID, out int landId))
            {
                Debug.LogError($"StoreLandSummary: Invalid land ID format: {emptyID}");
                return;
            }

            if (emptyLandCache.TryGetValue(landId, out EmptyLandData landData))
            {
                // EmptyLandData is a struct, so to modify it in the dictionary,
                // we get a copy, modify the copy, and then put the copy back.
                EmptyLandData updatedLandData = landData;
                updatedLandData.Summary = summary;
                emptyLandCache[landId] = updatedLandData;
                // Debug.Log($"Summary stored for land plot {landId}: {summary}");
            }
            else
            {
                Debug.LogError($"StoreLandSummary: Land configuration not found for ID: {landId}. Cannot store summary.");
            }
        }
        // Retrieves the design summary for a specific land plot by its ID.
        public string GetLandSummary(int landId)
        {
            if (emptyLandCache.TryGetValue(landId, out EmptyLandData landData))
            {
                if (!string.IsNullOrEmpty(landData.Summary))
                {
                    return landData.Summary;
                }
                else
                {
                    return "No design summary is available for this plot yet.";
                }
            }
            return $"Information for land plot ID {landId} not found.";
        }
        // Struct to hold data for a building prefab, typically loaded from the 'BuildingPrefab' database table.
        private struct BuildingPrefabData
        {
            public int ID;          // Unique identifier for the building prefab.
            public float Length;    // Length (one dimension) of the building.
            public float Width;     // Width (other dimension) of the building.
            public int Function;    // Categorical function of the building (e.g., residential, commercial).
            public int FloorType;   // Type of flooring or general construction style (e.g., low-rise, high-rise).
            public int Material;    // Primary material type used for the building.
        }

        // Struct to hold data for an empty land plot, typically loaded from the 'emptyland' database table.
        private struct EmptyLandData
        {
            public int ID;             // Unique identifier for the land plot.
            public float Length;       // Length of the land plot.
            public float Width;        // Width of the land plot.
            public Vector3 Position;   // Starting position (e.g., a corner or center) of the plot from the database.
            public float RotationY;    // Y-axis rotation of the plot as defined in the database.
            public int T;              // Special type flag 'T' from the database, influencing generation logic.
            public int S;              // Special type flag 'S' from the database, influencing generation logic (e.g., S=1 plots).
            // Tracks how many times each building ID (from BuildingPrefabData) has been spawned on this specific plot.
            public Dictionary<int, int> BuildingUsageCount;
            // A brief design summary or notes for this plot, potentially generated by an AI or user.
            public string Summary;
        }
    }
}