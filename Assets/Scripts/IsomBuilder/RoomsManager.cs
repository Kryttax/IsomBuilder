using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    public static RoomsManager instance = null;

    private List<Room> rooms;
    private Room currentRoom;

    private List<Tile> emptyTiles;
    private List<Vector2> occupiedTiles;

    public GameObject emptyTilePrefab;
    public GameObject[] roomTypes;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        rooms = new List<Room>();
        emptyTiles = new List<Tile>();
        occupiedTiles = new List<Vector2>();

        for (int i = 0; i < 9; ++i)
        {
            for (int j = 0; j < 9; ++j)
            {
                emptyTiles.Add(Tile.CreateTile(new Vector2(i,j), instance.gameObject, emptyTilePrefab));
            }
        }
    }

    public void StartRoomConstruction(int roomType = 0)
    {
        //GameObject newRoom = Object.Instantiate(new GameObject());
        //Room addRoom = newRoom.AddComponent<Room>() as Room;
        //currentRoom = addRoom;

        currentRoom = Instantiate(roomTypes[roomType]).GetComponent<Room>();
    }

    public void AddTileToRoom(Vector3 tilePos)
    {
        currentRoom.AddTile(new Vector2(tilePos.x, tilePos.z));
    }

    public void RemoveTileFromRoom(Vector3 tilePos)
    {
        currentRoom.RemoveTile(new Vector2(tilePos.x, tilePos.z));
    }

    public void RemoveEmptyTile(Vector2 position)
    {
        int index = GetEmptyTileAt(position);

        if(index != -1)
        {
            occupiedTiles.Add(new Vector2(emptyTiles[index].tilePosition.x, emptyTiles[index].tilePosition.y));
            Destroy(emptyTiles[index].gameObject);
            emptyTiles.RemoveAt(index);

            Debug.Log("Tile Removed at: " + position);
        }
        //else
        //{
        //    Debug.LogWarning("Position Given: " + position);
        //}
    }

    public bool IsTileInsideGrid(Vector2 position)
    {
        if (emptyTiles.FindIndex(x => (int)x.tilePosition.x == (int)position.x && (int)x.tilePosition.y == (int)position.y) == -1 &&
            occupiedTiles.FindIndex(x => (int)x.x == (int)position.x && (int)x.y == (int)position.y) == -1)
            return false;
        return true;
    }

    public int GetEmptyTileAt(Vector2 position)
    {
        return emptyTiles.FindIndex(x => (int)x.tilePosition.x == (int)position.x && (int)x.tilePosition.y == (int)position.y);
    }

    public void FillRoom(Vector2 initPos, Vector2 endPos)
    {
        Vector2[] points;
        Debug.Log("Init Point: " + initPos + "... End Point: " + endPos);

        points = RectangleHelper.GetRectanglePoints(initPos, endPos);
        //Debug.Log("Rectangle Size: " + points.Length);
        //for (int i = 0; i < points.Length; ++i)
        //{
        //    Debug.Log("Rectangle Point: " + points[i]);
        //}

        for(int i = 0; i < points.Length; ++i)
        {
            //Debug.Log("Adding Tile from rectangle: " + points[i]);
            currentRoom.AddTile(new Vector2(points[i].x, points[i].y));
        }

        currentRoom.FillRoomWithTiles();
        //FinishRoomConstruction();
    }

    public RoomData.ROOM_TILE_TYPE GetTileType(Vector2 tilePosition)
    {
        if (currentRoom)
        {
            RoomData.ROOM_TILE_TYPE type = currentRoom.GetTileTypeInRoom(tilePosition);

            if (type != RoomData.ROOM_TILE_TYPE.EMPTY)
                return type;
        }

        //Check other rooms
        Room room = rooms.Find(x => x.IsTileInRoom(tilePosition));
        if (room)
            return room.GetTileTypeInRoom(tilePosition);

        Debug.LogError("Tile not found in RoomsManager!");
        return RoomData.ROOM_TILE_TYPE.EMPTY;
    }

    public void UpdateRoom()
    {
        currentRoom.UpdateRoomTiles();
    }

    public void FinishRoomConstruction()
    {
        currentRoom.FillRoomWithTiles();
        rooms.Add(currentRoom);
        currentRoom = null;
    }
}
