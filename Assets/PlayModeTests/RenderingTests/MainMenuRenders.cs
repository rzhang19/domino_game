using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Tests to make sure main menu displays the required canvases, e.g. login.
[TestFixture]
public class MainMenuRenders {
    // Called before every test. Loads the scene
    [SetUp]
    public void Init()
    { 
        SceneManager.LoadScene("MainMenu");
    }

    // Called after every test. Destroys the scene
    [TearDown] 
    public void Cleanup()
    {
        Utility.StopInputSimulations();
        //SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        //Object.Destroy(dominoManager);
    }

    // Main menu scene should have the login, loading, and menu canvases.
    [UnityTest]
    public IEnumerator _Menu_Properly_Initializes() {
        yield return new WaitForFixedUpdate();

        Scene scene = SceneManager.GetActiveScene();
        Assert.AreEqual(scene.name, "MainMenu");

        GameObject menuCanvas = GameObject.Find("MenuCanvas");
        Assert.That(menuCanvas, Is.Not.Null);

        GameObject loginCanvas = GameObject.Find("LoginCanvas");
        Assert.That(loginCanvas, Is.Not.Null);

        GameObject loadingCanvas = GameObject.Find("LoadingCanvas");
        Assert.That(loadingCanvas, Is.Not.Null);
    }
}
