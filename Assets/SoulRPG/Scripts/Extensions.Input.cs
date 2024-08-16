using HK;
using UnityEngine.InputSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static string GetTag(this InputAction self)
        {
            return InputSprite.GetTag(self, TinyServiceLocator.Resolve<InputScheme>());
        }
    }
}
