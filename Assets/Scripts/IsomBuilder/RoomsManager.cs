using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IsomBuilder;

public class RoomsManager : MonoBehaviour
{
    public static RoomsManager instance = null;


    private List<Room> rooms;
    private Room currentRoom;

    private List<Tile> emptyTiles;
    private List<Vector2> occupiedTiles;

    public GameObject emptyTilePrefab;
    public RoomData[] roomTypes;

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

        List<Vector2> test = new List<Vector2>();
        test.Add(new Vector2(0, 1));
        test.Add(new Vector2(0, 2));
        test.Add(new Vector2(0, 10));
        //Serializer.Config.ResetToDefaultData();
        //Serializer.Config.Set("Total Rooms", rooms.Count);
        //Serializer.Config.Set("Empty Tiles", emptyTiles.Count);
        //Serializer.Config.Set("Occupied Tiles", occupiedTiles.Count);
        //Serializer.Config.Set("Vector2 Test", test);
        //int exampleValue = Serializer.Config.Get<int>("Empty Tiles");
        int size = Serializer.Config.Get<int>("Total Rooms");

        Debug.Log("Rooms to Load: " + size);

        if (size > 0)
        {
            for(int i = 0; i < size; ++i)
            {
                Debug.Log("Room Property Tile(" + i + ") " + Serializer.Config.Get<List<TileProperties>>("RedRoom").Count);
                List<TileProperties> loadedRoomTiles = Serializer.Config.Get<List<TileProperties>>("RedRoom");

                Room loadedRoom = RoomsManager.BuildRoomOfType(roomTypes[i], loadedRoomTiles);
                loadedRoom.LoadRoom();
                loadedRoom.UpdateRoomTiles();
                rooms.Add(loadedRoom);
            }
        }
    }

    public static Room BuildRoomOfType(RoomData type, List<TileProperties> properties = null)
    {
        return Room.CreateRoom(type, properties);
    }

    //public static Room BuildRoomOfType(RoomData type, RoomProperties properties = null)
    //{
    //    return Room.CreateRoom(type, properties);
    //}

    public void StartRoomConstruction(int roomType = 0)
    {
        //GameObject newRoom = Object.Instantiate(new GameObject());
        //Room addRoom = newRoom.AddComponent<Room>() as Room;
        //currentRoom = addRoom;
        currentRoom = Room.CreateRoom(roomTypes[roomType]);
    }

    public bool StartRoomDestruction()
    {
        //GameObject newRoom = Object.Instantiate(new GameObject());
        //Room addRoom = newRoom.AddComponent<Room>() as Room;
        //currentRoom = addRoom;
        return currentRoom != null;
    }

    //public void AddTileToRoom(Vector3 tilePos)
    //{
    //    currentRoom.AddTile(new Vector2(tilePos.x, tilePos.z));
    //}

    //public void RemoveTileFromRoom(Vector3 tilePos)
    //{
    //    currentRoom.RemoveTile(new Vector2(tilePos.x, tilePos.z));
    //}

    public void AddEmptyTile(Vector2 position)
    {
        int index = GetOccupiedTileAt(position);

        if (index != -1)
        {
            emptyTiles.Add(Tile.CreateTile(position, instance.gameObject, emptyTilePrefab));
            occupiedTiles.RemoveAt(index);
        }
        else
            Debug.LogError("This tile "+ position +" is already empty, cannot add a empty tile here.");
    }

    public void RemoveEmptyTile(Vector2 position)
    {
        int index = GetEmptyTileAt(position);

        if(index != -1)
        {
            occupiedTiles.Add(new Vector2(emptyTiles[index].tileData.tilePosition.x, emptyTiles[index].tileData.tilePosition.y));
            Destroy(emptyTiles[index].gameObject);
            emptyTiles.RemoveAt(index);

            Debug.Log("Tile Removed at: " + position);
        }
    }

    public bool IsTileInsideGrid(Vector2 position)
    {
        if (emptyTiles.FindIndex(x => (int)x.tileData.tilePosition.x == (int)position.x && (int)x.tileData.tilePosition.y == (int)position.y) == -1 &&
            occupiedTiles.FindIndex(x => (int)x.x == (int)position.x && (int)x.y == (int)position.y) == -1)
            return false;
        return true;
    }

    public int GetOccupiedTileAt(Vector2 position)
    {
        return occupiedTiles.FindIndex(x => (int)x.x == (int)position.x && (int)x.y == (int)position.y);
    }

    public int GetEmptyTileAt(Vector2 position)
    {
        return emptyTiles.FindIndex(x => (int)x.tileData.tilePosition.x == (int)position.x && (int)x.tileData.tilePosition.y == (int)position.y);
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

        currentRoom.CreateRoomTiles();
        //FinishRoomConstruction();
    }

    public void EmptyRoom(Vector2 initPos, Vector2 endPos)
    {
        Vector2[] points;

        points = RectangleHelper.GetRectanglePoints(initPos, endPos);

        for (int i = 0; i < points.Length; ++i)
        {
            //Debug.Log("Adding Tile from rectangle: " + points[i]);
            currentRoom.RemoveTile(new Vector2(points[i].x, points[i].y));
        }

        currentRoom.ClearRoomTiles();
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
        //currentRoom.CreateRoomTiles();
        rooms.Add(currentRoom);
        Serializer.Config.Set("Total Rooms", rooms.Count);
        currentRoom.SyncRoom();
        currentRoom = null;
    }
}
