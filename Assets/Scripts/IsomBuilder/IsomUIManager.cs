using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace IsomBuilder
{
    using KryTween;

    public class IsomUIManager : MonoBehaviour
    {
        public static IsomUIManager instance = null;
        #region BUTTONS
        [SerializeField]
        private GameObject generalButtonPrefab;

        [SerializeField]
        private Button buildRoom;
        [SerializeField]
        private Button finishRoom;
        [SerializeField]
        private Button editRoom;
        private List<Button> roomTypes;
        #endregion
        #region PANELS
        [SerializeField]
        public RectTransform backgroundDownPanel;
        [SerializeField]
        public RectTransform buttonsOrganizer;
        #endregion
        #region VALUES
        [SerializeField]
        private Vector2 fixedShowDownPosition;
        [SerializeField]
        private Vector2 fixedHideDownPosition;
        [SerializeField]
        private float moveGeneralSpeed;
        #endregion

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void Start()
        {

            buildRoom.onClick.AddListener(() => { OnBuildRoomClicked(); });
            finishRoom.onClick.AddListener(() => { OnFinishRoomClicked(); });

            MoveUIElementFixed(backgroundDownPanel, fixedHideDownPosition);
            MoveUIElementFixed(finishRoom.GetComponent<RectTransform>(), fixedHideDownPosition);
        }

        public void CreateRoomTypeButton(RoomData.ROOM_ID type)
        {    
            if(roomTypes == null)
                roomTypes = new List<Button>();

            GameObject newButton = Instantiate(generalButtonPrefab, buttonsOrganizer.transform);
            Button buttonRef = newButton.GetComponent<Button>();
            buttonRef.onClick.AddListener(() =>
            {
                OnRoomConstructClicked(type);
            });
            newButton.transform.Find("Text").GetComponent<Text>().text = type.ToString();
            roomTypes.Add(buttonRef);
        }

        public void OnBuildRoomClicked()
        {
            MoveUIElementFixed(buildRoom.GetComponent<RectTransform>(), fixedHideDownPosition);
            MoveUIElementFixed(backgroundDownPanel, fixedShowDownPosition);
        }

        public void OnRoomConstructClicked(RoomData.ROOM_ID type)
        {
            MoveUIElementFixed(backgroundDownPanel, fixedHideDownPosition);
            MoveUIElementFixed(finishRoom.GetComponent<RectTransform>(), fixedShowDownPosition);

            StateManager.ChangeStateTo(StateManager.BUILD_MODE.BUILDING);
            RoomsManager.instance.StartRoomConstruction(type);
            RoomsManager.instance.StartRoomScheme(type);
        }

        public void OnFinishRoomClicked()
        {
            MoveUIElementFixed(buildRoom.GetComponent<RectTransform>(), fixedShowDownPosition);
            MoveUIElementFixed(finishRoom.GetComponent<RectTransform>(), fixedHideDownPosition);

            if (StateManager.BuildingMode == IsomBuilder.StateManager.BUILD_MODE.BUILDING)
                RoomsManager.instance.FinishRoomConstruction();
            else if (StateManager.BuildingMode == IsomBuilder.StateManager.BUILD_MODE.EDITING)
                RoomsManager.instance.FinishRoomEditing();

            StateManager.ChangeStateTo(StateManager.BUILD_MODE.NONE);
        }

        public void MoveUIElementFixed(RectTransform elementRef, Vector2 moveVector)
        {
            KryTween.Move(this, elementRef, moveVector + elementRef.anchoredPosition, moveGeneralSpeed);
        }
    }

}
