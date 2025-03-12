using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRayControls : MonoBehaviour
{
    #region SerializeField
    [SerializeField] private GameObject cArmMainRotate;
    [SerializeField] private GameObject cArmSecondaryRotate;

    [SerializeField] private GameObject cArmSlideTranslate;
    [SerializeField] private GameObject cArmSlideMainRotate;
    [SerializeField] private GameObject cArmSlideSecondaryRotate;


    [SerializeField] private GameObject tableTop;

    [SerializeField] private AudioSource audioSource1;
    [SerializeField] private AudioClip continousSound1;
    [SerializeField] public List<GameObject> machines;
    #endregion
    private Vector3[] lastPositions;
    private Quaternion[] lastRotations;

    #region  Variables
    private float _tickRate; // set in Initialization/Awake
    // cArmMainRotate
    private float _camrMax = 90.0f; // Max End of Positive End Stop
    private float _camrMin = -90.0f; // Min End of Negative End Stop
    private float _camrRate; // set in Initialization/Awake

    // cArmSecondaryRotate
    private float _casrMax = 22.0f; // Max End of Positive End Stop
    private float _casrMin = -80.0f; // Min End of Negative End Stop
    private float _casrRate; // set in Initialization/Awake

    // cArmSlideTranslate
    private float _castMax = 2.35f; // Max End of Positive End Stop
    private float _castMin = -2.5f; // Min End of Negative End Stop
    private float _castRate; // set in Initialization/Awake

    // cArmSlideMainRotate
    private float _casmrMax = 25.0f; // Max End of Positive End Stop
    private float _casmrMin = -66.0f; // Min End of Negative End Stop
    private float _casmrRate; // set in Initialization/Awake

    // cArmSlideSecondaryRotate
    private float _cassrMax = 90.0f; // Max End of Positive End Stop
    private float _cassrMin = -90.0f; // Min End of Negative End Stop
    private float _cassrRate; // set in Initialization/Awake

    // Table Top
    private float _ttMax = 0.7f; // Max End of Positive End Stop
    private float _ttMin = 0.5f; // Min End of Negative End Stop
    private float _ttRate; // set in Initialization/Awake
    #endregion

    #region Joysticks
    private byte _joyStick1X, _joyStick1Y;
    private byte _joyStick2X, _joyStick2Y;
    private byte _joyStick3X, _joyStick3Y;
    private byte _joyStick4X, _joyStick4Y;
    private byte _joyStick5X, _joyStick5Y;
    #endregion

    #region Button Bytes
    private byte _buttonSet1, _buttonSet2, _buttonSet3, _buttonSet4;
    #endregion

    #region Button Bits into Bools
    private bool _buttonSet1Bit7,
        _buttonSet1Bit6,
        _buttonSet1Bit5,
        _buttonSet1Bit4,
        _buttonSet1Bit3,
        _buttonSet1Bit2,
        _buttonSet1Bit1,
        _buttonSet1Bit0;

    private bool _buttonSet2Bit7,
        _buttonSet2Bit6,
        _buttonSet2Bit5,
        _buttonSet2Bit4,
        _buttonSet2Bit3,
        _buttonSet2Bit2,
        _buttonSet2Bit1,
        _buttonSet2Bit0;

    private bool _buttonSet3Bit7,
        _buttonSet3Bit6,
        _buttonSet3Bit5,
        _buttonSet3Bit4,
        _buttonSet3Bit3,
        _buttonSet3Bit2,
        _buttonSet3Bit1,
        _buttonSet3Bit0;

    private bool _buttonSet4Bit7,
        _buttonSet4Bit6,
        _buttonSet4Bit5,
        _buttonSet4Bit4,
        _buttonSet4Bit3,
        _buttonSet4Bit2,
        _buttonSet4Bit1,
        _buttonSet4Bit0;
    #endregion

    #region Initialization
    private void Awake()
    {
        _tickRate = 0.02f; // millisecond tick rate
        _camrRate = 6.0f * _tickRate; // primary carm movment rate
        _casrRate = 6.0f * _tickRate; // secondary carm movement rate
        _castRate = 0.35f * _tickRate;
        _casmrRate = 6.0f * _tickRate;
        _cassrRate = 6.0f * _tickRate;
        _ttRate = 0.1f * _tickRate; // table top movement rate up and down
    }
    #endregion

    #region Audio
    private void Start()
    {
        // Check for missing AudioSource
        if (GetComponent<AudioSource>() == null)
        {
            Debug.LogWarning("Missing AudioSource component - adding one");
            audioSource1 = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            audioSource1 = GetComponent<AudioSource>();
        }
        
        // Check for missing audio clip
        if (continousSound1 == null)
        {
            Debug.LogError("Missing audio clip reference");
            // Don't assign null clip
        }
        else
        {
            audioSource1.clip = continousSound1;
        }

        // Check machines list
        if (machines == null || machines.Count == 0)
        {
            Debug.LogError("No machines assigned to XRayControls");
            lastPositions = new Vector3[0];
            lastRotations = new Quaternion[0];
            return;
        }

        // Initialize arrays with the initial positions and rotations
        lastPositions = new Vector3[machines.Count];
        lastRotations = new Quaternion[machines.Count];

        for (int i = 0; i < machines.Count; i++)
        {
            if (machines[i] == null)
            {
                Debug.LogError($"Machine at index {i} is null");
                continue;
            }
            lastPositions[i] = machines[i].transform.position;
            lastRotations[i] = machines[i].transform.rotation;
        }
    }

    private IEnumerator PlayContinuousSound1()
    {
        audioSource1.loop = true;
        audioSource1.Play();

        // Keep the loop active until stopped
        while (true)
        {
            yield return null;
        }
    }

    private bool IsMoving()
    {
        for (int i = 0; i < machines.Count; i++)
        {
            // Correctly use the loop variable i as the index
            if (machines[i].transform.position != lastPositions[i])
            {
                return true;
            }

            // Correctly use the loop variable i as the index
            if (machines[i].transform.rotation != lastRotations[i])
            {
                return true;
            }
        }

        return false;
    }

    private void Update()
    {
        if (IsMoving())
        {
            if (!audioSource1.isPlaying)
            {
                StartCoroutine(PlayContinuousSound1());
            }
        }
    }

    #endregion

    // Received information from X Ray Receiver
    public void ReceiveBytesArray(byte[] value)
    {
        // Assign joystick X and Y values
        _joyStick1X = value[0];
        _joyStick1Y = value[1];
        _joyStick2X = value[2];
        _joyStick2Y = value[3];
        _joyStick3X = value[4];
        _joyStick3Y = value[5];
        _joyStick4X = value[6];
        _joyStick4Y = value[7];
        _joyStick5X = value[8];
        _joyStick5Y = value[9];

        // Assign button set values
        _buttonSet1 = value[10];
        _buttonSet2 = value[11];
        _buttonSet3 = value[12];
        _buttonSet4 = value[13];

        // Check each bit from right to left (least significant to most significant)
        _buttonSet1Bit7 = (_buttonSet1 & 0x80) != 0 ? true : false;
        _buttonSet1Bit6 = (_buttonSet1 & 0x40) != 0 ? true : false;
        _buttonSet1Bit5 = (_buttonSet1 & 0x20) != 0 ? true : false;
        _buttonSet1Bit4 = (_buttonSet1 & 0x10) != 0 ? true : false;
        _buttonSet1Bit3 = (_buttonSet1 & 0x08) != 0 ? true : false;
        _buttonSet1Bit2 = (_buttonSet1 & 0x04) != 0 ? true : false;
        _buttonSet1Bit1 = (_buttonSet1 & 0x02) != 0 ? true : false;
        _buttonSet1Bit0 = (_buttonSet1 & 0x01) != 0 ? true : false;

        // Check each bit from right to left (least significant to most significant)
        _buttonSet2Bit7 = (_buttonSet2 & 0x80) != 0 ? true : false;
        _buttonSet2Bit6 = (_buttonSet2 & 0x40) != 0 ? true : false;
        _buttonSet2Bit5 = (_buttonSet2 & 0x20) != 0 ? true : false;
        _buttonSet2Bit4 = (_buttonSet2 & 0x10) != 0 ? true : false;
        _buttonSet2Bit3 = (_buttonSet2 & 0x08) != 0 ? true : false;
        _buttonSet2Bit2 = (_buttonSet2 & 0x04) != 0 ? true : false;
        _buttonSet2Bit1 = (_buttonSet2 & 0x02) != 0 ? true : false;
        _buttonSet2Bit0 = (_buttonSet2 & 0x01) != 0 ? true : false;

        // Check each bit from right to left (least significant to most significant)
        _buttonSet3Bit7 = (_buttonSet3 & 0x80) != 0 ? true : false;
        _buttonSet3Bit6 = (_buttonSet3 & 0x40) != 0 ? true : false;
        _buttonSet3Bit5 = (_buttonSet3 & 0x20) != 0 ? true : false;
        _buttonSet3Bit4 = (_buttonSet3 & 0x10) != 0 ? true : false;
        _buttonSet3Bit3 = (_buttonSet3 & 0x08) != 0 ? true : false;
        _buttonSet3Bit2 = (_buttonSet3 & 0x04) != 0 ? true : false;
        _buttonSet3Bit1 = (_buttonSet3 & 0x02) != 0 ? true : false;
        _buttonSet3Bit0 = (_buttonSet3 & 0x01) != 0 ? true : false;

        // Check each bit from right to left (least significant to most significant)
        _buttonSet4Bit7 = (_buttonSet4 & 0x80) != 0 ? true : false;
        _buttonSet4Bit6 = (_buttonSet4 & 0x40) != 0 ? true : false;
        _buttonSet4Bit5 = (_buttonSet4 & 0x20) != 0 ? true : false;
        _buttonSet4Bit4 = (_buttonSet4 & 0x10) != 0 ? true : false;
        _buttonSet4Bit3 = (_buttonSet4 & 0x08) != 0 ? true : false;
        _buttonSet4Bit2 = (_buttonSet4 & 0x04) != 0 ? true : false;
        _buttonSet4Bit1 = (_buttonSet4 & 0x02) != 0 ? true : false;
        _buttonSet4Bit0 = (_buttonSet4 & 0x01) != 0 ? true : false;

        MoveMachines();
    }

    //Execution of events
    private void MoveMachines()
    {
        //
        RotateObject(MapJoystickValue(_joyStick1X), _camrRate, _camrMin, _camrMax, "z", cArmMainRotate);
        
        //
        RotateObject(MapJoystickValue(_joyStick1Y), _casrRate, _casrMin, _casrMax, "x", cArmSecondaryRotate);
        
        // Translate C Arm Slide Forward Backwards
        TranslateObject(_buttonSet1Bit2, _buttonSet1Bit0, _castRate, _castMax, _castMin, cArmSlideTranslate, "z");
        
        // 
        TranslateObjectwJoystick(MapJoystickValue(_joyStick3X), _castRate, _castMin, _castMax, "z", cArmSlideTranslate);
        
        //
        RotateObject(MapJoystickValue(_joyStick2X), _casmrRate, _casmrMin, _casmrMax, "z", cArmSlideMainRotate);
        
        //
        RotateObject(MapJoystickValue(_joyStick2Y), _cassrRate, _cassrMin, _cassrMax, "y", cArmSlideSecondaryRotate);
        
        // Translate Table Up and Down
        TranslateObject(_buttonSet2Bit4, _buttonSet2Bit1, _ttRate, _ttMax, _ttMin, tableTop, "y");
    }

    // Handles the rate and minimum and maximum stop points
    public void RotateObject(float joystickInput, float rate, float min, float max, string rotationAxis, GameObject obj)
    {
        // Scale the joystick input using camrRate for responsiveness
        float clampedInput = joystickInput * rate;

        // Rotate object on the specified axis smoothly
        switch (rotationAxis.ToLower())
        {
            case "x":
                if ((obj.transform.rotation.x + clampedInput) > min && clampedInput < 0)
                {
                    obj.transform.Rotate(clampedInput, 0, 0);
                }

                if ((obj.transform.rotation.x + clampedInput) < max && clampedInput > 0)
                {
                    obj.transform.Rotate(clampedInput, 0, 0);
                }
                break;
            case "y":
                if ((obj.transform.rotation.y + clampedInput) > min && clampedInput < 0)
                {
                    obj.transform.Rotate(0, clampedInput, 0);
                }

                if ((obj.transform.rotation.y + clampedInput) < max && clampedInput > 0)
                {
                    obj.transform.Rotate(0, clampedInput, 0);
                }
                break;
            case "z":
                if ((obj.transform.rotation.z + clampedInput) > min && clampedInput < 0)
                {
                    obj.transform.Rotate(0, 0, clampedInput);
                }

                if ((obj.transform.rotation.z + clampedInput) < max && clampedInput > 0)
                {
                    obj.transform.Rotate(0, 0, clampedInput);
                }

                break;
            default:
                Debug.LogError("Invalid rotation axis specified.");
                return;
        }
    }

    // Normalizes Joystick info from 0-255 to -1-1
    public static float MapJoystickValue(byte rawValue)
    {
        // Define the deadzone range in terms of byte values
        const byte deadZoneStart = 113;
        const byte deadZoneEnd = 133;

        // Check if the value is within the deadzone
        if (rawValue is >= deadZoneStart and <= deadZoneEnd)
        {
            // If within the deadzone, return 0.0
            return 0.0f;
        }
        else
        {
            // Otherwise, map the value to the desired output range (-1.0 to 0.0 for 0-123, 0.0 to 1.0 for 123-255
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

    // Overloaded version for joystick 4 (which behaves differently)
    public static float MapJoystick4Value(byte rawValue, bool isYAxis)
    {
        // Define deadzone
        const byte DEADZONE_MIN = 100;
        const byte DEADZONE_MAX = 115;
        
        // Return 0 for deadzone values
        if (rawValue >= DEADZONE_MIN && rawValue <= DEADZONE_MAX)
        {
            return 0.0f;
        }
        
        if (isYAxis)
        {
            // Y-Axis: Forward = -126 to -1, Backward = 100 to 0
            if (rawValue >= 128) // Negative values (forward)
            {
                byte adjustedValue = (byte)(rawValue - 128);
                return Mathf.Lerp(1.0f, 0.0f, adjustedValue / 125f);
            }
            else // Positive values (backward)
            {
                return Mathf.Lerp(0.0f, -1.0f, (DEADZONE_MIN - rawValue) / DEADZONE_MIN);
            }
        }
        else
        {
            // X-Axis: Left = 100 to 0, Right = -126 to -17
            if (rawValue >= 128) // Negative values (right)
            {
                byte adjustedValue = (byte)(rawValue - 128);
                // Map from -126 to -17 (128-239) to -1.0 to 0.0
                if (adjustedValue >= 110) // -17 or higher (239 or lower in byte)
                {
                    return 0.0f; // Close to center
                }
                return Mathf.Lerp(-1.0f, 0.0f, (float)adjustedValue / 110f);
            }
            else // Positive values (left)
            {
                return Mathf.Lerp(0.0f, 1.0f, (DEADZONE_MIN - rawValue) / DEADZONE_MIN);
            }
        }
    }



    // Handles the rate, minimum, and maximum stop points for Translation
    public void TranslateObject(bool moveUp, bool moveDown, float rate, float max, float min, GameObject obj, string rotationAxis)
    {
        float translationAmount = rate;

        if (moveUp)
        {
            translationAmount = rate;
        }
        else if (moveDown)
        {
            translationAmount = -rate;
        }
        else if (moveDown && moveUp)
        {
            translationAmount = 0.0f;
        }
        else
        {
            return;
        }

        Debug.LogError("Translateion Amount " + translationAmount);
        Debug.LogError("Position: " + obj.transform.position + ", Rotation: " + obj.transform.rotation);        // Rotate on the specified axis smoothly

        switch (rotationAxis.ToLower())
        {
            case "x":
                if ((obj.transform.position.x + translationAmount) > min && translationAmount < 0)
                {
                    obj.transform.Translate(translationAmount, 0, 0);
                }

                if ((obj.transform.position.x + translationAmount) < max && translationAmount > 0)
                {
                    obj.transform.Translate(translationAmount, 0, 0);
                }
                break;
            case "y":
                if ((obj.transform.position.y + translationAmount) > min && translationAmount < 0)
                {
                    obj.transform.Translate(0, translationAmount, 0);
                }

                if ((obj.transform.position.y + translationAmount) < max && translationAmount > 0)
                {
                    obj.transform.Translate(0, translationAmount, 0);
                }
                break;
            case "z":
                if ((obj.transform.position.z + translationAmount) > min && translationAmount < 0)
                {
                    obj.transform.Translate(0, 0, translationAmount);
                }

                if ((obj.transform.position.z + translationAmount) < max && translationAmount > 0)
                {
                    obj.transform.Translate(0, 0, translationAmount);
                }

                break;
            default:
                Debug.LogError("Invalid Translation axis specified.");
                return;
        }
    }

    public void TranslateObjectwJoystick(float joystickInput, float rate, float min, float max, string translatenAxis, GameObject obj)
    {
        // Scale the joystick input using camrRate for responsiveness
        float clampedInput = joystickInput * rate;

        // Translate object on the specified axis smoothly
        switch (translatenAxis.ToLower())
        {
            case "x":
                if ((obj.transform.position.x + clampedInput) > min && clampedInput < 0)
                {
                    obj.transform.Translate(clampedInput, 0, 0);
                }

                if ((obj.transform.position.x + clampedInput) < max && clampedInput > 0)
                {
                    obj.transform.Translate(clampedInput, 0, 0);
                }
                break;
            case "y":
                if ((obj.transform.position.y + clampedInput) > min && clampedInput < 0)
                {
                    obj.transform.Translate(0, clampedInput, 0);
                }

                if ((obj.transform.position.y + clampedInput) < max && clampedInput > 0)
                {
                    obj.transform.Translate(0, clampedInput, 0);
                }
                break;
            case "z":
                if ((obj.transform.position.z + clampedInput) > min && clampedInput < 0)
                {
                    obj.transform.Translate(0, 0, clampedInput);
                }

                if ((obj.transform.position.z + clampedInput) < max && clampedInput > 0)
                {
                    obj.transform.Translate(0, 0, clampedInput);
                }

                break;
            default:
                Debug.LogError("Invalid position axis specified.");
                return;
        }
    }
}