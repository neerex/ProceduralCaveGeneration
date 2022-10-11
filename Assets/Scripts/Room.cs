using System;
using System.Collections.Generic;

public class Room : IComparable<Room>
{
    public List<Coord> Tiles;
    public List<Coord> EdgeTiles;
    public List<Room> ConnectedRooms;
    public int RoomSize;
    public bool IsAccessibleFromMainRoom;
    public bool IsMainRoom;

    public Room() { }

    public Room(List<Coord> roomTiles, int[,] map)
    {
        Tiles = roomTiles;
        RoomSize = Tiles.Count;
        ConnectedRooms = new List<Room>();
        EdgeTiles = new List<Coord>();

        CollectEdgeTiles(map);
    }

    public void SetAccessibleFromMainRoom()
    {
        if (!IsAccessibleFromMainRoom)
        {
            IsAccessibleFromMainRoom = true;
            foreach (Room connectedRoom in ConnectedRooms)
            {
                connectedRoom.SetAccessibleFromMainRoom();
            }
        }
    }
    
    public static void ConnectRooms(Room roomA, Room roomB)
    {
        if (roomA.IsAccessibleFromMainRoom)
            roomB.SetAccessibleFromMainRoom();
        else if (roomB.IsAccessibleFromMainRoom) 
            roomA.SetAccessibleFromMainRoom();
        
        roomA.ConnectedRooms.Add(roomB);
        roomB.ConnectedRooms.Add(roomA);
    }

    public bool IsConnected(Room otherRoom)
    {
        return ConnectedRooms.Contains(otherRoom);
    }

    public int CompareTo(Room otherRoom)
    {
        return otherRoom.RoomSize.CompareTo(RoomSize);
    }

    private void CollectEdgeTiles(int[,] map)
    {
        foreach (Coord tile in Tiles)
        {
            for (int x = tile.TileX - 1; x <= tile.TileX + 1; x++)
            {
                for (int y = tile.TileY - 1; y <= tile.TileY + 1; y++)
                {
                    bool isNotDiagonal = y == tile.TileY || x == tile.TileX;
                    if (isNotDiagonal)
                    {
                        if (map[x, y] == 1) // wall tile
                        {
                            EdgeTiles.Add(tile);
                        }
                    }
                }
            }
        }
    }
}