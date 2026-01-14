using UnityEngine;

namespace CozyTown.Input
{
    public static class MouseWorldPicker
    {
        // Simple: raycast onto y=0 plane (good for flat ground)
        public static bool TryGetPointOnGroundPlane(UnityEngine.Camera cam, Vector2 screenPos, float groundY, out Vector3 world)
        {
            world = default;
            if (cam == null) return false;

            Ray ray = cam.ScreenPointToRay(screenPos);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
            if (!plane.Raycast(ray, out float enter)) return false;

            world = ray.GetPoint(enter);
            return true;
        }
    }
}
