using TMPro;
using UnityEngine;

namespace UrbanXplain
{
    // Manages the changing of building/plot colors based on different global views,
    // such as land function or energy consumption. It allows toggling these views
    // and resetting all plot colors back to their original state.
    public class BuildingColorChanger : MonoBehaviour
    {
        [Header("References")]
        // Reference to the DeepSeekAPI script, which provides access to plot data
        // including their ChildColorToggler components and properties like function and energy.
        [Tooltip("Drag the GameObject containing the DeepSeekAPI script here.")]
        public DeepSeekAPI deepSeekAPI;

        [Header("Plot Function Color Mapping")]
        // Color to use for plots designated as Function 1 (e.g., Residential).
        public Color colorForFunction1 = Color.yellow;
        // Color to use for plots designated as Function 2 (e.g., Commercial).
        public Color colorForFunction2 = Color.blue;
        // Color to use for plots designated as Function 3 (e.g., Public Buildings).
        public Color colorForFunction3 = Color.green;
        // Color to use for plots designated as Function 4 (e.g., Cultural/Entertainment).
        public Color colorForFunction4 = Color.red;

        [Header("Plot Energy Consumption Color Mapping")]
        // Color representing highly energy-efficient plots (e.g., EnergyConsumption = 1).
        public Color energyEfficientColor = Color.green;
        // Color representing highly energy-inefficient plots (e.g., EnergyConsumption = 100).
        public Color energyInefficientColor = Color.red;

        // Tracks whether the plot function-based color view is currently active.
        private bool isFunctionColorViewActive = false;
        // Tracks whether the plot energy consumption-based color view is currently active.
        private bool isEnergyColorViewActive = false;

        [Header("Key Settings")]
        // KeyCode to toggle the plot function color view.
        public KeyCode highlightKeyByFunction = KeyCode.Alpha1;
        // KeyCode to toggle the plot energy consumption color view.
        public KeyCode highlightKeyByEnergy = KeyCode.Alpha2;

        public UIControl uIControl;
        [SerializeField] private TMP_InputField inputField;

        void Start()
        {
            // Log an error if the DeepSeekAPI reference is not set, as this script depends on it.
            if (deepSeekAPI == null)
            {
                Debug.LogError("BuildingColorChanger: DeepSeekAPI reference not set in the Inspector! This script will not function correctly.");
            }
        }

        void Update()
        {
            // Do nothing if DeepSeekAPI or its colorTogglerArray is not available.
            if (deepSeekAPI == null || deepSeekAPI.colorTogglerArray == null)
            {
                return;
            }

            // Check for input to toggle the function-based color view.
            if (GlobalInputManager.GetGameKeyDown(highlightKeyByFunction))
            {
                if (!inputField.isFocused)
                    ToggleFunctionColorView();
            }

            // Check for input to toggle the energy-based color view.
            if (GlobalInputManager.GetGameKeyDown(highlightKeyByEnergy))
            {
                if (!inputField.isFocused)
                    ToggleEnergyColorView();
            }
        }

        // Toggles the display of plot colors based on their assigned land function.
        void ToggleFunctionColorView()
        {
            isFunctionColorViewActive = !isFunctionColorViewActive;

            if (isFunctionColorViewActive)
            {
                // If the energy color view is also active, deactivate it to ensure only one global view is active.
                if (isEnergyColorViewActive)
                {
                    isEnergyColorViewActive = false;
                    // No explicit color reset is needed here, as ApplyFunctionColors will override existing colors.
                }
                ApplyFunctionColors();
            }
            else
            {
                // If toggling off, reset all plots to their original colors, effectively deactivating this view.
                ResetAllPlotColorsToOriginal();
            }
        }

        // Toggles the display of plot colors based on their energy consumption values.
        void ToggleEnergyColorView()
        {
            isEnergyColorViewActive = !isEnergyColorViewActive;

            if (isEnergyColorViewActive)
            {
                // If the function color view is also active, deactivate it.
                if (isFunctionColorViewActive)
                {
                    isFunctionColorViewActive = false;
                    // No explicit color reset is needed here, as ApplyEnergyColors will override existing colors.
                }
                ApplyEnergyColors();
            }
            else
            {
                // If toggling off, reset all plots to their original colors, effectively deactivating this view.
                ResetAllPlotColorsToOriginal();
            }
        }

        // Applies colors to all plots based on their 'landFunction' property.
        void ApplyFunctionColors()
        {
            if (deepSeekAPI.colorTogglerArray == null) return;

            foreach (ChildColorToggler toggler in deepSeekAPI.colorTogglerArray)
            {
                if (toggler == null) continue;

                Color targetColor = Color.white; // Default color if no specific function match.
                bool applySpecificColor = false;

                // Determine the target color based on the plot's landFunction.
                switch (toggler.landFunction)
                {
                    case 1: targetColor = colorForFunction1; applySpecificColor = true; break;
                    case 2: targetColor = colorForFunction2; applySpecificColor = true; break;
                    case 3: targetColor = colorForFunction3; applySpecificColor = true; break;
                    case 4: targetColor = colorForFunction4; applySpecificColor = true; break;
                    default:
                        // If landFunction is 0 or an unhandled value, reset this plot to its original color.
                        toggler.ResetChildrenColorToOriginal();
                        applySpecificColor = false; // Ensure no new color is applied from the switch.
                        break;
                }

                if (applySpecificColor)
                {
                    // Set the children's color, indicating it's part of an active global view.
                    toggler.SetChildrenColor(targetColor, true);
                }
            }
        }

        // Applies colors to all plots based on their 'landEnergyConsumption' property.
        void ApplyEnergyColors()
        {
            if (deepSeekAPI.colorTogglerArray == null) return;

            foreach (ChildColorToggler toggler in deepSeekAPI.colorTogglerArray)
            {
                if (toggler == null) continue;

                // Apply color only if energy consumption is within the valid range (1-100).
                if (toggler.landEnergyConsumption >= 1 && toggler.landEnergyConsumption <= 100)
                {
                    // Normalize the energy consumption value (1-100) to a 0-1 range for color interpolation.
                    // t=0 for most efficient (consumption=1), t=1 for least efficient (consumption=100).
                    float t = (toggler.landEnergyConsumption - 1) / 99.0f;
                    Color targetColor = Color.Lerp(energyEfficientColor, energyInefficientColor, t);
                    // Set the children's color, indicating it's part of an active global view.
                    toggler.SetChildrenColor(targetColor, true);
                }
                else
                {
                    // If energy consumption is 0 (unknown/default) or out of range, reset this plot to its original color.
                    toggler.ResetChildrenColorToOriginal();
                }
            }
        }

        // Public property to check if the function-based color view is active.
        public bool IsFunctionColorViewActive => isFunctionColorViewActive;
        // Public property to check if the energy-based color view is active.
        public bool IsEnergyColorViewActive => isEnergyColorViewActive;


        // Returns an array of the defined function colors.
        // This can be useful for UI elements, such as displaying a color legend.
        public Color[] GetFunctionColors()
        {
            return new Color[] { colorForFunction1, colorForFunction2, colorForFunction3, colorForFunction4 };
        }

        // Resets all plot colors to their original state and deactivates any active global color views.
        void ResetAllPlotColorsToOriginal()
        {
            // Ensure both global view flags are turned off, as this method signifies no global view is active.
            isFunctionColorViewActive = false;
            isEnergyColorViewActive = false;

            if (deepSeekAPI.colorTogglerArray == null) return;

            // Iterate through all ChildColorToggler components and reset their colors.
            foreach (ChildColorToggler toggler in deepSeekAPI.colorTogglerArray)
            {
                if (toggler == null) continue;
                toggler.ResetChildrenColorToOriginal();
            }
        }
    }
}