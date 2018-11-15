using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BH.DesignPatterns
{
    public class GameEventsListener : MonoBehaviour
    {
        [SerializeField] GameEvent[] _gameEvents;

        [System.Serializable]
        class ResponseWithDelay
        {
            public UnityEvent Response;
            public float Delay;
        }
        [SerializeField] ResponseWithDelay[] _responses;

        [SerializeField] float _baseDelay;

        List<UnityAction> _unityActions = new List<UnityAction>();

        void Awake()
        {
            foreach (GameEvent gameEvent in _gameEvents)
            {
                foreach (ResponseWithDelay rwd in _responses)
                {
                    _unityActions.Add(() => InvokeAfterDelay(rwd.Response, rwd.Delay + _baseDelay));
                }
            }
        }

        void OnEnable()
        {
            foreach (GameEvent gameEvent in _gameEvents)
            {
                foreach (UnityAction unityAction in _unityActions)
                    gameEvent.AddListener(unityAction);
            }
        }

        void OnDisable()
        {
            foreach (GameEvent gameEvent in _gameEvents)
            {
                foreach (UnityAction unityAction in _unityActions)
                    gameEvent.RemoveListener(unityAction);
            }
        }

        void InvokeAfterDelay(UnityEvent unityEvent, float delay = 0f)
        {
            StartCoroutine(AsyncInvokeAfterDelay(unityEvent, delay));
        }

        IEnumerator AsyncInvokeAfterDelay(UnityEvent unityEvent, float time)
        {
            yield return new WaitForSeconds(time);
            unityEvent.Invoke();
        }
    }
}
