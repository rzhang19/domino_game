using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Tests for UI actions in spectator mode. 
/// Basically click and drag to knock dominos over, and click a button to reset the domino layout.
[TestFixture]
public class SpectatorModeUI {
    // Workaround copied from BuildModeController's integration tests.
    // Is an alternative return value for helper functions that need to yield to give the game time to process actions, 
    // so their return type must be IEnumerator, not a Selectable.
    // Helpers will set this variable to the useful value instead of returning the value.
    private BH.Selectable requestedDomino;

    // Called before every test. Loads the scene and programmatically switches to spectator mode (since build mode is default)
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

    // Tests that clicking and dragging a domino will knock it over, which we'll detect by checking its changed rotation.
    [UnityTest]
    public IEnumerator Knocks_Domino_Over() 
    {        
        yield return new WaitForFixedUpdate();

        // Add a domino that we'll use for testing
        Vector3 targetMousePos = new Vector3(Screen.width/2, Screen.height/2, 0);
        Utility.ClickUIButton("ButtonSpawn");
        yield return SimulateUIToAddDominoAt(targetMousePos);
        BH.Selectable domino = requestedDomino;
        CustomTransform oldTransform = new CustomTransform(domino.transform);

        // Find the controller manager
        BH.ControllerManager controllerManager = GameObject.Find("ControllerManager").GetComponent<ControllerManager>();

        // Programmatically switch from build to spectator mode
        controllerManager.ToggleMode();
        yield return new WaitForEndOfFrame();

        // Simulate a left-click and drag from domino's center to its front (-y-axis on screen)
        float xPos = targetMousePos.x;
        float yPos = targetMousePos.y;
        InputManager.SimulateCursorMoveTo(new Vector3(xPos, yPos, 0)); //at domino center
        yield return new WaitForEndOfFrame();
        yield return Utility.SimulateKeyDown("Attack1"); // click
        int maxDist = 50;
        for (int dist=0; dist<maxDist; dist+=2)
        {
            InputManager.SimulateCursorMoveTo(new Vector3(xPos, yPos-dist, 0));
        }
        yield return Utility.SimulateKeyUp("Attack1"); // release click

        // Check the domino's rotation changed
        Assert.AreNotEqual(domino.transform.rotation, oldTransform.rotation);
    }

    // Tests that clicking the "Reset Dominos" button will reset domino to its previous rotation and position.
    // First, knocks domino over like in the previous test.
    [UnityTest]
    public IEnumerator Resets_Dominos() 
    {        
        yield return new WaitForFixedUpdate();

        // Add a domino that we'll use for testing
        Vector3 targetMousePos = new Vector3(Screen.width/2, Screen.height/2, 0);
        Utility.ClickUIButton("ButtonSpawn");
        yield return SimulateUIToAddDominoAt(targetMousePos);
        BH.Selectable domino = requestedDomino;
        CustomTransform oldTransform = new CustomTransform(domino.transform);

        // Find the controller manager
        BH.ControllerManager controllerManager = GameObject.Find("ControllerManager").GetComponent<ControllerManager>();

        // Programmatically switch from build to spectator mode
        controllerManager.ToggleMode();
        yield return new WaitForEndOfFrame();

        // Simulate a left-click and drag from domino's center to its front (-y-axis on screen)
        float xPos = targetMousePos.x;
        float yPos = targetMousePos.y;
        InputManager.SimulateCursorMoveTo(new Vector3(xPos, yPos, 0)); //at domino center
        yield return new WaitForEndOfFrame();
        yield return Utility.SimulateKeyDown("Attack1"); // click
        int maxDist = 50;
        for (int dist=0; dist<maxDist; dist+=2)
        {
            InputManager.SimulateCursorMoveTo(new Vector3(xPos, yPos-dist, 0));
        }
        yield return Utility.SimulateKeyUp("Attack1"); // release click

        // Check the domino's rotation changed, to confirm the domino was moved
        Assert.AreNotEqual(domino.transform.rotation, oldTransform.rotation);

        // Simulate pressing the Reset button
        Utility.ClickUIButton("ButtonReset");

        // Check that the domino's old rotation and position were restored
        Assert.AreEqual(domino.transform.rotation, oldTransform.rotation);
        Assert.AreEqual(domino.transform.position, oldTransform.position);
    }

    /*============================
        Private helpers
     ======================*/

    // Sets this.requestedDomino to a new domino added by simulating UI.
    // Assumes the user is in spawning mode (by clicking the button), call Utility.ClickUIButton("ButtonSpawn"); to ensure this.
    // This syncs the domino with every relevant game component
    private IEnumerator SimulateUIToAddDominoAt(Vector3 pos)
    {
        // Set up future domino lookup
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        System.Collections.Generic.List<BH.Selectable> oldList = new System.Collections.Generic.List<BH.Selectable>(dominoManager.GetActiveSelectables());

        // Now add by simulating a complete UI click on the screen
        InputManager.SimulateCursorMoveTo(pos);
        yield return new WaitForEndOfFrame();
        yield return Utility.SimulateKeyDown("Attack1");
        yield return Utility.SimulateKeyUp("Attack1");

        // Look up the newly added domino
        System.Collections.Generic.List<BH.Selectable> newList = dominoManager.GetActiveSelectables();
        requestedDomino = newList.Except(oldList).ToList()[0];
    }
}
