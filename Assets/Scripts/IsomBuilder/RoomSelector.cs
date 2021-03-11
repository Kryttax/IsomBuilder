using UnityEngine;
using UnityEditor;

namespace IsomBuilder
{
    public static class RoomSelector 
    {
        public static Room CreateRoomOfType(RoomData.ROOM_ID type)
        {
            return RoomsManager.instance.BuildRoomType(type);
        }
    }
}
