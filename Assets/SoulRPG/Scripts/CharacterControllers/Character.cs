using System;
using System.Threading;
using HK;
using R3;
using UnityEngine;

namespace SoulRPG.CharacterControllers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Character : IDisposable
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

        public Inventory Inventory { get; }

        public Equipment Equipment { get; }

        public CharacterEvents Events { get; } = new();

        public CharacterGrowthParameter GrowthParameter { get; } = new();

        public CharacterInstanceStatus InstanceStatus { get; }

        public Define.CharacterAttribute Attribute { get; }

        public string Name { get; }

        private readonly CancellationTokenSource lifeScopeSource = new();

        public CancellationToken LifeScope => lifeScopeSource.Token;

        public Character(string name, CharacterGrowthParameter growthParameter, EquipmentBlueprint equipmentBlueprint, Define.CharacterAttribute attribute)
        {
            Name = name;
            GrowthParameter = growthParameter;
            InstanceStatus = new CharacterInstanceStatus(this, growthParameter);
            Inventory = new Inventory(this);
            Equipment = new Equipment(equipmentBlueprint);
            Attribute = attribute;
            Equipment.BeginObserve(this);
        }

        public void Move(Vector2Int velocity)
        {
            var dungeonController = TinyServiceLocator.Resolve<DungeonController>();
            if (!dungeonController.CanMove(Position, velocity.ToDirection()))
            {
                return;
            }
            Position += velocity;
        }

        public void Warp(Vector2Int position)
        {
            Position = position;
        }

        public void Dispose()
        {
            lifeScopeSource.Cancel();
            lifeScopeSource.Dispose();
        }
    }
}
