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
    // ADD WALL AND OTHER ACCESSORIES TO ROOM DATA (INDEPENDENT FROM ROOM TYPE)
    public enum ROOM_TILE_TYPE { EMPTY, FLOOR, CONCAVE_CORNER, CONVEX_CORNER, DOUBLE_CONCAVE, DOUBLE_CONVEX, SIDE, DOUBLE_SIDED, TRIPLE_CONCAVE, CONVEX_CONCAVE,
    ACCESS_SINGLE, ACCESS_SIDE, ACCESS_CORNER }

    public ROOM_ID roomID;

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
        for (int i = 0; i < roomTiles.Length; ++i)
            if (roomTiles[i].type == newType)
                return roomTiles[i].floor;

        return null;
    }

    public GameObject GetRoomWall(ROOM_TILE_TYPE newType)
    {
        for (int i = 0; i < roomTiles.Length; ++i)
            if (roomTiles[i].type == newType)
                return roomTiles[i].wall;

        return null;
    }

}
