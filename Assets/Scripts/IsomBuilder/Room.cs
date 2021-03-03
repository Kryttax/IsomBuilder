using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    private static Tile[] tileGrid;
    private static List<Vector2> roomSize;

    private RoomData data;

    public Room()
    {
        roomSize = new List<Vector2>();
    }

    public void AddTile(Vector2 tilePosition)
    {
        int index = -1;
        if(!IsTileInRoom(tilePosition, out index))
            roomSize.Add(tilePosition);
    }

    public void RemoveTile(Vector2 tilePosition)
    {
        int index = -1;
        if(IsTileInRoom(tilePosition, out index))
        {
            roomSize.RemoveAt(index);
        }
    }

    private static bool IsTileInRoom(Vector2 tilePosition, out int result)
    {
        result = roomSize.FindIndex(pos => pos.x == tilePosition.x && pos.y == tilePosition.y);

        if (result == -1)
            return false;

        return true;
    }

    public void FillRoomWithTiles()
    {
        tileGrid = new Tile[roomSize.Count];
        for (int i = 0; i < roomSize.Count; ++i)
        {
            //tileGrid[(int)tilePosition.x, (int)tilePosition.y] = new Tile(tilePosition, this);
            //Tile newTile = UnityEngine.Object.Instantiate(new Tile(roomSize[i], this));
            //GameObject newTile = UnityEngine.Object.Instantiate(new GameObject());
            //Tile addTile = newTile.AddComponent<Tile>() as Tile;
            RoomsManager.instance.RemoveEmptyTile(roomSize[i]);
            tileGrid[i] = Tile.CreateTile(roomSize[i], this.gameObject, data.GetRoomTile(RoomData.ROOM_TILE_TYPE.FLOOR));
        }
    }
}
