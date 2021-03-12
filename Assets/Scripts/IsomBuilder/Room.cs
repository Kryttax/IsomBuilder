using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IsomBuilder;


namespace IsomBuilder
{
    [Serializable]
    public class RoomProperties
    {
        [SUCC.SaveThis] public List<TileProperties> tilesProperties;            //Room TileData (Serialization)

        public RoomProperties(List<TileProperties> tProperties = null)
        {
            if (tProperties != null)
                tilesProperties = new List<TileProperties>(tProperties);       
            else tilesProperties = new List<TileProperties>();
        }
    }
}

public class Room : MonoBehaviour
{
    private enum COORDINATES { UP, DOWN, LEFT, RIGHT }

    public RoomData roomData { get; private set; }
    public RoomProperties properties { get; private set; }
    public List<Tile> roomTiles { get; set; }                   //GameObject References (WIPE BEFORE/AFTER EVERY GAME INIT)
    private List<Vector2> tempRoomSize;                         //Resets every time player expands currentRoom
    private List<Vector2> tempRoomRemoval;                      //Resets every time player shrinks currentRoom

    private static GameObject roomRef;
    public static GameObject RoomObj
    {
        get
        {
            {
                roomRef = new GameObject("Room Object");
            }
            return roomRef;
        }
    }

    public static Room CreateRoom(RoomData rData, List<TileProperties> tProperties = null)
    {
        if (rData == null)
        {
            Debug.LogError("Room Data not defined. Cannot Create a new Room.");
            return null;
        }

        var thisRoom = RoomObj.AddComponent<Room>();

        thisRoom.roomData = rData;
        thisRoom.properties = new RoomProperties(tProperties);
        thisRoom.roomTiles = new List<Tile>();
        thisRoom.tempRoomSize = new List<Vector2>();
        thisRoom.tempRoomRemoval = new List<Vector2>();

        return thisRoom;
    }

    //Sets all room's tiles after being loaded from roomProperties with roomData
    public static void SaveRoom(string fieldName, RoomProperties saveData)
    {
        Debug.Log("Saving room data... ");

        //for (int i = 0; i < roomTiles.Count; ++i)
        //{
        //    saveData.tilesProperties.Add(roomTiles[i].tileData);
        //}

        Serializer.Config.Set(fieldName, saveData.tilesProperties);
    }

    public static Room LoadRoom(string fieldName, RoomData type)
    {
        Debug.Log("Loading room data... ");

        List<TileProperties> loadedRoomTiles = Serializer.Config.Get<List<TileProperties>>(fieldName);

        Room loadedRoom = Room.CreateRoom(type, loadedRoomTiles);

        for (int i = 0; i < loadedRoomTiles.Count; ++i)
            RoomsManager.instance.RemoveEmptyTile(loadedRoomTiles[i].tilePosition);

        for (int i = 0; i < loadedRoomTiles.Count; ++i)
        {
            loadedRoom.roomTiles.Add(Tile.CreateTile(loadedRoomTiles[i].tilePosition, loadedRoom.gameObject,
                loadedRoom.roomData.GetRoomTile(FindNeighbours(loadedRoom.roomTiles, loadedRoomTiles[i].tilePosition))));
        }

        return loadedRoom;

        //for (int i = 0; i < tProperties.Count; ++i)
        //{
        //    roomTiles.Add(Tile.CreateTile(properties.tilesProperties[i].tilePosition, this.gameObject, 
        //        roomData.GetRoomTile(FindNeighbours(roomTiles, properties.tilesProperties[i].tilePosition))));
        //}

    }

    public void AddTile(Vector2 tilePosition)
    {
        if (!IsTileInRoom(roomTiles, tilePosition))
            tempRoomSize.Add(tilePosition);
        
    }

    public void RemoveTile(Vector2 tilePosition)
    {
        if(IsTileInRoom(roomTiles, tilePosition))
            tempRoomRemoval.Add(tilePosition);
    }

    public static RoomData.ROOM_TILE_TYPE GetTileTypeInRoom(List<Tile> rTiles, Vector2 tilePosition)
    {
        int result = rTiles.FindIndex(pos => pos.tileData.tilePosition.x == tilePosition.x && pos.tileData.tilePosition.y == tilePosition.y);

        if (result == -1)
            return RoomData.ROOM_TILE_TYPE.EMPTY;
        
        return rTiles[result].tileData.tileType;
    }

    public static bool IsTileInRoom(List<Tile> rTiles, Vector2 tilePosition)
    {
        int result = rTiles.FindIndex(pos => pos.tileData.tilePosition.x == tilePosition.x && pos.tileData.tilePosition.y == tilePosition.y);

        if (result == -1)
            return false;

        return true;
    }

    public void FillRoomTiles()
    {
        //for (int i = 0; i < tempRoomSize.Count; ++i)
        //    RoomsManager.instance.RemoveEmptyTile(tempRoomSize[i]);

        for (int i = 0; i < tempRoomSize.Count; ++i)
        {
            Tile newTile = Tile.CreateTile(tempRoomSize[i], this.gameObject, roomData.GetRoomTile(FindNeighbours(roomTiles, tempRoomSize[i])));
            roomTiles.Add(newTile);
            properties.tilesProperties.Add(newTile.tileData);
        }

        tempRoomSize.Clear();
    }

    public void ClearRoomTiles()
    {
        //for (int i = 0; i < tempRoomRemoval.Count; ++i)
        //    RoomsManager.instance.AddEmptyTile(tempRoomRemoval[i]);

        for (int i = 0; i < tempRoomRemoval.Count; ++i)
        {
            int tileToDelete = roomTiles.FindIndex(x => x.tileData.tilePosition == tempRoomRemoval[i]);

            if(tileToDelete != -1)
            {
                roomTiles[tileToDelete].RemoveTile();
                roomTiles.RemoveAt(tileToDelete);
                properties.tilesProperties.RemoveAt(tileToDelete);
            }
        }

        tempRoomRemoval.Clear();
    }

    public void ClearEntireRoom()
    {
        for (int i = 0; i < roomTiles.Count; ++i)
        {
            //Tile newTile = Tile.CreateTile(tempRoomSize[i], this.gameObject, roomData.GetRoomTile(FindNeighbours(roomTiles, tempRoomSize[i])));
            roomTiles[i].RemoveTile();
            //properties.tilesProperties.RemoveAt(i);
            //roomTiles.RemoveAt(i);
        }
        properties.tilesProperties.Clear();
        roomTiles.Clear();
    }

    public void UpdateRoomTiles()
    {
        for (int i = 0; i < roomTiles.Count; ++i)
        {
            RoomData.ROOM_TILE_TYPE type = FindNeighbours(roomTiles, roomTiles[i].tileData.tilePosition);
            roomTiles[i].UpdateTile(roomData.GetRoomTile(type), type);
        }
    }

    private static RoomData.ROOM_TILE_TYPE FindAdjacentNeighbours(List<Tile> rTiles, Vector2 centralTile, out int neighbours)
    {
        List<COORDINATES> tileCoordinates = new List<COORDINATES>();
        neighbours = 0;   // sum

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i == 0 && j == 0) || (i != 0 && j != 0)) continue; // Skip center tile
                if (IsTileInRoom(rTiles, new Vector2(centralTile.x + i, centralTile.y + j)))
                {
                    if (i == 0 && j == 1)
                        tileCoordinates.Add(COORDINATES.UP);
                    if(i == 0 && j == -1)
                        tileCoordinates.Add(COORDINATES.DOWN);
                    if (i == -1 && j == 0)
                        tileCoordinates.Add(COORDINATES.LEFT);
                    if (i == 1 && j == 0)
                        tileCoordinates.Add(COORDINATES.RIGHT);

                    neighbours++;
                }
            }
        }

        RoomData.ROOM_TILE_TYPE tileType;

        switch (neighbours)
        {
            case 4:
                tileType = RoomData.ROOM_TILE_TYPE.FLOOR;
                break;
            case 3:
                tileType = RoomData.ROOM_TILE_TYPE.SIDE;
                break;
            case 2:
                if ((tileCoordinates.Contains(COORDINATES.LEFT) && tileCoordinates.Contains(COORDINATES.RIGHT))
                    || (tileCoordinates.Contains(COORDINATES.UP) && tileCoordinates.Contains(COORDINATES.DOWN)))
                    tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED;
                else
                    tileType = RoomData.ROOM_TILE_TYPE.CONVEX_CONCAVE;

                Debug.LogWarning("Two adjacent neighbours of type : " + tileType);
                break;
            case 1:
                tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_CONVEX;
                break;
            default:
                tileType = RoomData.ROOM_TILE_TYPE.EMPTY;
                Debug.LogWarning("Room Illegal!");
                break;
        }

        return tileType;
    }

    private static RoomData.ROOM_TILE_TYPE FindDistantNeighbours(List<Tile> rTiles, Vector2 centralTile)
    {
        int adjacentNeighbours = 0;   // sum
        RoomData.ROOM_TILE_TYPE tileType = FindAdjacentNeighbours(rTiles, centralTile, out adjacentNeighbours);

        if (tileType == RoomData.ROOM_TILE_TYPE.EMPTY)
            return tileType;
        
        int neighbours = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 || j == 0) continue; // Skip center tile
                if (IsTileInRoom(rTiles, new Vector2(centralTile.x + i, centralTile.y + j)))
                {
                    neighbours++;
                }
            }
        }

        switch (adjacentNeighbours)
        {
            case 4:
                switch (neighbours)
                {
                    case 4:
                        tileType = RoomData.ROOM_TILE_TYPE.FLOOR;
                        break;
                    case 3:
                        tileType = RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER;
                        break;
                    case 2:
                        tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED;
                        break;
                    case 1:
                        tileType = RoomData.ROOM_TILE_TYPE.TRIPLE_CONCAVE;
                        break;
                    default:
                        //Return adjacent type
                        break;
                }
                break;
            case 3:
                switch (neighbours)
                {
                    case 1:
                        tileType = RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER;
                        break;
                    default:
                        //Return adjacent type
                        break;
                }
                break;
            case 2:
                switch (neighbours)
                {
                    case 4:
                        if (tileType == RoomData.ROOM_TILE_TYPE.CONVEX_CONCAVE)
                            tileType = RoomData.ROOM_TILE_TYPE.EMPTY;   //ILLEGAL
                        break;
                    default:
                        if (tileType == RoomData.ROOM_TILE_TYPE.CONVEX_CONCAVE)
                            tileType = RoomData.ROOM_TILE_TYPE.CONVEX_CORNER;
                        break;
                }
                break;
            case 1:
                switch (neighbours)
                {
                    case 4:
                        tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED;
                        break;
                    case 3:
                        tileType = RoomData.ROOM_TILE_TYPE.CONVEX_CORNER;
                        break;
                    case 2:
                        tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED;
                        break;
                    case 1:
                        if (tileType == RoomData.ROOM_TILE_TYPE.CONVEX_CONCAVE)
                            tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_CONVEX;
                        else if (tileType == RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED)
                            tileType = RoomData.ROOM_TILE_TYPE.CONVEX_CORNER;
                        break;
                    default:
                        //Return adjacent type
                        break;
                }
                break;
            default:
                //Return adjacent type
                break;
        }

        return tileType;
    }

    private static RoomData.ROOM_TILE_TYPE FindNeighbours(List<Tile> rTiles, Vector2 centralTile)
    {
        return FindDistantNeighbours(rTiles, centralTile);
    }

    public void RemoveRoom()
    {
        for(int i = 0; i < roomTiles.Count; ++i)
        {
            roomTiles[i].RemoveTile();
        }

        roomTiles.Clear();

        Destroy(this.gameObject);
    }
}
