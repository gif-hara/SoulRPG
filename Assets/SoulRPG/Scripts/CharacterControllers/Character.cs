using R3;
using UnityEngine;

namespace SoulRPG.CharacterControllers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Character
    {
        private ReactiveProperty<Vector2Int> position = new ReactiveProperty<Vector2Int>();

        public Vector2Int Position
        {
            get => position.Value;
            set => position.Value = value;
        }

        public ReadOnlyReactiveProperty<Vector2Int> PositionAsObservable() => position;
    }
}
