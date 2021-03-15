using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IsomBuilder;

[System.Serializable]
public class MapProperties
{
    public Vector2 mapSize { get; set; }
    [SUCC.SaveThis] public List<TileProperties> tilesProperties;            //Map TileData (Serialization)

    public MapProperties(Vector2 mSize, List<TileProperties> tProperties = null)
    {
        mapSize = new Vector2(50, 50);

        if (tProperties != null)
            tilesProperties = new List<TileProperties>(tProperties);
        else tilesProperties = new List<TileProperties>();     
    }
}


public class RoomsManager : MonoBehaviour
{
    public static RoomsManager instance = null;

    private List<Room> rooms;
    private Room currentRoom;
    private Room schemeRoom;
    private List<Tile> emptyTiles;
    private List<Vector2> occupiedTiles;

    private List<Tile> mapTiles;

    public GameObject emptyTilePrefab;
    public RoomData[] roomTypes;

    private MapProperties mapProperties;

    //DEBUG ONLY
    public GameObject nodePrefab;

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
        mapTiles = new List<Tile>();

        List<TileProperties> mTileProperties = Serializer.MapConfig.Get<List<TileProperties>>("Map");

        if(mTileProperties == null)
            mapProperties = new MapProperties(new Vector2(9, 9));
        else
            mapProperties = new MapProperties(Serializer.MapConfig.Get<Vector2>("MapSize"), mTileProperties);

        for (int i = 0; i < mapProperties.mapSize.x; ++i)
        {
            for (int j = 0; j < mapProperties.mapSize.y; ++j)
            {
                Tile newMapTile = Tile.CreateTile(new Vector2(i, j), instance.gameObject, emptyTilePrefab);
                mapTiles.Add(newMapTile);
                emptyTiles.Add(newMapTile);
                mapProperties.tilesProperties.Add(newMapTile.tileData);

                Instantiate(nodePrefab, newMapTile.transform);
            }
        }

        //BASIC BEHAVIOUR. IN THE FUTURE MUST CHECK IF (NEW GAME/LOADING GAME)
        if (mTileProperties == null)
        {
            Serializer.MapConfig.Set("MapSize", mapProperties.mapSize);
            Serializer.MapConfig.Set("Map", mapProperties.tilesProperties);
        }
       

        //Serializer.Config.ResetToDefaultData();

        int size = Serializer.Config.Get<int>("Total Rooms");
        //if (size > 0)
        //{
        //    for(int i = 0; i < size; ++i)
        //    {
        //        Room loadedRoom = Room.LoadRoom(roomTypes[i].roomID.ToString(), roomTypes[i]);
        //        loadedRoom.UpdateRoomTiles();
        //        rooms.Add(loadedRoom);
        //    }
        //}
    }

    private RoomData GetRoomID(RoomData.ROOM_ID id)
    {
        for (int i = 0; i < roomTypes.Length; ++i)
            if (roomTypes[i].roomID == id)
                return roomTypes[i];

        return null;
    }

    public Room BuildRoomType(RoomData.ROOM_ID id)
    {
        return Room.CreateRoom(instance.GetRoomID(id));
    }

    public void StartRoomConstruction(RoomData.ROOM_ID id = RoomData.ROOM_ID.RED_ROOM) => currentRoom = BuildRoomType(id);

    public void StartRoomScheme(RoomData.ROOM_ID id = RoomData.ROOM_ID.RED_ROOM) => schemeRoom = BuildRoomType(id);

    //public bool StartRoomDestruction()
    //{
    //    return currentRoom != null;
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

    private Vector2 initPointScheme, endPointScheme;

    public void FillRoomScheme(Vector2 initPos, Vector2 endPos)
    {
        if(initPointScheme != initPos || endPointScheme != endPos)
        {
            initPointScheme = initPos;
            endPointScheme = endPos;

            //schemeRoom.ClearEntireRoom();
            //if(currentRoom.roomTiles.Count > 0)
            //{
            //    schemeRoom.roomTiles = new List<Tile>(currentRoom.roomTiles);
            //    schemeRoom.FillRoomTiles();
            //    for (int i = 0; i < currentRoom.roomTiles.Count; ++i)
            //        schemeRoom.AddTile(currentRoom.roomTiles[i].tileData.tilePosition);
            //}

            Vector2[] points;
            points = RectangleHelper.GetRectanglePoints(initPointScheme, endPointScheme);

            for (int i = 0; i < points.Length; ++i)
            {
                if(!Room.IsTileInRoom(schemeRoom.roomTiles, points[i]))
                    schemeRoom.AddTile(new Vector2(points[i].x, points[i].y));
            }

            schemeRoom.FillRoomTiles();
            schemeRoom.UpdateRoomTiles();
        }       
    }

    public bool CheckIfTileAccess(Vector2 tilePosition)
    {
        for(int i = 0; i < rooms.Count; ++i)
        {

            if (Room.IsTileInRoom(rooms[i].roomTiles, tilePosition))
                return true;
        }

        return false;
    }

    public void ReleaseBuildingParams()
    {
        initPointScheme = endPointScheme = Vector2.zero;
    }

    public void EmptyRoomScheme(Vector2 initPos, Vector2 endPos)
    {
        if (initPointScheme != initPos || endPointScheme != endPos)
        {
            initPointScheme = initPos;
            endPointScheme = endPos;

            //schemeRoom.ClearEntireRoom();
            //if(currentRoom.roomTiles.Count > 0)
            //{
            //    schemeRoom.roomTiles = new List<Tile>(currentRoom.roomTiles);
            //    schemeRoom.FillRoomTiles();
            //    for (int i = 0; i < currentRoom.roomTiles.Count; ++i)
            //        schemeRoom.AddTile(currentRoom.roomTiles[i].tileData.tilePosition);
            //}

            Vector2[] points;
            points = RectangleHelper.GetRectanglePoints(initPointScheme, endPointScheme);

            for (int i = 0; i < points.Length; ++i)
            {
                if (Room.IsTileInRoom(schemeRoom.roomTiles, points[i]))
                    schemeRoom.RemoveTile(new Vector2(points[i].x, points[i].y));
            }

            schemeRoom.ClearRoomTiles();
            schemeRoom.UpdateRoomTiles();
        }
    }

    public void BuildRoom()
    {
        for (int i = 0; i < schemeRoom.roomTiles.Count; ++i)
            currentRoom.AddTile(new Vector2(schemeRoom.roomTiles[i].tileData.tilePosition.x, 
                schemeRoom.roomTiles[i].tileData.tilePosition.y));

        schemeRoom.ClearEntireRoom();
        schemeRoom.RemoveRoom();
        currentRoom.FillRoomTiles();
    }

    public void FillRoom(Vector2 initPos, Vector2 endPos)
    {
        initPointScheme = endPointScheme = Vector2.zero; 

        Vector2[] points;
        points = RectangleHelper.GetRectanglePoints(initPos, endPos);

        for(int i = 0; i < points.Length; ++i)
        {
            currentRoom.AddTile(new Vector2(points[i].x, points[i].y));
        }

        currentRoom.FillRoomTiles();
    }

    public void FillRoom()
    {
        initPointScheme = endPointScheme = Vector2.zero;

        //Vector2[] points;
        //points = RectangleHelper.GetRectanglePoints(initPos, endPos);

        for (int i = 0; i < schemeRoom.roomTiles.Count; ++i)
        {
            currentRoom.AddTile(new Vector2(schemeRoom.roomTiles[i].tileData.tilePosition.x, 
                schemeRoom.roomTiles[i].tileData.tilePosition.y));
        }

        currentRoom.FillRoomTiles();
        schemeRoom.ClearEntireRoom();
    }

    public void EmptyRoom()
    {
        initPointScheme = endPointScheme = Vector2.zero;

        //Vector2[] points;
        //points = RectangleHelper.GetRectanglePoints(initPos, endPos);

        for (int i = 0; i < schemeRoom.roomTiles.Count; ++i)
        {
            currentRoom.RemoveTile(new Vector2(schemeRoom.roomTiles[i].tileData.tilePosition.x,
                schemeRoom.roomTiles[i].tileData.tilePosition.y));
        }

        currentRoom.ClearRoomTiles();
        schemeRoom.ClearEntireRoom();
    }


    public void EmptyRoom(Vector2 initPos, Vector2 endPos)
    {
        Vector2[] points;

        points = RectangleHelper.GetRectanglePoints(initPos, endPos);

        for (int i = 0; i < points.Length; ++i)
        {
            currentRoom.RemoveTile(new Vector2(points[i].x, points[i].y));
        }

        currentRoom.ClearRoomTiles();
    }

    public RoomData.ROOM_TILE_TYPE GetTileType(Vector2 tilePosition)
    {
        if (currentRoom)
        {
            RoomData.ROOM_TILE_TYPE type = Room.GetTileTypeInRoom(currentRoom.roomTiles, tilePosition);

            if (type != RoomData.ROOM_TILE_TYPE.EMPTY)
                return type;
        }

        //Check other rooms
        Room room = rooms.Find(x => Room.IsTileInRoom(x.roomTiles, tilePosition));
        if (room)
            return Room.GetTileTypeInRoom(room.roomTiles, tilePosition);

        Debug.LogError("Tile not found in RoomsManager!");
        return RoomData.ROOM_TILE_TYPE.EMPTY;
    }

    public void UpdateRoom()
    {
        currentRoom.UpdateRoomTiles();
    }

    public void FinishRoomConstruction()
    {
        rooms.Add(currentRoom);
        Serializer.Config.Set("Total Rooms", rooms.Count);
        Room.SaveRoom(roomTypes[0].roomID.ToString(), currentRoom.properties);
        currentRoom = null;
    }
}
