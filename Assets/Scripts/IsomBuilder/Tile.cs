using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IsomBuilder;
using System.Linq;
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

        public bool isAccess { get; set; }
        public TileProperties()
        {
            tilePosition = new Vector2();
            tileType = RoomData.ROOM_TILE_TYPE.EMPTY;
            isAccess = false;
        }

        public TileProperties(Vector2 position, RoomData.ROOM_TILE_TYPE type)
        {
            tilePosition = position;
            tileType = type;
            isAccess = false;
        }
    }
}

[Serializable]
public class Tile : MonoBehaviour
{
    public TileProperties tileData { get; private set; }

    private GameObject tilePrefabRef;
    private Dictionary<COORDINATES, GameObject> tileSidesRef;

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
        RoomData.ROOM_TILE_TYPE type = RoomData.ROOM_TILE_TYPE.EMPTY, int rotationTier = 0, Dictionary<COORDINATES, GameObject> sides = null)
    {
        var thisTile = TileObj.AddComponent<Tile>();
        thisTile.tileSidesRef = new Dictionary<COORDINATES, GameObject>();

        thisTile.tileSidesRef.Add(COORDINATES.UP, null);
        thisTile.tileSidesRef.Add(COORDINATES.DOWN, null);
        thisTile.tileSidesRef.Add(COORDINATES.LEFT, null);
        thisTile.tileSidesRef.Add(COORDINATES.RIGHT, null);

        thisTile.tileData = new TileProperties(gridPosition, type);

        if (tilePrefab)
        {
            thisTile.tilePrefabRef = GameObject.Instantiate(tilePrefab);
            thisTile.tilePrefabRef.transform.SetParent(thisTile.transform);
        }

        if(sides != null)
        {
            thisTile.AddTileSides(sides, rotationTier);
        }

        thisTile.transform.position = new Vector3(thisTile.tileData.tilePosition.x, 0, thisTile.tileData.tilePosition.y);
        thisTile.transform.SetParent(roomParent.transform);



        return thisTile;
    }

    public void UpdateTile(GameObject roomParent, GameObject tilePrefab = null,
       RoomData.ROOM_TILE_TYPE type = RoomData.ROOM_TILE_TYPE.EMPTY, int rotationTier = 0, Dictionary<COORDINATES, GameObject> sides = null)
    {
        if (this.tileData.tileType == type && type == RoomData.ROOM_TILE_TYPE.EMPTY)
            return;

        this.tileData = new TileProperties(this.tileData.tilePosition, type);

        if (tilePrefab)
        {
            Destroy(this.tilePrefabRef.gameObject);
            this.tilePrefabRef = GameObject.Instantiate(tilePrefab);
            this.tilePrefabRef.transform.SetParent(this.transform);
            this.tilePrefabRef.transform.localPosition = tilePrefab.transform.position;    //PIVOT
        }

        this.ClearSideReferences();

        if (sides != null)
        {
            this.AddTileSides(sides, rotationTier);
        }

        this.transform.SetParent(roomParent.transform);
    }

    //public static Tile UpdateTile(Tile thisTile, GameObject roomParent, GameObject tilePrefab = null,
    //    RoomData.ROOM_TILE_TYPE type = RoomData.ROOM_TILE_TYPE.EMPTY, int rotationTier = 0, Dictionary<COORDINATES, GameObject> sides = null)
    //{
    //    if (thisTile.tileData.tileType == type && type == RoomData.ROOM_TILE_TYPE.EMPTY)
    //        return thisTile;

    //    thisTile.tileData = new TileProperties(thisTile.tileData.tilePosition, type);

    //    if (tilePrefab)
    //    {
    //        Destroy(thisTile.tilePrefabRef.gameObject);
    //        thisTile.tilePrefabRef = GameObject.Instantiate(tilePrefab);
    //        thisTile.tilePrefabRef.transform.SetParent(thisTile.transform);
    //        thisTile.tilePrefabRef.transform.localPosition = tilePrefab.transform.position;    //PIVOT
    //    }

    //    thisTile.ClearSideReferences();

    //    if (sides != null)
    //    {
    //        //RemoveTileSides(thisTile.tileSidesRef);
    //        thisTile.AddTileSides(sides, rotationTier);
    //    }


    //    thisTile.transform.SetParent(roomParent.transform);
    //    return thisTile;
    //}

    public static Room GetTileRoom(Tile tile) { return tile.transform.parent.gameObject.GetComponent<Room>(); }

    public void AddTileSide(COORDINATES coord, GameObject sidePrefab, int rotationTier = 0)
    {
        if (this.tileSidesRef[coord])
            Destroy(this.tileSidesRef[coord].gameObject);

        switch (coord)
        {
            case COORDINATES.UP:
                rotationTier = 0;
                break;
            case COORDINATES.DOWN:
                rotationTier = 2;
                break;
            case COORDINATES.LEFT:
                rotationTier = 3;
                break;
            case COORDINATES.RIGHT:
                rotationTier = 1;
                break;
        }

        this.tileSidesRef[coord] = GameObject.Instantiate(sidePrefab, this.transform);
        this.tileSidesRef[coord].transform.localEulerAngles = new Vector3(0f, rotationTier * 90f, 0f);

    }

    public void AddTileSides(Dictionary<COORDINATES, GameObject> sides, int rotationTier = 0)
    {
        ClearSideReferences();
        foreach (COORDINATES coord in sides.Keys)
        {
            if(sides[coord] != null)
            {
                switch (coord)
                {
                    case COORDINATES.UP:
                        rotationTier = 0;
                        break;
                    case COORDINATES.DOWN:
                        rotationTier = 2;
                        break;
                    case COORDINATES.LEFT:
                        rotationTier = 3;
                        break;
                    case COORDINATES.RIGHT:
                        rotationTier = 1;
                        break;
                }
                this.tileSidesRef[coord] = GameObject.Instantiate(sides[coord], this.transform);
                this.tileSidesRef[coord].transform.localEulerAngles = new Vector3(0f, rotationTier * 90f, 0f);
            }
        }
    }

    //private static void RemoveTileSides(Dictionary<COORDINATES, GameObject> tileSides)
    //{
    //    foreach(COORDINATES coord in tileSides.Keys)
    //    {
    //        if (tileSides[coord] != null)
    //            Destroy(tileSides[coord].gameObject);
    //    }
    //}

    public void UpdateTileFloor(GameObject newTilePrefab, RoomData.ROOM_TILE_TYPE newType, int rotationTier = 0)
    {
        if (tilePrefabRef && newType != tileData.tileType)
        {
            Destroy(tilePrefabRef.gameObject);

            tilePrefabRef = GameObject.Instantiate(newTilePrefab, this.transform);
            //tilePrefabRef.transform.localEulerAngles = new Vector3(0f, rotationTier * 90f, 0f);
            tileData.tileType = newType;
        }
    }

    public void RemoveTile()
    {
        Destroy(this.gameObject);
    }

    //HELPER
    private void ClearSideReferences()
    {
        foreach (COORDINATES coord in this.tileSidesRef.Keys)
            if(this.tileSidesRef[coord])
                Destroy(this.tileSidesRef[coord].gameObject);
    }
}
