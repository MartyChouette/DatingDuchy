using UnityEngine;

namespace CozyTown.Sim
{
    public class HouseCapacity : MonoBehaviour
    {
        [Min(1)] public int beds = 3;
    }
}
