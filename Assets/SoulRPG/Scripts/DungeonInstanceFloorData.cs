using System.Collections.Generic;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DungeonInstanceFloorData
    {
        public Vector2Int Position { get; }

        public string ViewName { get; }

        protected DungeonInstanceFloorData(Vector2Int position, string eventType)
        {
            Position = position;
            ViewName = eventType;
        }

        public sealed class Item : DungeonInstanceFloorData
        {
            public List<(MasterData.Item item, int count)> Items { get; } = new();

            public Item(Vector2Int position, List<(MasterData.Item item, int count)> items)
                : base(position, "Item")
            {
                Items = items;
            }
        }

        public sealed class Enemy : DungeonInstanceFloorData
        {
            public int EnemyId { get; }

            public Enemy(Vector2Int position, int enemyId)
                : base(position, "Enemy")
            {
                EnemyId = enemyId;
            }
        }

        public sealed class Message : DungeonInstanceFloorData
        {
            public int MessageGroupId { get; }

            public Message(Vector2Int position, int messageGroupId)
                : base(position, "Message")
            {
                MessageGroupId = messageGroupId;
            }
        }

        public sealed class SavePoint : DungeonInstanceFloorData
        {
            public SavePoint(Vector2Int position)
                : base(position, "SavePoint")
            {
            }
        }
    }
}
