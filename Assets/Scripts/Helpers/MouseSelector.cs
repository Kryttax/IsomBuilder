using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelector : MonoBehaviour
{
    IsomBuilder.StateManager.BUILD_MODE buildingMode;

    bool destroyMode = false;
    public Texture selectTexture;
    public Shader transparentShaderMask;

    private Vector2 orgBoxPos = Vector2.zero;
    private Vector2 endBoxPos = Vector2.zero;

    private Vector2 prevOrgBoxPos = Vector2.zero;
    private Vector2 prevEndBoxPos = Vector2.zero;

    [SerializeField]
    private UnityEngine.UI.Button finishRoomButton;
    [SerializeField]
    private UnityEngine.UI.Button buildRoomButton;
    [SerializeField]
    private UnityEngine.UI.Button editRoomButton;

    private void Awake()
    {
        //finishRoomButton.onClick.AddListener(() => { OnFinishRoomButton(); });
        //buildRoomButton.onClick.AddListener(() => { OnBuildRoomButton(); });
        //editRoomButton.onClick.AddListener(() => { OnEditRoomButton(); });
    }

    private void Start()
    {
        buildingMode = IsomBuilder.StateManager.BuildingMode;
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
                    switch (IsomBuilder.StateManager.BuildingMode)
                    {
                        case IsomBuilder.StateManager.BUILD_MODE.NONE:
                            break;
                        case IsomBuilder.StateManager.BUILD_MODE.BUILDING:
                            orgBoxPos = gridPoint;
                            endBoxPos = gridPoint;
                            RoomsManager.instance.FillRoomScheme(orgBoxPos, endBoxPos);
                            break;
                        case IsomBuilder.StateManager.BUILD_MODE.EDITING:
                            orgBoxPos = gridPoint;
                            endBoxPos = gridPoint;

                            //if (RoomsManager.instance.GetOccupiedTileAt(gridPoint) != -1 &&
                            //    RoomsManager.instance.AssignRoomEdit(gridPoint))   //Double Check
                            //{
                            //    //buildingMode = BUILD_MODE.BUILDING;
                            //    //orgBoxPos = gridPoint;
                            //    //RoomsManager.instance.FillRoomScheme(orgBoxPos, endBoxPos);
                            //}
                            break;
                    }
                }

                if (Input.GetMouseButton(0))
                {
                    //Debug.Log("Mouse being HELD!");

                    if ((IsomBuilder.StateManager.BuildingMode == IsomBuilder.StateManager.BUILD_MODE.BUILDING || IsomBuilder.StateManager.BuildingMode == IsomBuilder.StateManager.BUILD_MODE.EDITING) && 
                        endBoxPos != gridPoint)
                    {
                        endBoxPos = gridPoint;
                        RoomsManager.instance.FillRoomScheme(orgBoxPos, endBoxPos);
                        //RoomsManager.instance.UpdateRoom();
                    }

                }

                if (Input.GetMouseButtonUp(0))
                {
                    Debug.Log("Mouse Released!");
                    
                    if (IsomBuilder.StateManager.BuildingMode == IsomBuilder.StateManager.BUILD_MODE.BUILDING || IsomBuilder.StateManager.BuildingMode == IsomBuilder.StateManager.BUILD_MODE.EDITING)
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
                    //if (buildingMode == BUILD_MODE.BUILDING)
                    //{
                    //    //RoomsManager.instance.StartRoomDestruction();
                    //    //buildingMode = BUILD_MODE.DESTROYING;

                    //    orgBoxPos = gridPoint;
                    //}
                    //RoomsManager.instance.AddTileToRoom(gridPoint);

                    switch (buildingMode)
                    {
                        case IsomBuilder.StateManager.BUILD_MODE.NONE:
                            break;
                        case IsomBuilder.StateManager.BUILD_MODE.BUILDING:
                            orgBoxPos = gridPoint;
                            endBoxPos = gridPoint;
                            break;
                        case IsomBuilder.StateManager.BUILD_MODE.EDITING:
                            //if (RoomsManager.instance.GetOccupiedTileAt(gridPoint) != -1 &&
                            //    RoomsManager.instance.AssignRoomEdit(gridPoint))   //Double Check
                            orgBoxPos = gridPoint;
                            endBoxPos = gridPoint;

                            if (RoomsManager.instance.AssignRoomEdit(gridPoint))
                            {
                                //buildingMode = BUILD_MODE.BUILDING;
                                //orgBoxPos = gridPoint;
                            }
                            break;
                    }

                }

                if (Input.GetMouseButton(1))
                {
                    //Debug.Log("Mouse being HELD!");

                    if (buildingMode == IsomBuilder.StateManager.BUILD_MODE.BUILDING || buildingMode == IsomBuilder.StateManager.BUILD_MODE.EDITING)
                    {
                        endBoxPos = gridPoint;
                        RoomsManager.instance.EmptyRoomScheme(orgBoxPos, endBoxPos);
                    }
                }

                if (Input.GetMouseButtonUp(1))
                {
                    Debug.Log("RIGHT Mouse Released!");

                    if (buildingMode == IsomBuilder.StateManager.BUILD_MODE.BUILDING || buildingMode == IsomBuilder.StateManager.BUILD_MODE.EDITING)
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
        if (buildingMode == IsomBuilder.StateManager.BUILD_MODE.BUILDING)
            RoomsManager.instance.FinishRoomConstruction();
        else if(buildingMode == IsomBuilder.StateManager.BUILD_MODE.EDITING)
            RoomsManager.instance.FinishRoomEditing();

        buildingMode = IsomBuilder.StateManager.BUILD_MODE.NONE;
    }

    public void OnBuildRoomButton()
    {
        if (buildingMode == IsomBuilder.StateManager.BUILD_MODE.NONE)
        {
            buildingMode = IsomBuilder.StateManager.BUILD_MODE.BUILDING;
            RoomsManager.instance.StartRoomConstruction();
            RoomsManager.instance.StartRoomScheme();
        }
    }

    //public void OnBuildRoomTypeButton(RoomData.ROOM_ID type)
    //{
    //    if (buildingMode == IsomBuilder.StateManager.BUILD_MODE.NONE)
    //    {
    //        buildingMode = IsomBuilder.StateManager.BUILD_MODE.BUILDING;
    //        RoomsManager.instance.StartRoomConstruction(type);
    //        RoomsManager.instance.StartRoomScheme(type);
    //    }
    //}

    public void OnEditRoomButton()
    {
        if (buildingMode == IsomBuilder.StateManager.BUILD_MODE.BUILDING)
        {
            OnFinishRoomButton();
        }

        buildingMode = IsomBuilder.StateManager.BUILD_MODE.EDITING;
    }
}
