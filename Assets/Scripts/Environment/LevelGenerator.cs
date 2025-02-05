using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class LevelGenerator : MonoBehaviour
{
    [Header("Terrain")]
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] float smoothness;
    [SerializeField] float seed;
    int[] perlinHeightList;

    [Header("Cave")]
    [Range(0, 100)]
    [SerializeField] int randomFillPercent;
    [SerializeField] int smoothAmount;
    [SerializeField] int caveStartDepth;

    [Header("Tiles")]
    [SerializeField] TileBase groundTile;
    [SerializeField] Tilemap groundTileMap;
    [SerializeField] TileBase caveTile;
    [SerializeField] Tilemap caveTileMap;


    int cameraOffsetX = 50;
    int cameraOffsetY = 50;
    int[,] map;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        perlinHeightList = new int[width];
        Generate();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            Generate();
        }
    }

    void Generate()
    {
        seed = Time.time;
        ClearMap();
        map = GenerateArray(width, height, true);
        map = GenerateTerrain(map);
        SmoothMap(smoothAmount);
        RenderMap(map, groundTileMap, groundTile, caveTileMap, caveTile);
    }

    public int[,] GenerateArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];
        for(int x = 0; x < width; x++)
        {
            for(int y = 0;  y < height; y++) 
            {
                map[x, y] = (empty) ? 0 : 1;
            }
        }
        return map;
    }

    public int[,] GenerateTerrain(int[,] map)
    {
        System.Random pesudoRandom = new System.Random(seed.GetHashCode());
        int perlinHeight;
        for(int x = 0; x < width; x++)
        {
            perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x/smoothness, seed) * height * 0.5f);
            perlinHeight += (int)(height * 0.5f);
            perlinHeightList[x] = perlinHeight;
            for(int y = 0; y < perlinHeight; y++)
            {
                map[x, y] = (pesudoRandom.Next(1, 100) < randomFillPercent) ? 1 : 2;
            }
        }
        return map;
    }

    void SmoothMap(int smoothAmount)
    {
        for(int i = 0; i < smoothAmount; i++) 
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < perlinHeightList[x]; y++)
                {
                    if (x == 0 || y == 0 || x == width - 1 || y >= perlinHeightList[x] - caveStartDepth)
                    {
                        map[x, y] = 1;
                    }
                    else
                    {
                        int surroundingGroudCount = GetSurroundingGroundCount(x, y);
                        if (surroundingGroudCount > 4)
                        {
                            map[x, y] = 1;
                        }
                        else if (surroundingGroudCount < 4)
                        {
                            map[x, y] = 2;
                        }
                    }
                }
            }
        }
        
    }

    int GetSurroundingGroundCount(int x, int y)
    {
        int groundCount = 0;
        for (int xNeighbor = x - 1; xNeighbor <= x + 1; xNeighbor++)
        {
            for (int yNeighbor = y - 1; yNeighbor <= y + 1; yNeighbor++)
            {
                if(xNeighbor >=0 && xNeighbor < width && yNeighbor >=0 && yNeighbor< height)
                {
                    if (xNeighbor != x || yNeighbor != y)
                    {
                        if (map[xNeighbor, yNeighbor] == 1)
                        {
                            groundCount++;
                        }                    
                    }
                }
            }
        }

        return groundCount;
    }

    public void RenderMap(int[,] map, Tilemap groundTileMap, TileBase groundTilebase, Tilemap caveTileMap, TileBase caveTilebase)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x,y] == 1)
                {
                    groundTileMap.SetTile(new Vector3Int(x - cameraOffsetX, y - cameraOffsetY, 0), groundTilebase);
                }
                else if (map[x,y] == 2)
                {
                    caveTileMap.SetTile(new Vector3Int(x - cameraOffsetX, y - cameraOffsetY, 0), caveTilebase);

                }
            }
        }
    }

    void ClearMap()
    {
        groundTileMap.ClearAllTiles();
        caveTileMap.ClearAllTiles();
    }
}
