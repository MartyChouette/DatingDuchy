using System;
using UnityEngine;

namespace CozyTown.Grid
{
    /// <summary>
    /// Axial hex coordinates (q, r). Pointy-top layout on XZ plane.
    /// </summary>
    [Serializable]
    public struct HexCoord : IEquatable<HexCoord>
    {
        public int q;
        public int r;

        public HexCoord(int q, int r) { this.q = q; this.r = r; }

        public int s => -q - r;

        public bool Equals(HexCoord other) => q == other.q && r == other.r;
        public override bool Equals(object obj) => obj is HexCoord other && Equals(other);
        public override int GetHashCode() => (q, r).GetHashCode();
        public override string ToString() => $"({q},{r})";

        // 6 neighbors in axial coords (pointy-top)
        private static readonly HexCoord[] _dirs =
        {
            new HexCoord(+1,  0),
            new HexCoord(+1, -1),
            new HexCoord( 0, -1),
            new HexCoord(-1,  0),
            new HexCoord(-1, +1),
            new HexCoord( 0, +1),
        };

        public HexCoord Neighbor(int dir01to06)
        {
            int i = Mathf.Clamp(dir01to06 - 1, 0, 5);
            return new HexCoord(q + _dirs[i].q, r + _dirs[i].r);
        }

        public static HexCoord operator +(HexCoord a, HexCoord b) => new HexCoord(a.q + b.q, a.r + b.r);
        public static HexCoord operator -(HexCoord a, HexCoord b) => new HexCoord(a.q - b.q, a.r - b.r);
    }
}
