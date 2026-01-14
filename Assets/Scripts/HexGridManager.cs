using UnityEngine;

namespace CozyTown.Grid
{
    /// <summary>
    /// Converts between HexCoord and world positions. Pointy-top hexes on XZ plane.
    /// </summary>
    public class HexGridManager : MonoBehaviour
    {
        [Header("Hex Layout")]
        [Tooltip("Radius from center to a corner in world units.")]
        public float hexSize = 1.0f;

        [Tooltip("World origin of (0,0).")]
        public Vector3 origin = Vector3.zero;

        [Header("Bounds")]
        public int radius = 25; // simple 'blob' bounds for now

        public bool IsInBounds(HexCoord h)
        {
            // radius-limited hex "circle" in cube coords
            int s = h.s;
            return Mathf.Abs(h.q) <= radius && Mathf.Abs(h.r) <= radius && Mathf.Abs(s) <= radius;
        }

        public Vector3 HexToWorld(HexCoord h)
        {
            // Pointy-top axial -> world (x,z)
            float x = hexSize * (Mathf.Sqrt(3f) * h.q + Mathf.Sqrt(3f) / 2f * h.r);
            float z = hexSize * (3f / 2f * h.r);
            return origin + new Vector3(x, 0f, z);
        }

        public HexCoord WorldToHex(Vector3 world)
        {
            Vector3 p = world - origin;
            float q = (Mathf.Sqrt(3f) / 3f * p.x - 1f / 3f * p.z) / hexSize;
            float r = (2f / 3f * p.z) / hexSize;
            return HexRound(q, r);
        }

        private static HexCoord HexRound(float q, float r)
        {
            // Convert axial -> cube, round, convert back
            float x = q;
            float z = r;
            float y = -x - z;

            int rx = Mathf.RoundToInt(x);
            int ry = Mathf.RoundToInt(y);
            int rz = Mathf.RoundToInt(z);

            float xDiff = Mathf.Abs(rx - x);
            float yDiff = Mathf.Abs(ry - y);
            float zDiff = Mathf.Abs(rz - z);

            if (xDiff > yDiff && xDiff > zDiff) rx = -ry - rz;
            else if (yDiff > zDiff) ry = -rx - rz;
            else rz = -rx - ry;

            return new HexCoord(rx, rz);
        }

        private void OnDrawGizmosSelected()
        {
            // Very lightweight debug: draw a few rings (optional)
            Gizmos.color = new Color(1f, 1f, 1f, 0.08f);
            for (int r = 1; r <= Mathf.Min(radius, 8); r++)
            {
                // draw a simple ring of points
                var h = new HexCoord(0, -r);
                for (int side = 0; side < 6; side++)
                {
                    for (int step = 0; step < r; step++)
                    {
                        Gizmos.DrawSphere(HexToWorld(h), 0.05f);
                        h = h.Neighbor(side + 1);
                    }
                }
            }
        }
    }
}
