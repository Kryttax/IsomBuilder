﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelector : MonoBehaviour
{
    enum BUILD_MODE { NONE, BUILDING }
   // Vector3 prevMousePos;
    BUILD_MODE buildingMode = BUILD_MODE.NONE;
    bool destroyMode = false;
    public Texture selectTexture;
    public Shader transparentShaderMask;

    private Vector2 orgBoxPos = Vector2.zero;
    private Vector2 endBoxPos = Vector2.zero;

    [SerializeField]
    private UnityEngine.UI.Button finishRoomButton;
    [SerializeField]
    private UnityEngine.UI.Button buildRoomButton;

    private void Awake()
    {
        finishRoomButton.onClick.AddListener(() => { OnFinishRoomButton(); });
        buildRoomButton.onClick.AddListener(() => { OnBuildRoomButton(); });
    }

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
            Vector2 gridPoint = TileMath.SnapPoint(point);

           
            //Debug.Log("Hit with object: " + hit.collider.gameObject.name + " at position: " + gridPoint);

            //if(hit.collider.tag == "Empty")
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Mouse Pressed!");
                    //prevMousePos = gridPoint;
                    //if(buildingMode == BUILD_MODE.NONE)
                    //{
                    //    RoomsManager.instance.StartRoomConstruction();
                    //    buildingMode = BUILD_MODE.BUILDING;
                    //}
                    //RoomsManager.instance.AddTileToRoom(gridPoint);
                    if (buildingMode == BUILD_MODE.BUILDING)
                    {

                        orgBoxPos = gridPoint;
                    }
                }

                if (Input.GetMouseButton(0))
                {
                    //Debug.Log("Mouse being HELD!");

                    if (buildingMode == BUILD_MODE.BUILDING)
                    {
                        endBoxPos = gridPoint;
                        RoomsManager.instance.FillRoomScheme(orgBoxPos, endBoxPos);
                        //RoomsManager.instance.UpdateRoom();
                    }

                }

                if (Input.GetMouseButtonUp(0))
                {
                    Debug.Log("Mouse Released!");
                    
                    if (buildingMode == BUILD_MODE.BUILDING)
                    {
                        //endBoxPos = gridPoint;
                        //RoomsManager.instance.FillRoom(orgBoxPos, endBoxPos);
                        //RoomsManager.instance.UpdateRoom();
                        RoomsManager.instance.ReleaseBuildingParams();
                    }
                    //buildingMode = false;

                    orgBoxPos = Vector2.zero;
                    endBoxPos = Vector2.zero;

                }          

            }
            //else
            {
                //Debug.Log("Hit with TILE TYPE: " + RoomsManager.instance.GetTileType(gridPoint) + " at position: " + gridPoint);

                if (Input.GetMouseButtonDown(1))
                {
                    Debug.Log("RIGHT Mouse Pressed!");
                    //prevMousePos = gridPoint;
                    if (buildingMode == BUILD_MODE.BUILDING)
                    {
                        //RoomsManager.instance.StartRoomDestruction();
                        //buildingMode = BUILD_MODE.DESTROYING;

                        orgBoxPos = gridPoint;
                    }
                    //RoomsManager.instance.AddTileToRoom(gridPoint);
                    
                }

                if (Input.GetMouseButton(1))
                {
                    //Debug.Log("Mouse being HELD!");

                    //if (prevMousePos != gridPoint && buildingMode)
                    //{
                    //    RoomsManager.instance.AddTileToRoom(gridPoint);
                    //}
                    endBoxPos = gridPoint;
                    RoomsManager.instance.EmptyRoomScheme(orgBoxPos, endBoxPos);

                }

                if (Input.GetMouseButtonUp(1))
                {
                    Debug.Log("RIGHT Mouse Released!");

                    if (buildingMode == BUILD_MODE.BUILDING)
                    {
                        // endBoxPos = gridPoint;
                        //RoomsManager.instance.EmptyRoom();
                        //RoomsManager.instance.UpdateRoom();
                        RoomsManager.instance.ReleaseBuildingParams();
                    }
                    //buildingMode = false;

                    orgBoxPos = Vector2.zero;
                    endBoxPos = Vector2.zero;

                }
            }
            //else
            //{
            //        orgBoxPos = Vector2.zero;
            //        endBoxPos = Vector2.zero;
            //}




        }
    }

    public void OnFinishRoomButton()
    {
        if (buildingMode == BUILD_MODE.BUILDING)
        {
            buildingMode = BUILD_MODE.NONE;
            RoomsManager.instance.FillRoom();
            RoomsManager.instance.UpdateRoom();
            RoomsManager.instance.FinishRoomConstruction();
        }
    }

    public void OnBuildRoomButton()
    {
        if (buildingMode == BUILD_MODE.NONE)
        {
            buildingMode = BUILD_MODE.BUILDING;
            RoomsManager.instance.StartRoomConstruction();
            RoomsManager.instance.StartRoomScheme();
        }
    }
}
