using System;
using Android.BLE;
using Android.BLE.Commands;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class DeviceButton : MonoBehaviour
{
    private string _deviceUuid = string.Empty;
    private string _deviceName = string.Empty;

    [SerializeField]
    private Text _deviceUuidText;
    [SerializeField]
    private Text _deviceNameText;

    [SerializeField]
    private Image _deviceButtonImage;
    [SerializeField]
    private Text _deviceButtonText;

    [SerializeField]
    private Color _onConnectedColor;
    private Color _previousColor;

    [SerializeField] private string _xRayReceiverTag = "XRayReceiver";
    private XRayReceiver _xRayReceiver;
    
    private bool _isConnected = false;

    private ConnectToDevice _connectCommand;
    private ReadFromCharacteristic _readFromCharacteristic;

    public void Show(string uuid, string name)
    {
        _deviceButtonText.text = "Connect";

        _deviceUuid = uuid;
        _deviceName = name;

        _deviceUuidText.text = uuid;
        _deviceNameText.text = name;
    }

    public void Connect()
    {
        if (!_isConnected)
        {
            _connectCommand = new ConnectToDevice(_deviceUuid, OnConnected, OnDisconnected);
            BleManager.Instance.QueueCommand(_connectCommand);
        }
        else
        {
            _connectCommand.Disconnect();
        }
    }

    public void SubscribeToExampleService()
    {
        //Replace these Characteristics with YOUR device's characteristics
        _readFromCharacteristic = new ReadFromCharacteristic(_deviceUuid, "1848", "03C4", (byte[] value) =>
        {
            Debug.Log(Encoding.UTF8.GetString(value));
        });
        BleManager.Instance.QueueCommand(_readFromCharacteristic);
    }

    private void OnConnected(string deviceUuid)
    {
        _previousColor = _deviceButtonImage.color;
        _deviceButtonImage.color = _onConnectedColor;

        _isConnected = true;
        _deviceButtonText.text = "Disconnect";

        //SubscribeToExampleService();
        ConnectToReceiver();
    }

    private void OnDisconnected(string deviceUuid)
    {
        _deviceButtonImage.color = _previousColor;

        _isConnected = false;
        _deviceButtonText.text = "Connect";
    }
    
    private void ConnectToReceiver()
    {
        // Attempt to find the ReceiverScript using the tag
        GameObject receiverObject = GameObject.FindWithTag(_xRayReceiverTag);
        
        // Checking object found
        if (receiverObject == null)
        {
            Debug.LogError("No GameObject found with the tag: " + _xRayReceiverTag);
            return;
        }
        
        _xRayReceiver = receiverObject.GetComponent<XRayReceiver>();
        // Checking Script
        if (_xRayReceiver == null)
        {
            Debug.LogError("ReceiverScript not found on the GameObject with tag: " + _xRayReceiverTag);
            return;
        }
        
        _xRayReceiver.ReceiveUuid(_deviceUuid);
    }
}
