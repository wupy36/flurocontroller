using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles translation of objects based on joystick or button input.
/// Attach directly to the GameObject that needs to be moved.
/// </summary>
public class TranslationController : MonoBehaviour
{
    [System.Serializable]
    public enum JoystickIndex
    {
        None = -1,
        Joystick1X = 0,
        Joystick1Y = 1,
        Joystick2X = 2,
        Joystick2Y = 3,
        Joystick3X = 4,
        Joystick3Y = 5,
        Joystick4X = 6,
        Joystick4Y = 7,
        Joystick5X = 8,
        Joystick5Y = 9
    }

    [System.Serializable]
    public enum TranslationAxis
    {
        X = 0,
        Y = 1,
        Z = 2
    }
    
    [System.Serializable]
    public enum ButtonControlType
    {
        None = 0,
        ButtonSet = 1
    }

    [System.Serializable]
    public enum ButtonSetIndex
    {
        Set1 = 0,
        Set2 = 1,
        Set3 = 2,
        Set4 = 3
    }

    [System.Serializable]
    public enum ButtonIndex
    {
        Button1 = 0,
        Button2 = 1,
        Button3 = 2,
        Button4 = 3,
        Button5 = 4,
        Button6 = 5,
        Button7 = 6,
        Button8 = 7
    }
    
    [System.Serializable]
    public class ButtonTranslationConfig
    {
        [Tooltip("Type of button control to use")]
        public ButtonControlType controlType = ButtonControlType.None;
        
        [Header("Positive Direction Control")]
        [Tooltip("Button set for positive direction")]
        public ButtonSetIndex posButtonSetIndex = ButtonSetIndex.Set1;
        
        [Tooltip("Button for positive direction")]
        public ButtonIndex posButtonIndex = ButtonIndex.Button1;
        
        [Tooltip("Invert positive direction")]
        public bool invertPosDirection = false;

        [Header("Negative Direction Control")]
        [Tooltip("Button set for negative direction")]
        public ButtonSetIndex negButtonSetIndex = ButtonSetIndex.Set1;
        
        [Tooltip("Button for negative direction")]
        public ButtonIndex negButtonIndex = ButtonIndex.Button2;
        
        [Tooltip("Invert negative direction")]
        public bool invertNegDirection = false;
    }

    [Header("Translation Configuration")]
    [Tooltip("Axis to translate along")]
    public TranslationAxis translationAxis = TranslationAxis.Z;
    
    [Tooltip("Translation rate (units per second)")]
    public float translationRate = 0.35f;
    
    [Tooltip("Minimum position on the axis")]
    public float minPosition = -2.5f;
    
    [Tooltip("Maximum position on the axis")]
    public float maxPosition = 2.35f;
    
    [Tooltip("Invert axis direction")]
    public bool invertAxis = false;
    
    [Header("Joystick Control")]
    [Tooltip("Joystick index for translation control")]
    public JoystickIndex joystickIndex = JoystickIndex.None;
    
    [Tooltip("Invert joystick direction")]
    public bool invertJoystick = false;
    
    [Header("Button Control")]
    [Tooltip("Button configuration for translation")]
    public ButtonTranslationConfig buttonControl = new ButtonTranslationConfig();
    
    [Header("Deadzone Configuration")]
    [Tooltip("Use custom deadzone settings for this controller")]
    public bool useCustomDeadzone = false;
    
    [Tooltip("Deadzone start value (0-255)")]
    [Range(0, 255)]
    public byte deadZoneStart = 100;
    
    [Tooltip("Deadzone end value (0-255)")]
    [Range(0, 255)]
    public byte deadZoneEnd = 126;
    
    [Tooltip("Center value for joystick (0-255)")]
    [Range(0, 255)]
    public byte centerValue = 123;
    
    [Header("Debug")]
    [Tooltip("Enable debug logs")]
    public bool debugMode = false;
    
    // Internal state tracking
    private float _normalizedRate;
    private Vector3 _startPosition;
    private string _axisName;
    private BleInputReadout inputReadout;
    
    // Default deadzone values (for when custom is disabled)
    private const byte DEFAULT_DEADZONE_START = 113;
    private const byte DEFAULT_DEADZONE_END = 133;
    private const byte DEFAULT_CENTER_VALUE = 123;

    private void Awake()
    {
        // Store starting position and calculate rate per frame
        _startPosition = transform.localPosition;
        _normalizedRate = translationRate * 0.02f; // Scale by 0.02 (tickRate from original script)
        
        // Set axis name for debug
        _axisName = translationAxis.ToString();
        
        // Validate deadzone settings
        ValidateDeadzoneSettings();
    }
    
    private void ValidateDeadzoneSettings()
    {
        // Ensure deadzone values are in correct order
        if (deadZoneStart > deadZoneEnd)
        {
            byte temp = deadZoneStart;
            deadZoneStart = deadZoneEnd;
            deadZoneEnd = temp;
            
            if (debugMode)
            {
                Debug.LogWarning($"Deadzone values were reversed on {gameObject.name}, fixed automatically.");
            }
        }
        
        // Ensure center value is within deadzone
        if (centerValue < deadZoneStart || centerValue > deadZoneEnd)
        {
            // Set center to middle of deadzone
            centerValue = (byte)((deadZoneStart + deadZoneEnd) / 2);
            
            if (debugMode)
            {
                Debug.LogWarning($"Center value was outside deadzone on {gameObject.name}, adjusted to {centerValue}.");
            }
        }
    }
    
    /// <summary>
    /// Register a input readout component for debugging
    /// </summary>
    public void RegisterReadout(BleInputReadout readout)
    {
        inputReadout = readout;
        
        // Register active controls
        if (readout != null)
        {
            string controlName = gameObject.name;
            string axisName = translationAxis.ToString();
            
            if (joystickIndex != JoystickIndex.None)
            {
                readout.RegisterActiveControl("Translation", controlName, axisName, joystickIndex.ToString());
            }
            
            if (buttonControl.controlType != ButtonControlType.None)
            {
                string buttonInfo = $"{buttonControl.posButtonSetIndex}.{buttonControl.posButtonIndex}/" +
                                    $"{buttonControl.negButtonSetIndex}.{buttonControl.negButtonIndex}";
                readout.RegisterActiveControl("Translation", controlName + "_Buttons", axisName, buttonInfo);
            }
            
            // Register deadzone info if custom
            if (useCustomDeadzone)
            {
                string deadzoneInfo = $"DZ:{deadZoneStart}-{deadZoneEnd} C:{centerValue}";
                readout.RegisterActiveControl("DeadzoneInfo", controlName, axisName, deadzoneInfo);
            }
        }
    }
    
    /// <summary>
    /// Process input values to translate the object
    /// </summary>
    public void ProcessInput(byte[] joystickValues, byte[] buttonSets)
    {
        // Process joystick if configured
        if (joystickIndex != JoystickIndex.None)
        {
            int index = (int)joystickIndex;
            if (index >= 0 && index < joystickValues.Length)
            {
                float joystickValue = MapJoystickValue(joystickValues[index]);
                if (invertJoystick) joystickValue *= -1;
                
                // Apply translation
                ApplyTranslation(joystickValue * _normalizedRate);
            }
        }
        
        // Process button input if configured
        if (buttonControl.controlType != ButtonControlType.None && buttonSets.Length >= 4)
        {
            ProcessButtonTranslation(buttonSets);
        }
    }
    
    /// <summary>
    /// Process button input for translation
    /// </summary>
    private void ProcessButtonTranslation(byte[] buttonSets)
    {
        // Get button states
        bool posButtonPressed = (buttonSets[(int)buttonControl.posButtonSetIndex] & 
                                (1 << (int)buttonControl.posButtonIndex)) != 0;
        
        bool negButtonPressed = (buttonSets[(int)buttonControl.negButtonSetIndex] & 
                                (1 << (int)buttonControl.negButtonIndex)) != 0;
        
        float translationAmount = 0;
        
        // Handle positive direction
        if (posButtonPressed)
        {
            float posAmount = _normalizedRate;
            if (buttonControl.invertPosDirection)
            {
                posAmount *= -1;
            }
            translationAmount += posAmount;
        }
        
        // Handle negative direction
        if (negButtonPressed)
        {
            float negAmount = -_normalizedRate;
            if (buttonControl.invertNegDirection)
            {
                negAmount *= -1;
            }
            translationAmount += negAmount;
        }
        
        if (translationAmount != 0)
        {
            ApplyTranslation(translationAmount);
        }
    }
    
   /// <summary>
    /// Apply translation to the object within limits
    /// </summary>
    private void ApplyTranslation(float amount)
    {
        if (amount == 0) return;
        
        // Apply global inversion if configured
        if (invertAxis)
        {
            amount *= -1;
        }
        
        // Get current position
        Vector3 currentPosition = transform.localPosition;
        float currentAxisPosition = GetAxisPosition(currentPosition);
        
        // Calculate new position
        float newAxisPosition = currentAxisPosition + amount;
        
        // Check boundaries - FIXED LOGIC
        if (newAxisPosition < minPosition || newAxisPosition > maxPosition)
        {
            if (debugMode)
            {
                Debug.LogWarning($"Hit position limit: current={currentAxisPosition}, " +
                                $"min={minPosition}, max={maxPosition}, attempt={amount}");
            }
            return;
        }
        
        // Apply translation based on axis
        Vector3 translationVector = Vector3.zero;
        switch (translationAxis)
        {
            case TranslationAxis.X:
                translationVector = new Vector3(amount, 0, 0);
                break;
            case TranslationAxis.Y:
                translationVector = new Vector3(0, amount, 0);
                break;
            case TranslationAxis.Z:
                translationVector = new Vector3(0, 0, amount);
                break;
        }
        
        transform.Translate(translationVector, Space.Self);
        
        if (debugMode)
        {
            Debug.Log($"Translated {_axisName} by {amount}, current: {GetAxisPosition(transform.localPosition)}");
        }
    }
    
    /// <summary>
    /// Get position value for the configured axis
    /// </summary>
    private float GetAxisPosition(Vector3 position)
    {
        switch (translationAxis)
        {
            case TranslationAxis.X: return position.x;
            case TranslationAxis.Y: return position.y;
            case TranslationAxis.Z: return position.z;
            default: return 0;
        }
    }
    
    /// <summary>
    /// Map joystick raw value (0-255) to normalized range (-1 to 1) with custom deadzone support
    /// </summary>
    private float MapJoystickValue(byte rawValue)
    {
        // Select which deadzone values to use
        byte dzStart = useCustomDeadzone ? deadZoneStart : DEFAULT_DEADZONE_START;
        byte dzEnd = useCustomDeadzone ? deadZoneEnd : DEFAULT_DEADZONE_END;
        byte centerVal = useCustomDeadzone ? centerValue : DEFAULT_CENTER_VALUE;

        // Check if the value is within the deadzone
        if (rawValue >= dzStart && rawValue <= dzEnd)
        {
            // If within the deadzone, return 0.0
            return 0.0f;
        }
        
        // Otherwise, map the value to the desired output range
        if (rawValue <= centerVal)
        {
            // Map 0-centerVal to -1.0 to 0.0
            return Mathf.Lerp(-1f, 0f, (rawValue / (float)centerVal));
        }
        else
        {
            // Map centerVal-255 to 0.0 to 1.0
            return Mathf.Lerp(0f, 1f, ((rawValue - centerVal) / (float)(255 - centerVal)));
        }
    }
}