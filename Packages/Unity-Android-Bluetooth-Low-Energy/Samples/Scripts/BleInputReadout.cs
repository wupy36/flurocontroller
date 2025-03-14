using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Modified BleInputReadout that correctly handles button display, bit positioning, and custom deadzones
/// </summary>
public class BleInputReadout : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public Text joystickReadoutText;
    [SerializeField] public Text buttonReadoutText;
    [SerializeField] public Text activeControlsText;
    [SerializeField] public Text deadzoneInfoText;  // New field for deadzone info display
    
    [Header("Display Options")]
    [SerializeField] public bool showRawValues = true;
    [SerializeField] public bool showNormalizedValues = true;
    [SerializeField] public bool showActiveControls = true;
    [SerializeField] public bool showDeadzoneInfo = true;  // New option for deadzone display
    [SerializeField] public float updateInterval = 0.1f;
    
    [Header("Debug Options")]
    [SerializeField] public bool showBinaryRepresentation = true;  // Show raw binary for each button set
    
    // Cached data for display
    private byte[] joystickValues = new byte[10];
    private byte[] buttonSets = new byte[4];
    private Dictionary<string, string> activeControls = new Dictionary<string, string>();
    private Dictionary<string, string> deadzoneInfos = new Dictionary<string, string>();  // New dictionary for deadzone info
    
    // User-friendly names for UI display
    private static readonly string[] JoystickNames = new string[] {
        "Joystick1X", "Joystick1Y",
        "Joystick2X", "Joystick2Y", 
        "Joystick3X", "Joystick3Y",
        "Joystick4X", "Joystick4Y",
        "Joystick5X", "Joystick5Y"
    };
    
    private static readonly string[] ButtonSetNames = new string[] {
        "Set1", "Set2", "Set3", "Set4"
    };
    
    private static readonly string[] ButtonNames = new string[] {
        "Button1", "Button2", "Button3", "Button4",
        "Button5", "Button6", "Button7", "Button8"
    };
    
    private float timeSinceLastUpdate = 0f;
    
    private void Start()
    {
        // Set initial text content
        UpdateJoystickDisplay();
        UpdateButtonDisplay();
        UpdateActiveControlsDisplay();
        UpdateDeadzoneInfoDisplay();
    }
    
    private void Update()
    {
        // Update the display at the specified interval
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            timeSinceLastUpdate = 0f;
            
            if (joystickReadoutText != null)
                UpdateJoystickDisplay();
                
            if (buttonReadoutText != null)
                UpdateButtonDisplay();
                
            if (activeControlsText != null && showActiveControls)
                UpdateActiveControlsDisplay();
                
            if (deadzoneInfoText != null && showDeadzoneInfo)
                UpdateDeadzoneInfoDisplay();
        }
    }
    
    /// <summary>
    /// Update input values for display. Call this from your InputControlManager.
    /// </summary>
    public void UpdateInputValues(byte[] newJoystickValues, byte[] newButtonSets)
    {
        // Make sure arrays are properly sized
        if (newJoystickValues.Length < joystickValues.Length || newButtonSets.Length < buttonSets.Length)
        {
            Debug.LogWarning("BleInputReadout: Input arrays are smaller than expected.");
            return;
        }
        
        // Copy values to internal arrays
        System.Array.Copy(newJoystickValues, joystickValues, joystickValues.Length);
        System.Array.Copy(newButtonSets, buttonSets, buttonSets.Length);
    }
    
    /// <summary>
    /// Register an active control for display in the UI
    /// </summary>
    public void RegisterActiveControl(string controlType, string controlName, string axisName, string inputName)
    {
        string key = $"{controlType}_{controlName}";
        string value = $"{axisName}: {inputName}";
        
        // Update or add the control info
        if (activeControls.ContainsKey(key))
            activeControls[key] = value;
        else
            activeControls.Add(key, value);
    }
    
    /// <summary>
    /// Register deadzone information for display in the UI
    /// </summary>
    public void RegisterDeadzoneInfo(string controlName, string axisName, byte start, byte end, byte center)
    {
        string key = $"DZ_{controlName}_{axisName}";
        string value = $"{controlName} ({axisName}): {start}-{end}, Center: {center}";
        
        // Update or add the deadzone info
        if (deadzoneInfos.ContainsKey(key))
            deadzoneInfos[key] = value;
        else
            deadzoneInfos.Add(key, value);
    }
    
    /// <summary>
    /// Clear a registered control from the active controls display
    /// </summary>
    public void ClearActiveControl(string controlType, string controlName)
    {
        string key = $"{controlType}_{controlName}";
        if (activeControls.ContainsKey(key))
            activeControls.Remove(key);
    }
    
    /// <summary>
    /// Clear deadzone information from the display
    /// </summary>
    public void ClearDeadzoneInfo(string controlName, string axisName)
    {
        string key = $"DZ_{controlName}_{axisName}";
        if (deadzoneInfos.ContainsKey(key))
            deadzoneInfos.Remove(key);
    }
    
    /// <summary>
    /// Update the joystick display text
    /// </summary>
    private void UpdateJoystickDisplay()
    {
        if (joystickReadoutText == null) return;
        
        StringBuilder sb = new StringBuilder("JOYSTICK VALUES:\n");
        
        for (int i = 0; i < joystickValues.Length; i++)
        {
            byte rawValue = joystickValues[i];
            sb.Append($"{JoystickNames[i]}: ");
            
            if (showRawValues)
                sb.Append($"{rawValue}");
                
            if (showRawValues && showNormalizedValues)
                sb.Append(" | ");
                
            if (showNormalizedValues)
                sb.Append($"{MapJoystickValue(rawValue):F2}");
                
            sb.Append("\n");
        }
        
        joystickReadoutText.text = sb.ToString();
    }
    
    /// <summary>
    /// Update the button display text with improved bit positioning and binary representation
    /// </summary>
    private void UpdateButtonDisplay()
    {
        if (buttonReadoutText == null) return;
        
        StringBuilder sb = new StringBuilder("BUTTON STATES:\n");
        
        for (int setIndex = 0; setIndex < buttonSets.Length; setIndex++)
        {
            byte buttonSet = buttonSets[setIndex];
            sb.Append($"{ButtonSetNames[setIndex]}: ");
            
            // Binary representation for debugging
            if (showBinaryRepresentation)
            {
                sb.Append($"[{System.Convert.ToString(buttonSet, 2).PadLeft(8, '0')}] ");
            }
            
            // Show pressed buttons with bit position
            bool anyPressed = false;
            for (int buttonIndex = 0; buttonIndex < 8; buttonIndex++)
            {
                bool isPressed = (buttonSet & (1 << buttonIndex)) != 0;
                sb.Append(isPressed ? "■" : "□");
                
                if (isPressed)
                {
                    anyPressed = true;
                    // Show the actual bit position for debugging
                    sb.Append($"({buttonIndex})");
                }
                
                if (buttonIndex < 7) sb.Append(" ");
            }
            
            // If no buttons pressed in this set, indicate that
            if (!anyPressed && showBinaryRepresentation)
            {
                sb.Append(" (none)");
            }
            
            sb.Append("\n");
        }
        
        buttonReadoutText.text = sb.ToString();
    }
    
    /// <summary>
    /// Update the active controls display text
    /// </summary>
    private void UpdateActiveControlsDisplay()
    {
        if (activeControlsText == null) return;
        
        if (activeControls.Count == 0)
        {
            activeControlsText.text = "ACTIVE CONTROLS:\nNone";
            return;
        }
        
        StringBuilder sb = new StringBuilder("ACTIVE CONTROLS:\n");
        
        foreach (var pair in activeControls)
        {
            sb.Append($"{pair.Key}: {pair.Value}\n");
        }
        
        activeControlsText.text = sb.ToString();
    }
    
    /// <summary>
    /// Update the deadzone info display text
    /// </summary>
    private void UpdateDeadzoneInfoDisplay()
    {
        if (deadzoneInfoText == null) return;
        
        if (deadzoneInfos.Count == 0)
        {
            deadzoneInfoText.text = "DEADZONE CONFIGURATION:\nUsing defaults";
            return;
        }
        
        StringBuilder sb = new StringBuilder("DEADZONE CONFIGURATION:\n");
        
        foreach (var pair in deadzoneInfos)
        {
            sb.Append($"{pair.Value}\n");
        }
        
        deadzoneInfoText.text = sb.ToString();
    }
    
    /// <summary>
    /// Map joystick raw value (0-255) to normalized range (-1 to 1)
    /// </summary>
    private float MapJoystickValue(byte rawValue)
    {
        // Define the deadzone range in terms of byte values
        const byte deadZoneStart = 113;
        const byte deadZoneEnd = 133;

        // Check if the value is within the deadzone
        if (rawValue >= deadZoneStart && rawValue <= deadZoneEnd)
        {
            // If within the deadzone, return 0.0
            return 0.0f;
        }
        
        // Otherwise, map the value to the desired output range
        if (rawValue <= 123)
        {
            // Map 0-123 to -1.0 to 0.0
            return Mathf.Lerp(-1f, 0f, (rawValue / 123f));
        }
        else
        {
            // Map 123-255 to 0.0 to 1.0
            return Mathf.Lerp(0f, 1f, ((rawValue - 123) / 132f));
        }
    }
}