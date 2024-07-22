using HK;
using R3;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoulRPG.CharacterControllers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Character
    {
        private readonly ReactiveProperty<Vector2Int> position = new();

        public Vector2Int Position
        {
            get => position.Value;
            private set => position.Value = value;
        }

        public ReadOnlyReactiveProperty<Vector2Int> PositionAsObservable() => position;

        private readonly ReactiveProperty<Define.Direction> direction = new();

        public Define.Direction Direction
        {
            get => direction.Value;
            set => direction.Value = value;
        }

        public ReadOnlyReactiveProperty<Define.Direction> DirectionAsObservable() => direction;

        private readonly Inventory inventory = new();

        public Inventory Inventory => inventory;

        public void Move(Vector2Int velocity)
        {
            var dungeonController = TinyServiceLocator.Resolve<DungeonController>();
            if (dungeonController.CurrentDungeon.IsExistWall(Position, velocity.ToDirection()))
            {
                return;
            }
            Position += velocity;
        }

        public void Warp(Vector2Int position)
        {
            Position = position;
        }
    }
}
