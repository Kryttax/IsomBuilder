using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class TileData
{
    public TileData()
    {
        tilePosition = new Vector2();
        tileType = RoomData.ROOM_TILE_TYPE.EMPTY;
    }

    public TileData(RoomData.ROOM_TILE_TYPE type, Vector2 position)
    {
        tileType = type;
        tilePosition = position;
    }


    public RoomData.ROOM_TILE_TYPE tileType { get; set; }
    public Vector2 tilePosition { get; set; }
}


[Serializable]
public class Tile : MonoBehaviour
{
    //// BUILDABLE | NON_BUILDABLE: BEFORE ROOM CREATION 
    //// EMPTY | OCCUPIED: AFTER ROOM CREATION (OBJECT/AI LOGIC)
    //public enum TILE_STATUS { BUILDABLE, NON_BUILDABLE, EMPTY, OCCUPIED }

    //public TILE_STATUS currentStatus { get; private set; }


    //public GameObject roomParent { get; private set; }
    //public RoomData.ROOM_TILE_TYPE tileType { get; private set; }
    //public Vector2 tilePosition { get; private set; }

    public TileData tileData { get; private set; }
    private GameObject tilePrefabRef;

    private static GameObject tileRef;
    public static GameObject TileObj
    {
        get
        {
            //if (tileRef == null)
            {
                tileRef = new GameObject("Tile Object");
            }
            return tileRef;
        }
    }

    public static Tile CreateTile(Vector2 gridPosition, GameObject roomParent, GameObject tilePrefab = null, RoomData.ROOM_TILE_TYPE type = RoomData.ROOM_TILE_TYPE.EMPTY)
    {
        var thisTile = TileObj.AddComponent<Tile>();

        //thisTile.tilePrefabRef = tileType;
        thisTile.tileData = new TileData();
        //thisTile.tilePosition = thisTile.tilePrefabRef.transform.position;
        thisTile.tileData.tilePosition = gridPosition;
        //thisTile.tilePosition = new Vector3(thisTile.tilePosition.x + tilePosition.x, thisTile.tilePosition.y, thisTile.tilePosition.z + tilePosition.y);
        //thisTile.roomParent = roomParent;

        if (tilePrefab)
        {
            thisTile.tilePrefabRef = GameObject.Instantiate(tilePrefab, thisTile.transform);
            thisTile.tileData.tileType = type;
        }

        thisTile.transform.position = new Vector3(thisTile.tileData.tilePosition.x , 0, thisTile.tileData.tilePosition.y); 
        thisTile.transform.SetParent(roomParent.transform);
        return thisTile;
    }

    public void UpdateTile(GameObject newTilePrefab, RoomData.ROOM_TILE_TYPE newType)
    {
        //var thisTile = GetComponent<Tile>();
        
        if (tilePrefabRef && newType != tileData.tileType)
        {
            //Debug.LogWarning("Destroying previous tile prefab and updating...");
            Destroy(tilePrefabRef.gameObject);

            tilePrefabRef = GameObject.Instantiate(newTilePrefab, this.transform);
            tileData.tileType = newType;

        }
        //else
        //    Debug.LogWarning("Tile prefab not updating...");

       
    }

    public void RemoveTile()
    {
        Destroy(this.gameObject);
    }
}
