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

            string traits = $"CHM:{insp.charm} KND:{insp.kindness} WIT:{insp.wit} CRG:{insp.courage} WRM:{insp.warmth}";

            string romanceInfo = "";
            var rs = RelationshipSystem.Instance;
            if (rs != null)
            {
                var partners = rs.GetRomancePartners(pid.id);
                if (partners != null && partners.Count > 0)
                {
                    romanceInfo = "\n<b>Romances:</b>";
                    foreach (int partnerId in partners)
                    {
                        var rel = rs.GetRelation(pid.id, partnerId);
                        string stage = rel.isLovers ? "Lovers" : rel.isDating ? "Dating" : "Crush";
                        string partnerName = AgentBase.TryGet(partnerId, out var partnerAgent)
                            ? (partnerAgent.GetComponent<InspectablePerson>()?.displayName ?? $"#{partnerId}")
                            : $"#{partnerId}";
                        romanceInfo += $"\n  {partnerName} ({stage})";
                    }
                }
            }

            string s =
                $"<b>{insp.displayName}</b> (id {pid.id})\n" +
                $"HP: {agent?.hp}/{agent?.maxHP}\n" +
                $"Gold: {agent?.gold}\n" +
                $"{traits}" +
                $"{romanceInfo}\n" +
                $"{insp.notes}";

            if (output != null) output.text = s;
            else Debug.Log(s);
        }
    }
}
