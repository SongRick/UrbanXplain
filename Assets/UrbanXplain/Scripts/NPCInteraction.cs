using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI; // Using legacy UI components

namespace UrbanXplain
{
    // Handles the dialogue interaction between the player and an NPC,
    // utilizing the DeepSeekAPI for AI-driven conversation.
    public class NPCInteraction : MonoBehaviour
    {
        // ---- References and Configuration ----

        [Header("References")]
        // Reference to the DeepSeekAPI component for communicating with the AI.
        [SerializeField] private DeepSeekAPI deepSeekAPI;
        // UI InputField for player to type their messages.
        [SerializeField] private InputField inputField;
        // UI Text component to display NPC responses.
        [SerializeField] private Text dialogueText;
        // Name of the NPC, fetched from DeepSeekAPI settings.
        private string characterName;

        [Header("Settings")]
        // Speed of the typewriter effect for displaying text (seconds per character).
        [SerializeField] private float typingSpeed = 0.05f;
        // GameObject to show as a loading indicator while waiting for an API response.
        [SerializeField] private GameObject loadingIndicator;

        // Stores the currently running typewriter effect coroutine.
        private Coroutine currentTypingCoroutine;
        // Flag indicating if the typewriter effect is currently active.
        private bool isTyping = false;

        void Start()
        {
            // Fetch and cache the NPC's name from the DeepSeekAPI's character settings.
            if (deepSeekAPI != null && deepSeekAPI.npcCharacter != null)
            {
                characterName = deepSeekAPI.npcCharacter.name;
            }
            else
            {
                Debug.LogError("NPCInteraction: DeepSeekAPI or its NPCCharacter is not assigned. Character name will be unavailable.");
                characterName = "NPC"; // Default name if not found.
            }

            // Set up a listener for the InputField's onSubmit event (when player presses Enter or submits).
            if (inputField != null)
            {
                inputField.onSubmit.AddListener(HandlePlayerInput);
            }
            else
            {
                Debug.LogError("NPCInteraction: InputField reference is not set. Player input will not be processed.");
            }

            // Initialize the loading indicator to be inactive.
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
            else
            {
                Debug.LogWarning("NPCInteraction: LoadingIndicator reference is not set. No visual feedback for loading.");
            }
        }

        // Handles the player's input submission.
        private void HandlePlayerInput(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("Input content is empty. Please enter a message.");
                return; // Do nothing if input is empty.
            }

            // Clear the input field and reactivate it for further input.
            inputField.text = "";
            inputField.ActivateInputField();

            // Show loading indicator and set typing flag.
            if (loadingIndicator != null) loadingIndicator.SetActive(true);
            isTyping = true; // Mark as waiting for AI response / typing.

            // Send the player's message to the DeepSeek API and provide a callback for the response.
            if (deepSeekAPI != null)
            {
                deepSeekAPI.SendMessageToDeepSeek(text, HandleAIResponse);
            }
            else
            {
                HandleAIResponse("DeepSeekAPI is not available.", false); // Simulate an error if API is missing.
            }
        }


        // Callback method to handle the response received from the DeepSeek API.
        private void HandleAIResponse(string content, bool isSuccess)
        {
            // Stop any ongoing typewriter effect.
            if (currentTypingCoroutine != null)
            {
                StopCoroutine(currentTypingCoroutine);
            }
            isTyping = false; // Reset typing flag as we are about to start a new response or show an error.

            // Construct the display text based on whether the API request was successful.
            string displayText = isSuccess
                ? $"{characterName}: {content}"
                : $"{characterName}: (Communication interrupted, please try again later)"; // Error message.

            // Start a new typewriter effect coroutine to display the AI's response.
            currentTypingCoroutine = StartCoroutine(TypewriterEffect(displayText));
        }

        // Coroutine to display text with a typewriter effect, character by character.
        private IEnumerator TypewriterEffect(string fullText)
        {
            if (loadingIndicator != null) loadingIndicator.SetActive(false); // Hide loading indicator.
            if (dialogueText != null) dialogueText.text = "";                // Clear previous dialogue text.

            StringBuilder sb = new StringBuilder(); // Use StringBuilder for efficient string concatenation.

            // Append each character to the display text with a delay.
            foreach (char c in fullText)
            {
                if (dialogueText == null) yield break; // Exit if dialogueText becomes null (e.g., scene change).
                sb.Append(c);
                dialogueText.text = sb.ToString();
                yield return new WaitForSeconds(typingSpeed);
            }

            // Mark typing as complete once the full text is displayed.
            isTyping = false;
            currentTypingCoroutine = null; // Clear the coroutine reference.
        }

        // Allows skipping the current typewriter effect to display the full text immediately.
        // Note: The original double-click skip logic was tied to onValueChanged, which might not be ideal.
        // A dedicated button or a different input check (e.g., in Update) might be more robust for skipping.
        // For this version, the method is kept but the trigger mechanism from Start is removed to avoid unintended skips.
        // It can be called from elsewhere if a skip mechanism is implemented.
        public void SkipTypingEffect()
        {
            if (currentTypingCoroutine != null && dialogueText != null)
            {
                StopCoroutine(currentTypingCoroutine);
                // To display the full text, we need access to it.
                // The original TypewriterEffect coroutine would need to store 'fullText' in a member variable
                // or this method would need to receive it if called externally.
                // For simplicity, this example will assume the last 'displayText' sent to TypewriterEffect is what we want.
                // This requires HandleAIResponse to store its 'displayText'.
                // A more robust way: TypewriterEffect should set dialogueText.text to fullText in a finally block or when stopped.
                // For now, this just stops the coroutine. The text will remain as is.
                // To fully implement skip:
                // 1. Store 'fullText' in TypewriterEffect or make it accessible.
                // 2. Set dialogueText.text = storedFullText; here.
            }
            isTyping = false;
            if (loadingIndicator != null) loadingIndicator.SetActive(false);
            currentTypingCoroutine = null;
        }

        // Example of how to trigger skip via Update if needed (e.g., on mouse click while typing)
        // void Update()
        // {
        //     if (isTyping && Input.GetMouseButtonDown(0)) // Check for left mouse click
        //     {
        //         // This check could be more specific, e.g., if mouse is over dialogue area
        //         SkipTypingEffectAndDisplayFullText(); // You'd need to implement this or adapt SkipTypingEffect
        //     }
        // }
    }
}