using UnityEngine;

namespace CozyTown.Camera
{
    public class TownCameraController : MonoBehaviour
    {
        [Header("Rig")]
        public Transform pivot;          // rotates yaw around this
        public UnityEngine.Camera cam;

        [Header("2.5D Settings")]
        [Range(10f, 80f)] public float pitchDegrees = 45f;
        public float yawDegrees = 45f;

        [Header("Zoom")]
        public float zoom = 18f;
        public float zoomMin = 8f;
        public float zoomMax = 28f;

        [Header("Speeds")]
        public float yawSpeedDegPerSec = 120f;
        public float panSpeedUnitsPerSec = 12f;
        public float zoomSpeedPerSec = 12f;

        [Header("Smoothing")]
        public float smooth = 12f;

        private float _targetYaw;
        private float _targetZoom;
        private Vector3 _targetPivotPos;

        private void Reset()
        {
            cam = UnityEngine.Camera.main;
        }

        private void Awake()
        {
            _targetYaw = yawDegrees;
            _targetZoom = zoom;
            if (pivot != null) _targetPivotPos = pivot.position;
        }

        public void AddYawInput(float axis)
        {
            _targetYaw += axis * yawSpeedDegPerSec * Time.deltaTime;
        }

        public void AddPanInput(Vector2 move)
        {
            if (pivot == null) return;

            // Pan relative to yaw, on XZ plane
            Quaternion yawRot = Quaternion.Euler(0f, _targetYaw, 0f);
            Vector3 right = yawRot * Vector3.right;
            Vector3 forward = yawRot * Vector3.forward;

            Vector3 delta = (right * move.x + forward * move.y) * (panSpeedUnitsPerSec * Time.deltaTime);
            _targetPivotPos += delta;
        }

        public void AddZoomInput(float axis)
        {
            _targetZoom = Mathf.Clamp(_targetZoom - axis * zoomSpeedPerSec * Time.deltaTime, zoomMin, zoomMax);
        }

        public void SnapTo(Transform target)
        {
            if (target == null || pivot == null) return;
            _targetPivotPos = target.position;
        }

        private void LateUpdate()
        {
            if (pivot == null || cam == null) return;

            yawDegrees = Mathf.LerpAngle(yawDegrees, _targetYaw, 1f - Mathf.Exp(-smooth * Time.deltaTime));
            zoom = Mathf.Lerp(zoom, _targetZoom, 1f - Mathf.Exp(-smooth * Time.deltaTime));

            pivot.position = Vector3.Lerp(pivot.position, _targetPivotPos, 1f - Mathf.Exp(-smooth * Time.deltaTime));
            pivot.rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);

            // camera sits back along local -Z
            cam.transform.position = pivot.position + pivot.rotation * new Vector3(0f, 0f, -zoom);
            cam.transform.rotation = pivot.rotation;
        }
    }
}
