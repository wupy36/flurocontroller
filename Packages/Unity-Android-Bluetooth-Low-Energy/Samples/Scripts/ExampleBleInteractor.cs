using Android.BLE;
using Android.BLE.Commands;
using UnityEngine;
using UnityEngine.Scripting;

public class ExampleBleInteractor : MonoBehaviour
{
    [SerializeField]
    private GameObject _deviceButton;
    [SerializeField]
    private Transform _deviceList;

    [SerializeField]
    private int _scanTime = 10;

    private float _scanTimer = 0f;

    private bool _isScanning = false;

    private AppPermissions permissionsManager;
    private bool permissionsGranted = false;


    private void Awake()
    {
        // Find or add AppPermissions
        permissionsManager = FindObjectOfType<AppPermissions>();
        if (permissionsManager == null)
        {
            GameObject permissionsObj = new GameObject("AppPermissions");
            permissionsManager = permissionsObj.AddComponent<AppPermissions>();
        }

        // Subscribe to the permissions granted event
        permissionsManager.allPermissionsGrantedEvent.AddListener(OnPermissionsGranted);
    }

    private void Start()
    {
        // Instead of starting scan immediately, first check permissions
        permissionsManager.CheckPermissions();
        
        // Debug logging to help track the flow
        Debug.Log("Checking permissions before BLE scan");
    }

    private void OnPermissionsGranted()
    {
        permissionsGranted = true;
        Debug.Log("Permissions granted, now scanning for BLE devices");
        ScanForDevices();
    }

    public void ScanForDevices()
    {
        if (!permissionsGranted)
        {
            Debug.LogError("Cannot scan for devices - permissions not granted");
            return;
        }

        if (!_isScanning)
        {
            Debug.Log("Starting BLE scan");
            _isScanning = true;
            BleManager.Instance.QueueCommand(new DiscoverDevices(OnDeviceFound, _scanTime * 1000));
        }
    }

    private void Update()
    {
        if (_isScanning)
        {
            _scanTimer += Time.deltaTime;
            if (_scanTimer > _scanTime)
            {
                _scanTimer = 0f;
                _isScanning = false;
            }
        }
    }

    private void OnDeviceFound(string name, string device)
    {
        Debug.LogError("Device: " + device);

        if (device.Contains("Fluoro"))
        {
            DeviceButton button = Instantiate(_deviceButton, _deviceList).GetComponent<DeviceButton>();
            button.Show(name, device);
            Debug.LogError("Connected Button: " + device);
            button.Connect();
        }
    }
}
