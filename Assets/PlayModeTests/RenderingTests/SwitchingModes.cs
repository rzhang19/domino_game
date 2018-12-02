using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Tests to check that correct mode displays/canvas appear while switching between build/spectator/free-fly modes. 
/// No UI, just programmatically switches via ControllerManager class
[TestFixture]
public class SwitchingModes {

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
        //SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        //Object.Destroy(dominoManager);
    }

    /// Switching from build to free-fly should hide build canvas and show free-fly canvas.
    /// Reverse should be true, too
    [UnityTest]
    public IEnumerator Switch_Between_Build_And_FreeFly() {
        // Find the controller manager
        BH.ControllerManager controllerManager = GameObject.Find("ControllerManager").GetComponent<ControllerManager>();

        // Make sure we're starting on build mode, i.e. build display/canvas is present
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, true);

        // Programmatically switch to free-fly
        controllerManager.ToggleFreeFly();
        yield return new WaitForEndOfFrame();

        // Check build mode's display is gone, and free-fly's display is present
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, false);
        Assert.AreEqual(GameObject.Find("FreeFlyCanvas").GetComponent<Canvas>().enabled, true);

        // Programmatically switch back to build
        controllerManager.ToggleFreeFly();
        yield return new WaitForEndOfFrame();

        // Check build mode's display is present, and free-fly's display is gone
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, true);
        Assert.AreEqual(GameObject.Find("FreeFlyCanvas").GetComponent<Canvas>().enabled, false);

    }

    /// Switching from build to spectator should hide build canvas and show spectator canvas.
    /// Reverse should be true, too
    [UnityTest]
    public IEnumerator Switch_Between_Build_And_Spectator() {
        // Find the controller manager
        BH.ControllerManager controllerManager = GameObject.Find("ControllerManager").GetComponent<ControllerManager>();

        // Make sure we're starting on build mode, i.e. build display/canvas is present
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, true);

        // Programmatically switch to spectator mode
        controllerManager.ToggleMode();
        yield return new WaitForEndOfFrame();

        // Check build mode's display is gone, and spectator mode's display is present
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, false);
        Assert.AreEqual(GameObject.Find("SpectatorModeCanvas").GetComponent<Canvas>().enabled, true);

        // Programmatically switch from spectator back to build
        controllerManager.ToggleMode();
        yield return new WaitForEndOfFrame();

        // Check build mode's display is present, and spectator mode's display is gone
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, true);
        Assert.AreEqual(GameObject.Find("SpectatorModeCanvas").GetComponent<Canvas>().enabled, false);
    }

    /// Switching from spectator to freefly should hide spectator canvas and show freefly canvas.
    /// Reverse should be true, too.
    /// Note: assumes switching from build->spectator is correct, which another test covers
    [UnityTest]
    public IEnumerator Switch_Between_Spectator_And_Freefly() {
        // Find the controller manager
        BH.ControllerManager controllerManager = GameObject.Find("ControllerManager").GetComponent<ControllerManager>();

        // Make sure we're starting on build mode, i.e. build display/canvas is present
        Assert.AreEqual(GameObject.Find("BuildModeCanvas").GetComponent<Canvas>().enabled, true);

        // Programmatically switch from build to spectator mode
        controllerManager.ToggleMode();
        yield return new WaitForEndOfFrame();

        // Sanity check that spectator canvas is present
        Assert.AreEqual(GameObject.Find("SpectatorModeCanvas").GetComponent<Canvas>().enabled, true);

        // Programmatically switch from spectator to freefly
        controllerManager.ToggleFreeFly();
        yield return new WaitForEndOfFrame();

        // Check spectator mode's display is gone, and free-fly's display is present
        Assert.AreEqual(GameObject.Find("SpectatorModeCanvas").GetComponent<Canvas>().enabled, false);
        Assert.AreEqual(GameObject.Find("FreeFlyCanvas").GetComponent<Canvas>().enabled, true);

        // Programmatically switch back to spectator
        controllerManager.ToggleFreeFly();
        yield return new WaitForEndOfFrame();

        // Check spectator mode's display is present, and free-fly's display is gone
        Assert.AreEqual(GameObject.Find("SpectatorModeCanvas").GetComponent<Canvas>().enabled, true);
        Assert.AreEqual(GameObject.Find("FreeFlyCanvas").GetComponent<Canvas>().enabled, false);
    }
}
