using UnityEngine;

namespace CozyTown.Sim
{
    public class PersonId : MonoBehaviour
    {
        private static int _next = 1000;
        public int id;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => _next = 1000;

        private void Awake() => EnsureId();

        public void EnsureId()
        {
            if (id == 0) id = _next++;
        }
    }
}
