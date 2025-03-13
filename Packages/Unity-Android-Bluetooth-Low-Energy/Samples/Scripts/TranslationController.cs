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
    public class ButtonTranslationConfig
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
    
    [Header("Debug")]
    [Tooltip("Enable debug logs")]
    public bool debugMode = false;
    
    // Internal state tracking
    private float _normalizedRate;
    private Vector3 _startPosition;
    private string _axisName;
    
    private void Awake()
    {
        // Store starting position and calculate rate per frame
        _startPosition = transform.localPosition;
        _normalizedRate = translationRate * 0.02f; // Scale by 0.02 (tickRate from original script)
        
        // Set axis name for debug
        _axisName = translationAxis.ToString();
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
            ProcessButtonTranslation(buttonSets[buttonControl.buttonSetIndex - 1]);
        }
    }
    
    /// <summary>
    /// Process button input for translation
    /// </summary>
    private void ProcessButtonTranslation(byte buttonSet)
    {
        bool posButtonPressed = (buttonSet & (1 << buttonControl.posButtonBit)) != 0;
        bool negButtonPressed = (buttonSet & (1 << buttonControl.negButtonBit)) != 0;
        
        float translationAmount = 0;
        
        if (posButtonPressed && !negButtonPressed)
        {
            translationAmount = _normalizedRate;
        }
        else if (negButtonPressed && !posButtonPressed)
        {
            translationAmount = -_normalizedRate;
        }
        
        if (buttonControl.invertDirection)
        {
            translationAmount *= -1;
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
        
        // Check boundaries
        if ((newAxisPosition < minPosition && amount < 0) || 
            (newAxisPosition > maxPosition && amount > 0))
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