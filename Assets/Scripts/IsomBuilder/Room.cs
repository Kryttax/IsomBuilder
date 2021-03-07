using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField]
    private RoomData data;

    private static List<Tile> tileGrid;         //Global Room Tile Reference
    private static List<Vector2> roomSize;      //Global Room Size

    private List<Vector2> tempRoomSize;         //Resets every time player expands/shrinks currentRoom

    public Room()
    {
        roomSize = new List<Vector2>();
        tileGrid = new List<Tile>();
        tempRoomSize = new List<Vector2>();
    }

    public void AddTile(Vector2 tilePosition)
    {
        int index = -1;
        if(!IsTileInRoom(tilePosition, out index))
        {
            roomSize.Add(tilePosition);
            tempRoomSize.Add(tilePosition);
        }
    }

    public void RemoveTile(Vector2 tilePosition)
    {
        int index = -1;
        if(IsTileInRoom(tilePosition, out index))
        {
            roomSize.RemoveAt(index);
        }
    }

    public RoomData.ROOM_TILE_TYPE GetTileTypeInRoom(Vector2 tilePosition)
    {
        int result = tileGrid.FindIndex(pos => pos.tilePosition.x == tilePosition.x && pos.tilePosition.y == tilePosition.y);

        if (result == -1)
            return RoomData.ROOM_TILE_TYPE.EMPTY;
        
        return tileGrid[result].tileType;
    }

    public bool IsTileInRoom(Vector2 tilePosition)
    {
        int result = roomSize.FindIndex(pos => pos.x == tilePosition.x && pos.y == tilePosition.y);

        if (result == -1)
            return false;

        return true;
    }

    private bool IsTileInRoom(Vector2 tilePosition, out int result)
    {
        result = roomSize.FindIndex(pos => pos.x == tilePosition.x && pos.y == tilePosition.y);

        if (result == -1)
            return false;

        return true;
    }

    public void FillRoomWithTiles()
    {
        //Debug.Log("Room Size: " + roomSize.Count);

        for(int i = 0; i < tempRoomSize.Count; ++i)
            RoomsManager.instance.RemoveEmptyTile(tempRoomSize[i]);

        for (int i = 0; i < tempRoomSize.Count; ++i)
        {
            //tileGrid[(int)tilePosition.x, (int)tilePosition.y] = new Tile(tilePosition, this);
            //Tile newTile = UnityEngine.Object.Instantiate(new Tile(roomSize[i], this));
            //GameObject newTile = UnityEngine.Object.Instantiate(new GameObject());
            //Tile addTile = newTile.AddComponent<Tile>() as Tile;
            tileGrid.Add(Tile.CreateTile(tempRoomSize[i], this.gameObject, data.GetRoomTile(FindNeighbours(roomSize[i]))));
            
        }

        tempRoomSize.Clear();

        //for (int i = 0; i < roomSize.Count; ++i)
        //    tileGrid[i].UpdateTile(data.GetRoomTile(Room.FindNeighbours(roomSize[i])));
    }

    public void UpdateRoomTiles()
    {
        for (int i = 0; i < roomSize.Count; ++i)
        {
            RoomData.ROOM_TILE_TYPE type = FindNeighbours(roomSize[i]);
            tileGrid.Find(x => x.tilePosition == roomSize[i]).UpdateTile(data.GetRoomTile(type), type);
        }
    }

    private RoomData.ROOM_TILE_TYPE FindNeighbours(Vector2 centralTile)
    {
        int neighbours = 0;   // sum

        //char centerTile = roomTiles[x, y]; // Cache center tile
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; // Skip center tile
                if (IsTileInRoom(new Vector2(centralTile.x + i, centralTile.y + j)))
                {
                    neighbours++;
                }
            }
        }

        RoomData.ROOM_TILE_TYPE tileType;

        switch (neighbours)
        {
            case 8:
                tileType = RoomData.ROOM_TILE_TYPE.FLOOR;
                break;
            case 7:
                tileType = RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER;
                break;
            case 6:
                tileType = RoomData.ROOM_TILE_TYPE.FLOOR;
                break;
            case 5:
                tileType = RoomData.ROOM_TILE_TYPE.SIDE;
                break;
            case 4:
                tileType = RoomData.ROOM_TILE_TYPE.CONVEX_CORNER;
                break;
            case 3:
                tileType = RoomData.ROOM_TILE_TYPE.CONVEX_CORNER;
                break;
            case 2:
                tileType = RoomData.ROOM_TILE_TYPE.TWO_SIDED;
                break;
            case 1:
                tileType = RoomData.ROOM_TILE_TYPE.END_CORNER;
                break;
            default:
                tileType = RoomData.ROOM_TILE_TYPE.FLOOR;
                Debug.LogWarning("This neighbour number: " + neighbours + " is not defined!");
                break;
        }

        Debug.Log("Tile Option: " + neighbours);

        return tileType;
    }
}
