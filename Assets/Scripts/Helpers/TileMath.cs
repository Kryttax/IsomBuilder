using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMath 
{

    public static Vector2 SnapPoint(Vector2 point)
    {
        Vector2 snappedPoint = Vector2.zero;
        snappedPoint.x = Mathf.Round(point.x);
        snappedPoint.y = Mathf.Round(point.y);

        return snappedPoint;
    }
    public static Vector3 SnapPoint(Vector3 point)
    {
        Vector3 snappedPoint = Vector3.zero;
        snappedPoint.x = Mathf.Round(point.x);
        snappedPoint.y = Mathf.Round(point.y);
        snappedPoint.z = Mathf.Round(point.z);

        return snappedPoint;
    }


}
