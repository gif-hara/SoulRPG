using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HK;
using SoulRPG.BattleSystems.AI;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static bool IsExistWall(this MasterData.Dungeon self, Vector2Int position, Define.Direction direction)
        {
            switch (direction)
            {
                case Define.Direction.Up:
                    return self.wall.ContainsKey((position, position + Vector2Int.right));
                case Define.Direction.Down:
                    return self.wall.ContainsKey((position + Vector2Int.down, position + Vector2Int.down + Vector2Int.right));
                case Define.Direction.Left:
                    return self.wall.ContainsKey((position, position + Vector2Int.down));
                case Define.Direction.Right:
                    return self.wall.ContainsKey((position + Vector2Int.right, position + Vector2Int.right + Vector2Int.down));
                default:
                    Debug.LogError($"Invalid direction: {direction}");
                    return false;
            }
        }

        public static bool IsValidPosition(this MasterData.Dungeon self, Vector2Int position)
        {
            return position.x >= 0 && position.x < self.range.x && position.y >= 0 && position.y < self.range.y;
        }

        public static List<MasterData.DungeonWall> GetWalls(this MasterData.Dungeon self, RectInt rect)
        {
            var walls = new List<MasterData.DungeonWall>();
            for (var x = rect.xMin; x < rect.xMax; x++)
            {
                for (var y = rect.yMin; y < rect.yMax; y++)
                {
                    var position = new Vector2Int(x, y);
                    AddSafe(() => self.wall.TryGetTop(position));
                    AddSafe(() => self.wall.TryGetBottom(position));
                    AddSafe(() => self.wall.TryGetLeft(position));
                    AddSafe(() => self.wall.TryGetRight(position));
                }
            }
            return walls.Distinct().ToList();
            void AddSafe(Func<MasterData.DungeonWall> selector)
            {
                var wall = selector();
                if (wall != null)
                {
                    walls.Add(wall);
                }
            }
        }

        public static MasterData.DungeonWall TryGetTop(this MasterData.DungeonWall.DictionaryList self, Vector2Int position)
        {
            self.TryGetValue((position, position + Vector2Int.right), out var wall);
            return wall;
        }

        public static MasterData.DungeonWall TryGetBottom(this MasterData.DungeonWall.DictionaryList self, Vector2Int position)
        {
            self.TryGetValue((position + Vector2Int.down, position + Vector2Int.down + Vector2Int.right), out var wall);
            return wall;
        }

        public static MasterData.DungeonWall TryGetLeft(this MasterData.DungeonWall.DictionaryList self, Vector2Int position)
        {
            self.TryGetValue((position, position + Vector2Int.down), out var wall);
            return wall;
        }

        public static MasterData.DungeonWall TryGetRight(this MasterData.DungeonWall.DictionaryList self, Vector2Int position)
        {
            self.TryGetValue((position + Vector2Int.right, position + Vector2Int.right + Vector2Int.down), out var wall);
            return wall;
        }

        public static BattleCharacter CreateBattleCharacter(this MasterData.Enemy self)
        {
            return new BattleCharacter(new CharacterBattleStatus(self), new Enemy(self.AISequences), self.BattleCharacterSequences);
        }

        public static string CreateAdditionalDescription(this MasterData.SkillAdditionalDescription self)
        {
            switch (self.Type)
            {
                case 0:
                    var ailment = TinyServiceLocator.Resolve<MasterData>().Ailments.Get(self.Id);
                    return $"{ailment.Name.Localized()} : {ailment.Description.Localized()}";
                default:
                    Debug.LogError($"Invalid type: {self.Type}");
                    return string.Empty;
            }
        }

        public static string FullDescription(this MasterData.Skill self)
        {
            var sb = new StringBuilder();
            sb.Append("[BP:");
            for (var i = 0; i < self.NeedBehaviourPoint; i++)
            {
                sb.Append("<sprite name=\"BehaviourPoint\">");
            }
            sb.Append(" ST:");
            sb.Append(self.NeedStamina);
            sb.Append("] ");
            sb.Append(self.Description.Localized());
            sb.Append("<size=80%>");
            sb.Append("<color=#FFDDDD>");
            foreach (var i in self.AdditionalDescriptions)
            {
                sb.Append(i.CreateAdditionalDescription());
            }
            sb.Append("</color>");
            sb.Append("</size>");
            return sb.ToString();
        }

        public static MasterData.FloorEventSequenceData GetFloorEventSequenceData(this string self)
        {
            return TinyServiceLocator.Resolve<MasterData>().FloorEventSequences.Get(self);
        }
    }
}
