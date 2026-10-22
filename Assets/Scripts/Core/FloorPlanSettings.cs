using System;
using UnityEngine;

namespace HousePlanner.Core
{
    /// <summary>
    /// Floor plan configuration settings controlling grid behaviour,
    /// default wall dimensions, and display options.
    /// </summary>
    [Serializable]
    public class FloorPlanSettings
    {
        /// <summary>Distance in meters between grid lines used for snapping.</summary>
        public float gridSize = 0.5f;

        /// <summary>Whether objects snap to the nearest grid intersection.</summary>
        public bool snapToGrid = true;

        /// <summary>Whether on-screen measurement labels are displayed.</summary>
        public bool showMeasurements = true;

        /// <summary>Default height of newly created walls in meters.</summary>
        public float defaultWallHeight = 2.8f;

        /// <summary>Default thickness of newly created walls in meters.</summary>
        public float defaultWallThickness = 0.15f;

        /// <summary>Unit label used for all measurements (e.g. "meters").</summary>
        public string units = "meters";

        /// <summary>
        /// Validates the settings and clamps values to sensible ranges.
        /// </summary>
        /// <returns><c>true</c> if the settings are valid; otherwise <c>false</c>.</returns>
        public bool Validate()
        {
            bool isValid = true;

            if (gridSize <= 0f)
            {
                Debug.LogWarning("[FloorPlanSettings] gridSize must be positive. Clamping to 0.1m.");
                gridSize = 0.1f;
                isValid = false;
            }

            if (defaultWallHeight <= 0f)
            {
                Debug.LogWarning("[FloorPlanSettings] defaultWallHeight must be positive. Clamping to 2.4m.");
                defaultWallHeight = 2.4f;
                isValid = false;
            }

            if (defaultWallThickness <= 0f)
            {
                Debug.LogWarning("[FloorPlanSettings] defaultWallThickness must be positive. Clamping to 0.1m.");
                defaultWallThickness = 0.1f;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(units))
            {
                Debug.LogWarning("[FloorPlanSettings] units is empty. Defaulting to 'meters'.");
                units = "meters";
                isValid = false;
            }

            return isValid;
        }
    }
}
