using UnityEngine;

namespace CozyTown.Sim
{
    public class JobProvider : MonoBehaviour
    {
        [Min(0)] public int jobSlots = 2;
    }
}
