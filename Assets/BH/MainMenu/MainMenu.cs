using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameEvent[] _invokeOnStart;
    [SerializeField] float _delayBeforeLoadingGame;

    void Start()
    {
        foreach (GameEvent gameEvent in _invokeOnStart)
        {
            gameEvent.Invoke();
        }
    }

    // Source: https://www.youtube.com/watch?v=rXnZE8MwK-E
    /// <summary>Loads the gameplay scene.</summary>
    public void LoadGame()
    {
        StartCoroutine(AsyncLoadGame());
    }

    IEnumerator AsyncLoadGame()
    {
        yield return new WaitForSeconds(_delayBeforeLoadingGame);

        AsyncOperation async = SceneManager.LoadSceneAsync("SpectatorMode");
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            if (async.progress >= 0.9f)
            {
                async.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    /// <summary>Exits the game.</summary>
    public void Exit()
    {
        Application.Quit();
    }
}
