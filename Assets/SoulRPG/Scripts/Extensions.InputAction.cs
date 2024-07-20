using R3;
using UnityEngine.InputSystem;

namespace HK
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static Observable<InputAction.CallbackContext> OnPerformedAsObservable(this InputAction inputAction)
        {
            return Observable.FromEvent<InputAction.CallbackContext>(
                h => inputAction.performed += h,
                h => inputAction.performed -= h
            );
        }

        public static Observable<InputAction.CallbackContext> OnCanceledAsObservable(this InputAction inputAction)
        {
            return Observable.FromEvent<InputAction.CallbackContext>(
                h => inputAction.canceled += h,
                h => inputAction.canceled -= h
            );
        }

        public static Observable<InputAction.CallbackContext> OnStartedAsObservable(this InputAction inputAction)
        {
            return Observable.FromEvent<InputAction.CallbackContext>(
                h => inputAction.started += h,
                h => inputAction.started -= h
            );
        }
    }
}