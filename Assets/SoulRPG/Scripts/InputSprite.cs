using System.Linq;
using System.Threading;
using R3;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

namespace HK
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class InputSprite
    {
        public enum InputSchemeType
        {
            KeyboardAndMouse,
            GamePad
        }

        private readonly ReactiveProperty<InputSchemeType> inputSchemeTypeReactiveProperty = new();
        public ReadOnlyReactiveProperty<InputSchemeType> InputSchemeTypeReactiveProperty => inputSchemeTypeReactiveProperty;
        public InputSchemeType CurrentInputSchemeType => inputSchemeTypeReactiveProperty.Value;

        private readonly ReactiveProperty<Gamepad> gamepadReactiveProperty = new();
        public ReadOnlyReactiveProperty<Gamepad> GamepadReactiveProperty => gamepadReactiveProperty;
        public Gamepad CurrentGamepad => gamepadReactiveProperty.Value;

        public InputSprite(CancellationToken scope)
        {
            InputSystem.onEvent += OnEvent;
            scope.Register(() => InputSystem.onEvent -= OnEvent);
        }

        public string GetTag(InputAction action)
        {
            var schemeName = CurrentInputSchemeType switch
            {
                InputSchemeType.KeyboardAndMouse => "Keyboard&Mouse",
                InputSchemeType.GamePad => "GamePad",
                _ => "Unknown"
            };

            var bindingMask = InputBinding.MaskByGroup(schemeName);

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
                    if (control.device is Gamepad && CurrentGamepad != null)
                    {
                        if (control.device != CurrentGamepad)
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
                        SwitchProControllerHID => "SwitchProController",
                        _ => "Unknown"
                    };
                    var controlPathContent = control.path.Substring(control.device.name.Length + 2);
                    var iconName = $"{deviceIconGroup}-{controlPathContent}";
                    var spriteIndex = TMP_Settings.GetSpriteAsset().GetSpriteIndexFromName(iconName);
                    return spriteIndex >= 0 ? $"<sprite={spriteIndex}>" : $"<sprite=UnknownTag iconName:{iconName}>";
                }
            }

            return $"<sprite=UnknownTag schemeName:{schemeName} action:{action.name}>";
        }

        private void OnEvent(InputEventPtr inputEventPtr, InputDevice inputDevice)
        {
            var eventType = inputEventPtr.type;
            if (eventType != StateEvent.Type && eventType != DeltaStateEvent.Type)
            {
                return;
            }

            var controls = inputEventPtr.EnumerateControls(
                InputControlExtensions.Enumerate.IncludeNonLeafControls |
                InputControlExtensions.Enumerate.IncludeSyntheticControls |
                InputControlExtensions.Enumerate.IgnoreControlsInCurrentState |
                InputControlExtensions.Enumerate.IgnoreControlsInDefaultState
                );
            var anyControl = controls.GetEnumerator().MoveNext();
            if (!anyControl)
            {
                return;
            }

            if (inputDevice is Gamepad gamepad && gamepadReactiveProperty.Value != gamepad)
            {
                gamepadReactiveProperty.Value = gamepad;
            }

            var newSchemeType = inputDevice switch
            {
                Keyboard or Mouse => InputSchemeType.KeyboardAndMouse,
                Gamepad => InputSchemeType.GamePad,
                _ => CurrentInputSchemeType
            };

            if (newSchemeType != CurrentInputSchemeType)
            {
                inputSchemeTypeReactiveProperty.Value = newSchemeType;
            }
        }
    }
}
