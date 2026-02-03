using UnityEngine;

namespace CozyTown.Sim
{
    public class InspectablePerson : MonoBehaviour
    {
        [Header("Identity")]
        public string displayName = "Citizen";

        [Header("Personality (0-10)")]
        public int charm = 5;
        public int kindness = 5;
        public int wit = 5;
        public int courage = 5;
        public int warmth = 5;

        [Header("Hidden Traits (0-10)")]
        public int flirtiness = 5;
        public int loyalty = 5;

        [Header("Debug Notes")]
        [TextArea(2, 6)]
        public string notes;

        void Awake()
        {
            charm     = Mathf.Clamp(charm     + Random.Range(-3, 4), 0, 10);
            kindness  = Mathf.Clamp(kindness  + Random.Range(-3, 4), 0, 10);
            wit       = Mathf.Clamp(wit       + Random.Range(-3, 4), 0, 10);
            courage   = Mathf.Clamp(courage   + Random.Range(-3, 4), 0, 10);
            warmth    = Mathf.Clamp(warmth    + Random.Range(-3, 4), 0, 10);
            flirtiness = Mathf.Clamp(flirtiness + Random.Range(-3, 4), 0, 10);
            loyalty   = Mathf.Clamp(loyalty   + Random.Range(-3, 4), 0, 10);
        }
    }
}
