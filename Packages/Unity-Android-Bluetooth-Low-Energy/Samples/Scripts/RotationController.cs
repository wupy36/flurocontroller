using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles rotation of objects based on joystick or button input.
/// Attach directly to the GameObject that needs to be rotated.
/// </summary>
public class RotationController : MonoBehaviour
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
    public enum RotationAxis
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
    public class ButtonRotationConfig
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

    [Header("Rotation Configuration")]
    [Tooltip("Axis to rotate around")]
    public RotationAxis rotationAxis = RotationAxis.Z;
    
    [Tooltip("Rotation rate (degrees per second)")]
    public float rotationRate = 30.0f;
    
    [Tooltip("Minimum rotation angle (degrees)")]
    public float minRotation = -90.0f;
    
    [Tooltip("Maximum rotation angle (degrees)")]
    public float maxRotation = 90.0f;

    [Header("Primary Joystick Control")]
    [Tooltip("Joystick index for primary control")]
    public JoystickIndex primaryJoystick = JoystickIndex.Joystick1X;
    
    [Tooltip("Invert primary joystick direction")]
    public bool invertPrimaryJoystick = false;
    
    [Header("Secondary Joystick Control (Optional)")]
    [Tooltip("Joystick index for secondary control")]
    public JoystickIndex secondaryJoystick = JoystickIndex.None;
    
    [Tooltip("Invert secondary joystick direction")]
    public bool invertSecondaryJoystick = false;
    
    [Header("Button Control (Optional)")]
    [Tooltip("Button configuration for rotation")]
    public ButtonRotationConfig buttonControl = new ButtonRotationConfig();
    
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
    private Vector3 _startRotation;
    private string _axisName;
    private BleInputReadout inputReadout;

    // Default deadzone values (for when custom is disabled)
    private const byte DEFAULT_DEADZONE_START = 113;
    private const byte DEFAULT_DEADZONE_END = 133;
    private const byte DEFAULT_CENTER_VALUE = 123;

    private void Awake()
    {
        // Store starting rotation and calculate rate per frame
        _startRotation = transform.eulerAngles;
        _normalizedRate = rotationRate * 0.02f; // Scale by 0.02 (tickRate from original script)
        
        // Set axis name for debug
        _axisName = rotationAxis.ToString();
        
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
            string axisName = rotationAxis.ToString();
            
            if (primaryJoystick != JoystickIndex.None)
            {
                readout.RegisterActiveControl("Rotation", controlName, axisName, primaryJoystick.ToString());
            }
            
            if (secondaryJoystick != JoystickIndex.None)
            {
                readout.RegisterActiveControl("Rotation", controlName + "_Secondary", axisName, secondaryJoystick.ToString());
            }
            
            if (buttonControl.controlType != ButtonControlType.None)
            {
                string buttonInfo = $"{buttonControl.posButtonSetIndex}.{buttonControl.posButtonIndex}/" +
                                    $"{buttonControl.negButtonSetIndex}.{buttonControl.negButtonIndex}";
                readout.RegisterActiveControl("Rotation", controlName + "_Buttons", axisName, buttonInfo);
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
    /// Process input values to rotate the object
    /// </summary>
    public void ProcessInput(byte[] joystickValues, byte[] buttonSets)
    {
        // Process primary joystick if configured
        if (primaryJoystick != JoystickIndex.None)
        {
            int index = (int)primaryJoystick;
            if (index >= 0 && index < joystickValues.Length)
            {
                float joystickValue = MapJoystickValue(joystickValues[index]);
                if (invertPrimaryJoystick) joystickValue *= -1;
                
                ApplyRotation(joystickValue * _normalizedRate);
            }
        }
        
        // Process secondary joystick if configured
        if (secondaryJoystick != JoystickIndex.None)
        {
            int index = (int)secondaryJoystick;
            if (index >= 0 && index < joystickValues.Length)
            {
                float joystickValue = MapJoystickValue(joystickValues[index]);
                if (invertSecondaryJoystick) joystickValue *= -1;
                
                ApplyRotation(joystickValue * _normalizedRate);
            }
        }
        
        // Process button input if configured
        if (buttonControl.controlType != ButtonControlType.None && buttonSets.Length >= 4)
        {
            ProcessButtonRotation(buttonSets);
        }
    }
    
    /// <summary>
    /// Process button input for rotation
    /// </summary>
    private void ProcessButtonRotation(byte[] buttonSets)
    {
        // Get button states
        bool posButtonPressed = (buttonSets[(int)buttonControl.posButtonSetIndex] & 
                                (1 << (int)buttonControl.posButtonIndex)) != 0;
        
        bool negButtonPressed = (buttonSets[(int)buttonControl.negButtonSetIndex] & 
                                (1 << (int)buttonControl.negButtonIndex)) != 0;
        
        float rotationAmount = 0;
        
        // Handle positive direction
        if (posButtonPressed)
        {
            float posAmount = _normalizedRate;
            if (buttonControl.invertPosDirection)
            {
                posAmount *= -1;
            }
            rotationAmount += posAmount;
        }
        
        // Handle negative direction
        if (negButtonPressed)
        {
            float negAmount = -_normalizedRate;
            if (buttonControl.invertNegDirection)
            {
                negAmount *= -1;
            }
            rotationAmount += negAmount;
        }
        
        if (rotationAmount != 0)
        {
            ApplyRotation(rotationAmount);
        }
    }
    
    /// <summary>
    /// Apply rotation to the object within limits
    /// </summary>
    private void ApplyRotation(float amount)
    {
        if (amount == 0) return;
        
        // Get current rotation in Euler angles
        Vector3 currentRotation = transform.eulerAngles;
        
        // Convert to -180 to 180 range for easier limit checking
        float currentAngle = GetNormalizedAngle(GetAxisRotation(currentRotation));
        
        // Calculate new angle
        float newAngle = currentAngle + amount;
        
        // Check limits
        if (newAngle < minRotation && amount < 0)
        {
            if (debugMode) Debug.LogWarning($"Hit min rotation limit: {minRotation}");
            return;
        }
        
        if (newAngle > maxRotation && amount > 0)
        {
            if (debugMode) Debug.LogWarning($"Hit max rotation limit: {maxRotation}");
            return;
        }
        
        // Apply rotation based on axis
        Vector3 rotationVector = Vector3.zero;
        switch (rotationAxis)
        {
            case RotationAxis.X:
                rotationVector = new Vector3(amount, 0, 0);
                break;
            case RotationAxis.Y:
                rotationVector = new Vector3(0, amount, 0);
                break;
            case RotationAxis.Z:
                rotationVector = new Vector3(0, 0, amount);
                break;
        }
        
        transform.Rotate(rotationVector);
        
        if (debugMode)
        {
            Debug.Log($"Rotated {_axisName} by {amount}, current: {GetAxisRotation(transform.eulerAngles)}");
        }
    }
    
    /// <summary>
    /// Get rotation value for the configured axis
    /// </summary>
    private float GetAxisRotation(Vector3 rotation)
    {
        switch (rotationAxis)
        {
            case RotationAxis.X: return rotation.x;
            case RotationAxis.Y: return rotation.y;
            case RotationAxis.Z: return rotation.z;
            default: return 0;
        }
    }
    
    /// <summary>
    /// Convert angle to -180 to 180 range
    /// </summary>
    private float GetNormalizedAngle(float angle)
    {
        // Convert 0-360 range to -180 to 180 range
        if (angle > 180)
        {
            angle -= 360;
        }
        return angle;
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