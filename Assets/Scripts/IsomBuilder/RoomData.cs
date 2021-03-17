using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[Serializable]
//public class TilesDictionary : SerializableDictionary<TILE_TYPE, GameObject> { }

[CreateAssetMenu(fileName = "New RoomData", menuName = "Room Data", order = 51)]
public class RoomData : ScriptableObject
{
    public enum ROOM_ID { GENERIC, RED_ROOM, BLUE_ROOM, GREEN_ROOM }
    
    public enum TILE_PREFAB_TYPE { GENERIC_WALL, GENERIC_FLOOR, GENERIC_CORNER, GENERIC_ACCESS_SIDE, GENERIC_ACCESS_SINGLE }
    // ADD WALL AND OTHER ACCESSORIES TO ROOM DATA (INDEPENDENT FROM ROOM TYPE)
    public enum ROOM_TILE_TYPE { EMPTY, FLOOR, CONCAVE_CORNER, CONVEX_CORNER, DOUBLE_CONCAVE, DOUBLE_CONVEX, SIDE, DOUBLE_SIDED, TRIPLE_CONCAVE, CONVEX_CONCAVE,
    ACCESS_SINGLE, ACCESS_SIDE, ACCESS_CORNER }

    public ROOM_ID roomID;

    [Serializable]
    public struct TilePrefabTypes
    {
        public TILE_PREFAB_TYPE type;
        public GameObject prefab;
    }
    public TilePrefabTypes[] tilePrefabs = new TilePrefabTypes[2];

    [Serializable]
    public struct TileTypes
    {
        public ROOM_TILE_TYPE type;
        public GameObject floor;
        public GameObject wall;
    }
    public TileTypes[] roomTiles;

    //public TilesDictionary roomTiles;
    public GameObject GetRoomFloor(ROOM_TILE_TYPE newType)
    {
        //for (int i = 0; i < roomTiles.Length; ++i)
        //    if (roomTiles[i].type == newType)
        //        return roomTiles[i].floor;

        //return null;

        for (int i = 0; i < tilePrefabs.Length; ++i)
            if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_FLOOR)
                return tilePrefabs[i].prefab;

        return null;
    }

    public GameObject GetRoomAccess(ROOM_TILE_TYPE newType)
    {
        switch (newType)
        {
            default:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SINGLE)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.SIDE:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SIDE)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.CONCAVE_CORNER:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SINGLE)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.DOUBLE_CONCAVE:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SINGLE)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.TRIPLE_CONCAVE:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SINGLE)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.ACCESS_SINGLE:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SINGLE)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.ACCESS_SIDE:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SIDE)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.ACCESS_CORNER:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SINGLE)
                        return tilePrefabs[i].prefab;
                break;
        }

        return null;
    }

    public GameObject GetRoomWall(ROOM_TILE_TYPE newType)
    {
        //for (int i = 0; i < roomTiles.Length; ++i)
        //    if (roomTiles[i].type == newType)
        //        return roomTiles[i].wall;

        //return null;

        switch (newType)
        {
            default:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_WALL)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.CONCAVE_CORNER:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_CORNER)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.DOUBLE_CONCAVE:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_CORNER)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.TRIPLE_CONCAVE:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_CORNER)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.ACCESS_SINGLE:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SINGLE)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.ACCESS_SIDE:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SIDE)
                        return tilePrefabs[i].prefab;
                break;
            case ROOM_TILE_TYPE.ACCESS_CORNER:
                for (int i = 0; i < tilePrefabs.Length; ++i)
                    if (tilePrefabs[i].type == TILE_PREFAB_TYPE.GENERIC_ACCESS_SINGLE)
                        return tilePrefabs[i].prefab;
                break;
        }       

        return null;
    }

}
