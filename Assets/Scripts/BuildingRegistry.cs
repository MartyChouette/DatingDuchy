using System;
using System.Collections.Generic;
using UnityEngine;
using CozyTown.Grid;

namespace CozyTown.Build
{
    public class BuildingRegistry : MonoBehaviour
    {
        public event Action<BuildingInstance> OnBuildingPlaced;

        private readonly Dictionary<HexCoord, BuildingInstance> _occupied = new();
        private readonly List<BuildingInstance> _buildings = new();
        public IReadOnlyList<BuildingInstance> Buildings => _buildings;

        public bool IsOccupied(HexCoord h) => _occupied.ContainsKey(h);

        public void Unregister(BuildingInstance inst)
        {
            if (inst == null) return;

            _buildings.Remove(inst);

            // Remove all hex cells that point to this instance
            var toRemove = new List<HexCoord>();
            foreach (var kvp in _occupied)
                if (kvp.Value == inst) toRemove.Add(kvp.Key);
            foreach (var h in toRemove)
                _occupied.Remove(h);
        }

        public bool CanPlace(BuildingDefinition def, HexCoord origin, int rotationSteps01to06, HexGridManager grid)
        {
            foreach (var cell in GetFootprintCells(def, origin, rotationSteps01to06))
            {
                if (grid != null && !grid.IsInBounds(cell)) return false;
                if (_occupied.ContainsKey(cell)) return false;
            }
            return true;
        }

        public BuildingInstance Place(BuildingDefinition def, HexCoord origin, int rotationSteps01to06, HexGridManager grid, Transform parent = null)
        {
            if (!CanPlace(def, origin, rotationSteps01to06, grid)) return null;

            GameObject go = new GameObject($"Building_{def.id}");
            if (parent != null) go.transform.SetParent(parent, true);

            var inst = go.AddComponent<BuildingInstance>();
            inst.Initialize(def, origin, rotationSteps01to06);
            inst.Registry = this;

            BuildingWorldRegistry.Register(inst);


            foreach (var cell in GetFootprintCells(def, origin, rotationSteps01to06))
                _occupied[cell] = inst;

            _buildings.Add(inst);
            OnBuildingPlaced?.Invoke(inst);

            return inst;
        }

        public IEnumerable<HexCoord> GetFootprintCells(BuildingDefinition def, HexCoord origin, int rotationSteps01to06)
        {
            // rotationSteps01to06 = 0..5 (60� steps)
            foreach (var off in def.footprint)
            {
                HexCoord rotated = RotateAxial(off.ToHex(), rotationSteps01to06);
                yield return origin + rotated;
            }
        }

        private static HexCoord RotateAxial(HexCoord h, int steps)
        {
            // Axial rotation about origin in 60� steps (pointy-top).
            // Convert to cube (x=q, z=r, y=-x-z) then rotate.
            int x = h.q;
            int z = h.r;
            int y = -x - z;

            steps = ((steps % 6) + 6) % 6;
            for (int i = 0; i < steps; i++)
            {
                // rotate cube coords: (x,y,z) -> (-z, -x, -y)
                int nx = -z;
                int ny = -x;
                int nz = -y;
                x = nx; y = ny; z = nz;
            }
            return new HexCoord(x, z);
        }
    }
}
