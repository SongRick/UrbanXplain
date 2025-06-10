//
// WebGLInput.jslib
//
// This library prevents the browser from handling specific key presses
// when the Unity WebGL canvas is in focus, allowing the game to
// receive and process them without interruption (e.g., Alt key opening a menu).
//

mergeInto(LibraryManager.library, {

  // This function is intended to be called from a Unity script (e.g., WebGLInputController.cs)
  // to initialize the event listeners.
  WebGLInputInit: function() {

    // Add a 'keydown' event listener to the entire window.
    // The 'true' argument makes it a "capturing" event listener, which can intercept
    // the event before it bubbles up to default browser handlers.
    window.addEventListener("keydown", function(e) {
      
      // We only want to prevent default actions when the user is interacting with the game canvas.
      // Unity's default canvas ID is 'canvas' or 'unity-canvas' in newer templates.
      // We check for both to be safe.
      if (e.target.id === 'canvas' || e.target.id === 'unity-canvas') {

        // --- KEY BLOCKING LOGIC ---
        // Here we list all the 'e.code' values for keys we want to intercept.
        // For a full list of key codes, see: https://developer.mozilla.org/en-US/docs/Web/API/UI_Events/Keyboard_event_code_values

        if (
          // --- From your Controls Menu ---
          e.code === "AltLeft" ||          // Prevents the browser menu from activating.
          e.code === "AltRight"
        ) {
          // This is the most important part: it stops the browser's default action.
          e.preventDefault();
        }
      }
    }, true);


    // Optional: Also prevent the right-click context menu from appearing over the game.
    const unityCanvas = document.querySelector("#canvas, #unity-canvas");
    if (unityCanvas) {
        unityCanvas.addEventListener("contextmenu", function(e) {
            e.preventDefault();
        });
    }

  },

});