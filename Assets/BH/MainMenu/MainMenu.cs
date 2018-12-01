using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameEvent[] _invokeOnStart;

    void Start()
    {
        foreach (GameEvent gameEvent in _invokeOnStart)
        {
            gameEvent.Invoke();
        }
    }
}
