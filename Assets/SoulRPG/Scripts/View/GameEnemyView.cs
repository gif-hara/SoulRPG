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
        private readonly HKUIDocument document;

        public GameEnemyView(HKUIDocument documentPrefab, CancellationToken scope)
        {
            TinyServiceLocator.Register(this);
            document = Object.Instantiate(documentPrefab);
            document.gameObject.SetActive(false);
            scope.Register(() =>
            {
                TinyServiceLocator.Remove<GameEnemyView>();
                if (document == null)
                {
                    return;
                }
                Object.Destroy(document.gameObject);
            });
        }

        public UniTask BeginAnimationAsync(string animationName)
        {
            return document.Q<HKUIDocument>("Sequences").Q<SequenceMonobehaviour>(animationName).PlayAsync();
        }

        public void Open(MasterData.Enemy masterDataEnemy, BattleCharacter enemy, CancellationToken scope)
        {
            document.gameObject.SetActive(true);
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
        }
    }
}
