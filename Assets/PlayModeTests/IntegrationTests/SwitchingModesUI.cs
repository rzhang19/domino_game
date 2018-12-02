using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Tests to toggle between build/spectator/free-fly modes by pressing the 't' key.
/// Checks that modes are entered by checking their canvases/displays.
[TestFixture]
public class SwitchingModesUI {
    // Called before every test. Loads the scene
    [SetUp]
    public void Init()
    { 
        SceneManager.LoadScene("SpectatorMode");
    }

    // Called after every test. Destroys the scene
    [TearDown] 
    public void Cleanup()
    {
        Utility.StopInputSimulations();
        //SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        //Object.Destroy(dominoManager);
    }

    /// Pressing 'f' in build mode should toggle free-fly on/off
    [UnityTest]
    public IEnumerator Switch_Between_Build_And_FreeFly() {
        yield return new WaitForFixedUpdate();

        // Make sure we're starting on build mode, i.e. build display/canvas is present
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, true);

        // Simulate 'f' keypress to toggle free-fly on
        yield return Utility.SimulateKeyDown("Toggle Free-fly");
        yield return Utility.SimulateKeyUp("Toggle Free-fly");

        // Check build mode's display is gone, and free-fly's display is present
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, false);
        Assert.AreEqual(GameObject.Find("FreeFlyCanvas").GetComponent<Canvas>().enabled, true);

        // Simulate 'f' keypress to toggle free-fly off
        yield return Utility.SimulateKeyDown("Toggle Free-fly");
        yield return Utility.SimulateKeyUp("Toggle Free-fly");

        // Check build mode's display is present, and free-fly's display is gone
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, true);
        Assert.AreEqual(GameObject.Find("FreeFlyCanvas").GetComponent<Canvas>().enabled, false);
    }

    /// Pressing 't' in build/spectator mode should toggle to the other mode
    [UnityTest]
    public IEnumerator Switch_Between_Build_And_Spectator() {
        yield return new WaitForFixedUpdate();

       // Make sure we're starting on build mode, i.e. build display/canvas is present
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, true);

        // Simulate 't' keypress to toggle spectator mode
        yield return Utility.SimulateKeyDown("Toggle Build/Spectate");
        yield return Utility.SimulateKeyUp("Toggle Build/Spectate");

        // Check build mode's display is gone, and spectator mode's display is present
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, false);
        Assert.AreEqual(GameObject.Find("SpectatorModeCanvas").GetComponent<Canvas>().enabled, true);

        // Simulate 't' keypress to toggle build mode again
        yield return Utility.SimulateKeyDown("Toggle Build/Spectate");
        yield return Utility.SimulateKeyUp("Toggle Build/Spectate");

        // Check build mode's display is present, and spectator mode's display is gone
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, true);
        Assert.AreEqual(GameObject.Find("SpectatorModeCanvas").GetComponent<Canvas>().enabled, false);
    }

    /// Pressing 'f' in spectator mode should toggle free-fly on/off
    /// Note: assumes pressing 't' to switch from build->spectator is correct, which another test covers
    [UnityTest]
    public IEnumerator Switch_Between_Spectator_And_Freefly() {
        yield return new WaitForFixedUpdate();
        
        // Make sure we're starting on build mode, i.e. build display/canvas is present
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, true);

        // Simulate 't' keypress to toggle spectator mode
        yield return Utility.SimulateKeyDown("Toggle Build/Spectate");
        yield return Utility.SimulateKeyUp("Toggle Build/Spectate");

        // Sanity check that spectator canvas is present
        Assert.AreEqual(GameObject.Find("SpectatorModeCanvas").GetComponent<Canvas>().enabled, true);

        // Simulate 'f' keypress to toggle free-fly on
        yield return Utility.SimulateKeyDown("Toggle Free-fly");
        yield return Utility.SimulateKeyUp("Toggle Free-fly");

        // Check spectator mode's display is gone, and free-fly's display is present
        Assert.AreEqual(GameObject.Find("SpectatorModeCanvas").GetComponent<Canvas>().enabled, false);
        Assert.AreEqual(GameObject.Find("FreeFlyCanvas").GetComponent<Canvas>().enabled, true);

        // Simulate 'f' keypress to toggle free-fly off
        yield return Utility.SimulateKeyDown("Toggle Free-fly");
        yield return Utility.SimulateKeyUp("Toggle Free-fly");

        // Check spectator mode's display is present, and free-fly's display is gone
        Assert.AreEqual(GameObject.Find("SpectatorModeCanvas").GetComponent<Canvas>().enabled, true);
        Assert.AreEqual(GameObject.Find("FreeFlyCanvas").GetComponent<Canvas>().enabled, false);
    }
}
