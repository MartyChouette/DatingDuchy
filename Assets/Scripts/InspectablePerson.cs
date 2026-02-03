using UnityEngine;

namespace CozyTown.Sim
{
    public class InspectablePerson : MonoBehaviour
    {
        [Header("Identity")]
        public string displayName = "Citizen";

        [Header("Debug Notes")]
        [TextArea(2, 6)]
        public string notes;
    }
}
