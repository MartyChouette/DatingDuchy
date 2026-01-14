using UnityEngine;

namespace CozyTown.Sim
{
    public class PersonId : MonoBehaviour
    {
        private static int _next = 1000;
        public int id;

        private void Awake()
        {
            if (id == 0) id = _next++;
        }
    }
}
