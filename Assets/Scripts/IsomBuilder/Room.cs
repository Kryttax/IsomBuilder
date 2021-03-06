﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IsomBuilder;
using SUCC;


[Serializable]
public class RoomProperties
{
    [SaveThis] public List<TileProperties> tilesProperties;            //Room TileData (Serialization)

    public RoomProperties(List<TileProperties> tProperties = null)
    {

        if (tProperties != null)
        {
            Debug.Log("tProperties Count: " + tProperties.Count);
            tilesProperties = new List<TileProperties>(tProperties);
        }
        else tilesProperties = new List<TileProperties>();
    }
}

public class Room : MonoBehaviour
{
    private enum COORDINATES { UP, DOWN, LEFT, RIGHT }

    public List<Tile> roomTiles;                    //GameObject References (WIPE BEFORE/AFTER EVERY GAME INIT)
    public RoomData roomData { get; private set; }

    private RoomProperties properties;
    //private List<Tile> tileGrid;                  //Global Room Tile Reference
    //private List<Vector2> roomSize;               //Global Room Size

    private List<Vector2> tempRoomSize;             //Resets every time player expands/shrinks currentRoom
    private List<Vector2> tempRoomRemoval;


    //public Room()
    //{
    //    properties = new RoomProperties();
    //    //roomSize = new List<Vector2>();
    //    //tileGrid = new List<Tile>();
    //    tempRoomSize = new List<Vector2>();
    //    tempRoomRemoval = new List<Vector2>();
    //}

    private static GameObject roomRef;
    public static GameObject RoomObj
    {
        get
        {
            //if (tileRef == null)
            {
                roomRef = new GameObject("Room Object");
            }
            return roomRef;
        }
    }

    public static Room CreateRoom(RoomData rData, List<TileProperties> tProperties = null)
    {
        var thisRoom = RoomObj.AddComponent<Room>();

   
        thisRoom.properties = new RoomProperties(tProperties);


        thisRoom.roomData = rData;
        thisRoom.roomTiles = new List<Tile>();
        thisRoom.tempRoomSize = new List<Vector2>();
        thisRoom.tempRoomRemoval = new List<Vector2>();

        return thisRoom;
    }

    //Sets all room's tiles after being loaded from roomProperties with roomData
    public void SyncRoom()
    {
        Debug.Log("Syncing room data... ");

        for (int i = 0; i < roomTiles.Count; ++i)
        {
            //Debug.Log("Tile property: " + roomTiles[i].tileData.tileType);
            properties.tilesProperties.Add(roomTiles[i].tileData);
        }

        Serializer.Config.Set(roomData.name, properties.tilesProperties);
    }

    public void LoadRoom()
    {
        Debug.Log("Loading room data... ");

        for (int i = 0; i < properties.tilesProperties.Count; ++i)
            RoomsManager.instance.RemoveEmptyTile(properties.tilesProperties[i].tilePosition);

        for (int i = 0; i < properties.tilesProperties.Count; ++i)
        {
            roomTiles.Add(Tile.CreateTile(properties.tilesProperties[i].tilePosition, this.gameObject, roomData.GetRoomTile(FindNeighbours(properties.tilesProperties[i].tilePosition))));
        }

        Debug.Log("Room Loading success!");
    }

    //public void UpdateTile(GameObject newTilePrefab, RoomData.ROOM_TILE_TYPE newType)
    //{
    //    //var thisTile = GetComponent<Tile>();

    //    if (tilePrefabRef && newType != tileData.tileType)
    //    {
    //        //Debug.LogWarning("Destroying previous tile prefab and updating...");
    //        Destroy(tilePrefabRef.gameObject);

    //        tilePrefabRef = GameObject.Instantiate(newTilePrefab, this.transform);
    //        tileData.tileType = newType;

    //    }
    //    //else
    //    //    Debug.LogWarning("Tile prefab not updating...");


    //}

    public void AddTile(Vector2 tilePosition)
    {
        int index = -1;
        if(!IsTileInRoom(tilePosition, out index))
        {
            //roomSize.Add(tilePosition);
            tempRoomSize.Add(tilePosition);
        }
    }

    public void RemoveTile(Vector2 tilePosition)
    {
        int index = -1;
        if(IsTileInRoom(tilePosition, out index))
        {
            tempRoomRemoval.Add(tilePosition);
            //roomSize.RemoveAt(index);
        }
    }

    public RoomData.ROOM_TILE_TYPE GetTileTypeInRoom(Vector2 tilePosition)
    {
        int result = roomTiles.FindIndex(pos => pos.tileData.tilePosition.x == tilePosition.x && pos.tileData.tilePosition.y == tilePosition.y);

        if (result == -1)
            return RoomData.ROOM_TILE_TYPE.EMPTY;
        
        return roomTiles[result].tileData.tileType;
    }

    public bool IsTileInRoom(Vector2 tilePosition)
    {
        int result = roomTiles.FindIndex(pos => pos.tileData.tilePosition.x == tilePosition.x && pos.tileData.tilePosition.y == tilePosition.y);

        if (result == -1)
            return false;

        return true;
    }

    private bool IsTileInRoom(Vector2 tilePosition, out int result)
    {
        result = roomTiles.FindIndex(pos => pos.tileData.tilePosition.x == tilePosition.x && pos.tileData.tilePosition.y == tilePosition.y);

        if (result == -1)
            return false;

        return true;
    }

    //public void FillRoomWithTiles()
    //{
    //    //Debug.Log("Room Size: " + roomSize.Count);

    //    for(int i = 0; i < tempRoomSize.Count; ++i)
    //        RoomsManager.instance.RemoveEmptyTile(tempRoomSize[i]);

    //    for (int i = 0; i < tempRoomSize.Count; ++i)
    //    {
    //        //tileGrid[(int)tilePosition.x, (int)tilePosition.y] = new Tile(tilePosition, this);
    //        //Tile newTile = UnityEngine.Object.Instantiate(new Tile(roomSize[i], this));
    //        //GameObject newTile = UnityEngine.Object.Instantiate(new GameObject());
    //        //Tile addTile = newTile.AddComponent<Tile>() as Tile;
    //        tileGrid.Add(Tile.CreateTile(tempRoomSize[i], this.gameObject, data.GetRoomTile(FindNeighbours(roomSize[i]))));

    //    }

    //    tempRoomSize.Clear();

    //    //for (int i = 0; i < roomSize.Count; ++i)
    //    //    tileGrid[i].UpdateTile(data.GetRoomTile(Room.FindNeighbours(roomSize[i])));
    //}


    public void CreateRoomTiles()
    {
        for (int i = 0; i < tempRoomSize.Count; ++i)
            RoomsManager.instance.RemoveEmptyTile(tempRoomSize[i]);

        for (int i = 0; i < tempRoomSize.Count; ++i)
        {
            roomTiles.Add(Tile.CreateTile(tempRoomSize[i], this.gameObject, roomData.GetRoomTile(FindNeighbours(tempRoomSize[i]))));
        }

        tempRoomSize.Clear();
    }

    public void ClearRoomTiles()
    {
        for (int i = 0; i < tempRoomRemoval.Count; ++i)
            RoomsManager.instance.AddEmptyTile(tempRoomRemoval[i]);

        for (int i = 0; i < tempRoomRemoval.Count; ++i)
        {
            int tileToDelete = roomTiles.FindIndex(x => x.tileData.tilePosition == tempRoomRemoval[i]);

            if(tileToDelete != -1)
            {
                roomTiles[tileToDelete].RemoveTile();
                roomTiles.RemoveAt(tileToDelete);
            }
        }

        tempRoomRemoval.Clear();
    }

    public void UpdateRoomTiles()
    {
        for (int i = 0; i < roomTiles.Count; ++i)
        {
            RoomData.ROOM_TILE_TYPE type = FindNeighbours(roomTiles[i].tileData.tilePosition);
            roomTiles[i].UpdateTile(roomData.GetRoomTile(type), type);
        }
    }

    private RoomData.ROOM_TILE_TYPE FindAdjacentNeighbours(Vector2 centralTile, out int neighbours)
    {
        List<COORDINATES> tileCoordinates = new List<COORDINATES>();
        neighbours = 0;   // sum

        //char centerTile = roomTiles[x, y]; // Cache center tile
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i == 0 && j == 0) || (i != 0 && j != 0)) continue; // Skip center tile
                if (IsTileInRoom(new Vector2(centralTile.x + i, centralTile.y + j)))
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

    private RoomData.ROOM_TILE_TYPE FindDistantNeighbours(Vector2 centralTile)
    {
        int adjacentNeighbours = 0;   // sum
        RoomData.ROOM_TILE_TYPE tileType = FindAdjacentNeighbours(centralTile, out adjacentNeighbours);

        if (tileType == RoomData.ROOM_TILE_TYPE.EMPTY)
        {
            Debug.LogWarning("Empty adjacent neighbours!");
            return tileType;
        }

        Debug.LogWarning("Adjacent: " + adjacentNeighbours);

        int neighbours = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 || j == 0) continue; // Skip center tile
                if (IsTileInRoom(new Vector2(centralTile.x + i, centralTile.y + j)))
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

        Debug.Log("Tile Option: " + neighbours);

        return tileType;
    }

    private RoomData.ROOM_TILE_TYPE FindNeighbours(Vector2 centralTile)
    {
        return FindDistantNeighbours(centralTile);
    }

    //private RoomData.ROOM_TILE_TYPE FindNeighbours(Vector2 centralTile)
    //{
    //    int neighbours = 0;   // sum

    //    //char centerTile = roomTiles[x, y]; // Cache center tile
    //    for (int i = -1; i <= 1; i++)
    //    {
    //        for (int j = -1; j <= 1; j++)
    //        {
    //            if (i == 0 && j == 0) continue; // Skip center tile
    //            if (IsTileInRoom(new Vector2(centralTile.x + i, centralTile.y + j)))
    //            {
    //                neighbours++;
    //            }
    //        }
    //    }

    //    RoomData.ROOM_TILE_TYPE tileType;

    //    switch (neighbours)
    //    {
    //        case 8:
    //            tileType = RoomData.ROOM_TILE_TYPE.FLOOR;
    //            break;
    //        case 7:
    //            tileType = RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER;
    //            break;
    //        case 6:
    //            tileType = RoomData.ROOM_TILE_TYPE.SIDE;    //OR TWO-SIDED
    //            break;
    //        case 5:
    //            tileType = RoomData.ROOM_TILE_TYPE.SIDE;    //OR CONVEX
    //            break;
    //        case 4:
    //            tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED;
    //            break;
    //        case 3:
    //            tileType = RoomData.ROOM_TILE_TYPE.CONVEX_CORNER;
    //            break;
    //        case 2:
    //            tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED;
    //            break;
    //        case 1:
    //            tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_CONVEX;
    //            break;
    //        default:
    //            tileType = RoomData.ROOM_TILE_TYPE.EMPTY;
    //            Debug.LogWarning("This neighbour number: " + neighbours + " is not defined!");
    //            break;
    //    }

    //    Debug.Log("Tile Option: " + neighbours);

    //    return tileType;
    //}
}
