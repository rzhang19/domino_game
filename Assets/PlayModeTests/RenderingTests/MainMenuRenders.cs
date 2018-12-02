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

    // Initial main menu should display "Play", "Login" and "Exit" buttons.
    [UnityTest]
    public IEnumerator _Menu_Properly_Initializes() {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
    }
}
