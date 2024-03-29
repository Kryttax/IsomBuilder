﻿using System;
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
        [SUCC.SaveThis] public string roomIdentifier;                           //Room UNIQUE ID (Serialization)

        public RoomProperties(string identifier, List<TileProperties> tProperties = null)
        {
            tilesProperties = tProperties != null ? new List<TileProperties>(tProperties) : new List<TileProperties>();

            roomIdentifier = identifier;
        }
    }
}

public class Room : MonoBehaviour
{
    public RoomData roomData { get; private set; }
    public RoomProperties properties { get; private set; }
    public List<Tile> roomTiles { get; set; }                           //GameObject References (WIPE BEFORE/AFTER EVERY GAME INIT)
    public List<Vector2> roomTilesPosition { get; set; }                //Tiles Vector2 References (WIPE BEFORE/AFTER EVERY GAME INIT)
    private List<Vector2> tempRoomSize;                                 //Resets every time player expands currentRoom
    private List<Vector2> tempRoomRemoval;                              //Resets every time player shrinks currentRoom

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

    public static Room CreateRoom(RoomData rData, List<TileProperties> tProperties = null, string identifier = null)
    {
        if (rData == null)
        {
            Debug.LogError("Room Data not defined. Cannot Create a new Room.");
            return null;
        }
        if (identifier == null)
        {
            //Generate Room Identifier
            identifier = rData.roomID.ToString() + RoomsManager.instance.GetRoomNumber().ToString();
        }

        var thisRoom = RoomObj.AddComponent<Room>();
        thisRoom.name = identifier;
        thisRoom.roomData = rData;
        thisRoom.properties = new RoomProperties(identifier, tProperties);
        thisRoom.roomTiles = new List<Tile>();
        thisRoom.roomTilesPosition = new List<Vector2>();
        thisRoom.tempRoomSize = new List<Vector2>();
        thisRoom.tempRoomRemoval = new List<Vector2>();

        if (tProperties == null) return thisRoom;
        for (int i = 0; i < tProperties.Count; ++i)
            thisRoom.roomTilesPosition.Add(tProperties[i].tilePosition);

        for (int i = 0; i < tProperties.Count; ++i)
        {
            Tile newTile = GenerateRoomTile(tProperties[i].tilePosition, thisRoom.gameObject, thisRoom.roomData, thisRoom.roomTilesPosition);
            thisRoom.roomTiles.Add(newTile);
        }

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

        for (int i = 0; i < loadedRoomTiles.Count; ++i)
            RoomsManager.instance.RemoveRock(loadedRoomTiles[i].tilePosition);

        Room loadedRoom = Room.CreateRoom(type, loadedRoomTiles, fieldName);
 
        return loadedRoom;
    }

    public static bool IsTileInRoom(List<Vector2> rTiles, Vector2 tilePosition)
    {
        int result = rTiles.FindIndex(pos => pos.x == tilePosition.x && pos.y == tilePosition.y);

        if (result == -1)
            return false;

        return true;
    }

    public static bool IsTileInRoom(List<Tile> rTiles, Vector2 tilePosition)
    {
        int result = rTiles.FindIndex(pos => pos.tileData.tilePosition.x == tilePosition.x && pos.tileData.tilePosition.y == tilePosition.y);

        return result != -1;
    }

    public static bool IsTileInRoom(List<Tile> rTiles, Vector2 tilePosition, out Tile tileInRoom)
    {
        tileInRoom = null;
        int result = rTiles.FindIndex(pos => pos.tileData.tilePosition.x == tilePosition.x && pos.tileData.tilePosition.y == tilePosition.y);

        if (result == -1)
            return false;

        tileInRoom = rTiles[result];
        return true;
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

    public void FillRoomTiles()
    {
        for (int i = 0; i < tempRoomSize.Count; ++i)
        {
            Tile newTile = GenerateRoomTile(tempRoomSize[i], this.gameObject, roomData, tempRoomSize);
            roomTiles.Add(newTile);
            roomTilesPosition.Add(newTile.tileData.tilePosition);
            properties.tilesProperties.Add(newTile.tileData);

            UpdateTileNeighbours(newTile, roomTiles);
            UpdateTileContext(newTile);
        }

        tempRoomSize.Clear();
    }

    public void ClearRoomTiles()
    {
        for (int i = 0; i < tempRoomRemoval.Count; ++i)
        {
            Tile tileToDelete = roomTiles.Find(x => x.tileData.tilePosition == tempRoomRemoval[i]);
            if (!tileToDelete) continue;
            if (tileToDelete.tileData.isAccess)
                RoomsManager.instance.PropagateChangesToAccess(tileToDelete.tileData.tilePosition);
            RoomsManager.instance.GenerateEmptyRock(tileToDelete.tileData.tilePosition);

            roomTiles.Remove(tileToDelete);
            roomTilesPosition.Remove(tileToDelete.tileData.tilePosition);
            properties.tilesProperties.Remove(tileToDelete.tileData);

            UpdateTileNeighbours(tileToDelete, roomTiles);
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

        roomTilesPosition.Clear();
        properties.tilesProperties.Clear();
        roomTiles.Clear();
    }

    public void UpdateRoomTiles()
    {
        for (int i = 0; i < roomTiles.Count; ++i)
        {
            int tNeighbours = 0;
            List<COORDINATES> tCoord = new List<COORDINATES>();
            RoomData.ROOM_TILE_TYPE tType = FindNeighbours(roomTilesPosition, roomTiles[i].tileData.tilePosition, out tNeighbours, out tCoord);
            int rotTier = Room.GetRotationTileTier(tType, tCoord);

            roomTiles[i].UpdateTileFloor(roomData.GetRoomFloor(tType), tType, rotTier);
            roomTiles[i].AddTileSides(GetTileSides(this.roomData, tType, rotTier), rotTier);

            //ACCESS TILES 
            UpdateTileContext(roomTiles[i]);
        }
    }

    private void UpdateTileNeighbours(Tile tile, List<Tile> roomTiles)
    {
        Dictionary<COORDINATES, Vector2>  neighbours = Room.GetTileAdjacentCoordinates(tile.tileData.tilePosition);

        foreach(Vector2 pos in neighbours.Values)
        {
            Tile neighbourTile;
            if(Room.IsTileInRoom(roomTiles, pos, out neighbourTile))
            {
                if (UpdateRoomTileFixed(neighbourTile))
                {
                    UpdateTileContext(neighbourTile);
                    UpdateTileNeighbours(neighbourTile, roomTiles);
                }
            }
        }
        
    }

    public bool UpdateRoomTileFixed(Tile tile)
    {
        int tNeighbours = 0;
        List<COORDINATES> tCoord = new List<COORDINATES>();
        RoomData.ROOM_TILE_TYPE tType = FindNeighbours(roomTilesPosition, tile.tileData.tilePosition, out tNeighbours, out tCoord);     
        
        if (tile.tileData.tileType != tType)
        {
            int rotTier = Room.GetRotationTileTier(tType, tCoord);
            tile.UpdateTileFloor(roomData.GetRoomFloor(tType), tType, rotTier);
            tile.AddTileSides(GetTileSides(this.roomData, tType, rotTier), rotTier);
            return true;
        }

        return false;
    }

    public void UpdateRoomTile(Tile tile)
    {
        int tNeighbours = 0;
        List<COORDINATES> tCoord = new List<COORDINATES>();
        RoomData.ROOM_TILE_TYPE tType = FindNeighbours(roomTilesPosition, tile.tileData.tilePosition, out tNeighbours, out tCoord);
        int rotTier = Room.GetRotationTileTier(tType, tCoord);

        if (tile.tileData.tileType != tType || tile.tileData.isAccess)
        {
            tile.UpdateTileFloor(roomData.GetRoomFloor(tType), tType, rotTier);
            tile.AddTileSides(GetTileSides(this.roomData, tType, rotTier), rotTier);
        }  
    }

    public void UpdateRoomTileAccess(Tile tile, COORDINATES coord)
    {
        roomData.GetRoomWall(tile.tileData.tileType);
        tile.AddTileSide(coord, roomData.GetRoomAccess(tile.tileData.tileType));
    }

    //HELPER
    private List<COORDINATES> GetAdjacentCoordinates()
    {
        List<COORDINATES> adjacent = new List<COORDINATES>();
        adjacent.Add(COORDINATES.UP);
        adjacent.Add(COORDINATES.RIGHT);
        adjacent.Add(COORDINATES.DOWN);
        adjacent.Add(COORDINATES.LEFT);

        return adjacent;
    }

    public void UpdateTileContext(Tile tile)
    {
        RoomData.ROOM_TILE_TYPE accessType;

        switch (tile.tileData.tileType)
        {
            case RoomData.ROOM_TILE_TYPE.CONVEX_CORNER:
                accessType = RoomData.ROOM_TILE_TYPE.ACCESS_CORNER;
                break;
            case RoomData.ROOM_TILE_TYPE.SIDE:
                accessType = RoomData.ROOM_TILE_TYPE.ACCESS_SIDE;
                break;
            default:
                return;
        }

        List<COORDINATES> tCoord = GetAdjacentCoordinates();

        foreach (COORDINATES coord in tCoord)
            if (RoomsManager.instance.CheckIfTileAccess(tile.tileData.tilePosition,
                GetTileCoordinatePosition(tile.tileData.tilePosition, coord), this))
            {
                tile.tileData.isAccess = true;
                tile.AddTileSide(coord, roomData.GetRoomWall(accessType));
            }
    }

    //HELPER 
    //COORDINATES SWAP FOR CONTEXT ROOM CORRECT ROTATION (DO NOT CHANGE!)
    public static COORDINATES GetTileOppositeCoordinate(Vector2 centralTile, Vector2 coordinatePosition)
    {
        COORDINATES tileCoord = COORDINATES.UP;

        //UP OR DOWN
        if(centralTile.x == coordinatePosition.x)
        {
            if (centralTile.y == coordinatePosition.y - 1)
                tileCoord = COORDINATES.DOWN;
            else if(centralTile.y == coordinatePosition.y + 1)
                tileCoord = COORDINATES.UP;
        }
        //LEFT OR RIGHT
        else if(centralTile.y == coordinatePosition.y)
        {
            if (centralTile.x == coordinatePosition.x - 1)
                tileCoord = COORDINATES.LEFT;
            else if (centralTile.x == coordinatePosition.x + 1)
                tileCoord = COORDINATES.RIGHT;
        }

        return tileCoord;
    }

    public static Dictionary<COORDINATES, Vector2> GetTileAdjacentCoordinates(Vector2 centralTile)
    {
        Dictionary<COORDINATES, Vector2> coordinates = new Dictionary<COORDINATES, Vector2>();

        coordinates.Add(COORDINATES.UP, new Vector2(centralTile.x, centralTile.y + 1));
        coordinates.Add(COORDINATES.DOWN, new Vector2(centralTile.x, centralTile.y - 1));
        coordinates.Add(COORDINATES.RIGHT, new Vector2(centralTile.x + 1, centralTile.y));
        coordinates.Add(COORDINATES.LEFT, new Vector2(centralTile.x - 1, centralTile.y));
        
        return coordinates;
    }

    public static List<Vector2> GetTileAdjacentPositions(Vector2 centralTile)
    {
        List<Vector2> coordinates = new List<Vector2>();

        coordinates.Add(new Vector2(centralTile.x, centralTile.y + 1));
        coordinates.Add(new Vector2(centralTile.x, centralTile.y - 1));
        coordinates.Add(new Vector2(centralTile.x + 1, centralTile.y));
        coordinates.Add(new Vector2(centralTile.x - 1, centralTile.y));

        return coordinates;
    }

    //HELPER
    public static Vector2 GetTileCoordinatePosition(Vector2 centralTile, COORDINATES coordinate)
    {
        switch (coordinate)
        {
            case COORDINATES.UP:
                return new Vector2(centralTile.x, centralTile.y + 1);
            case COORDINATES.DOWN:
                return new Vector2(centralTile.x, centralTile.y - 1);
            case COORDINATES.LEFT:
                return new Vector2(centralTile.x - 1, centralTile.y);
            case COORDINATES.RIGHT:
                return new Vector2(centralTile.x + 1, centralTile.y);
            case COORDINATES.UPLEFT:
                return new Vector2(centralTile.x - 1, centralTile.y + 1);
            case COORDINATES.UPRIGHT:
                return new Vector2(centralTile.x + 1, centralTile.y + 1);
            case COORDINATES.DOWNLEFT:
                return new Vector2(centralTile.x - 1, centralTile.y - 1);
            case COORDINATES.DOWNRIGHT:
                return new Vector2(centralTile.x + 1, centralTile.y - 1);
            default:
                //Debug.LogError("Coordinate not defined! Please check coordinate is correctly assigned.");
                return centralTile;
        }
    }

    //public static COORDINATES GetOppositeCoordinate(COORDINATES coord)
    //{
    //    switch (coord)
    //    {
    //        case COORDINATES.UP:
    //            return COORDINATES.DOWN;
    //        case COORDINATES.DOWN:
    //            return COORDINATES.UP;
    //        case COORDINATES.LEFT:
    //            return COORDINATES.RIGHT;
    //        case COORDINATES.RIGHT:
    //            return COORDINATES.LEFT;
    //        default:
    //            Debug.LogWarning("Coordinate " + coord + " has no opposite side.");
    //            return COORDINATES.UP;
    //    }
    //}

    //HELPER
    //private static Tile CreateRoomTile(Vector2 tilePosition, GameObject parent, RoomData roomDataRef, List<Tile> roomTiles)
    //{
    //    int tNeighbours = 0;
    //    List<COORDINATES> tCoord = new List<COORDINATES>();
    //    RoomData.ROOM_TILE_TYPE tType = FindNeighbours(roomTiles, tilePosition, out tNeighbours, out tCoord);
    //    int rotTier = Room.GetRotationTileTier(tType, tCoord);
    //    return Tile.CreateTile(tilePosition, parent, roomDataRef.GetRoomFloor(tType), tType, rotTier, GetTileSides(roomDataRef, tType, rotTier));
    //}

    private static Tile GenerateRoomTile(Vector2 tilePosition, GameObject parent, RoomData roomDataRef, List<Vector2> roomTilesPos)
    {
        int tNeighbours = 0;
        List<COORDINATES> tCoord = new List<COORDINATES>();
        RoomData.ROOM_TILE_TYPE tType = FindNeighbours(roomTilesPos, tilePosition, out tNeighbours, out tCoord);
        
        int rotTier = Room.GetRotationTileTier(tType, tCoord);
        Tile newTile = RoomsManager.instance.GetMapTile(tilePosition);
        newTile.UpdateTile(parent, roomDataRef.GetRoomFloor(tType), tType, rotTier, GetTileSides(roomDataRef, tType, rotTier));
        return newTile;
    }

    // !!!
    private static Dictionary<COORDINATES, GameObject> GetTileSides(RoomData roomData, RoomData.ROOM_TILE_TYPE tType, int rotTier)
    {
        Dictionary<COORDINATES, GameObject> sides = new Dictionary<COORDINATES, GameObject>();
        sides.Add(COORDINATES.UP, null);
        sides.Add(COORDINATES.DOWN, null);
        sides.Add(COORDINATES.LEFT, null);
        sides.Add(COORDINATES.RIGHT, null);

        switch (tType)
        {
            case RoomData.ROOM_TILE_TYPE.EMPTY:
                break;
            case RoomData.ROOM_TILE_TYPE.FLOOR:
                break;
            case RoomData.ROOM_TILE_TYPE.SINGLE_FLOOR:
                sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                break;
            case RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER:
                switch (rotTier)
                {
                    case 0:
                        //sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 1:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        //sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 2:
                        //sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 3:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        //sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.CONVEX_CORNER:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        break;
                    case 1:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        break;
                    case 2:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        break;
                    case 3:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.DOUBLE_CONCAVE:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 1:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 2:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 3:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                }
                break;

            case RoomData.ROOM_TILE_TYPE.DOUBLE_EYED_CONCAVE:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 1:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    //case 2:
                    //    sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                    //    sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                    //    break;
                    //case 3:
                    //    sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                    //    sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                    //    break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.DOUBLE_CONVEX:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        break;
                    case 1:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        break;
                    case 2:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        break;
                    case 3:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.SIDE:
                switch(rotTier)
                {
                    case 0:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        break;
                    case 1:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        break;
                    case 2:
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        break;
                    case 3:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT:
                switch (rotTier)
                {          
                    case 0:
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);                           
                        break;
                    case 1:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);                       
                        break;
                    case 2:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);       
                        break;
                    case 3:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);                   
                        break;
                    case 1:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);                     
                        break;
                    case 2:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);                    
                        break;
                    case 3:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        break;
                    case 1:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        break;
                    case 1:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        break;
                    case 2:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        break;
                    case 3:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.SIDE);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.TRIPLE_CONCAVE:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 1:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 2:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 3:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.FULL_CONCAVE:
                sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                break;
            case RoomData.ROOM_TILE_TYPE.CONVEX_CONCAVE:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 1:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 2:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                    case 3:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.ACCESS_SINGLE:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SINGLE);
                        break;
                    case 1:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SINGLE);
                        break;
                    case 2:
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SINGLE);
                        break;
                    case 3:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SINGLE);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.ACCESS_SIDE:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SIDE);
                        break;
                    case 1:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SIDE);
                        break;
                    case 2:
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SIDE);
                        break;
                    case 3:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SIDE);
                        break;
                }
                break;
            case RoomData.ROOM_TILE_TYPE.ACCESS_CORNER:
                switch (rotTier)
                {
                    case 0:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SINGLE);
                        break;
                    case 1:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SINGLE);
                        sides[COORDINATES.UP] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        break;
                    case 2:
                        sides[COORDINATES.RIGHT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SINGLE);
                        break;
                    case 3:
                        sides[COORDINATES.LEFT] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SINGLE);
                        sides[COORDINATES.DOWN] = roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.CONVEX_CORNER);
                        break;
                }
                break;
            default:
                break;
        }

        return sides;
    }


    // !!!
    private static int GetRotationTileTier(RoomData.ROOM_TILE_TYPE tType, List<COORDINATES> tCoordinates)
    {
        int tier = 0;

        switch (tType)
        {
            case RoomData.ROOM_TILE_TYPE.EMPTY:
                tier = 0;
                break;
            case RoomData.ROOM_TILE_TYPE.FLOOR:
                tier = 0;
                break;
            case RoomData.ROOM_TILE_TYPE.SINGLE_FLOOR:
                tier = 0;
                break;
            case RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER:
                if (!tCoordinates.Contains(COORDINATES.UPLEFT))
                    tier = 0;
                else if (!tCoordinates.Contains(COORDINATES.UPRIGHT))
                    tier = 1;
                else if (!tCoordinates.Contains(COORDINATES.DOWNRIGHT))
                    tier = 2;
                else if (!tCoordinates.Contains(COORDINATES.DOWNLEFT))
                    tier = 3;
                else
                    Debug.LogError("Tile " + tType + " is not well defined for rotation purposes.");
                break;
            case RoomData.ROOM_TILE_TYPE.CONVEX_CORNER:
                if (tCoordinates.Contains(COORDINATES.DOWN) && tCoordinates.Contains(COORDINATES.RIGHT))
                    tier = 0;
                else if (tCoordinates.Contains(COORDINATES.DOWN) && tCoordinates.Contains(COORDINATES.LEFT))
                    tier = 1;
                else if (tCoordinates.Contains(COORDINATES.UP) && tCoordinates.Contains(COORDINATES.LEFT))
                    tier = 2;
                else if (tCoordinates.Contains(COORDINATES.UP) && tCoordinates.Contains(COORDINATES.RIGHT))
                    tier = 3;
                else
                    Debug.LogError("Tile " + tType + " is not well defined for rotation purposes.");
                break;
            case RoomData.ROOM_TILE_TYPE.DOUBLE_CONCAVE:
                if (!tCoordinates.Contains(COORDINATES.UPLEFT) && !tCoordinates.Contains(COORDINATES.UPRIGHT))
                    tier = 0;
                else if (!tCoordinates.Contains(COORDINATES.UPRIGHT) && !tCoordinates.Contains(COORDINATES.DOWNRIGHT))
                    tier = 1;
                else if (!tCoordinates.Contains(COORDINATES.DOWNLEFT) && !tCoordinates.Contains(COORDINATES.DOWNRIGHT))
                    tier = 2;
                else if (!tCoordinates.Contains(COORDINATES.UPLEFT) && !tCoordinates.Contains(COORDINATES.DOWNLEFT))
                    tier = 3;
                else
                    Debug.LogError("Tile " + tType + " is not well defined for rotation purposes.");
                break;
            case RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE:
                if (!tCoordinates.Contains(COORDINATES.DOWN))
                    tier = 0;
                else if (!tCoordinates.Contains(COORDINATES.LEFT))
                    tier = 1;
                else if (!tCoordinates.Contains(COORDINATES.UP))
                    tier = 2;
                else if (!tCoordinates.Contains(COORDINATES.RIGHT))
                    tier = 3;
                else
                    Debug.LogError("Tile " + tType + " is not well defined for rotation purposes.");
                break;
            case RoomData.ROOM_TILE_TYPE.DOUBLE_EYED_CONCAVE:
                if (!tCoordinates.Contains(COORDINATES.UPRIGHT) && !tCoordinates.Contains(COORDINATES.DOWNLEFT))
                    tier = 1;
                else if (!tCoordinates.Contains(COORDINATES.UPLEFT) && !tCoordinates.Contains(COORDINATES.DOWNRIGHT))
                    tier = 0;
                else
                    Debug.LogError("Tile " + tType + " is not well defined for rotation purposes.");
                break;
            case RoomData.ROOM_TILE_TYPE.DOUBLE_CONVEX:
                if (tCoordinates.Contains(COORDINATES.DOWN))
                    tier = 0;
                else if (tCoordinates.Contains(COORDINATES.LEFT))
                    tier = 1;
                else if (tCoordinates.Contains(COORDINATES.UP))
                    tier = 2;
                else if (tCoordinates.Contains(COORDINATES.RIGHT))
                    tier = 3;
                else
                    Debug.LogError("Tile " + tType + " is not well defined for rotation purposes.");
                break;
            case RoomData.ROOM_TILE_TYPE.SIDE:
                if (!tCoordinates.Contains(COORDINATES.UP))
                    tier = 0;
                else if (!tCoordinates.Contains(COORDINATES.RIGHT))
                    tier = 1;
                else if (!tCoordinates.Contains(COORDINATES.DOWN))
                    tier = 2;
                else if (!tCoordinates.Contains(COORDINATES.LEFT))
                    tier = 3;
                else
                    Debug.LogError("Tile " + tType + " is not well defined for rotation purposes.");
                break;
            case RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED:
                if (tCoordinates.Contains(COORDINATES.LEFT) && tCoordinates.Contains(COORDINATES.RIGHT))
                    tier = 0;
                else if (tCoordinates.Contains(COORDINATES.UP) && tCoordinates.Contains(COORDINATES.DOWN))
                    tier = 1;
                else
                    Debug.LogError("Tile " + tType + " is not well defined for rotation purposes. Coordinates Size: " + tCoordinates.Count);
                break;
            case RoomData.ROOM_TILE_TYPE.TRIPLE_CONCAVE:
                if (tCoordinates.Contains(COORDINATES.DOWNRIGHT))
                    tier = 0;
                else if (tCoordinates.Contains(COORDINATES.DOWNLEFT))
                    tier = 1;
                else if (tCoordinates.Contains(COORDINATES.UPLEFT))
                    tier = 2;
                else if (tCoordinates.Contains(COORDINATES.UPRIGHT))
                    tier = 3;
                else
                    Debug.LogError("Tile " + tType + " is not well defined for rotation purposes.");
                break;
            case RoomData.ROOM_TILE_TYPE.CONVEX_CONCAVE:
                if (tCoordinates.Contains(COORDINATES.DOWN) && tCoordinates.Contains(COORDINATES.RIGHT))
                    tier = 0;
                else if (tCoordinates.Contains(COORDINATES.DOWN) && tCoordinates.Contains(COORDINATES.LEFT))
                    tier = 1;
                else if (tCoordinates.Contains(COORDINATES.UP) && tCoordinates.Contains(COORDINATES.LEFT))
                    tier = 2;
                else if (tCoordinates.Contains(COORDINATES.UP) && tCoordinates.Contains(COORDINATES.RIGHT))
                    tier = 3;
                else
                    Debug.LogError("Tile " + tType + " is not well defined for rotation purposes.");
                break;
            case RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT:
                if (!tCoordinates.Contains(COORDINATES.DOWN))
                    tier = 0;
                else if (!tCoordinates.Contains(COORDINATES.LEFT))
                    tier = 1;
                else if (!tCoordinates.Contains(COORDINATES.UP))
                    tier = 2;
                else if (!tCoordinates.Contains(COORDINATES.RIGHT))
                    tier = 3;
                break;
            case RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT:
                if (!tCoordinates.Contains(COORDINATES.DOWN))
                    tier = 0;
                else if(!tCoordinates.Contains(COORDINATES.LEFT))
                    tier = 1;
                else if (!tCoordinates.Contains(COORDINATES.UP))
                    tier = 2;
                else if (!tCoordinates.Contains(COORDINATES.RIGHT))
                    tier = 3;
                break;
            default:
                Debug.LogError("Tile type data is not well defined for rotation purposes.");
                tier = 0;
                break;
        }

        return tier;
    }

    private static RoomData.ROOM_TILE_TYPE FindAdjacentNeighbours(List<Vector2> rTiles, Vector2 centralTile, out int totalNeighbours, out List<COORDINATES> tileCoordinates)
    {
        tileCoordinates = new List<COORDINATES>();
        int neighbours = 0;   // sum

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

        totalNeighbours = neighbours;

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

                //Debug.LogWarning("Two adjacent neighbours of type : " + tileType);
                break;
            case 1:
                tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_CONVEX;
                break;
            case 0:
                tileType = RoomData.ROOM_TILE_TYPE.SINGLE_FLOOR;
                break;
            default:
                tileType = RoomData.ROOM_TILE_TYPE.EMPTY;
                Debug.LogWarning("Room Illegal!");
                break;
        }

        return tileType;
    }

    private static RoomData.ROOM_TILE_TYPE FindDistantNeighbours(List<Vector2> rTiles, Vector2 centralTile, out int totalNeighbours, out List<COORDINATES> tileCoordinates)
    {
        int adjacentNeighbours = 0;   // sum
        RoomData.ROOM_TILE_TYPE tileType = FindAdjacentNeighbours(rTiles, centralTile, out adjacentNeighbours, out tileCoordinates);

        totalNeighbours = adjacentNeighbours;

        if (tileType == RoomData.ROOM_TILE_TYPE.EMPTY || tileType == RoomData.ROOM_TILE_TYPE.SINGLE_FLOOR)
            return tileType;
        
        int neighbours = 0;
        //List<COORDINATES> tileCoordinates = new List<COORDINATES>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 || j == 0) continue; // Skip center tile
                if (IsTileInRoom(rTiles, new Vector2(centralTile.x + i, centralTile.y + j)))
                {
                    if (i == -1 && j == 1)
                        tileCoordinates.Add(COORDINATES.UPLEFT);
                    if (i == 1 && j == 1)
                        tileCoordinates.Add(COORDINATES.UPRIGHT);
                    if (i == -1 && j == -1)
                        tileCoordinates.Add(COORDINATES.DOWNLEFT);
                    if (i == 1 && j == -1)
                        tileCoordinates.Add(COORDINATES.DOWNRIGHT);

                    neighbours++;
                }
            }
        }

        totalNeighbours += neighbours;

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
                        //tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED;
                        if ((tileCoordinates.Contains(COORDINATES.UPLEFT) && tileCoordinates.Contains(COORDINATES.UPRIGHT)) ||
                            (tileCoordinates.Contains(COORDINATES.DOWNLEFT) && tileCoordinates.Contains(COORDINATES.DOWNRIGHT)) ||
                            (tileCoordinates.Contains(COORDINATES.UPLEFT) && tileCoordinates.Contains(COORDINATES.DOWNLEFT)) ||
                            (tileCoordinates.Contains(COORDINATES.UPRIGHT) && tileCoordinates.Contains(COORDINATES.DOWNRIGHT)))
                            tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_CONCAVE;
                        else
                            tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_EYED_CONCAVE;
                        break;
                    case 1:
                        tileType = RoomData.ROOM_TILE_TYPE.TRIPLE_CONCAVE;
                        break;
                    case 0:
                        tileType = RoomData.ROOM_TILE_TYPE.FULL_CONCAVE;
                        break;
                    default:
                        //Return adjacent type
                        break;
                }
                break;
            case 3:
                switch (neighbours)
                {
                    case 0:
                        tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                        break;
                    case 1:
                        //if ((!tileCoordinates.Contains(COORDINATES.DOWN) && tileCoordinates.Contains(COORDINATES.UPRIGHT)) ||
                        //  (!tileCoordinates.Contains(COORDINATES.LEFT) && tileCoordinates.Contains(COORDINATES.DOWNRIGHT)) ||
                        //  (!tileCoordinates.Contains(COORDINATES.UP) && tileCoordinates.Contains(COORDINATES.DOWNLEFT)) ||
                        //  (!tileCoordinates.Contains(COORDINATES.RIGHT) && tileCoordinates.Contains(COORDINATES.UPLEFT)))
                        if (!tileCoordinates.Contains(COORDINATES.DOWN))
                        {
                            if (tileCoordinates.Contains(COORDINATES.UPRIGHT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                            else if (tileCoordinates.Contains(COORDINATES.UPLEFT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                            else
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                        }
                        else if
                           (!tileCoordinates.Contains(COORDINATES.LEFT))
                        {
                            if (tileCoordinates.Contains(COORDINATES.DOWNRIGHT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                            else if (tileCoordinates.Contains(COORDINATES.UPRIGHT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                            else
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                        }
                        else if (!tileCoordinates.Contains(COORDINATES.UP))
                        {
                            if (tileCoordinates.Contains(COORDINATES.DOWNLEFT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                            else if (tileCoordinates.Contains(COORDINATES.DOWNRIGHT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                            else
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                        }
                        else if (!tileCoordinates.Contains(COORDINATES.RIGHT))
                        {
                            if (tileCoordinates.Contains(COORDINATES.UPLEFT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                            else if (tileCoordinates.Contains(COORDINATES.DOWNLEFT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                            else
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                        }
                        break;
                    case 2:
                        if ((!tileCoordinates.Contains(COORDINATES.UP) && tileCoordinates.Contains(COORDINATES.DOWNLEFT) && tileCoordinates.Contains(COORDINATES.DOWNRIGHT)) ||
                       (!tileCoordinates.Contains(COORDINATES.DOWN) && tileCoordinates.Contains(COORDINATES.UPLEFT) && tileCoordinates.Contains(COORDINATES.UPRIGHT)) ||
                       (!tileCoordinates.Contains(COORDINATES.RIGHT) && tileCoordinates.Contains(COORDINATES.UPLEFT) && tileCoordinates.Contains(COORDINATES.DOWNLEFT)) ||
                       (!tileCoordinates.Contains(COORDINATES.LEFT) && tileCoordinates.Contains(COORDINATES.UPRIGHT) && tileCoordinates.Contains(COORDINATES.DOWNRIGHT)))
                            tileType = RoomData.ROOM_TILE_TYPE.SIDE;
                        else
                        {
                            if(!tileCoordinates.Contains(COORDINATES.DOWN))
                            {
                                if(tileCoordinates.Contains(COORDINATES.UPRIGHT))
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                                else if(tileCoordinates.Contains(COORDINATES.UPLEFT))
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                                else
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                            }
                            else if(!tileCoordinates.Contains(COORDINATES.LEFT))
                            {
                                if (tileCoordinates.Contains(COORDINATES.DOWNRIGHT))
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                                else if (tileCoordinates.Contains(COORDINATES.UPRIGHT))
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                                else
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                            }
                            else if (!tileCoordinates.Contains(COORDINATES.RIGHT))
                            {
                                if (tileCoordinates.Contains(COORDINATES.UPLEFT))
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                                else if (tileCoordinates.Contains(COORDINATES.DOWNLEFT))
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                                else
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                            }
                            else if (!tileCoordinates.Contains(COORDINATES.UP))
                            {
                                if (tileCoordinates.Contains(COORDINATES.DOWNLEFT))
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                                else if (tileCoordinates.Contains(COORDINATES.DOWNRIGHT))
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                                else
                                    tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                            }
                         //   if ((!tileCoordinates.Contains(COORDINATES.DOWN) && tileCoordinates.Contains(COORDINATES.UPRIGHT)) ||
                         //(!tileCoordinates.Contains(COORDINATES.LEFT) && tileCoordinates.Contains(COORDINATES.DOWNRIGHT)) ||
                         //(!tileCoordinates.Contains(COORDINATES.UP) && tileCoordinates.Contains(COORDINATES.DOWNLEFT)) ||
                         //(!tileCoordinates.Contains(COORDINATES.RIGHT) && tileCoordinates.Contains(COORDINATES.UPLEFT)))
                         //       tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                         //   else
                         //   {
                         //       //tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                         //       tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                         //   }
                                
                        }
                        break;
                    case 3:
                        if (!tileCoordinates.Contains(COORDINATES.DOWN))
                        {
                            if (tileCoordinates.Contains(COORDINATES.UPLEFT) && !tileCoordinates.Contains(COORDINATES.UPRIGHT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                            else if (!tileCoordinates.Contains(COORDINATES.UPLEFT) && tileCoordinates.Contains(COORDINATES.UPRIGHT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                            else if (tileCoordinates.Contains(COORDINATES.UPLEFT) && tileCoordinates.Contains(COORDINATES.UPRIGHT))
                                break;
                            else
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                        }
                        else if
                           (!tileCoordinates.Contains(COORDINATES.LEFT))
                        {
                            if (tileCoordinates.Contains(COORDINATES.UPRIGHT) && !tileCoordinates.Contains(COORDINATES.DOWNRIGHT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                            else if (!tileCoordinates.Contains(COORDINATES.UPRIGHT) && tileCoordinates.Contains(COORDINATES.DOWNRIGHT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                            else if (tileCoordinates.Contains(COORDINATES.UPRIGHT) && tileCoordinates.Contains(COORDINATES.DOWNRIGHT))
                                break;
                            else
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                        }
                        else if (!tileCoordinates.Contains(COORDINATES.UP))
                        {
                            if (tileCoordinates.Contains(COORDINATES.DOWNLEFT) && !tileCoordinates.Contains(COORDINATES.DOWNRIGHT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                            else if (!tileCoordinates.Contains(COORDINATES.DOWNLEFT) && tileCoordinates.Contains(COORDINATES.DOWNRIGHT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                            else if (tileCoordinates.Contains(COORDINATES.DOWNLEFT) && tileCoordinates.Contains(COORDINATES.DOWNRIGHT))
                                break;
                            else
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                        }
                        else if (!tileCoordinates.Contains(COORDINATES.RIGHT))
                        {
                            if (tileCoordinates.Contains(COORDINATES.UPLEFT) && !tileCoordinates.Contains(COORDINATES.DOWNLEFT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_LEFT;
                            else if (!tileCoordinates.Contains(COORDINATES.UPLEFT) && tileCoordinates.Contains(COORDINATES.DOWNLEFT))
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_CONCAVE_RIGHT;
                            else if (tileCoordinates.Contains(COORDINATES.UPLEFT) && tileCoordinates.Contains(COORDINATES.DOWNLEFT))
                                break;
                            else
                                tileType = RoomData.ROOM_TILE_TYPE.SIDE_DOUBLE_CONCAVE;
                        }
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
                    case 0:
                        //Return adjacent type
                        break;
                    default:
                        if ((tileCoordinates.Contains(COORDINATES.DOWN) && tileCoordinates.Contains(COORDINATES.RIGHT) && 
                            tileCoordinates.Contains(COORDINATES.DOWNRIGHT)) ||
                            (tileCoordinates.Contains(COORDINATES.DOWN) && tileCoordinates.Contains(COORDINATES.LEFT) &&
                            tileCoordinates.Contains(COORDINATES.DOWNLEFT)) ||
                            (tileCoordinates.Contains(COORDINATES.UP) && tileCoordinates.Contains(COORDINATES.LEFT) &&
                            tileCoordinates.Contains(COORDINATES.UPLEFT)) ||
                            (tileCoordinates.Contains(COORDINATES.UP) && tileCoordinates.Contains(COORDINATES.RIGHT) &&
                            tileCoordinates.Contains(COORDINATES.UPRIGHT)))
                                tileType = RoomData.ROOM_TILE_TYPE.CONVEX_CORNER;

                        //if (tileType == RoomData.ROOM_TILE_TYPE.CONVEX_CONCAVE)
                        //    tileType = RoomData.ROOM_TILE_TYPE.CONVEX_CORNER;
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
                        //tileType = RoomData.ROOM_TILE_TYPE.DOUBLE_SIDED;
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

    //private static RoomData.ROOM_TILE_TYPE FindNeighbours(List<Tile> rTiles, Vector2 centralTile, out int totalNeighbours, out List<COORDINATES> neighboursCoordinates)
    //{      
    //    return FindDistantNeighbours(rTiles, centralTile, out totalNeighbours, out neighboursCoordinates);
    //}

    private static RoomData.ROOM_TILE_TYPE FindNeighbours(List<Vector2> rTiles, Vector2 centralTile, out int totalNeighbours, out List<COORDINATES> neighboursCoordinates)
    {
        return FindDistantNeighbours(rTiles, centralTile, out totalNeighbours, out neighboursCoordinates);
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
