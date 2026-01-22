using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

// node class for A* pathfinding
class Node
{
    public Vector2Int pos;
    public Node parent;
    public float gCost; // Distance from start
    public float hCost; // Heuristic distance to end
    public float fCost { get { return gCost + hCost; } }

    public Node(Vector2Int position, Node parentNode, float g, float h)
    {
        pos = position;
        parent = parentNode;
        gCost = g;
        hCost = h;
    }
}

public class ProcGen : MonoBehaviour
{
    [Header("Generation Settings")]
    [Range(0, 1000)]
    public int seed = 0;

    [Header("Tilemap References")]
    public Tilemap wallTilemap;
    public Tilemap edgeTilemap;
    public Tilemap floorTilemap;
    public TileBase wallTile;
    public TileBase edgeTile;
    public TileBase floorTile;
    public TileBase highlightFloorTile;

    [Header("Entity References")]
    public GameObject player;
    public GameObject enemy;
    public GameObject chest;

    // 0 = floor, 1 = wall
    private int[,] map;
    private List<int> bottomEdge;
    private List<int> topEdge;

    // maze parameters
    private int width = 15;
    private int height = 15;
    private Vector2Int start;
    private Vector2Int end;

    void Start()
    {
        Random.InitState(seed);
        end = new Vector2Int(0, height-1);
        start = new Vector2Int(0, 0);

        GenerateMaze();

        Debug.Log("Initial maze - Start: " + start + " End: " + end);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateMaze();

            Debug.Log("New maze - Start: " + start + " End: " + end);
        }
    }

    /*
        Helper Functions for generation/pathfinding
    */

    void GenerateMaze()
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

        // start at (0, 0) to have map fill the board
        DFS(0, 0);

        // then find open tiles for entrance/exit (after maze is generated)
        List<int> bottomOpenTiles = new List<int>();
        List<int> topOpenTiles = new List<int>();

        for (int x = 0; x < width; x++)
        {
            if (map[x, height - 1] == 0)
                topOpenTiles.Add(x);
            if (map[x, 0] == 0)
                bottomOpenTiles.Add(x);
        }

        // select random start
        if (bottomOpenTiles.Count > 0)
            start.x = bottomOpenTiles[Random.Range(0, bottomOpenTiles.Count)];
        else
            start.x = 0;

        //Debug.Log("[" + string.Join(", ", bottomOpenTiles) + "]");

        // select random end
        if (topOpenTiles.Count > 0)
            end.x = topOpenTiles[Random.Range(0, topOpenTiles.Count)];
        else
            end.x = 0;

        //Debug.Log("[" + string.Join(", ", topOpenTiles) + "]");

        // set up edges
        bottomEdge = new List<int>();
        topEdge = new List<int>();

        for (int x = 0; x < width; x++)
        {
            bottomEdge.Add(1);
            topEdge.Add(1);
        }

        bottomEdge[start.x] = 0;
        topEdge[end.x] = 0;

        DrawMap();

        GeneratePath();
    }

    void GeneratePath()
    {
        List<Vector2Int> path = AStar(start, end);

        if (path != null && path.Count > 0)
        {
            Debug.Log("Path found with " + path.Count + " tiles");

            // Highlight the path on the floor tilemap
            foreach (Vector2Int pos in path)
            {
                Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
                floorTilemap.SetTile(tilePos, highlightFloorTile);
            }
        }
        else
        {
            Debug.LogWarning("No path found between start and end!");
        }
    }

    void DrawMap()
    {
        // take the values from the map array and draw them to the tilemap
        for (int x = 0; x < width; x++)
        {
            // top edge tiles
            Vector3Int topEdgePos = new Vector3Int(x + 1, height + 1, 0);
            if (topEdge[x] == 0)
            {
                edgeTilemap.SetTile(topEdgePos, null);
            }
            else
            {
                edgeTilemap.SetTile(topEdgePos, edgeTile);
            }

            // bottom edge tiles
            Vector3Int bottomEdgePos = new Vector3Int(x + 1, 0, 0);
            if (bottomEdge[x] == 0)
            {
                edgeTilemap.SetTile(bottomEdgePos, null);
                // put the player in the entrance tile
                player.transform.position = edgeTilemap.GetCellCenterWorld(new Vector3Int(x + 1, 0, 0));
            }
            else
            {
                edgeTilemap.SetTile(bottomEdgePos, edgeTile);
            }

            // place walls
            for (int y = 0; y < height; y++)
            {
                Vector3Int wallPos = new Vector3Int(x, y, 0);
                if (map[x, y] == 0)
                {
                    wallTilemap.SetTile(wallPos, null);
                }
                else
                {
                    wallTilemap.SetTile(wallPos, wallTile);
                }

                floorTilemap.SetTile(wallPos, floorTile);
            }
        }
    }

    /*
        DFS maze generation and helper functions
    */

    void DFS(int x, int y)
    {
        // randomize directions
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

            // check bounds
            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                if (map[nx, ny] == 1) // unvisited
                {
                    // carve path between current and neighbor
                    map[x + dir.x / 2, y + dir.y / 2] = 0;
                    map[nx, ny] = 0;

                    DFS(nx, ny);
                }
            }
        }
    }

    void Shuffle<T>(List<T> list)
    {
        // Fisher-Yates shuffle
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    /*
        A* pathfinding and helper functions
    */

    List<Vector2Int> AStar(Vector2Int startPos, Vector2Int endPos)
    {
        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        // Start node
        Node startNode = new Node(startPos, null, 0, Heuristic(startPos, endPos));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Find node with lowest fCost
            Node currentNode = openList[0];
            for (int i = 1; i<openList.Count; i++)
            {
                if (openList[i].fCost<currentNode.fCost || 
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost<currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.pos);

            // Check if we reached the end
            if (currentNode.pos == endPos)
            {
                return RetracePath(currentNode);
            }

            // Check all neighbors (4-directional movement)
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),  // up
                new Vector2Int(0, -1), // down
                new Vector2Int(1, 0),  // right
                new Vector2Int(-1, 0)  // left
            };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = currentNode.pos + dir;

                // Check if neighbor is walkable and not in closed list
                if (!IsWalkable(neighborPos) || closedList.Contains(neighborPos))
                    continue;

                float newGCost = currentNode.gCost + 1;

                // Check if neighbor is in open list
                Node neighborNode = openList.Find(n => n.pos == neighborPos);

                if (neighborNode == null)
                {
                    // Add new node to open list
                    neighborNode = new Node(neighborPos, currentNode, newGCost, Heuristic(neighborPos, endPos));
                    openList.Add(neighborNode);
                }
                else if (newGCost < neighborNode.gCost)
                {
                    // Update existing node with better path
                    neighborNode.gCost = newGCost;
                    neighborNode.parent = currentNode;
                }
            }
        }

        // No path found
        return null;
    }

    bool IsWalkable(Vector2Int pos)
    {
        // Check bounds
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return false;

        // Check if it's a floor tile (0 = floor, 1 = wall)
        return map[pos.x, pos.y] == 0;
    }

    float Heuristic(Vector2Int a, Vector2Int b)
    {
        // Manhattan distance (good for 4-directional movement)
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector2Int> RetracePath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.pos);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
}