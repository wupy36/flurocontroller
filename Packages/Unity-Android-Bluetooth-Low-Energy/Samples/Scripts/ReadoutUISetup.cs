using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper class to set up a debug UI for the BLE input readout.
/// Attach to an empty GameObject and call SetupDebugUI() to create the UI elements.
/// </summary>
public class ReadoutUISetup : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Vector2 panelSize = new Vector2(300, 500); // Increased panel size for deadzone info
    [SerializeField] private int fontSize = 14;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private Color textColor = Color.white;
    
    [Header("References")]
    [SerializeField] private InputControlManager inputManager;
    
    private BleInputReadout inputReadout;
    
    /// <summary>
    /// Set up the debug UI elements
    /// </summary>
    public void SetupDebugUI()
    {
        // Create canvas if needed
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DebugCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add canvas scaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            
            // Add raycaster
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create panel
        GameObject panelObj = new GameObject("InputReadoutPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 1);
        panelRect.anchoredPosition = new Vector2(0, 0);
        panelRect.sizeDelta = panelSize;
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = backgroundColor;
        
        // Create text areas (adjusted spacing for deadzone info)
        GameObject joystickTextObj = CreateTextArea("JoystickReadout", panelObj.transform, new Vector2(0, 1), 0);
        GameObject buttonTextObj = CreateTextArea("ButtonReadout", panelObj.transform, new Vector2(0, 0.7f), 1);
        GameObject controlsTextObj = CreateTextArea("ActiveControls", panelObj.transform, new Vector2(0, 0.45f), 2);
        GameObject deadzoneTextObj = CreateTextArea("DeadzoneInfo", panelObj.transform, new Vector2(0, 0.2f), 3);
        
        // Add readout component
        inputReadout = panelObj.AddComponent<BleInputReadout>();
        inputReadout.showRawValues = true;
        inputReadout.showNormalizedValues = true;
        inputReadout.showActiveControls = true;
        inputReadout.showDeadzoneInfo = true;
        inputReadout.updateInterval = 0.1f;
        
        // Set up references
        inputReadout.joystickReadoutText = joystickTextObj.GetComponent<Text>();
        inputReadout.buttonReadoutText = buttonTextObj.GetComponent<Text>();
        inputReadout.activeControlsText = controlsTextObj.GetComponent<Text>();
        inputReadout.deadzoneInfoText = deadzoneTextObj.GetComponent<Text>();
        
        // Register with input manager
        if (inputManager == null)
        {
            inputManager = FindObjectOfType<InputControlManager>();
        }
        
        if (inputManager != null)
        {
            inputManager.RegisterReadout(inputReadout);
            
            // Find and register all controllers
            RotationController[] rotationControllers = FindObjectsOfType<RotationController>();
            foreach (var controller in rotationControllers)
            {
                controller.RegisterReadout(inputReadout);
                
                // Register deadzone info if using custom deadzone
                if (controller.useCustomDeadzone)
                {
                    inputReadout.RegisterDeadzoneInfo(
                        controller.gameObject.name,
                        controller.rotationAxis.ToString(),
                        controller.deadZoneStart,
                        controller.deadZoneEnd,
                        controller.centerValue
                    );
                }
            }
            
            TranslationController[] translationControllers = FindObjectsOfType<TranslationController>();
            foreach (var controller in translationControllers)
            {
                controller.RegisterReadout(inputReadout);
                
                // Register deadzone info if using custom deadzone
                if (controller.useCustomDeadzone)
                {
                    inputReadout.RegisterDeadzoneInfo(
                        controller.gameObject.name,
                        controller.translationAxis.ToString(),
                        controller.deadZoneStart,
                        controller.deadZoneEnd,
                        controller.centerValue
                    );
                }
            }
        }
        else
        {
            Debug.LogError("InputControlManager not found. Readout won't receive updates.");
        }
    }
    
    private GameObject CreateTextArea(string name, Transform parent, Vector2 anchorMin, int order)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        // Adjust the height based on the order
        float height = (order == 3) ? 0.2f : 0.25f; // Make deadzone area slightly smaller
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, anchorMin.y - height);
        textRect.anchorMax = new Vector2(1, anchorMin.y);
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = textColor;
        text.raycastTarget = false;
        text.supportRichText = true;
        text.alignment = TextAnchor.UpperLeft;
        text.text = name;
        
        return textObj;
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Add menu item to create the debug UI
    /// </summary>
    [UnityEditor.MenuItem("BLE/Create Input Readout UI")]
    public static void CreateDebugUI()
    {
        GameObject setupObj = new GameObject("ReadoutUISetup");
        ReadoutUISetup setup = setupObj.AddComponent<ReadoutUISetup>();
        setup.SetupDebugUI();
        
        // Destroy setup object after creating UI
        DestroyImmediate(setupObj);
    }
#endif
}