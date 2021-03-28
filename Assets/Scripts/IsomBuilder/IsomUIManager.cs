using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace IsomBuilder
{
    using KryTween;

    public class IsomUIManager : MonoBehaviour
    {
        #region BUTTONS
        [SerializeField]
        private Button buildRoom;
        [SerializeField]
        private Button finishRoom;
        [SerializeField]
        private Button editRoom;
        [SerializeField]
        private List<Button> roomTypes;
        #endregion
        #region PANELS
        [SerializeField]
        public RectTransform backgroundDownPanel;
        #endregion
        #region VALUES
        [SerializeField]
        private Vector2 fixedShowDownPosition;
        [SerializeField]
        private Vector2 fixedHideDownPosition;
        [SerializeField]
        private float moveGeneralSpeed;
        #endregion


        private void Start()
        {
            //KryTween.MoveAndReturn(this, backgroundDownPanel, new Vector2(0, -150), 1f, 3f);
            buildRoom.onClick.AddListener(() => { OnBuildRoomClicked(); });
            finishRoom.onClick.AddListener(() => { OnFinishRoomClicked(); });
            //buildRoom.onClick.AddListener(() => { OnBuildRoomClicked(); });

            for(int i = 0; i < roomTypes.Count; ++i)
            {
                roomTypes[i].onClick.AddListener(() => { OnRoomConstructClicked(); });
            }


            MoveUIElementFixed(backgroundDownPanel, fixedHideDownPosition);
            MoveUIElementFixed(finishRoom.GetComponent<RectTransform>(), fixedHideDownPosition);
        }

        public void OnBuildRoomClicked()
        {
            MoveUIElementFixed(buildRoom.GetComponent<RectTransform>(), fixedHideDownPosition);
            MoveUIElementFixed(backgroundDownPanel, fixedShowDownPosition);
        }

        public void OnRoomConstructClicked()
        {
            MoveUIElementFixed(backgroundDownPanel, fixedHideDownPosition);
            MoveUIElementFixed(finishRoom.GetComponent<RectTransform>(), fixedShowDownPosition);
        }

        public void OnFinishRoomClicked()
        {
            MoveUIElementFixed(buildRoom.GetComponent<RectTransform>(), fixedShowDownPosition);
            MoveUIElementFixed(backgroundDownPanel, fixedHideDownPosition);
            MoveUIElementFixed(finishRoom.GetComponent<RectTransform>(), fixedHideDownPosition);
        }

        public void MoveUIElementFixed(RectTransform elementRef, Vector2 moveVector)
        {
            KryTween.Move(this, elementRef, moveVector + elementRef.anchoredPosition, moveGeneralSpeed);
        }
    }

}
