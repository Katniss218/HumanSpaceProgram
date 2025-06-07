using System;

namespace UnityEngine
{
    /// <summary>
    /// Allows calling monobehaviour callback method remotely, as events.
    /// </summary>
    public class MonoBehaviourProxy : MonoBehaviour
    {
        public Action onStart;

        public Action onUpdate;
        public Action onLateUpdate;

        public Action onFixedUpdate;

        public Action onEnable;
        public Action onDisable;

        public Action onDestroy;

        private void Start()
        {
            onStart?.Invoke();
        }

        private void Update()
        {
            onUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            onLateUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke();
        }

        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }
        private void OnEnable()
        {
            onEnable?.Invoke();
        }
        private void OnDisable()
        {
            onDisable?.Invoke();
        }
    }
}