using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class TileMath 
{

    public static Vector2 SnapPoint(Vector3 point)
    {
        Vector2 snappedPoint = Vector2.zero;
        snappedPoint.x = Mathf.RoundToInt(point.x);
        snappedPoint.y = Mathf.RoundToInt(point.z);

        return snappedPoint;
    }

}
