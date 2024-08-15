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
    public sealed class InputScheme
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

        public InputScheme(CancellationToken scope)
        {
            InputSystem.onEvent += OnEvent;
            scope.Register(() => InputSystem.onEvent -= OnEvent);
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
