using System.Collections.Generic;
using UnityEngine;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DungeonInstanceFloorData
    {
        public Vector2Int Position { get; }

        public string ViewName { get; }

        protected DungeonInstanceFloorData(Vector2Int position, string viewName)
        {
            Position = position;
            ViewName = viewName;
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

        public sealed class SequenceEvent : DungeonInstanceFloorData
        {
            public ScriptableSequences Sequences { get; }

            public SequenceEvent(Vector2Int position, string viewName, ScriptableSequences sequences)
                : base(position, viewName)
            {
                Sequences = sequences;
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
