using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProcGen : MonoBehaviour
{
    public int width = 15;
    public int height = 15;

    public Tilemap tilemap;
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase edgeTile;
    public TileBase exitTile;

    // 0 = floor, 1 = wall
    private int[,] map;
    private int[] bottomEdge;
    private int[] topEdge;

    void Start()
    {
        int startX = Random.Range(0, 15);
        int endX = Random.Range(0, 15);

        ResetMap();
        DrawMap();

        GenerateMaze();
        DrawMap();
    }

    void Update()
    {
        
    }

    void ResetMap()
    {
        // fill map with walls
        map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = 1;
            }
        }
    }

    void GenerateMaze()
    {
        // start dfs
        int startX = 7;
        int startY = 0;
        map[startX, startY] = 0;

        DFS(startX, startY);
    }

    void DFS(int x, int y)
    {
        // Randomize directions
        List<Vector2Int> directions = new List<Vector2Int>()
        {
            new Vector2Int(0, 2),  // up
            new Vector2Int(0, -2), // down
            new Vector2Int(2, 0),  // right
            new Vector2Int(-2, 0)  // left
        };
        Shuffle(directions);

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            // Check bounds
            if (nx > 0 && nx < width && ny > 0 && ny < height)
            {
                if (map[nx, ny] == 1) // unvisited
                {
                    // Carve path between current and neighbor
                    map[x + dir.x / 2, y + dir.y / 2] = 0;
                    map[nx, ny] = 0;

                    DFS(nx, ny);
                }
            }
        }
    }

    // Draw the map onto the Tilemap
    void DrawMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (map[x, y] == 0)
                    tilemap.SetTile(pos, null);
                else
                    tilemap.SetTile(pos, wallTile);
            }
        }
    }

    // Fisher-Yates shuffle
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}