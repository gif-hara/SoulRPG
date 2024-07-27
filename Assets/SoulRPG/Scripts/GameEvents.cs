using Cysharp.Threading.Tasks;
using R3;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameEvents
    {
        public readonly Subject<string> OnRequestShowMessage = new();

        public readonly Subject<Unit> OnSubmitInput = new();

        public readonly Subject<bool> OnRequestSetActiveMessageArrow = new();

        public async UniTask WaitForSubmitInputAsync()
        {
            OnRequestSetActiveMessageArrow.OnNext(true);
            await OnSubmitInput.FirstAsync();
            OnRequestSetActiveMessageArrow.OnNext(false);
        }
    }
}
