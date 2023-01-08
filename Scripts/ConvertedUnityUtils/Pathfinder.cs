using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class Pathfinder
{
    private static int[,] map;
    private static int[,] objects;
    private static Vector2Int size;

    public static void SetMap(int[,] newMap, Vector2Int newSize)
    {
        map = newMap;
        size = newSize;
        objects = new int[newSize.x, newSize.y];
    }

    public static void PlaceObject(Vector2Int pos)
    {
        objects[pos.x, pos.y]++;
    }

    public static void RemoveObject(Vector2Int pos)
    {
        objects[pos.x, pos.y]--;
    }

    public static void MoveObject(Vector2Int oldPos, Vector2Int newPos)
    {
        RemoveObject(oldPos);
        PlaceObject(newPos);
    }

    public static float GetTrueDistance(Vector2Int start, Vector2Int end)
    {
        if (start == end)
        {
            return 0;
        }
        List<Vector2Int> parts = GetPath(start, end);
        float sum = 0;
        for (int i = 0; i < parts.Count - 1; i++)
        {
            sum += Vector2Int.Distance(parts[i], parts[i + 1]);
        }
        return sum;
    }

    public static bool HasLineOfSight(Vector2Int start, Vector2Int end)
    {
        return HasLineOfSight(new Point(start), new Point(end));
    }

    public static List<Vector2Int> GetPath(Vector2Int sourceVec, Vector2Int destinationVec)
    {
        // From Wikipedia...
        Point source = new Point(sourceVec);
        Point destination = new Point(destinationVec);
        if (source == destination)
        {
            throw new Exception("Same source & destination!");
        }
        if (!CanMove(destination.x, destination.y))
        {
            throw new Exception("Destination is a blocked tile! (" + destination + ")");
        }

        List<Point> openSet = new List<Point>();
        openSet.Add(source);

        Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();

        Dictionary<Point, int> gScore = new Dictionary<Point, int>();
        gScore.Add(source, 0);

        Dictionary<Point, int> fScore = new Dictionary<Point, int>();
        fScore.Add(source, GetCost(source, destination));
        // Horrible infinte loop fix
        int attempts = 0;
        while (openSet.Count > 0)
        {
            if (attempts++ >= 100)
            {
                break;
            }
            Point current = openSet[0];
            int minValue = int.MaxValue;
            openSet.ForEach(a => { if (fScore.SafeGetKey(a, int.MaxValue) < minValue) minValue = fScore.SafeGetKey(current = a, int.MaxValue); });
            if (current == destination)
            {
                return RecoverPath(cameFrom, current);
            }
            openSet.Remove(current);
            foreach (Point neighbor in current.GetNeighbors())
            {
                if (CanMove(neighbor.x, neighbor.y))
                { 
                    int tentativeScore = gScore[current] + GetDistance(current, neighbor); // No safe as the current should always have a gValue
                    if (tentativeScore < gScore.SafeGetKey(neighbor, int.MaxValue))
                    {
                        cameFrom.AddOrSet(neighbor, current);
                        gScore.AddOrSet(neighbor, tentativeScore);
                        fScore.AddOrSet(neighbor, tentativeScore + GetCost(neighbor, destination));
                        if (openSet.FindIndex(a => a == neighbor) < 0)
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
        }
        // This should be impossible
        return new List<Vector2Int>();
    }

    public static List<Vector2Int> GetMoveArea(Vector2Int start, int move)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        result.Add(start);
        if (move <= 0)
        {
            return result;
        }
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i != 0 && j == 0) || (j != 0 && i == 0))
                {
                    if (CanMove(start.x + i, start.y + j))
                    {
                        result.AddRange(GetMoveArea(start + new Vector2Int(i, j), move - 1));
                    }
                }
            }
        }
        return result;
    }

    public static List<Vector2Int> GetAttackArea(Vector2Int start, Vector2Int range, int move = 0)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        if (move >= range.x && move <= range.y)
        {
            result.Add(start);
        }
        if (move >= range.y)
        {
            return result;
        }
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i != 0 && j == 0) || (j != 0 && i == 0))
                {
                    if (CanMove(start.x + i, start.y + j, true))
                    {
                        result.AddRange(GetAttackArea(start + new Vector2Int(i, j), range, move + 1));
                    }
                }
            }
        }
        return result;
    }

    private static List<Vector2Int> RecoverPath(Dictionary<Point, Point> cameFrom, Point current)
    {
        void Squash(List<Point> totalPathArg, int curr, int next)
        {
            for (int i = curr + 1; i < next - 1; i++)
            {
                totalPathArg.RemoveAt(curr + 1);
            }
        }

        List<Point> totalPath = new List<Point>();
        totalPath.Add(current);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        // Post-process path
        //if (totalPath.Count > 2) // No need to squash if it's just 2 steps...
        //{
        //    int curr = 0, next = 2;
        //    while (next < totalPath.Count)
        //    {
        //        if (!HasLineOfSight(totalPath[curr], totalPath[next]))
        //        {
        //            Squash(totalPath, curr, next);
        //            curr++;
        //            next = curr + 2; // Must have a line of sight with neighbors, so no need to check them
        //        }
        //        else
        //        {
        //            next++;
        //        }
        //    }
        //    Squash(totalPath, curr, next);
        //}
        // Reverse & convert path
        List<Vector2Int> reversed = new List<Vector2Int>();
        for (int i = totalPath.Count - 1; i >= 0; i--)
        {
            reversed.Add(totalPath[i].ToVector2Int());
        }
        return reversed;
    }

    private static int GetCost(Point pos, Point destination)
    {
        return pos.GetDistance(destination);
    }

    private static int GetDistance(Point current, Point neighbor)
    {
        return 1; // No need to calculate, we know it's always 1
    }

    private static bool CanMove(int x, int y, bool ignoreObjects = false)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
        {
            return true;
        }
        return (map[x, y] <= 0 || (ignoreObjects && map[x,y] == 3)) && (ignoreObjects || objects[x, y] <= 0);
    }

    private static bool HasLineOfSight(Point start, Point end)
    {
        // Adapted from https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
        int x = start.x, y = start.y;
        int x2 = end.x, y2 = end.y;
        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            if (!CanMove(x, y))
            {
                return false;
            }
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                if (i < longest && (!CanMove(x + dx1, y) || !CanMove(x, y + dy1)))
                {
                    //Debug.Log("No line of sight between " + new Vector2Int(x, y) + " and " + new Vector2Int(x + dx1, y + dy1) + " - checking " + start + " to " + end);
                    return false;
                }
                x += dx1;
                y += dy1;
            }
            else
            {
                if (i < longest && (!CanMove(x + dx2, y) || !CanMove(x, y + dy2)))
                {
                    //Debug.Log("No line of sight between " + new Vector2Int(x, y) + " and " + new Vector2Int(x + dx2, y + dy2) + " - checking " + start + " to " + end);
                    return false;
                }
                x += dx2;
                y += dy2;
            }
        }
        return true;
    }

    private class Point
    {
        public int x;
        public int y;

        public static bool operator ==(Point a, Point b)
        {
            if ((object)a == null) return (object)b == null;
            if ((object)b == null) return (object)a == null;
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public Point(Vector2Int vector2Int)
        {
            x = vector2Int.x;
            y = vector2Int.y;
        }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public List<Point> GetNeighbors()
        {
            List<Point> result = new List<Point>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if ((i == 0 || j == 0) && (i != 0 || j != 0))
                    {
                        result.Add(new Point(x + i, y + j));
                    }
                }
            }
            return result;
        }

        public int GetDistance(Point other)
        {
            return Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(other.x - x, 2) + Mathf.Pow(other.y + y, 2))); // Mathf.Abs(other.x - x) + Mathf.Abs(other.y - y);
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }
	
	// Dictionary extensions
	
    public static S SafeGetKey<T, S>(this Dictionary<T, S> dictionary, T key, S defaultValue = default)
    {
        return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
    }

    public static void AddOrSet<T, S>(this Dictionary<T, S> dictionary, T key, S value)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
        }
        else
        {
            dictionary[key] = value;
        }
    }
}
