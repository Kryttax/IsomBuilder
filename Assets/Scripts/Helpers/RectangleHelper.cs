using System;
using UnityEngine;

public class RectangleHelper 
{
    public static Vector2[] GetRectanglePoints(Vector2 originPoint, Vector2 endPoint)
    {
        Vector2[] rectanglePoints;
        int size;

        int x1, x2, y1, y2;

        if (originPoint.x >= endPoint.x)
        {
            x1 = (int)originPoint.x;
            x2 = (int)endPoint.x;
        }
        else
        {
            x1 = (int)endPoint.x; 
            x2 = (int)originPoint.x;
        }

        if (originPoint.y >= endPoint.y)
        {
            y1 = (int)originPoint.y;
            y2 = (int)endPoint.y;
        }
        else
        {
            y1 = (int)endPoint.y;
            y2 = (int)originPoint.y;
        }

        size = GetRectangleSize(x1, x2, y1, y2);

        rectanglePoints = new Vector2[size];

        int index = 0;

        for(int i = x1; i >= x2; i--)
            for(int j = y1; j >= y2; j--)
            {
                rectanglePoints[index] = new Vector2(i, j);
                index++;
            }

        return rectanglePoints;
    }

    public static int GetRectangleSize(int x1, int x2, int y1, int y2)
    {
        return (x1 + 1 - x2) * (y1 + 1 - y2);
    }

    //public static RoomData.ROOM_TILE_TYPE FindNeighbours(Vector2 centralTile)
    //{
    //    int neighbours = 0;   // Hash sum to return
    //    //int hashValue = 1; // Increases as power of 2 (1,2,4,8,...128)
        
    //    //char centerTile = roomTiles[x, y]; // Cache center tile

    //    for (int i = -1; i <= 1; i++)
    //    {
    //        for (int j = -1; j <= 1; j++)
    //        {
    //            if (i == 0 && j == 0) continue; // Skip center tile
    //            if (RoomsManager.instance.GetEmptyTileAt(new Vector2(centralTile.x + i, centralTile.y + j)) != -1)
    //            {
    //                neighbours++;
    //            }
    //        }
    //    }

    //    RoomData.ROOM_TILE_TYPE tileType;

    //    switch (neighbours)
    //    {
    //        case 8:
    //            tileType = RoomData.ROOM_TILE_TYPE.FLOOR;
    //            break;
    //        case 7:
    //            tileType = RoomData.ROOM_TILE_TYPE.CONCAVE_CORNER;
    //            break;
    //        case 5:
    //            tileType = RoomData.ROOM_TILE_TYPE.SIDE;
    //            break;
    //        case 3:
    //            tileType = RoomData.ROOM_TILE_TYPE.CONVEX_CORNER;
    //            break;
    //        default:
    //            tileType = RoomData.ROOM_TILE_TYPE.FLOOR;
    //            Debug.LogWarning("This neighbour number: " + neighbours + " is not defined!");
    //            break;
    //    }

    //    return tileType;
    //}
}