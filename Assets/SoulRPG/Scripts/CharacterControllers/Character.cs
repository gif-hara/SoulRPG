using R3;
using UnityEngine;

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
            set => position.Value = value;
        }

        public ReadOnlyReactiveProperty<Vector2Int> PositionAsObservable() => position;

        private ReactiveProperty<Define.Direction> direction = new();

        public Define.Direction Direction
        {
            get => direction.Value;
            set => direction.Value = value;
        }

        public ReadOnlyReactiveProperty<Define.Direction> DirectionAsObservable() => direction;

        public void Move(Vector2Int velocity)
        {
            Position += velocity;
        }
    }
}
