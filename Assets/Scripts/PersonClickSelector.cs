using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace CozyTown.Sim
{
    public class PersonClickSelector : MonoBehaviour
    {
        public UnityEngine.Camera cam;
        public LayerMask personLayer;
        public TMP_Text output;

        void Update()
        {
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            if (cam == null) cam = UnityEngine. Camera.main;
            if (cam == null) return;

            Vector2 screen = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(screen);

            if (!Physics.Raycast(ray, out RaycastHit hit, 200f, personLayer, QueryTriggerInteraction.Ignore))
                return;

            var insp = hit.collider.GetComponentInParent<InspectablePerson>();
            var pid = hit.collider.GetComponentInParent<PersonId>();
            var agent = hit.collider.GetComponentInParent<AgentBase>();

            if (insp == null || pid == null) return;

            string s =
                $"<b>{insp.displayName}</b> (id {pid.id})\n" +
                $"HP: {agent?.hp}/{agent?.maxHP}\n" +
                $"Gold: {agent?.gold}\n" +
                $"{insp.notes}";

            if (output != null) output.text = s;
            else Debug.Log(s);
        }
    }
}
