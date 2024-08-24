using System.Linq;
using R3;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
#if !UNITY_WEBGL
using UnityEngine.InputSystem.Switch;
#endif
using UnityEngine.InputSystem.XInput;

namespace HK
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class InputSprite
    {
        public static string GetTag(InputAction action, InputScheme inputScheme)
        {
            var schemeName = inputScheme.CurrentInputSchemeType switch
            {
                InputScheme.InputSchemeType.KeyboardAndMouse => "Keyboard&Mouse",
                InputScheme.InputSchemeType.GamePad => "Gamepad",
                _ => "Unknown"
            };

            var bindingMask = InputBinding.MaskByGroup(schemeName);
            var sb = new System.Text.StringBuilder();

            foreach (var binding in action.bindings)
            {
                if (!bindingMask.Matches(binding))
                {
                    continue;
                }
                var path = binding.effectivePath;
                var matchedControls = action.controls.Where(x => InputControlPath.Matches(path, x));
                foreach (var control in matchedControls)
                {
                    if (control is InputDevice)
                    {
                        continue;
                    }
                    if (control.device is Gamepad && inputScheme.CurrentGamepad != null)
                    {
                        if (control.device != inputScheme.CurrentGamepad)
                        {
                            continue;
                        }
                    }

                    var deviceIconGroup = control.device switch
                    {
                        Keyboard => "Keyboard",
                        Mouse => "Mouse",
                        XInputController => "XInputController",
                        DualShockGamepad => "DualShockGamepad",
#if !UNITY_WEBGL
                        SwitchProControllerHID => "SwitchProController",
#endif
                        _ => "DualShockGamepad"
                    };
                    var controlPathContent = control.path.Substring(control.device.name.Length + 2).Replace('/', '-');
                    sb.Append($"<sprite name={deviceIconGroup}-{controlPathContent}>");
                }
            }

            if (sb.Length > 0)
            {
                return sb.ToString();
            }
            else
            {
                return $"<sprite=UnknownTag schemeName:{schemeName} action:{action.name}>";
            }
        }

        public static string GetTag(InputAction action)
        {
            return GetTag(action, TinyServiceLocator.Resolve<InputScheme>());
        }
    }
}
