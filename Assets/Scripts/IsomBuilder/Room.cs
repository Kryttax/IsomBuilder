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
    public enum COORDINATES { UP, DOWN, LEFT, RIGHT, UPLEFT, UPRIGHT, DOWNLEFT, DOWNRIGHT }

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
            Tile newTile = CreateRoomTile(loadedRoomTiles[i].tilePosition, loadedRoom.gameObject, loadedRoom.roomData, loadedRoom.roomTiles);
            //loadedRoom.roomTiles.Add(Tile.CreateTile(loadedRoomTiles[i].tilePosition, loadedRoom.gameObject,
            //    loadedRoom.roomData.GetRoomTile(FindNeighbours(loadedRoom.roomTiles, loadedRoomTiles[i].tilePosition))));
            loadedRoom.roomTiles.Add(newTile);
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

    public static bool IsTileInRoom(List<Tile> rTiles, Vector2 tilePosition, out Tile tileInRoom)
    {
        tileInRoom = null;
        int result = rTiles.FindIndex(pos => pos.tileData.tilePosition.x == tilePosition.x && pos.tileData.tilePosition.y == tilePosition.y);

        if (result == -1)
            return false;

        tileInRoom = rTiles[result];
        return true;
    }

    public static bool IsTileInRoom(List<Tile> rTiles, Vector2 tilePosition, out RoomData.ROOM_TILE_TYPE tileType)
    {
        tileType = RoomData.ROOM_TILE_TYPE.EMPTY;
        int result = rTiles.FindIndex(pos => pos.tileData.tilePosition.x == tilePosition.x && pos.tileData.tilePosition.y == tilePosition.y);

        if (result == -1)
            return false;

        tileType = rTiles[result].tileData.tileType;
        return true;
    }

    public void FillRoomTiles()
    {
        //for (int i = 0; i < tempRoomSize.Count; ++i)
        //    RoomsManager.instance.RemoveEmptyTile(tempRoomSize[i]);

        for (int i = 0; i < tempRoomSize.Count; ++i)
        {
            //int tNeighbours = 0;
            //List<COORDINATES> tCoord = new List<COORDINATES>();
            //RoomData.ROOM_TILE_TYPE tType = FindNeighbours(roomTiles, tempRoomSize[i], out tNeighbours, out tCoord);
            //int rotTier = Room.GetRotationTileTier(tType, tCoord);
            //Tile newTile = Tile.CreateTile(tempRoomSize[i], this.gameObject, roomData.GetRoomTile(tType), tType, rotTier);
            Tile newTile = CreateRoomTile(tempRoomSize[i], this.gameObject, roomData, roomTiles);
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
            int tNeighbours = 0;
            List<COORDINATES> tCoord = new List<COORDINATES>();
            RoomData.ROOM_TILE_TYPE tType = FindNeighbours(roomTiles, roomTiles[i].tileData.tilePosition, out tNeighbours, out tCoord);
            int rotTier = Room.GetRotationTileTier(tType, tCoord);

            roomTiles[i].UpdateTileFloor(roomData.GetRoomFloor(tType), tType, rotTier);
            roomTiles[i].AddTileSides(GetTileSides(this.roomData, tType, rotTier), rotTier);

            //UNCOMMENT THIS FOR ACCESS TILES 
            UpdateTileContext(roomTiles[i]);
            //CheckRoomContext(roomTiles[i], GetAdjacentCoordinates());
        }
    }

    public void UpdateRoomTile(Tile tile, COORDINATES coord)
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

    //private void CheckRoomContext(Tile tile, List<COORDINATES> tCoord)
    //{
    //    if (tile.tileData.tileType == RoomData.ROOM_TILE_TYPE.SIDE)
    //    {
    //        foreach (Room.COORDINATES coord in tCoord)
    //            if (RoomsManager.instance.CheckIfLocalTileAccess(GetTileCoordinatePosition(tile.tileData.tilePosition, coord)))
    //            {
    //                Debug.LogWarning("CHANGIN SIDE TO ACCESS SIDE!");
    //                //tType = RoomData.ROOM_TILE_TYPE.ACCESS_SIDE;
    //                tile.AddTileSide(coord, roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SIDE));
    //                break;
    //            }
    //    }

    //    else if (tile.tileData.tileType == RoomData.ROOM_TILE_TYPE.CONVEX_CORNER)
    //    {
    //        foreach (Room.COORDINATES coord in tCoord)
    //            if (RoomsManager.instance.CheckIfLocalTileAccess(GetTileCoordinatePosition(tile.tileData.tilePosition, coord)))
    //            {
    //                Debug.LogWarning("CHANGIN CONVEX TO ACCESS CONVEX!");
    //                //tType = RoomData.ROOM_TILE_TYPE.ACCESS_CORNER;
    //                tile.AddTileSide(coord, roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_CORNER));
    //                break;
    //            }
    //    }
    //}

    public void UpdateTileContext(Tile tile)
    {
        List<COORDINATES> tCoord = GetAdjacentCoordinates();

        if (tile.tileData.tileType == RoomData.ROOM_TILE_TYPE.SIDE)
        {
            foreach (Room.COORDINATES coord in tCoord)
                if (RoomsManager.instance.CheckIfTileAccess(tile.tileData.tilePosition,
                    GetTileCoordinatePosition(tile.tileData.tilePosition, coord), this))
                {
                    Debug.LogWarning("CHANGIN SIDE TO ACCESS SIDE!");
                    //tType = RoomData.ROOM_TILE_TYPE.ACCESS_SIDE;
                    tile.AddTileSide(coord, roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_SIDE));
                    break;
                }
        }

        else if (tile.tileData.tileType == RoomData.ROOM_TILE_TYPE.CONVEX_CORNER)
        {
            foreach (Room.COORDINATES coord in tCoord)
                if (RoomsManager.instance.CheckIfTileAccess(tile.tileData.tilePosition, 
                    GetTileCoordinatePosition(tile.tileData.tilePosition, coord), this))
                {
                    Debug.LogWarning("CHANGIN CONVEX TO ACCESS CONVEX!");
                    //tType = RoomData.ROOM_TILE_TYPE.ACCESS_CORNER;
                    tile.AddTileSide(coord, roomData.GetRoomWall(RoomData.ROOM_TILE_TYPE.ACCESS_CORNER));
                    break;
                }
        }
    }

    //HELPER 
    //COORDINATES SWAP FOR CONTEXT ROOM CORRECT ROTATION (DO NOT CHANGE!)
    public static Room.COORDINATES GetTileCoordinate(Vector2 centralTile, Vector2 coordinatePosition)
    {
        Room.COORDINATES tileCoord = COORDINATES.UP;

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

    //HELPER
    public static Vector2 GetTileCoordinatePosition(Vector2 centralTile, Room.COORDINATES coordinate)
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
                Debug.LogError("Coordinate not defined! Please check coordinate is correctly assigned.");
                return centralTile;
        }
    }

    //HELPER
    private static Tile CreateRoomTile(Vector2 tilePosition, GameObject parent, RoomData roomDataRef, List<Tile> roomTiles)
    {
        int tNeighbours = 0;
        List<COORDINATES> tCoord = new List<COORDINATES>();
        RoomData.ROOM_TILE_TYPE tType = FindNeighbours(roomTiles, tilePosition, out tNeighbours, out tCoord);
        int rotTier = Room.GetRotationTileTier(tType, tCoord);
        return Tile.CreateTile(tilePosition, parent, roomDataRef.GetRoomFloor(tType), tType, rotTier, GetTileSides(roomDataRef, tType, rotTier));
    }

    // !!!
    private static Dictionary<Room.COORDINATES, GameObject> GetTileSides(RoomData roomData, RoomData.ROOM_TILE_TYPE tType, int rotTier)
    {
        Dictionary<Room.COORDINATES, GameObject> sides = new Dictionary<COORDINATES, GameObject>();
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
            default:
                Debug.LogError("Tile type data is not well defined for rotation purposes.");
                tier = 0;
                break;
        }

        return tier;
    }

    private static RoomData.ROOM_TILE_TYPE FindAdjacentNeighbours(List<Tile> rTiles, Vector2 centralTile, out int totalNeighbours, out List<COORDINATES> tileCoordinates)
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
            default:
                tileType = RoomData.ROOM_TILE_TYPE.EMPTY;
                Debug.LogWarning("Room Illegal!");
                break;
        }

        return tileType;
    }

    private static RoomData.ROOM_TILE_TYPE FindDistantNeighbours(List<Tile> rTiles, Vector2 centralTile, out int totalNeighbours, out List<COORDINATES> tileCoordinates)
    {
        int adjacentNeighbours = 0;   // sum
        RoomData.ROOM_TILE_TYPE tileType = FindAdjacentNeighbours(rTiles, centralTile, out adjacentNeighbours, out tileCoordinates);

        totalNeighbours = adjacentNeighbours;

        if (tileType == RoomData.ROOM_TILE_TYPE.EMPTY)
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

    private static RoomData.ROOM_TILE_TYPE FindNeighbours(List<Tile> rTiles, Vector2 centralTile, out int totalNeighbours, out List<COORDINATES> neighboursCoordinates)
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
