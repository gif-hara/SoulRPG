using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class EnemyController
    {
        public static void Attach(Character enemy, Character player, DungeonController dungeonController, bool findPlayer)
        {
            var stateMachine = new TinyStateMachine();
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            var inputController = TinyServiceLocator.Resolve<InputController>();
            enemy.LifeScope.Register(() =>
            {
                stateMachine.Dispose();
            });

            if (findPlayer)
            {
                stateMachine.Change(StateChaseAsync);
            }
            else
            {
                stateMachine.Change(StateIdleAsync);
            }

            UniTask StateIdleAsync(CancellationToken cancellationToken)
            {
                Observable.Merge(
                    player.PositionAsObservable().AsUnitObservable(),
                    gameEvents.OnOpenDoor
                    )
                    .Subscribe(x =>
                    {
                        if (
                            CanSeePlayer(Define.Direction.Up) ||
                            CanSeePlayer(Define.Direction.Down) ||
                            CanSeePlayer(Define.Direction.Left) ||
                            CanSeePlayer(Define.Direction.Right)
                            )
                        {
                            AudioManager.PlaySfx(TinyServiceLocator.Resolve<GameRule>().AudioDatabase.Get("Sfx.FindPlayer.0").Clip);
                            stateMachine.Change(StateChaseAsync);
                        }
                    })
                    .RegisterTo(cancellationToken);
                return UniTask.CompletedTask;

                bool CanSeePlayer(Define.Direction direction)
                {
                    var distance = enemy.MasterDataEnemy.FieldOfVision;
                    var playerPosition = player.Position;
                    var isHorizontal = direction == Define.Direction.Left || direction == Define.Direction.Right;
                    if (isHorizontal)
                    {
                        if (playerPosition.y != enemy.Position.y)
                        {
                            return false;
                        }
                        var diff = Mathf.Abs(playerPosition.x - enemy.Position.x);
                        if (distance < diff)
                        {
                            return false;
                        }
                        var position = enemy.Position;
                        for (var i = 0; i < distance; i++)
                        {
                            if (!dungeonController.CanMove(position, direction))
                            {
                                return false;
                            }
                            position += direction.ToVector2Int();
                            if (position == playerPosition)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (playerPosition.x != enemy.Position.x)
                        {
                            return false;
                        }
                        var diff = Mathf.Abs(playerPosition.x - enemy.Position.x);
                        if (distance < diff)
                        {
                            return false;
                        }
                        var position = enemy.Position;
                        for (var i = 0; i < distance; i++)
                        {
                            if (!dungeonController.CanMove(position, direction))
                            {
                                return false;
                            }
                            position += direction.ToVector2Int();
                            if (position == playerPosition)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }

            UniTask StateChaseAsync(CancellationToken cancellationToken)
            {
                enemy.FindPlayer = true;
                var canMove = true;
                Observable.Interval(TimeSpan.FromSeconds(enemy.MasterDataEnemy.MoveIntervalSeconds))
                    .Subscribe(_ =>
                    {
                        if (!canMove)
                        {
                            return;
                        }
                        var next = dungeonController.FindNextPosition(enemy.Position, player.Position);
                        if (!next.HasValue)
                        {
                            return;
                        }
                        var direction = next.Value - enemy.Position;
                        if(dungeonController.IsExistEnemy(enemy.Position + direction))
                        {
                            return;
                        }
                        var walkId = UnityEngine.Random.Range(0, 2);
                        AudioManager.PlaySfx(TinyServiceLocator.Resolve<GameRule>().AudioDatabase.Get($"Sfx.Walk.{walkId}").Clip);
                        enemy.Move(direction);
                        if (enemy.Position == player.Position)
                        {
                            dungeonController.BeginForceBattle(player, enemy);
                        }
                    })
                    .RegisterTo(cancellationToken);
                inputController.OnChangeInputType
                    .Subscribe(x =>
                    {
                        canMove = x == InputController.InputType.InGame;
                    })
                    .RegisterTo(cancellationToken);
                return UniTask.CompletedTask;
            }
        }
    }
}
