using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelector : MonoBehaviour
{
    Vector3 prevMousePos;

    private void Update()
    {
        RaycastToMouse();
    }

    private void RaycastToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 point = hit.point;
            Vector3 gridPoint = TileMath.SnapPoint(point);

            //Debug.Log("Hit with object: " + hit.collider.gameObject.name + " at position: " + gridPoint);

            if(hit.collider.tag == "Empty")
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Mouse Pressed!");
                    prevMousePos = gridPoint;
                    RoomsManager.instance.StartRoomConstruction();
                    RoomsManager.instance.AddTileToRoom(gridPoint);
                }

                if (Input.GetMouseButton(0))
                {
                    Debug.Log("Mouse being HELD!");

                    if (prevMousePos != gridPoint)
                    {
                        RoomsManager.instance.AddTileToRoom(gridPoint);
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    Debug.Log("Mouse Released!");
                    RoomsManager.instance.FinishRoomConstruction();
                }

                //if (Input.GetMouseButtonDown(1))
                //{
                //    Debug.Log("Mouse Pressed!");
                //    prevMousePos = gridPoint;
                //    RoomsManager.instance.StartRoomConstruction(1);
                //    RoomsManager.instance.AddTileToRoom(gridPoint);
                //}

                //if (Input.GetMouseButton(1))
                //{
                //    Debug.Log("Mouse being HELD!");

                //    if (prevMousePos != gridPoint)
                //    {
                //        RoomsManager.instance.AddTileToRoom(gridPoint);
                //    }
                //}

                //if (Input.GetMouseButtonUp(1))
                //{
                //    Debug.Log("Mouse Released!");

                //    RoomsManager.instance.FinishRoomConstruction();
                //}
            }




        }
    }
}
