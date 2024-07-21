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
        private ReactiveProperty<Vector2Int> position = new();

        public Vector2Int Position
        {
            get => position.Value;
            private set => position.Value = value;
        }

        public ReadOnlyReactiveProperty<Vector2Int> PositionAsObservable() => position;

        private ReactiveProperty<Define.Direction> direction = new();

        public Define.Direction Direction
        {
            get => direction.Value;
            set => direction.Value = value;
        }

        public ReadOnlyReactiveProperty<Define.Direction> DirectionAsObservable() => direction;

        private MasterData.Dungeon dungeon;

        public void Move(Vector2Int velocity)
        {
            Assert.IsNotNull(dungeon, "Dungeon is null");
            if (dungeon.IsExistWall(Position, velocity.ToDirection()))
            {
                return;
            }
            Position += velocity;
        }

        public void Warp(Vector2Int position)
        {
            Position = position;
        }

        public void SetDungeon(MasterData.Dungeon dungeon, Vector2Int position)
        {
            this.dungeon = dungeon;
            Warp(position);
        }
    }
}
