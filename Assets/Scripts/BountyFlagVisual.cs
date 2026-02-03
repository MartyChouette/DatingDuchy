using UnityEngine;

namespace CozyTown.Sim
{
    public class BountyFlagVisual : MonoBehaviour
    {
        public Transform followTarget;
        public Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
        public bool faceCamera = true;

        UnityEngine.Camera _cam;

        void Awake()
        {
            _cam = UnityEngine.Camera.main;
        }

        void LateUpdate()
        {
            if (followTarget == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = followTarget.position + worldOffset;

            if (faceCamera)
            {
                if (_cam == null) _cam =     UnityEngine.Camera.main;
                if (_cam != null)
                {
                    Vector3 f = (_cam.transform.position - transform.position);
                    f.y = 0f;
                    if (f.sqrMagnitude > 0.0001f)
                        transform.rotation = Quaternion.LookRotation(-f.normalized, Vector3.up);
                }
            }
        }
    }
}
