using UnityEngine;
using System.Collections;

namespace BH.DesignPatterns
{
    public class GameEventInvoker : MonoBehaviour
    {
        [System.Serializable]
        class GameEventWithDelay
        {
            public GameEvent _gameEvent;
            public float _delay;
        }
        [SerializeField] GameEventWithDelay[] _gameEventsWithDelays;
        
        [SerializeField] float _baseDelay;

        public void InvokeGameEvent()
        {
            foreach (GameEventWithDelay gameEventWithDelay in _gameEventsWithDelays)
            {
                StartCoroutine(InvokeAfterSeconds(gameEventWithDelay._gameEvent, _baseDelay + gameEventWithDelay._delay));
            }
        }
        
        IEnumerator InvokeAfterSeconds(GameEvent gameEvent, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            gameEvent.Invoke();
        }
    }
}
