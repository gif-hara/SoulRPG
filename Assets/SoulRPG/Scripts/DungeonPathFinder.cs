using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DungeonPathFinder
    {
        public Vector2Int? FindPath(DungeonController dungeonController, Vector2Int start, Vector2Int goal)
        {
            if (start == goal)
            {
                return null;
            }
            var range = dungeonController.CurrentDungeon.range;
            var width = range.x;
            var height = range.y;
            var distances = new int[width, height];
            var previous = new Vector2Int[width, height];
            var visited = new bool[width, height];
            var neighbors = new List<Vector2Int>();
            var directions = new Vector2Int[]
            {
                new(0, 1),
                new(1, 0),
                new(0, -1),
                new(-1, 0)
            };

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    distances[x, y] = int.MaxValue;
                }
            }

            distances[start.x, start.y] = 0;

            while (true)
            {
                var current = GetMinDistanceNode(visited, width, height, distances);
                if (current.x == -1 || current.y == -1)
                {
                    break;
                }

                if (current == goal || distances[current.x, current.y] == int.MaxValue)
                {
                    break;
                }

                visited[current.x, current.y] = true;
                neighbors.Clear();

                foreach (var direction in directions)
                {
                    if (!dungeonController.CanMove(current, direction.ToDirection()))
                    {
                        continue;
                    }
                    var neighbor = current + direction;
                    if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height)
                    {
                        neighbors.Add(neighbor);
                    }
                }

                foreach (var neighbor in neighbors)
                {
                    if (visited[neighbor.x, neighbor.y])
                    {
                        continue;
                    }

                    var tentativeDistance = distances[current.x, current.y] + 1;

                    if (tentativeDistance < distances[neighbor.x, neighbor.y])
                    {
                        distances[neighbor.x, neighbor.y] = tentativeDistance;
                        previous[neighbor.x, neighbor.y] = current;
                    }
                }
            }

            if (distances[goal.x, goal.y] == int.MaxValue)
            {
                return null;
            }

            return ConstructPath(goal, distances, previous);
        }

        private static Vector2Int GetMinDistanceNode(bool[,] visited, int width, int height, int[,] distances)
        {
            var minNode = new Vector2Int(-1, -1);
            var minDistance = int.MaxValue;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (!visited[x, y] && distances[x, y] < minDistance)
                    {
                        minDistance = distances[x, y];
                        minNode = new Vector2Int(x, y);
                    }
                }
            }

            return minNode;
        }


        private static Vector2Int? ConstructPath(Vector2Int goal, int[,] distances, Vector2Int[,] previous)
        {
            var current = goal;
            var result = current;

            while (distances[current.x, current.y] != 0)
            {
                result = current;
                current = previous[current.x, current.y];
            }

            return result;
        }
    }
}
