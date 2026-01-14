using UnityEngine;
using CozyTown.Grid;

namespace CozyTown.Build
{
    public class BuildToolController : MonoBehaviour
    {
        [Header("Refs")]
        public HexGridManager grid;
        public BuildingRegistry registry;
        public HexCursorController cursor;
        public Transform buildingsParent;

        [Header("Placement State")]
        public BuildingDefinition currentBuilding;
        [Range(0, 5)] public int rotationSteps = 0;

        public bool IsPlacing => currentBuilding != null;

        // --- New API ---
        public void BeginPlacement(BuildingDefinition def)
        {
            currentBuilding = def;
            rotationSteps = 0;
            // TODO: show ghost (later)
        }

        public void CancelPlacement()
        {
            currentBuilding = null;
            // TODO: hide ghost
        }

        public void RotateBuilding()
        {
            if (!IsPlacing) return;
            rotationSteps = (rotationSteps + 1) % 6;
        }

        public bool TryPlaceAtCursor()
        {
            if (currentBuilding == null || grid == null || registry == null || cursor == null)
                return false;

            HexCoord h = cursor.CurrentHex;

            if (!registry.CanPlace(currentBuilding, h, rotationSteps, grid))
                return false;

            var inst = registry.Place(currentBuilding, h, rotationSteps, grid, buildingsParent);
            if (inst == null)
                return false;

            inst.transform.position = grid.HexToWorld(h);
            inst.transform.rotation = Quaternion.Euler(0f, rotationSteps * 60f, 0f);
            return true;
        }

        // Keep Place() if your input router calls it
        public void Place() => TryPlaceAtCursor();
    }
}
