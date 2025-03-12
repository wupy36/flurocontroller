using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;


    [StructLayout(LayoutKind.Explicit, Size = 6)]
    public struct XRayControllerState : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('H', 'I', 'D');
        
        [FieldOffset(0)] public byte reportId;
        
        [InputControl(name = "leftStick", layout = "Stick", format = "VC2B")]
        [InputControl(name = "leftStick/x", offset = 0, format = "BYTE",
            parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "leftStick/left", offset = 0, format = "BYTE",
            parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "leftStick/right", offset = 0, format = "BYTE",
            parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
        [InputControl(name = "leftStick/y", offset = 1, format = "BYTE",
            parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "leftStick/up", offset = 1, format = "BYTE",
            parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "leftStick/down", offset = 1, format = "BYTE",
            parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
        [FieldOffset(1)] public byte leftStickX;
        [FieldOffset(2)] public byte leftStickY;

        [InputControl(name = "rightStick", layout = "Stick", format = "VC2B")]
        [InputControl(name = "rightStick/x", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "rightStick/left", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "rightStick/right", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
        [InputControl(name = "rightStick/y", offset = 1, format = "BYTE", parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "rightStick/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "rightStick/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
        [FieldOffset(3)] public byte rightStickX;
        [FieldOffset(4)] public byte rightStickY;
        
        [InputControl(name = "tableUpButton", layout = "Button", bit = 0, displayName = "Table Up Button")]
        [InputControl(name = "tableDownButton", layout = "Button", bit = 1, displayName = "Table Down Button")]
        [InputControl(name = "cArmForwardButton", layout = "Button", bit = 2, displayName = "C Arm Forward Button")]
        [InputControl(name = "cArmBackwardButton", layout = "Button", bit = 3, displayName = "C Arm Backward Button")]
        [FieldOffset(5)] public byte buttons1;

    }
    
#if UNITY_EDITOR
    [InitializeOnLoad] 
#endif
    [InputControlLayout(stateType = typeof(XRayControllerState))]
    public class XRayController : InputDevice
    {
        static XRayController()
        {
            // Trigger our RegisterLayout code in the editor.
            Initialize();
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            //InputSystem.RegisterLayout<XRayController>( matches: new InputDeviceMatcher().WithCapability("productId", 0x11A1).WithCapability("vendorId", 0x2fa));
        }

        public ButtonControl tableUpButton { get; protected set; }
        public ButtonControl tableDownButton { get; protected set; }
        public ButtonControl cArmForwardButton { get; protected set; }
        public ButtonControl cArmBackwardButton { get; protected set; }
        public StickControl leftStick { get; protected set; }
        public StickControl rightStick { get; protected set; }
        
        protected override void FinishSetup()
        {
            base.FinishSetup();

            tableUpButton = GetChildControl<ButtonControl>("tableUpButton");
            tableDownButton = GetChildControl<ButtonControl>("tableDownButton");
            cArmForwardButton = GetChildControl<ButtonControl>("cArmForwardButton");
            cArmBackwardButton = GetChildControl<ButtonControl>("cArmBackwardButton");
            leftStick = GetChildControl<StickControl>("leftStick");
            rightStick = GetChildControl<StickControl>("rightStick");
        }
        
        public static XRayController current { get; private set; }
        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }
    }
