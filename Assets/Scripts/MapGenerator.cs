using System;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Linq;
using Random = System.Random;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Bounds")]
    [SerializeField] [Range(0,100)] private int _randomFillPercent = 45;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    
    [Header("Map Specifics")]
    
    [Tooltip("Border thickness of the map")]
    [SerializeField] [Range(0,30)] private int _borderSize = 5;
    
    [Tooltip("Removing isolated islands on the map after generating with this specific tile count")]
    [SerializeField] private int _wallThresholdSize = 50;
    
    [Tooltip("Removing isolated rooms on the map after generating with this specific tile count")]
    [SerializeField] private int _roomThresholdSize = 300;
    
    [Tooltip("Corridor radius between rooms")]
    [SerializeField] [Range(1,10)] private int _corridorRadius = 1;
    
    [Header("Random")]
    [SerializeField] private string _seed;
    [SerializeField] private bool _useRandomSeed;
    [SerializeField] [Range(0,10)] private int _smoothIterations = 5;
    
    private MeshGenerator _meshGenerator;
    private int[,] _map;

    private void Awake()
    {
        _meshGenerator = GetComponent<MeshGenerator>();
    }

    private void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
            GenerateMap();
    }

    private void GenerateMap()
    {
        _map = new int[_width, _height];
        RandomFillMap();

        for (int i = 0; i < _smoothIterations; i++) 
            SmoothMap();
        
        ProcessMap();

        var borderedMap = GenerateBorderedMap();

        _meshGenerator.GenerateMesh(borderedMap, 1);
    }

    private int[,] GenerateBorderedMap()
    {
        int[,] borderedMap = new int[_width + _borderSize * 2, _height + _borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                bool isInsideOfBorders = x >= _borderSize && x < _width + _borderSize &&
                                         y >= _borderSize && y < _height + _borderSize;

                if (isInsideOfBorders)
                    borderedMap[x, y] = _map[x - _borderSize, y - _borderSize];
                else
                    borderedMap[x, y] = 1;
            }
        }
        
        return borderedMap;
    }

    private void RandomFillMap()
    {
        if (_useRandomSeed) _seed = Time.time.ToString(CultureInfo.InvariantCulture);
        Random pseudoRandom = new Random(_seed.GetHashCode());

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                bool isBorderOfTheMap = x == 0 || x == _width - 1 || y == 0 || y == _height - 1;
                if (isBorderOfTheMap)
                    _map[x, y] = 1;
                else
                    _map[x, y] = pseudoRandom.Next(0, 100) < _randomFillPercent ? 1 : 0;
            }
        }
    }

    private void SmoothMap()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int neighbourWallCount = GetSurroundingWallCount(x, y);
                
                if (neighbourWallCount > 4)
                    _map[x, y] = 1;
                else if(neighbourWallCount < 4) 
                    _map[x, y] = 0;
            }
        }
    }

    private void ProcessMap()
    {
        // removes isolated islands with _wallThresholdSize tile count
        List<List<Coord>> wallRegions = GetRegions(1);
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < _wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    _map[tile.TileX, tile.TileY] = 0; // convert to empty
                }
            }
        }
        
        // removes isolated rooms with _roomThresholdSize tile count
        List<List<Coord>> roomRegions = GetRegions(0);
        List<Room> survivingRooms = new();
        
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < _roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    _map[tile.TileX, tile.TileY] = 1; //convert to wall
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, _map));
            }
        }
        survivingRooms.Sort(); //descending sort
        survivingRooms[0].IsMainRoom = true;
        survivingRooms[0].IsAccessibleFromMainRoom = true;
        ConnectClosestRooms(survivingRooms);
    }
    
    private void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new();
        List<Room> roomListB = new();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.IsAccessibleFromMainRoom) 
                    roomListB.Add(room);
                else
                    roomListA.Add(room);
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;
        
        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                
                if (roomA.ConnectedRooms.Count > 0)
                    continue;
            }

            foreach (Room roomB in roomListB)
            {
                if(roomA == roomB || roomA.IsConnected(roomB)) 
                    continue;
                
                for (int tileIndexA = 0; tileIndexA < roomA.EdgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.EdgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.EdgeTiles[tileIndexA];
                        Coord tileB = roomB.EdgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)Mathf.Pow(tileA.TileX - tileB.TileX, 2) + (int)Mathf.Pow(tileA.TileY - tileB.TileY, 2);
                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
        
        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    private void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA,roomB);
        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 120f);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord coord in line)
        {
            DrawCircle(coord, _corridorRadius);
        }
    }

    private void DrawCircle(Coord coord, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = coord.TileX + x;
                    int drawY = coord.TileY + y;

                    if (IsInMapRange(drawX, drawY))
                    {
                        _map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    private List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new();
        bool isInverted = false;
        
        int x = from.TileX;
        int y = from.TileY;

        int dx = to.TileX - from.TileX;
        int dy = to.TileY - from.TileY;

        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            isInverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x,y));
            
            if (isInverted) 
                y += step;
            else 
                x += step;

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (isInverted) 
                    x += gradientStep;
                else
                    y += gradientStep;

                gradientAccumulation -= longest;
            }
        }
        return line;
    }

    private Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-_width / 2 + 0.5f + tile.TileX, 2, -_height / 2 + 0.5f + tile.TileY);
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY) 
                        wallCount += _map[neighbourX, neighbourY];
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    private List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new();
        int[,] mapFlags = new int[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (mapFlags[x, y] == 0 && _map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);
                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.TileX, tile.TileY] = 1; // mark as looked at
                    }
                }
            }
        }

        return regions;
    }

    private List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new();
        int[,] mapFlags = new int[_width, _height];
        int tileType = _map[startX, startY];

        Queue<Coord> queue = new();
        queue.Enqueue(new Coord(startX,startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.TileX - 1; x <= tile.TileX + 1; x++)
            {
                for (int y = tile.TileY - 1; y <= tile.TileY + 1; y++)
                {
                    bool isNotDiagonal = y == tile.TileY || x == tile.TileX;
                    if (IsInMapRange(x, y) && isNotDiagonal)
                    {
                        if (mapFlags[x, y] == 0 && _map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x,y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    private bool IsInMapRange(int x, int y) => x >= 0 && x < _width && y >= 0 && y < _height;
}