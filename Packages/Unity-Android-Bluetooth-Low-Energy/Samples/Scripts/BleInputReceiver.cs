using Android.BLE;
using Android.BLE.Commands;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Receives raw input data from BLE device and forwards it to the input control system.
/// </summary>
public class BleInputReceiver : MonoBehaviour
{
    [SerializeField] private Text debugTextField;
    [SerializeField] private string inputManagerTag = "InputManager";

    private string _deviceUuid;
    private SubscribeToCharacteristic _subscribe;
    private string _bytestring = "no bytes";
    private InputControlManager _controlManager;

    private void Start()
    {
        // Try to find the control manager at start
        FindControlManager();
    }

    public void ReceiveUuid(string uuid)
    {
        _deviceUuid = uuid;
        SubscribeToCharacteristic();
    }

    public void SubscribeToCharacteristic()
    {
        if (string.IsNullOrEmpty(_deviceUuid))
        {
            UpdateDebugText("Connect to Device");
            return;
        }

        UpdateDebugText("Subscribed Successfully");

        _subscribe = new SubscribeToCharacteristic(_deviceUuid, "1848", "03c4", HandleCharacteristicValueChanged);
        BleManager.Instance.QueueCommand(_subscribe);
    }

    public void UpdateDebugText(string text)
    {
        if (debugTextField != null)
        {
            debugTextField.text = text;
        }
    }

    public void HandleCharacteristicValueChanged(byte[] value)
    {
        if (_controlManager == null)
        {
            FindControlManager();
        }

        if (_controlManager != null)
        {
            _controlManager.ReceiveBytesArray(value);
        }
        else
        {
            Debug.LogError("InputControlManager not found. Cannot process input data.");
        }

        // Update debug display with a sample value
        if (value.Length > 10)
        {
            _bytestring = $"JS1: {value[0]},{value[1]} | BTN: {value[10]}";
            UpdateDebugText(_bytestring);
        }
    }

    public void UnsubscribeFromCharacteristic()
    {
        if (_subscribe != null)
        {
            // Stop listening for characteristic value changes
            _subscribe.Unsubscribe();
            UpdateDebugText("Unsubscribed Successfully");
        }
    }

    private void FindControlManager()
    {
        // Attempt to find the InputControlManager using the tag
        GameObject managerObject = GameObject.FindWithTag(inputManagerTag);

        // Check if object was found
        if (managerObject == null)
        {
            Debug.LogError("No GameObject found with the tag: " + inputManagerTag);
            return;
        }

        _controlManager = managerObject.GetComponent<InputControlManager>();
        
        // Check if component was found
        if (_controlManager == null)
        {
            Debug.LogError("InputControlManager component not found on the GameObject with tag: " + inputManagerTag);
        }
    }
}