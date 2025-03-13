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
    public class ButtonRotationConfig
    {
        [Tooltip("Type of button control to use")]
        public ButtonControlType controlType = ButtonControlType.None;
        
        [Tooltip("Button set index (1-4)")]
        [Range(1, 4)]
        public int buttonSetIndex = 1;
        
        [Tooltip("Positive direction button bit (0-7)")]
        [Range(0, 7)]
        public int posButtonBit = 0;
        
        [Tooltip("Negative direction button bit (0-7)")]
        [Range(0, 7)]
        public int negButtonBit = 1;
        
        [Tooltip("Invert button direction")]
        public bool invertDirection = false;
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
    
    [Header("Debug")]
    [Tooltip("Enable debug logs")]
    public bool debugMode = false;
    
    // Internal state tracking
    private float _normalizedRate;
    private Vector3 _startRotation;
    private string _axisName;
    
    private void Awake()
    {
        // Store starting rotation and calculate rate per frame
        _startRotation = transform.eulerAngles;
        _normalizedRate = rotationRate * 0.02f; // Scale by 0.02 (tickRate from original script)
        
        // Set axis name for debug
        _axisName = rotationAxis.ToString();
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
            ProcessButtonRotation(buttonSets[buttonControl.buttonSetIndex - 1]);
        }
    }
    
    /// <summary>
    /// Process button input for rotation
    /// </summary>
    private void ProcessButtonRotation(byte buttonSet)
    {
        bool posButtonPressed = (buttonSet & (1 << buttonControl.posButtonBit)) != 0;
        bool negButtonPressed = (buttonSet & (1 << buttonControl.negButtonBit)) != 0;
        
        float rotationAmount = 0;
        
        if (posButtonPressed && !negButtonPressed)
        {
            rotationAmount = _normalizedRate;
        }
        else if (negButtonPressed && !posButtonPressed)
        {
            rotationAmount = -_normalizedRate;
        }
        
        if (buttonControl.invertDirection)
        {
            rotationAmount *= -1;
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