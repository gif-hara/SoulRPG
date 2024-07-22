using HK;
using R3;
using SoulRPG.CharacterControllers;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DungeonController
    {
        public MasterData.Dungeon CurrentDungeon { get; private set; }

        public void Setup(string dungeonName)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            CurrentDungeon = masterData.Dungeons.Get(dungeonName);
        }

        public void Interact(Character character)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            if (masterData.DungeonEvents.TryGetValue(character, out var dungeonEvent))
            {
                Debug.Log($"{dungeonEvent.EventType}");
            }
            else
            {
                Debug.Log("Not Found DungeonEvent");
            }
        }
    }
}
