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
    public enum ROOM_TILE_TYPE { EMPTY, FLOOR, CONCAVE_CORNER, CONVEX_CORNER, DOUBLE_CONCAVE, DOUBLE_CONVEX, SIDE, DOUBLE_SIDED, TRIPLE_CONCAVE, CONVEX_CONCAVE }

    public ROOM_ID roomID;

    [Serializable]
    public struct TileTypes
    {
        public ROOM_TILE_TYPE type;
        public GameObject tile;
    }
    public TileTypes[] roomTiles;

    //public TilesDictionary roomTiles;

    public GameObject GetRoomTile(ROOM_TILE_TYPE newType)
    {
        for (int i = 0; i < roomTiles.Length; ++i)
            if (roomTiles[i].type == newType)
                return roomTiles[i].tile;

        return null;
    }

}
