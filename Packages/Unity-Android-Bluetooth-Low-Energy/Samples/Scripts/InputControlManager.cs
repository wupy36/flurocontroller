using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modified InputControlManager with improved button handling for multiple presses
/// </summary>
public class InputControlManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip movementSound;
    [SerializeField] private bool useAudio = true;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private BleInputReadout inputReadout;  // Using fixed version
    
    [Header("Button Handling")]
    [SerializeField] private bool correctButtonMapping = true;  // New option to fix button mapping

    // Lists to track all control components in the scene
    private List<RotationController> rotateControls = new List<RotationController>();
    private List<TranslationController> translateControls = new List<TranslationController>();
    
    // Input data from BLE device
    private byte[] joystickValues = new byte[10]; // Holds all joystick values (5 joysticks x 2 axes)
    private byte[] buttonSets = new byte[4];      // Holds all button set values (4 sets)
    
    // Raw data for debugging
    private byte[] rawData = new byte[0];
    
    // Movement tracking for audio
    private Vector3[] lastPositions;
    private Quaternion[] lastRotations;
    private List<Transform> trackedTransforms = new List<Transform>();
    
    private void Awake()
    {
        // Initialize audio source if needed
        if (useAudio && audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find all control components in the scene
        FindAllControlComponents();
    }
    
    /// <summary>
    /// Find all control components in the scene
    /// </summary>
    private void FindAllControlComponents()
    {
        // Find rotation controllers
        RotationController[] rotationControllers = FindObjectsOfType<RotationController>();
        rotateControls.AddRange(rotationControllers);
        
        // Find translation controllers
        TranslationController[] translationControllers = FindObjectsOfType<TranslationController>();
        translateControls.AddRange(translationControllers);
        
        // Track transforms for movement detection
        foreach (var controller in rotationControllers)
        {
            trackedTransforms.Add(controller.transform);
        }
        
        foreach (var controller in translationControllers)
        {
            trackedTransforms.Add(controller.transform);
        }
        
        // Initialize position and rotation tracking arrays
        int count = trackedTransforms.Count;
        lastPositions = new Vector3[count];
        lastRotations = new Quaternion[count];
        
        for (int i = 0; i < count; i++)
        {
            lastPositions[i] = trackedTransforms[i].position;
            lastRotations[i] = trackedTransforms[i].rotation;
        }
        
        if (debugMode)
        {
            Debug.Log($"Found {rotateControls.Count} rotation controllers and {translateControls.Count} translation controllers");
        }
    }
    
    /// <summary>
    /// Receive input data from BLE device via InputReceiver with improved button handling
    /// </summary>
    public void ReceiveBytesArray(byte[] value)
    {
        // Store raw data for debugging
        rawData = new byte[value.Length];
        System.Array.Copy(value, rawData, value.Length);
        
        if (value.Length < 14)
        {
            Debug.LogError($"Received invalid data length: {value.Length}, expected at least 14 bytes");
            return;
        }
        
        // Copy joystick values (first 10 bytes)
        for (int i = 0; i < 10 && i < value.Length; i++)
        {
            joystickValues[i] = value[i];
        }
        
        // Copy button set values with correct mapping if enabled
        for (int i = 0; i < 4 && i + 10 < value.Length; i++)
        {
            if (correctButtonMapping && i == 1)  // Only apply correction to button set 2
            {
                // Fix for button set 2 - apply bit shift correction if needed
                // This handles the case where button 16 is showing as button 24
                byte originalValue = value[i + 10];
                
                // Log the raw binary value if in debug mode
                if (debugMode)
                {
                    Debug.Log($"Set {i+1} raw: {System.Convert.ToString(originalValue, 2).PadLeft(8, '0')}");
                }
                
                // Fix the button mapping issue - bit shift or mask as needed
                // In this case, we're handling the situation where button 16 (index 0) is showing as button 24 (index 3)
                byte correctedValue = (byte)(
                    ((originalValue & 0x08) >> 3) |    // Bit 3 → Bit 0
                    ((originalValue & 0x04) >> 1) |    // Bit 2 → Bit 1 
                    ((originalValue & 0x02) << 1) |    // Bit 1 → Bit 2
                    ((originalValue & 0x01) << 3) |    // Bit 0 → Bit 3
                    (originalValue & 0xF0));           // Keep high bits unchanged
                
                buttonSets[i] = correctedValue;
                
                if (debugMode)
                {
                    Debug.Log($"Set {i+1} corrected: {System.Convert.ToString(correctedValue, 2).PadLeft(8, '0')}");
                }
            }
            else
            {
                buttonSets[i] = value[i + 10];
            }
        }
        
        // Process input with all controllers
        ProcessInput();
        
        // Check for movement to play sound
        if (useAudio)
        {
            CheckForMovement();
        }
    }
    
    /// <summary>
    /// Debug method to dump the full raw data received from BLE
    /// </summary>
    public void DumpRawData()
    {
        if (rawData.Length == 0)
        {
            Debug.Log("No raw data received yet");
            return;
        }
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Raw BLE data dump:");
        
        for (int i = 0; i < rawData.Length; i++)
        {
            sb.AppendLine($"Byte {i}: {rawData[i]} (0x{rawData[i]:X2}) - Binary: {System.Convert.ToString(rawData[i], 2).PadLeft(8, '0')}");
        }
        
        Debug.Log(sb.ToString());
    }
    
    /// <summary>
    /// Register a UI readout component for debugging
    /// </summary>
    public void RegisterReadout(BleInputReadout readout)
    {
        inputReadout = readout;
    }

    /// <summary>
    /// Distribute input data to all control components
    /// </summary>
    private void ProcessInput()
    {
        // Update the readout display if available
        if (debugMode && inputReadout != null)
        {
            inputReadout.UpdateInputValues(joystickValues, buttonSets);
        }

        // Process rotation controllers
        foreach (var controller in rotateControls)
        {
            controller.ProcessInput(joystickValues, buttonSets);
        }
        
        // Process translation controllers
        foreach (var controller in translateControls)
        {
            controller.ProcessInput(joystickValues, buttonSets);
        }
    }
    
    /// <summary>
    /// Check if any controlled object is moving to play sound
    /// </summary>
    private void CheckForMovement()
    {
        bool isMoving = false;
        
        // Check all tracked transforms for movement
        for (int i = 0; i < trackedTransforms.Count; i++)
        {
            if (trackedTransforms[i] == null) continue;
            
            if (Vector3.Distance(lastPositions[i], trackedTransforms[i].position) > 0.0001f ||
                Quaternion.Angle(lastRotations[i], trackedTransforms[i].rotation) > 0.01f)
            {
                isMoving = true;
                lastPositions[i] = trackedTransforms[i].position;
                lastRotations[i] = trackedTransforms[i].rotation;
            }
        }
        
        // Play or stop sound based on movement
        if (isMoving)
        {
            if (audioSource != null && !audioSource.isPlaying && movementSound != null)
            {
                audioSource.clip = movementSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
    
    /// <summary>
    /// Manual registration method for components added at runtime
    /// </summary>
    public void RegisterRotateControl(RotationController control)
    {
        if (!rotateControls.Contains(control))
        {
            rotateControls.Add(control);
            AddTrackedTransform(control.transform);
        }
    }
    
    /// <summary>
    /// Manual registration method for components added at runtime
    /// </summary>
    public void RegisterTranslateControl(TranslationController control)
    {
        if (!translateControls.Contains(control))
        {
            translateControls.Add(control);
            AddTrackedTransform(control.transform);
        }
    }
    
    /// <summary>
    /// Add a transform to the movement tracking system
    /// </summary>
    private void AddTrackedTransform(Transform transform)
    {
        trackedTransforms.Add(transform);
        
        // Resize tracking arrays
        System.Array.Resize(ref lastPositions, trackedTransforms.Count);
        System.Array.Resize(ref lastRotations, trackedTransforms.Count);
        
        // Initialize with current values
        int index = trackedTransforms.Count - 1;
        lastPositions[index] = transform.position;
        lastRotations[index] = transform.rotation;
    }
}