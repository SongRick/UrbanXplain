using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.IO;
using Newtonsoft.Json.Linq;

namespace UrbanXplain
{
    // Manages communication with the DeepSeek AI API to obtain urban planning suggestions.
    // It then applies these suggestions to the scene using BuildingSpawnerJson,
    // parses the API response, and updates relevant plot components (ChildColorToggler).
    public class DeepSeekAPI : MonoBehaviour
    {
        // Reference to the BuildingSpawnerJson script, responsible for actual building instantiation.
        public BuildingSpawnerJson buildingSpawnerJson;

        [Header("API Settings")]
        // API key for authenticating with the DeepSeek service.
        private string apiKey = "sk-50b19a4873f447d5ba0e50c11a836765"; // Replace with your actual API key if different.
        // The specific DeepSeek model to be used for generating responses.
        private string modelName = "deepseek-reasoner";
        // The endpoint URL for the DeepSeek chat completions API.
        private string apiUrl = "https://api.deepseek.com/v1/chat/completions";

        [Header("Dialogue Settings")]
        // Controls the randomness of the AI's output. Lower values make it more deterministic.
        [Range(0, 1)] public float temperature = 0.5f;
        // Maximum number of tokens the API can generate in a single response.
        private int maxTokens = 16384;
        // Specifies the desired response format from the API, set to "json_object" for structured JSON output.
        private string responseFormatType = "json_object"; // Named 'strType' in original, clarified name.
        // Array of ChildColorToggler components, one for each land plot, used to update plot properties
        // like function and energy consumption based on API response.
        public ChildColorToggler[] colorTogglerArray;


        // Represents the AI persona and its detailed instructions for urban planning.
        [System.Serializable]
        public class NPCCharacter
        {
            public string name = "urban planning expert proficient in Unity";
            [TextArea(3, 10)]
            // This is the system prompt that guides the AI's behavior, constraints,
            // and the expected JSON output format for urban planning tasks.
            // It includes information about the city, lot data (emptyLand.csv format), and specific design rules.
            public string personalityPrompt = "In a Unity urban scene, there are multiple rectangular empty lots (see emptyLand.csv) and building prefab models. You need to analyze and understand the orientation of these empty lots within the city based on city and lot information. The user will provide you with urban planning requirements. Please plan the functional zoning, building floor type, and building material for all empty lots. For each lot, provide a summary explanation (why this lot is designed this way and how this design meets user requirements). Design a plan that meets the user's requirements.\r\n\r\nConstraints:\r\n1. Residential buildings cannot be super high-rise.\r\n2. Commercial buildings cannot be mid-rise.\r\n3. Public buildings cannot be low-rise.\r\n4. 172x172 extra-large lots can only accommodate extra-large buildings, which must be high-rise with glass curtain wall materials, such as super shopping malls, exhibition centers, and cultural art centers.\r\n\r\nGuidelines:\r\n1. Regarding orientation: the positive X-axis is East, and the positive Z-axis is North.\r\n2. The city is rectangular. The southwest corner coordinates are (X:-600, Y:0, Z:-450). The northeast corner coordinates are (X:600, Y:0, Z:450). (Assuming Y is the vertical axis and 0 represents ground level).\r\n3. The city center is at coordinates (0,0,0).\r\n\r\nFinally, output the plan as a JSON array. Output ONLY the JSON, with no other content. You must process all 43 lots from emptyLand.csv, ensuring the output array contains 43 JSON objects. The JSON format for a single empty lot is as follows:\r\n{\r\n\"EmptyID\": \"<Lot ID>\",\r\n\"Function\": \"<1: Residential (Apartments, Villas, High-rise residential buildings, etc.) 2: Commercial (Street-front shops, Malls, Office buildings, Hotels, etc.) 3: Public (Schools, City Hall, Hospitals, Libraries, Community centers, etc.) 4: Cultural & Entertainment (Museums, Concert halls, Science centers, Cinemas, Stadiums, etc.)>\",\r\n\"FloorType\": \"<1: Low-rise (1-3 stories) 2: Mid-rise (4-8 stories) 3: High-rise (9-30 stories) 4: Super high-rise (30+ stories)>\",\r\n\"Material\": \"<1: Glass Curtain Wall 2: Concrete>\",\r\n\"EnergyConsumption\": \"<1-100 (Integer; 1=most efficient, 100=least). Assign based on material, floor type, and function. Generally, glass curtain walls, taller buildings (high/super high-rise), and large commercial/public facilities tend towards higher values (e.g., 60-90). Concrete, residential, and lower/mid-rise buildings tend towards lower values (e.g., 15-45).>\",\r\n\"Summary\": \"<Explain why this lot is designed this way and how this design meets user requirements (VERY IMPORTANT!). Over 40 words. Do not mention the specific dimensions of the empty lot. Example: 'As an extra-large lot in the southwest, it's designed as a high-rise shopping mall with a glass curtain wall, fulfilling commercial needs and adhering to extra-large lot restrictions. Its proximity to main roads enhances regional commercial vitality and supports shopping needs within a 15-minute living circle.'>\"\r\n}\r\n\r\nAfter outputting, perform a self-check: Verify the array length is 43, IDs are sequential and unique, EnergyConsumption is an integer between 1 and 100, and each lot's Summary includes the design rationale and explanation of how it meets user requirements. If not, regenerate.\r\n\r\nemptyLand.csv:\r\nID,Length,Width,StartPosX,StartPosY,StartPosZ,RotationY,EndPosX,EndPosY,EndPosZ\r\n1,172,72,-564,0.151,-286,270,-636,0.151,-114\r\n2,100,36,-564,0.151,-114,270,-600,0.151,-14\r\n3,272,72,-564,0.151,14,270,-636,0.151,286\r\n4,172,72,-464,0.151,-386,270,-536,0.151,-214\r\n5,172,72,-364,0.151,-386,270,-436,0.151,-214\r\n6,72,72,-464,0.151,-188.85,270,-536,0.151,-116.85\r\n7,72,72,-364,0.151,-188.85,270,-436,0.151,-116.85\r\n8,172,172,-364,0.151,-86,270,-536,0.151,86\r\n9,272,72,-436,0.151,114,0,-164,0.151,186\r\n10,72,72,-536,0.151,214,0,-464,0.151,286\r\n11,112,36,-436,0.151,214,0,-324,0.151,250\r\n12,212,72,-536,0.151,314,0,-324,0.151,386\r\n13,272,72,-436,0.151,414,0,-164,0.151,486\r\n14,272,72,-164,0.151,-414,180,-436,0.151,-486\r\n15,272,72,-264,0.151,-286,270,-336,0.151,-14\r\n16,172,72,-64,0.151,-314,180,-236,0.151,-386\r\n17,172,72,-64,0.151,-214,180,-236,0.151,-286\r\n18,272,72,136,0.151,-114,180,-136,0.151,-186\r\n19,186,72,-50,0.151,-14,180,-236,0.151,-86\r\n20,186,72,-236,0.151,14,0,-50,0.151,86\r\n21,272,72,136,0.151,186,180,-136,0.151,114\r\n22,72,72,-236,0.151,214,0,-164,0.151,286\r\n23,72,72,-136,0.151,214,0,-64,0.151,286\r\n24,72,72,-136,0.151,314,0,-64,0.151,386\r\n25,212,72,236,0.151,-314,180,24,0.151,-386\r\n26,212,72,236,0.151,-214,180,24,0.151,-286\r\n27,186,72,236,0.151,-14,180,50,0.151,-86\r\n28,186,72,50,0.151,14,0,236,0.151,86\r\n29,172,172,64,0.151,214,0,236,0.151,386\r\n30,272,72,436,0.151,-414,180,164,0.151,-486\r\n31,272,72,264,0.151,-14,90,336,0.151,-286\r\n32,272,72,164,0.151,114,0,436,0.151,186\r\n33,72,72,264,0.151,214,0,336,0.151,286\r\n34,72,72,264,0.151,314,0,336,0.151,386\r\n35,272,72,164,0.151,414,0,436,0.151,486\r\n36,212,72,364,0.151,-174,90,436,0.151,-386\r\n37,212,72,464,0.151,-174,90,536,0.151,-386\r\n38,172,72,364,0.151,86,90,436,0.151,-86\r\n39,172,72,464,0.151,86,90,536,0.151,-86\r\n40,172,72,364,0.151,214,0,536,0.151,286\r\n41,172,72,364,0.151,314,0,536,0.151,386\r\n42,272,72,564,0.151,-14,90,636,0.151,-286\r\n43,272,72,564,0.151,286,90,636,0.151,14\r\n";
        }
        // Instance of the NPCCharacter configuration, accessible in the Inspector.
        [SerializeField] public NPCCharacter npcCharacter;

        // Text area in the Inspector for pasting a JSON string to test parsing and spawning logic
        // without making an actual API call. This is useful for debugging and rapid iteration.
        [TextArea(3, 10)]
        public string TestJsonInput = @"";

        private void Start()
        {
            // Waits for BuildingSpawnerJson to complete its database loading,
            // then processes TestJsonInput if it's not empty.
            StartCoroutine(ExecuteTestJsonProcessing());
        }

        // Coroutine to delay test JSON processing until dependencies are ready.
        private IEnumerator ExecuteTestJsonProcessing()
        {
            // Wait until BuildingSpawnerJson is assigned and has finished loading its data.
            yield return new WaitUntil(() => buildingSpawnerJson != null && buildingSpawnerJson.IsDatabaseLoaded);
            ProcessTestJsonData(); // Renamed from TestSpawnerJson for clarity.
        }

        // Processes the JSON string provided in the TestJsonInput field.
        void ProcessTestJsonData() // Renamed from TestSpawnerJson for clarity.
        {
            if (!string.IsNullOrEmpty(TestJsonInput))
            {
                Debug.Log("Using Test JSON Input for city planning.");
                List<EmptyLandData> landDataList = null; // Changed variable name for clarity.
                try
                {
                    // Attempt to deserialize the test JSON string into a list of EmptyLandData objects.
                    landDataList = JsonConvert.DeserializeObject<List<EmptyLandData>>(TestJsonInput);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error deserializing TestJsonInput: {e.Message}");
                    landDataList = null; // Ensure it's null on error.
                }

                if (landDataList != null)
                {
                    // If deserialization is successful, process the land data.
                    ProcessLandData(landDataList);
                }
            }
            else
            {
                Debug.Log("TestJsonInput is empty. No buildings will be generated from test JSON.");
            }
            // Initialize color togglers for all plots after processing, regardless of whether test JSON was used.
            InitializeAllColorTogglers();
        }

        // Public method to send a user message to the DeepSeek API and get a planning response.
        public void SendMessageToDeepSeek(string userMessage, DialogueCallback callback)
        {
            string systemPrompt = npcCharacter.personalityPrompt;
            StartCoroutine(PostRequest(userMessage, callback, systemPrompt));
        }

        // Coroutine to handle the HTTP POST request to the DeepSeek API.
        IEnumerator PostRequest(string userMessage, DialogueCallback callback, string systemPrompt)
        {
            // Construct the list of messages for the API request, including system and user roles.
            List<Message> messages = new List<Message>
            {
                new Message { role = "system", content = systemPrompt },
                new Message { role = "user", content = userMessage }
            };

            // Create the request body object using the ChatRequest structure defined in this script.
            // This uses the top-level 'type' field for specifying JSON object response format,
            // as per the original script's structure.
            ChatRequest requestBody = new ChatRequest
            {
                model = modelName,
                messages = messages,
                temperature = temperature,
                max_tokens = maxTokens,
                type = responseFormatType // Use the 'type' field directly for "json_object".
            };

            // Serialize the request body to a JSON string.
            string jsonBody = JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Debug.Log("Request JSON Body: " + jsonBody); // Log the request body for debugging.

            // Create and send the UnityWebRequest.
            UnityWebRequest request = CreateWebRequest(jsonBody);
            yield return request.SendWebRequest();

            // Handle API errors, including rate limiting (HTTP 429).
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                if (request.responseCode == 429) // HTTP 429: Too Many Requests (Rate limit).
                {
                    Debug.LogWarning("Rate limit hit. Retrying in 5 seconds...");
                    yield return new WaitForSeconds(5);
                    StartCoroutine(PostRequest(userMessage, callback, systemPrompt)); // Retry the request.
                }
                else
                {
                    Debug.LogError($"API Error: {request.responseCode} - {request.error}\nResponse: {request.downloadHandler.text}");
                    callback?.Invoke($"API request failed: {request.downloadHandler.text}", false);
                }
                request.Dispose(); // Dispose of the request object.
                yield break;       // Exit the coroutine on error.
            }

            // Log the raw API response for debugging.
            Debug.Log("Response JSON: " + request.downloadHandler.text);
            // Store the response in TestJsonInput, useful for debugging or re-testing without new API calls.
            TestJsonInput = request.downloadHandler.text;
            // Parse the API response to extract the list of land data.
            List<EmptyLandData> landDataList = ParseApiResponse(request.downloadHandler.text); // Renamed variable for clarity.

            if (landDataList != null)
            {
                ProcessLandData(landDataList);
                InitializeAllColorTogglers(); // Initialize togglers after applying new data.
                callback?.Invoke("Planning data parsed and applied successfully.", true);
            }
            else
            {
                callback?.Invoke("Failed to parse planning data from API response.", false);
            }
            request.Dispose(); // Dispose of the request object.
        }

        // Parses the JSON response string from the API to extract a list of EmptyLandData objects.
        private List<EmptyLandData> ParseApiResponse(string jsonResponse) // Renamed from ParseResponse for clarity.
        {
            try
            {
                // Parse the entire response as a JObject to navigate its structure.
                var responseObj = JObject.Parse(jsonResponse);
                // Extract the 'content' field, which is expected to contain the JSON array of land data.
                string content = responseObj["choices"]?[0]?["message"]?["content"]?.ToString();

                if (string.IsNullOrEmpty(content))
                {
                    // If content is missing, check if the main response object contains an error.
                    if (responseObj["error"] != null)
                    {
                        Debug.LogError($"API returned an error in the main response object: {responseObj["error"]}");
                        return null;
                    }
                    Debug.LogError("JSON parsing error: 'content' field is missing or empty in the API response.");
                    return null;
                }

                // Remove common Markdown formatting (like ```json ... ```) if the API wraps JSON in it.
                content = content.Replace("```json", "").Replace("```", "").Trim();

                // Ensure the extracted content string is a valid JSON array.
                // If not, attempt to find a JSON array embedded within the content string.
                if (!content.StartsWith("[") || !content.EndsWith("]"))
                {
                    int jsonStart = content.IndexOf('[');
                    int jsonEnd = content.LastIndexOf(']') + 1; // +1 to include the closing bracket.

                    if (jsonStart != -1 && jsonEnd != -1 && jsonEnd > jsonStart)
                    {
                        // If an array is found, extract it.
                        content = content.Substring(jsonStart, jsonEnd - jsonStart);
                    }
                    else
                    {
                        Debug.LogError("JSON parsing error: Extracted content does not appear to be a valid JSON array nor does it contain one. Content: " + content);
                        return null;
                    }
                }
                // Deserialize the cleaned content string into a list of EmptyLandData objects.
                return JsonConvert.DeserializeObject<List<EmptyLandData>>(content);
            }
            catch (JsonReaderException readerEx) // Catch errors during the initial parsing of the jsonResponse string.
            {
                Debug.LogError($"JSON Reader Error (likely malformed response string from API): {readerEx.Message}\nOriginal Response: {jsonResponse}");
                return null;
            }
            catch (JsonException jsonEx) // Catch errors during the deserialization of the 'content' part.
            {
                Debug.LogError($"JSON Deserialization Error (parsing 'content' field): {jsonEx.Message}\nContent being parsed: {(jsonResponse.Length > 500 ? jsonResponse.Substring(0, 500) + "..." : jsonResponse)}");
                return null;
            }
            catch (System.Exception e) // Catch any other general exceptions during parsing.
            {
                Debug.LogError($"General error during JSON parsing: {e.Message}\nOriginal Response: {jsonResponse}");
                return null;
            }
        }

        // Processes the list of land data received from the API (or from test input).
        // This involves instructing BuildingSpawnerJson to spawn buildings and updating ChildColorTogglers
        // with new function and energy consumption data.
        private void ProcessLandData(List<EmptyLandData> landDataList)
        {
            if (buildingSpawnerJson == null)
            {
                Debug.LogError("BuildingSpawnerJson reference is not set in DeepSeekAPI. Cannot process land data.");
                return;
            }
            // Clear any existing buildings before spawning new ones based on the received data.
            buildingSpawnerJson.RemoveAllBuildings();

            if (landDataList == null || landDataList.Count == 0)
            {
                Debug.LogWarning("ProcessLandData received a null or empty list of land data. No new buildings will be spawned.");
                return;
            }

            // Iterate through each land data entry received.
            foreach (var landInfo in landDataList) // Renamed 'land' to 'landInfo' for clarity
            {
                if (landInfo == null)
                {
                    Debug.LogWarning("Encountered a null land entry in the provided landDataList. Skipping this entry.");
                    continue;
                }

                // Store the summary for the land plot using BuildingSpawnerJson.
                buildingSpawnerJson.StoreLandSummary(landInfo.EmptyID, landInfo.Summary);
                // Instruct BuildingSpawnerJson to spawn buildings on the plot based on its properties.
                buildingSpawnerJson.SpawnBuilding(
                    landInfo.EmptyID,
                    landInfo.Function,
                    landInfo.FloorType,
                    landInfo.Material
                );

                // Update the corresponding ChildColorToggler with function and energy consumption data.
                if (int.TryParse(landInfo.EmptyID, out int landIdNumeric))
                {
                    int togglerIndex = landIdNumeric - 1; // Land IDs are 1-based, array indices are 0-based.
                    if (colorTogglerArray != null && togglerIndex >= 0 && togglerIndex < colorTogglerArray.Length)
                    {
                        ChildColorToggler currentToggler = colorTogglerArray[togglerIndex];
                        if (currentToggler != null)
                        {
                            // Parse and set land function.
                            if (int.TryParse(landInfo.Function, out int functionNumeric))
                            {
                                currentToggler.landFunction = functionNumeric;
                            }
                            else
                            {
                                Debug.LogWarning($"Could not parse Function '{landInfo.Function}' for land ID {landInfo.EmptyID}. Setting landFunction to 0 (default/unknown).");
                                currentToggler.landFunction = 0; // Default or error value.
                            }

                            // Parse and set land energy consumption, ensuring it's within the valid range (1-100).
                            if (int.TryParse(landInfo.EnergyConsumption, out int energyNumeric))
                            {
                                if (energyNumeric >= 1 && energyNumeric <= 100)
                                {
                                    currentToggler.landEnergyConsumption = energyNumeric;
                                }
                                else
                                {
                                    Debug.LogWarning($"EnergyConsumption value '{landInfo.EnergyConsumption}' for land ID {landInfo.EmptyID} is out of range (1-100). Setting to 0 (default/unknown).");
                                    currentToggler.landEnergyConsumption = 0; // Default or error value if out of range.
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"Could not parse EnergyConsumption '{landInfo.EnergyConsumption}' for land ID {landInfo.EmptyID}. Setting landEnergyConsumption to 0 (default/unknown).");
                                currentToggler.landEnergyConsumption = 0; // Default or error value.
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"ChildColorToggler at index {togglerIndex} (for land ID {landIdNumeric}) is null.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Toggler index {togglerIndex} (for land ID {landIdNumeric}) is out of bounds for colorTogglerArray (length {colorTogglerArray?.Length ?? 0}).");
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not parse EmptyID '{landInfo.EmptyID}' to a numeric ID for toggler lookup.");
                }
            }
        }

        // Initializes all ChildColorToggler components in the colorTogglerArray.
        // This is typically called after new data is processed to ensure togglers
        // correctly cache their renderers for subsequent color manipulation.
        private void InitializeAllColorTogglers()
        {
            if (colorTogglerArray == null)
            {
                Debug.LogWarning("colorTogglerArray is null. Cannot initialize togglers.");
                return;
            }
            for (int i = 0; i < colorTogglerArray.Length; i++)
            {
                if (colorTogglerArray[i] != null)
                {
                    colorTogglerArray[i].PublicInitializeRenderers();
                }
                else
                {
                    Debug.LogWarning($"ChildColorToggler at index {i} in colorTogglerArray is null.");
                }
            }
        }

        // Helper method to create and configure a UnityWebRequest for the API call.
        private UnityWebRequest CreateWebRequest(string jsonBody)
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            return request;
        }


        // Data structure representing a single land plot's planning information received from the API.
        [System.Serializable]
        public class EmptyLandData
        {
            public string EmptyID;           // Unique identifier for the empty land plot.
            public string Function;          // Functional zoning category (e.g., "1" for Residential).
            public string FloorType;         // Building floor type category (e.g., "1" for Low-rise).
            public string Material;          // Building material category (e.g., "1" for Glass Curtain Wall).
            public string EnergyConsumption; // Estimated energy consumption (1-100).
            public string Summary;           // AI-generated design rationale and summary for the plot.
        }

        // Delegate for callbacks used after an API request, indicating success/failure and providing content.
        public delegate void DialogueCallback(string content, bool isSuccess);

        // Data structure for the chat request body sent to the DeepSeek API.
        // This structure includes a top-level 'type' field for specifying JSON object response format.
        [System.Serializable]
        private class ChatRequest
        {
            public string model;
            public List<Message> messages;
            public float temperature;
            public int max_tokens;
            public string type; // This field is used to request "json_object" format from the API.
        }

        // Data structure representing a single message in the API request (can be system or user role).
        [System.Serializable]
        public class Message
        {
            public string role;    // Role of the message sender (e.g., "system", "user").
            public string content; // Content of the message.
        }
    }

#if UNITY_EDITOR
    // Custom editor for the DeepSeekAPI component to add utility buttons in the Unity Inspector.
    [UnityEditor.CustomEditor(typeof(DeepSeekAPI))]
    public class DeepSeekApiEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector fields.
            base.OnInspectorGUI();
            // Get a reference to the DeepSeekAPI script instance being inspected.
            DeepSeekAPI deepSeekApiScript = (DeepSeekAPI)target;

            // Add a button to easily clear the TestJsonInput field in the Inspector.
            if (GUILayout.Button("Clear TestJsonInput"))
            {
                deepSeekApiScript.TestJsonInput = ""; // Clear the string.
                // Mark the object as "dirty" so Unity knows to save the change.
                UnityEditor.EditorUtility.SetDirty(deepSeekApiScript);
            }
        }
    }
#endif
}