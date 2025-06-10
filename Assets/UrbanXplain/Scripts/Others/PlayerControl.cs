using UnityEngine;

namespace UrbanXplain
{
    // Controls player movement (horizontal, vertical, sprint) and camera rotation (look up/down, turn left/right).
    // Requires a CharacterController component on the same GameObject and a Transform reference for the camera.
    public class PlayerControl : MonoBehaviour
    {
        // Base movement speed of the player.
        private float speed = 40.0f;
        // Current vertical rotation of the camera (pitch).
        private float xRotation = 0f;
        // Transform of the camera GameObject, typically a child of the player, used for looking up and down.
        public Transform cam;
        // Reference to the CharacterController component for physics-based movement.
        private CharacterController charController;
        // Multiplier applied to the base speed when sprinting (holding Left Shift).
        private float sprintMultiplier = 5f;
        // Speed for vertical movement (flying up/down with E/Q keys).
        private float verticalSpeed = 10.0f;
        // Flag to enable or disable player movement and camera control.
        private bool allowMovement = true;

        void Start()
        {
            // Get the CharacterController component attached to this GameObject.
            charController = GetComponent<CharacterController>();
            if (charController == null)
            {
                Debug.LogError("PlayerControl: CharacterController component not found on this GameObject.", this);
                enabled = false; // Disable script if CharacterController is missing.
            }
            if (cam == null)
            {
                Debug.LogError("PlayerControl: Camera transform (cam) is not assigned in the Inspector.", this);
                // Movement might still work, but camera control will fail.
            }
        }

        void Update()
        {
            if (Time.timeScale == 0f)
                return;
            // Only process movement and camera controls if allowed.
            if (allowMovement)
            {
                PlayerMovement();
                CameraMovement();
            }
        }

        // Handles horizontal (WASD) and vertical (E/Q for flying) player movement.
        void PlayerMovement()
        {
            // Get horizontal and vertical input axes (typically WASD or arrow keys).
            // Movement is relative to the player's current orientation (transform.right and transform.forward).
            Vector3 moveDirection = (transform.right * GlobalInputManager.GetGameAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical"));

            // Handle vertical "flying" movement.
            float verticalInput = 0f;
            if (GlobalInputManager.GetGameKey(KeyCode.E)) // Move up
            {
                verticalInput = verticalSpeed;
            }
            else if (GlobalInputManager.GetGameKey(KeyCode.Q)) // Move down
            {
                verticalInput = -verticalSpeed;
            }
            moveDirection.y = verticalInput; // Apply vertical movement to the y-component.

            // Determine current speed: base speed or sprint speed.
            float currentSpeed = GlobalInputManager.GetGameKey(KeyCode.LeftShift) ? speed * sprintMultiplier : speed;

            // Move the player using CharacterController.Move.
            // Vector3.ClampMagnitude ensures diagonal movement isn't faster.
            // Time.deltaTime makes movement frame-rate independent.
            if (charController != null && charController.enabled)
            {
                charController.Move(Vector3.ClampMagnitude(moveDirection, 1.0f) * currentSpeed * Time.deltaTime);
            }
        }

        // Handles camera rotation based on mouse input.
        void CameraMovement()
        {
            if (cam == null) return; // Do nothing if camera transform is not assigned.

            // Get raw mouse input for X (horizontal) and Y (vertical) axes.
            Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Calculate vertical camera rotation (pitch).
            xRotation -= mouseDelta.y; // Subtract because mouse Y is typically inverted for looking up/down.
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp pitch to prevent flipping.

            // Apply pitch rotation to the camera's local X-axis.
            cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Apply horizontal rotation (yaw) to the player GameObject itself, making the whole player turn.
            transform.Rotate(Vector3.up * mouseDelta.x);
        }

        // Public method to toggle player movement and camera control on or off.
        // 'inputModeActive' being true typically means a UI input field is active, so player control should be disabled.
        public void ToggleMovement(bool inputModeActive)
        {
            allowMovement = !inputModeActive; // Player movement is allowed if input mode is NOT active.
        }
    }
}