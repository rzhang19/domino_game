using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Integration test to oversee login from the login canvas to entering the game
[TestFixture]
public class Login {
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

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator LoginWithEnumeratorPasses() {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
    }
}
