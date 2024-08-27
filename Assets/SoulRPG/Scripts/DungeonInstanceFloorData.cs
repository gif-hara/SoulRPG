using System.Collections.Generic;
using System.Threading;
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

        public CancellationTokenSource LifeScope { get; } = new();

        protected DungeonInstanceFloorData(Vector2Int position, string viewName)
        {
            Position = position;
            ViewName = viewName;
        }

        public void OnRemove()
        {
            LifeScope.Cancel();
            LifeScope.Dispose();
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

            public string PromptMessage { get; }

            public SequenceEvent(Vector2Int position, string viewName, ScriptableSequences sequences, string promptMessage)
                : base(position, viewName)
            {
                Sequences = sequences;
                PromptMessage = promptMessage;
            }
        }
    }
}
