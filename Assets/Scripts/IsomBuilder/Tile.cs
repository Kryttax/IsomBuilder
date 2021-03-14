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
    private Dictionary<Room.COORDINATES, GameObject> tileSidesRef;

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

    public static Tile CreateTile(Vector2 gridPosition, GameObject roomParent, GameObject tilePrefab = null, 
        RoomData.ROOM_TILE_TYPE type = RoomData.ROOM_TILE_TYPE.EMPTY, int rotationTier = 0, Dictionary<Room.COORDINATES, GameObject> sides = null)
    {
        var thisTile = TileObj.AddComponent<Tile>();
        thisTile.tileSidesRef = new Dictionary<Room.COORDINATES, GameObject>();

        thisTile.tileSidesRef.Add(Room.COORDINATES.UP, null);
        thisTile.tileSidesRef.Add(Room.COORDINATES.DOWN, null);
        thisTile.tileSidesRef.Add(Room.COORDINATES.LEFT, null);
        thisTile.tileSidesRef.Add(Room.COORDINATES.RIGHT, null);

        thisTile.tileData = new TileProperties(gridPosition, type);

        if (tilePrefab)
        {
            thisTile.tilePrefabRef = GameObject.Instantiate(tilePrefab, thisTile.transform);
            thisTile.tilePrefabRef.transform.localEulerAngles = new Vector3(0f, rotationTier * 90f, 0f);
        }

        if(sides != null)
        {
            foreach (Room.COORDINATES coord in sides.Keys)
                thisTile.AddTileSide(coord, sides[coord], rotationTier);
        }

        thisTile.transform.position = new Vector3(thisTile.tileData.tilePosition.x , 0, thisTile.tileData.tilePosition.y);
        thisTile.transform.SetParent(roomParent.transform);
        return thisTile;
    }

    public void AddTileSide(Room.COORDINATES coord, GameObject sidePrefab = null, int rotationTier = 0)
    {
        if(sidePrefab)
        {
            this.tileSidesRef[coord] = GameObject.Instantiate(sidePrefab, this.transform);
            this.tileSidesRef[coord].transform.localEulerAngles = new Vector3(0f, rotationTier * 90f, 0f);
        }
    }

    public void AddTileSides(Dictionary<Room.COORDINATES, GameObject> sides, int rotationTier = 0)
    {
        ClearSideReferences();
        foreach (Room.COORDINATES coord in sides.Keys)
        {
            if(sides[coord] != null)
            {
                this.tileSidesRef[coord] = GameObject.Instantiate(sides[coord], this.transform);
                this.tileSidesRef[coord].transform.localEulerAngles = new Vector3(0f, rotationTier * 90f, 0f);
            }
        }

    }

    //public void UpdateTile(GameObject newTilePrefab, RoomData.ROOM_TILE_TYPE newType, int rotationTier = 0)
    //{
    //    if (tilePrefabRef && newType != tileData.tileType)
    //    {
    //        Destroy(tilePrefabRef.gameObject);
    //        tilePrefabRef = GameObject.Instantiate(newTilePrefab, this.transform);
    //        tileData.tileType = newType;
    //        this.tilePrefabRef.transform.localEulerAngles = new Vector3(0f, rotationTier * 90f, 0f);
    //    }
    //}

    public void UpdateTileFloor(GameObject newTilePrefab, RoomData.ROOM_TILE_TYPE newType, int rotationTier = 0)
    {
        if (tilePrefabRef && newType != tileData.tileType)
        {
            Destroy(tilePrefabRef.gameObject);
            tilePrefabRef = GameObject.Instantiate(newTilePrefab, this.transform);
            this.tilePrefabRef.transform.localEulerAngles = new Vector3(0f, rotationTier * 90f, 0f);
            tileData.tileType = newType;
        }
    }

    public void RemoveTile()
    {
        Destroy(this.gameObject);
    }

    private void ClearSideReferences()
    {
        foreach (Room.COORDINATES coord in this.tileSidesRef.Keys)
            Destroy(this.tileSidesRef[coord]);
    }
}
