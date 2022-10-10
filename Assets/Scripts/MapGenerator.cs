using System;
using UnityEngine;
using System.Globalization;
using Random = System.Random;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Bounds")]
    [SerializeField] [Range(0,100)] private int _randomFillPercent = 45;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    
    [Header("Random")]
    [SerializeField] private string _seed;
    [SerializeField] private bool _useRandomSeed;
    [SerializeField] [Range(0,10)] private int _smoothIterations = 5;
    
    private int[,] _map;

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

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                bool isInBounds = neighbourX >= 0 && neighbourX < _width && neighbourY >= 0 && neighbourY < _height
                if (isInBounds)
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

    private void OnDrawGizmos()
    {
        if (_map == null) return;
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Gizmos.color = _map[x, y] == 1 ? Color.black : Color.white;
                Vector3 pos = new Vector3(-_width/2 + x + 0.5f, 0, -_height/2 + y + 0.5f);
                Gizmos.DrawCube(pos,Vector3.one * 0.95f);
            }
        }
    }
}
