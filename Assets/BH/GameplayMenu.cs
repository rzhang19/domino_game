using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayMenu : MonoBehaviour
{
    public void LoadMainMenu()
    {
        StartCoroutine(AsyncLoadMainMenu());
    }

    IEnumerator AsyncLoadMainMenu()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("MainMenu");
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
}
