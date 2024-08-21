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
        public static void Attach(Character enemy, Character player, DungeonController dungeonController)
        {
            var stateMachine = new TinyStateMachine();
            var gameEvents = TinyServiceLocator.Resolve<GameEvents>();
            enemy.LifeScope.Register(() =>
            {
                stateMachine.Dispose();
            });

            stateMachine.Change(StateIdleAsync);

            UniTask StateIdleAsync(CancellationToken cancellationToken)
            {
                player.PositionAsObservable()
                    .Subscribe(x =>
                    {
                        if (
                            CanSeePlayer(Define.Direction.Up) ||
                            CanSeePlayer(Define.Direction.Down) ||
                            CanSeePlayer(Define.Direction.Left) ||
                            CanSeePlayer(Define.Direction.Right)
                            )
                        {
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
                var canMove = true;
                Observable.Interval(TimeSpan.FromSeconds(enemy.MasterDataEnemy.MoveIntervalSeconds))
                    .Subscribe(_ =>
                    {
                        if (!canMove)
                        {
                            return;
                        }
                        var diff = player.Position - enemy.Position;
                        var directions = new List<Define.Direction>();
                        if (diff.x > 0 && dungeonController.CanMove(enemy.Position, Define.Direction.Right))
                        {
                            directions.Add(Define.Direction.Right);
                        }
                        else if (diff.x < 0 && dungeonController.CanMove(enemy.Position, Define.Direction.Left))
                        {
                            directions.Add(Define.Direction.Left);
                        }
                        if (diff.y > 0 && dungeonController.CanMove(enemy.Position, Define.Direction.Up))
                        {
                            directions.Add(Define.Direction.Up);
                        }
                        else if (diff.y < 0 && dungeonController.CanMove(enemy.Position, Define.Direction.Down))
                        {
                            directions.Add(Define.Direction.Down);
                        }
                        var direction = directions[UnityEngine.Random.Range(0, directions.Count)];
                        enemy.Move(direction.ToVector2Int());
                        if (enemy.Position == player.Position)
                        {
                            dungeonController.BeginForceBattle(player, enemy);
                        }
                    })
                    .RegisterTo(cancellationToken);
                gameEvents.OnBeginBattle
                    .Subscribe(_ =>
                    {
                        canMove = false;
                    })
                    .RegisterTo(cancellationToken);
                gameEvents.OnEndBattle
                    .Subscribe(_ =>
                    {
                        canMove = true;
                    })
                    .RegisterTo(cancellationToken);
                return UniTask.CompletedTask;
            }
        }
    }
}
