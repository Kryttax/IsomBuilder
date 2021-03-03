using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[CreateAssetMenu(fileName = "New TileData", menuName = "Tile Data", order = 51)]
//public class TileData : ScriptableObject
//{
//    // BUILDABLE | NON_BUILDABLE: BEFORE ROOM CREATION 
//    // EMPTY | OCCUPIED: AFTER ROOM CREATION (OBJECT/AI LOGIC)
//    public enum TILE_STATUS { BUILDABLE, NON_BUILDABLE, EMPTY, OCCUPIED }

//    public TILE_STATUS currentStatus { get; private set; }
//    public GameObject roomParent { get; private set; }
//    public Vector2 tilePosition { get; private set; }
//}


[Serializable]
public class Tile : MonoBehaviour
{
    // BUILDABLE | NON_BUILDABLE: BEFORE ROOM CREATION 
    // EMPTY | OCCUPIED: AFTER ROOM CREATION (OBJECT/AI LOGIC)
    public enum TILE_STATUS { BUILDABLE, NON_BUILDABLE, EMPTY, OCCUPIED }

    public TILE_STATUS currentStatus { get; private set; }
    public GameObject roomParent { get; private set; }
    public Vector2 tilePosition { get; private set; }
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

    public static Tile CreateTile(Vector2 gridPosition, GameObject roomParent, GameObject tilePrefab)
    {
        var thisTile = TileObj.AddComponent<Tile>();

        //thisTile.tilePrefabRef = tileType;

        //thisTile.tilePosition = thisTile.tilePrefabRef.transform.position;
        thisTile.tilePosition = gridPosition;
        //thisTile.tilePosition = new Vector3(thisTile.tilePosition.x + tilePosition.x, thisTile.tilePosition.y, thisTile.tilePosition.z + tilePosition.y);
        thisTile.roomParent = roomParent;
        thisTile.tilePrefabRef = GameObject.Instantiate(tilePrefab, thisTile.transform);
        thisTile.transform.position = new Vector3(thisTile.tilePosition.x , 0, thisTile.tilePosition.y); 
        thisTile.transform.SetParent(roomParent.transform);
        return thisTile;
    }

    public static void RemoveTile()
    {
        Destroy(TileObj);
    }
}
