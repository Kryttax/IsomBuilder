using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    public static RoomsManager instance = null;

    private List<Room> rooms;
    private Room currentRoom;

    private List<Tile> emptyTiles;

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

        for (int i = 0; i < 9; ++i)
        {
            for (int j = 0; j < 9; ++j)
            {
                emptyTiles.Add(Tile.CreateTile(new Vector2(i,j), this.gameObject, emptyTilePrefab));
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
            Destroy(emptyTiles[index]);
            emptyTiles.RemoveAt(index);
        }
        else
        {
            Debug.LogWarning("Position Given: " + position);
        }
    }

    private int GetEmptyTileAt(Vector2 position)
    {
        return emptyTiles.FindIndex(x => (int)x.tilePosition.x == (int)position.x && (int)x.tilePosition.y == (int)position.y);
    }

    public void FinishRoomConstruction()
    {
        currentRoom.FillRoomWithTiles();
        rooms.Add(currentRoom);
        currentRoom = null;
    }
}
