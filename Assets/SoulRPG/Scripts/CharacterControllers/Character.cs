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

        public Define.AllyType AllyType { get; }

        public MasterData.Enemy MasterDataEnemy { get; }

        public bool FindPlayer { get; set; }

        private readonly CancellationTokenSource lifeScopeSource = new();

        public CancellationToken LifeScope => lifeScopeSource.Token;

        public Character(string name, CharacterGrowthParameter growthParameter, EquipmentBlueprint equipmentBlueprint, Define.CharacterAttribute attribute)
        {
            Name = name;
            GrowthParameter = growthParameter;
            Equipment = new Equipment(this, equipmentBlueprint);
            InstanceStatus = new CharacterInstanceStatus(this, growthParameter);
            Inventory = new Inventory(this);
            Attribute = attribute;
            Equipment.BeginObserve(this);
            AllyType = Define.AllyType.Player;
        }

        public Character(MasterData.Enemy masterDataEnemy)
        {
            MasterDataEnemy = masterDataEnemy;
            AllyType = Define.AllyType.Enemy;
        }

        public bool Move(Vector2Int velocity)
        {
            var dungeonController = TinyServiceLocator.Resolve<DungeonController>();
            if (!dungeonController.CanMove(Position, velocity.ToDirection()))
            {
                return false;
            }
            Position += velocity;
            return true;
        }

        public void Warp(Vector2Int position)
        {
            Position = position;
        }

        public void ResetAll()
        {
            var gameRule = TinyServiceLocator.Resolve<GameRule>();
            Inventory.Sync(gameRule.InitialItemDatabase);
            Equipment.Sync(gameRule.InitialEquipment);
            GrowthParameter.Sync(gameRule.PlayerGrowthParameter, this);
            InstanceStatus.ResetExperience();
            InstanceStatus.FullRecovery();
        }

        public void Dispose()
        {
            lifeScopeSource.Cancel();
            lifeScopeSource.Dispose();
        }

        public void SyncFromSaveData(SaveData saveData)
        {
            Position = saveData.suspendData.playerPosition;
            Direction = saveData.suspendData.playerDirection;
            Inventory.SyncFromSaveData(saveData.suspendData.playerItemData);
            Equipment.SyncFromSaveData(saveData.suspendData.playerEquipmentData);
            InstanceStatus.SyncFromSaveData(saveData.suspendData.playerInstanceData);
            GrowthParameter.Sync(saveData.suspendData.growthParameter, this);
        }
    }
}
