using Android.BLE;
using Android.BLE.Commands;
using UnityEngine;
using UnityEngine.UI;

public class XRayReceiver : MonoBehaviour
{
    [SerializeField] private Text xRayDebugField;

    private string _deviceUuid;

    private SubscribeToCharacteristic _subscribe;

    private string _bytestring = "no bytes";

    [SerializeField] private string xRayControlsTag = "XRayControls";
    private XRayControls _xRayControls;

    public void ReceiveUuid(string uuid)
    {
        _deviceUuid = uuid;
        SubscribeToCharacteristic();
    }

    public void SubscribeToCharacteristic()
    {
        if (string.IsNullOrEmpty(_deviceUuid))
        {
            xRayDebugField.text = "Connect to Device";
            return;
        }

        xRayDebugField.text = "Subscribed Successfully";

        _subscribe = new SubscribeToCharacteristic(_deviceUuid, "1848", "03c4", HandleCharacteristicValueChanged);

        BleManager.Instance.QueueCommand(_subscribe);
    }

    public void UpdateText()
    {
        xRayDebugField.text = _bytestring;
    }

    public void HandleCharacteristicValueChanged(byte[] value)
    {
        ConnectToReceiver(value);
        byte firstByte = value[10];
        byte secondByte = value[1];
        _bytestring = firstByte.ToString();
        UpdateText();
    }

    public void UnsubscribeFromCharacteristic()
    {
        if (_subscribe != null)
        {
            // Stop listening for characteristic value changes
            _subscribe.Unsubscribe();
            xRayDebugField.text = "Unsubscribed Successfully";
        }
    }

    private void ConnectToReceiver(byte[] value)
    {
        // Attempt to find the ReceiverScript using the tag
        GameObject receiverObject = GameObject.FindWithTag(xRayControlsTag);

        // Checking object found
        if (receiverObject == null)
        {
            Debug.LogError("No GameObject found with the tag: " + xRayControlsTag);
            return;
        }

        _xRayControls = receiverObject.GetComponent<XRayControls>();
        // Checking Script
        if (_xRayControls == null)
        {
            Debug.LogError("ReceiverScript not found on the GameObject with tag: " + xRayControlsTag);
            return;
        }

        _xRayControls.ReceiveBytesArray(value);
    }
}