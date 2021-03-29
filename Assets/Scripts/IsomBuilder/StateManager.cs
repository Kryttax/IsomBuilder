using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IsomBuilder
{
    public class StateManager 
    {
        public enum BUILD_MODE { NONE, BUILDING, EDITING }
        // Vector3 prevMousePos;
        public static BUILD_MODE BuildingMode { get; private set; }

        public static void ChangeStateTo(BUILD_MODE newMode)
        {
            BuildingMode = newMode;
        }

    }

}
