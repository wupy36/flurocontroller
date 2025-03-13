using Android.BLE;
using Android.BLE.Commands;
using UnityEngine;
using UnityEngine.Scripting;
using System.Collections;

public class BleAutoConnector : MonoBehaviour
{
    [SerializeField]
    private GameObject _deviceButton;
    [SerializeField]
    private Transform _deviceList;

    [SerializeField]
    private int _scanTime = 10;
    
    [SerializeField]
    private float _autoRescanDelay = 2f; // Delay between auto-rescans in seconds
    
    [SerializeField]
    private bool _autoRescanEnabled = true; // Toggle for auto-rescan functionality
    
    [SerializeField]
    private GameObject _scanningIndicator; // Optional UI element to show scanning status

    private float _scanTimer = 0f;
    private bool _isScanning = false;
    private bool _deviceConnected = false;
    private bool _permissionsGranted = false;

    private AppPermissions permissionsManager;

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
        
        // Initialize UI elements
        if (_scanningIndicator != null)
        {
            _scanningIndicator.SetActive(false);
        }
    }

    private void Start()
    {
        // First check permissions
        permissionsManager.CheckPermissions();
        Debug.Log("Checking permissions before BLE scan");
    }

    private void OnPermissionsGranted()
    {
        _permissionsGranted = true;
        Debug.Log("Permissions granted, now scanning for BLE devices");
        ScanForDevices();
    }

    public void ScanForDevices()
    {
        if (!_permissionsGranted)
        {
            Debug.LogError("Cannot scan for devices - permissions not granted");
            return;
        }

        if (!_isScanning && !_deviceConnected)
        {
            Debug.Log("Starting BLE scan");
            _isScanning = true;
            _scanTimer = 0f;
            
            // Show scanning indicator if available
            if (_scanningIndicator != null)
            {
                _scanningIndicator.SetActive(true);
            }
            
            BleManager.Instance.QueueCommand(new DiscoverDevices(OnDeviceFound, OnScanComplete, _scanTime * 1000));
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
                
                // Hide scanning indicator when scan completes
                if (_scanningIndicator != null)
                {
                    _scanningIndicator.SetActive(false);
                }
            }
        }
    }

    private void OnScanComplete()
    {
        Debug.Log("BLE scan completed");
        _isScanning = false;
        
        // Hide scanning indicator
        if (_scanningIndicator != null)
        {
            _scanningIndicator.SetActive(false);
        }
        
        // Auto-rescan if enabled, no device is connected, and we have permissions
        if (_autoRescanEnabled && !_deviceConnected && _permissionsGranted)
        {
            Debug.Log($"No device connected. Will rescan in {_autoRescanDelay} seconds");
            StartCoroutine(RescanAfterDelay());
        }
    }
    
    private IEnumerator RescanAfterDelay()
    {
        yield return new WaitForSeconds(_autoRescanDelay);
        
        // Double-check we still need to scan
        if (!_deviceConnected)
        {
            ScanForDevices();
        }
    }

    private void OnDeviceFound(string name, string device)
    {
        Debug.Log($"Device found: {device}");

        if (device.Contains("Fluoro"))
        {
            BleDeviceConnector button = Instantiate(_deviceButton, _deviceList).GetComponent<BleDeviceConnector>();
            button.Show(name, device);
            Debug.Log($"Connecting to device: {device}");
            button.SetBleInteractor(this);  // Pass a reference to this interactor
            button.Connect();
        }
    }
    
    // Called by DeviceButton when a connection is established
    public void OnDeviceConnected()
    {
        _deviceConnected = true;
        Debug.Log("Device connected successfully");
        StopAllCoroutines(); // Stop any pending rescans
    }
    
    // Called by DeviceButton when a device is disconnected
    public void OnDeviceDisconnected()
    {
        _deviceConnected = false;
        Debug.Log("Device disconnected");
        
        // Resume scanning if auto-rescan is enabled
        if (_autoRescanEnabled && _permissionsGranted)
        {
            StartCoroutine(RescanAfterDelay());
        }
    }
    
    // Public method to manually trigger a new scan (can be called from UI)
    public void ManualRescan()
    {
        StopAllCoroutines();
        ScanForDevices();
    }
}