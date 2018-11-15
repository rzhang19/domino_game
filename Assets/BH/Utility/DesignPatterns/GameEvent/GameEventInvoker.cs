using UnityEngine;

namespace BH.DesignPatterns
{
    public class GameEventInvoker : MonoBehaviour
    {
        [SerializeField] GameEvent _gameEvent;

        public void InvokeGameEvent()
        {
            _gameEvent.Invoke();
        }
    }
}
