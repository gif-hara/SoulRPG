using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameEnemyView
    {
        public static async UniTask OpenAsync(HKUIDocument documentPrefab, MasterData.Enemy masterDataEnemy, BattleCharacter enemy, CancellationToken scope)
        {
            var document = Object.Instantiate(documentPrefab);
            var sequenceDocument = document.Q<HKUIDocument>("Sequences");
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            document.Q<Image>("Image").sprite = masterDataEnemy.Thumbnail;
            enemy.Events.OnTakeDamage
                .Subscribe(_ =>
                {
                    sequenceDocument.Q<SequenceMonobehaviour>("Animation.OnTakeDamage").PlayAsync().Forget();
                })
                .RegisterTo(scope);
            enemy.Events.OnDeadMessage
                .Subscribe(_ =>
                {
                    sequenceDocument.Q<SequenceMonobehaviour>("Animation.OnDeadMessage").PlayAsync().Forget();
                })
                .RegisterTo(scope);
            gameEvents.OnRequestChangeEnemySprite
                .Subscribe(sprite =>
                {
                    document.Q<Image>("Image").sprite = sprite;
                })
                .RegisterTo(scope);
            await scope.WaitUntilCanceled();
            Object.Destroy(document.gameObject);
        }
    }
}
