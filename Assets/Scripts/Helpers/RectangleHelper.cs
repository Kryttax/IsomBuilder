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
}