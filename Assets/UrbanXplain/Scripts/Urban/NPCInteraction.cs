using System.Collections;
using System.Text;
using UnityEngine;
using TMPro;

namespace UrbanXplain
{
    public class NPCInteraction : MonoBehaviour
    {
        // ---- 引用 ----
        [Header("References")]
        [SerializeField] private DeepSeekAPI deepSeekAPI;
        [SerializeField] private TextMeshProUGUI dialogueText;
        // **新增**: 引用UI控制器
        [SerializeField] private InstructionInputController inputController;

        // ---- 配置 ----
        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.05f;

        // ---- 内部状态 ----
        private string characterName;
        private Coroutine currentTypingCoroutine;
        private bool isTyping = false;
        private string fullTextForSkipping;

        private void Awake()
        {
            if (deepSeekAPI != null && deepSeekAPI.npcCharacter != null)
            {
                characterName = deepSeekAPI.npcCharacter.name;
            }
            else
            {
                Debug.LogError("NPCInteraction: DeepSeekAPI or its NPCCharacter is not assigned.");
                characterName = "NPC";
            }
        }

        private void OnEnable()
        {
            // **核心**: 当脚本启用时，订阅UI控制器的事件
            if (inputController != null)
            {
                inputController.OnInstructionSubmitted += HandleInstructionSubmitted;
            }
        }

        private void OnDisable()
        {
            // **核心**: 当脚本禁用时，取消订阅，防止内存泄漏
            if (inputController != null)
            {
                inputController.OnInstructionSubmitted -= HandleInstructionSubmitted;
            }
        }

        // **核心**: 这是响应UI事件的方法
        private void HandleInstructionSubmitted(string instruction)
        {
            // 1. 通知UI控制器进入“处理中”状态
            inputController.SetProcessingState(true);

            // 2. 发送消息给API
            if (deepSeekAPI != null)
            {
                deepSeekAPI.SendMessageToDeepSeek(instruction, HandleAIResponse);
            }
            else
            {
                HandleAIResponse("DeepSeekAPI is not available.", false);
            }
        }

        private void HandleAIResponse(string content, bool isSuccess)
        {
            // 3. 收到回复后，通知UI控制器解除“处理中”状态
            inputController.SetProcessingState(false);

            if (currentTypingCoroutine != null) StopCoroutine(currentTypingCoroutine);
            isTyping = false;

            string displayText = isSuccess
                ? $"{characterName}: {content}"
                : $"{characterName}: (Communication interrupted, please try again later)";

            fullTextForSkipping = displayText;
            currentTypingCoroutine = StartCoroutine(TypewriterEffect(displayText));
        }

        // ---- 打字机效果和跳过逻辑 (这部分属于业务表现，留在这里是合适的) ----
        private IEnumerator TypewriterEffect(string fullText)
        {
            isTyping = true;
            if (dialogueText != null) dialogueText.text = "";

            StringBuilder sb = new StringBuilder();
            foreach (char c in fullText)
            {
                if (dialogueText == null) yield break;
                sb.Append(c);
                dialogueText.text = sb.ToString();
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
            currentTypingCoroutine = null;
            fullTextForSkipping = null;
        }

        public void SkipTypingEffect()
        {
            if (isTyping && currentTypingCoroutine != null)
            {
                StopCoroutine(currentTypingCoroutine);
                if (dialogueText != null && !string.IsNullOrEmpty(fullTextForSkipping))
                {
                    dialogueText.text = fullTextForSkipping;
                }
                isTyping = false;
                currentTypingCoroutine = null;
                fullTextForSkipping = null;
            }
        }

        void Update()
        {
            if (isTyping && Input.GetMouseButtonDown(0))
            {
                SkipTypingEffect();
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}