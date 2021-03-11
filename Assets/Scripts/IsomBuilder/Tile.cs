using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IsomBuilder;

namespace IsomBuilder
{
    [Serializable]
    public class TileProperties
    {
        //// BUILDABLE | NON_BUILDABLE: BEFORE ROOM CREATION 
        //// EMPTY | OCCUPIED: AFTER ROOM CREATION (OBJECT/AI LOGIC)
        //public enum TILE_STATUS { BUILDABLE, NON_BUILDABLE, EMPTY, OCCUPIED }

        //public TILE_STATUS currentStatus { get; private set; }
        public RoomData.ROOM_TILE_TYPE tileType { get; set; }
        public Vector2 tilePosition { get; set; }

        public TileProperties()
        {
            tilePosition = new Vector2();
            tileType = RoomData.ROOM_TILE_TYPE.EMPTY;
        }

        public TileProperties(Vector2 position, RoomData.ROOM_TILE_TYPE type)
        {
            tilePosition = position;
            tileType = type;
        }
    }
}

[Serializable]
public class Tile : MonoBehaviour
{
    public TileProperties tileData { get; private set; }

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

        thisTile.tileData = new TileProperties(gridPosition, type);
        if (tilePrefab)       
            thisTile.tilePrefabRef = GameObject.Instantiate(tilePrefab, thisTile.transform);

        thisTile.transform.position = new Vector3(thisTile.tileData.tilePosition.x , 0, thisTile.tileData.tilePosition.y); 
        thisTile.transform.SetParent(roomParent.transform);
        return thisTile;
    }

    public void UpdateTile(GameObject newTilePrefab, RoomData.ROOM_TILE_TYPE newType)
    {
        if (tilePrefabRef && newType != tileData.tileType)
        {
            Destroy(tilePrefabRef.gameObject);
            tilePrefabRef = GameObject.Instantiate(newTilePrefab, this.transform);
            tileData.tileType = newType;
        }
    }

    public void RemoveTile()
    {
        Destroy(this.gameObject);
    }
}
