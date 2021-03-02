using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector3 tilePosition { get; private set; }
    private GameObject roomParent;

    private GameObject tilePrefabRef;

    private static GameObject tileRef;
    public static GameObject TileObj
    {
        get
        {
            if (tileRef == null)
            {
                tileRef = new GameObject("Tile Object");
            }
            return tileRef;
        }
    }

    public static Tile CreateTile(Vector2 newPosition, GameObject parent, GameObject tileType)
    {
        var thisTile = TileObj.AddComponent<Tile>();

        thisTile.tilePrefabRef = tileType;

        thisTile.tilePosition = thisTile.tilePrefabRef.transform.position;
        thisTile.tilePosition = new Vector3(thisTile.tilePosition.x + newPosition.x, thisTile.tilePosition.y, thisTile.tilePosition.z + newPosition.y);
        thisTile.roomParent = parent;

        tileRef = Object.Instantiate(thisTile.tilePrefabRef, thisTile.roomParent.transform);
        thisTile.transform.position = thisTile.tilePosition;
        return thisTile;
    }
}
