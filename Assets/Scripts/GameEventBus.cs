using System;
using System.Collections.Generic;
using UnityEngine;

namespace CozyTown.Core
{
    /// <summary>
    /// Append-only event stream for the current play session.
    /// </summary>
    public class GameEventBus : MonoBehaviour
    {
        public static GameEventBus Instance { get; private set; }

        public static event Action<GameEvent> OnEvent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => OnEvent = null;

        [Header("Buffer")]
        [Tooltip("How many recent events to keep in memory for UI.")]
        public int maxBufferedEvents = 500;

        private readonly Queue<GameEvent> _buffer = new Queue<GameEvent>(512);

        public IEnumerable<GameEvent> BufferedEvents => _buffer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public static void Emit(GameEvent e)
        {
            if (Instance == null) return;

            Instance.InternalEmit(e);
            OnEvent?.Invoke(e);
        }

        private void InternalEmit(GameEvent e)
        {
            _buffer.Enqueue(e);
            while (_buffer.Count > maxBufferedEvents)
                _buffer.Dequeue();
        }
    }
}
