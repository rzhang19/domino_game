using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Includes tests for domino manipulation triggered by the user.
/// Integration tests between frontend UI, "middleman" BuildModeController, and backend SelectableManager/Selectable classes.
[TestFixture]
public class DominoManipulation 
{
    // Workaround; is an alternative return value for helper functions that need to yield for the game to process actions, 
    // so their return type must be IEnumerator, not a Selectable.
    // Helpers will set this variable to the useful value instead of returning the value.
    private BH.Selectable requestedDomino;

    // Called before every test. Loads the scene
    [SetUp]
    public void Init()
    { 
        SceneManager.LoadScene("SpectatorMode");
    }

    // // Called after every test. Destroys the scene
    [TearDown] 
    public void Cleanup()
    {
        StopInputSimulations();
        //SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        //Object.Destroy(dominoManager);
    }

    /// User should click "Toggle Spawnables" button, then click on screen to add a domino that's tracked by the SelectableManager class.
    [UnityTest]
    public IEnumerator _Adds_New_Domino_UI()
    {
        yield return new WaitForFixedUpdate();

        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();

        // Check no dominos
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        // Now add by enabling spawn mode and simulating UI clicks on the screen
        ClickUIButton("ButtonSpawn");
        yield return new WaitForFixedUpdate();
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f,Screen.height/2f,0));

        // Check a domino has been created
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 1);
    }

    /// User should be able to right-click on a domino on the screen to select it.
    [UnityTest]
    public IEnumerator _Selects_Domino_UI()
    {
        yield return new WaitForFixedUpdate();

        // Add a domino to test with
        BH.Selectable newDomino = Utility.ProgrammaticallyAddDomino();

        // Simulate right clicking on the domino
        InputManager.SimulateCursorMoveTo(newDomino.transform.position);
        InputManager.SimulateKeyDown("Attack2");
        
        Assert.That(newDomino.IsSelected(), Is.True);

        StopInputSimulations();
    }

    /// User should click a button to delete all selected dominos.
    [UnityTest]
    public IEnumerator _Deletes_Selected_Dominos_UI()
    {
        yield return new WaitForFixedUpdate();

        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();

        // Starting with no dominos in the world
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        // Programmatically create and select 2 dominos
        ClickUIButton("ButtonSpawn");
        InputManager.SimulatePointerOverGameObject();
        for (int i = 0; i < 2; i++)
        {
            yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+i*50,Screen.height/2f,0));
            BH.Selectable newDomino = requestedDomino;
            Utility.ProgrammaticallySelectDomino(newDomino);
        }

        // Add an unselected domino
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+100,Screen.height/2f,0));
        BH.Selectable unselectedDomino = requestedDomino;
        
        ClickUIButton("ButtonDelete");

        // Check that the only domino remaining is the unselected one
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 1);
        Assert.AreEqual(dominoManager.GetActiveSelectables()[0], unselectedDomino);
    }

    /// User should click a button to save all dominos' current transforms (i.e. position, rotation, etc).
    /// These transforms are restored when the domino layout resets.
    /// Todo: trigger layout reset thru keypress input, e.g. switching to spectator mode and back
    [UnityTest]
    public IEnumerator _Saves_Dominos_UI()
    {
        yield return new WaitForFixedUpdate();

        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        System.Collections.Generic.List<BH.Selectable> dominoRefs = new System.Collections.Generic.List<BH.Selectable>();
        System.Collections.Generic.List<Transform> savedTransforms = new System.Collections.Generic.List<Transform>();

        // Create random domino transforms that we'll save
        for (int i = 0; i < 10; i++)
        {
            BH.Selectable newDomino = Utility.ProgrammaticallyAddDomino();
            Utility.RandTransformChange(newDomino.transform);
            dominoRefs.Add(newDomino);
            savedTransforms.Add(newDomino.transform);
        }

        ClickUIButton("ButtonSave");

        // Change domino transforms
        foreach (BH.Selectable domino in dominoRefs)
        {
            Utility.RandTransformChange(domino.transform);
        }

        dominoManager.ResetData(); // ResetLayout() -> ResetData(). - Brandon

        // Check if original transforms were restored
        for (int i = 0; i < 10; i++)
        {
            BH.Selectable domino = dominoRefs[i];
            Transform expectedTransform = savedTransforms[i];
            Assert.AreEqual(domino.transform, expectedTransform);
        }
    }

    /// User should change the colors of (programmatically) selected dominos by adjusting an RGB slider (UI).
    /// Unselected dominos shouldn't change.
    [UnityTest]
    public IEnumerator _Recolors_Selected_Dominos_UI()
    {
        yield return new WaitForFixedUpdate();

        // Create 2 selected dominos
        BH.Selectable selectedDomino1 = Utility.ProgrammaticallyAddDomino();
        Utility.ProgrammaticallySelectDomino(selectedDomino1);
        BH.Selectable selectedDomino2 = Utility.ProgrammaticallyAddDomino();
        Utility.ProgrammaticallySelectDomino(selectedDomino2);

        // Create 1 unselected domino
        BH.Selectable unselectedDomino = Utility.ProgrammaticallyAddDomino();
        Color normalizedUnchangedColor = unselectedDomino.GetColor();

        // Generate a new random color different from both dominos' original colors
        Color oldColor1 = selectedDomino1.GetColor();
        Color oldColor2 = selectedDomino2.GetColor();
        Color newColor;
        do {
            newColor = new Color(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255));
        } while (newColor == oldColor1 || newColor == oldColor2);

        // Simulate the user updating the RGB slider to the new color
        Slider redSlider = GameObject.Find("RedSlider").GetComponent<Slider>();
        redSlider.value = newColor.r;
        Slider greenSlider = GameObject.Find("GreenSlider").GetComponent<Slider>();
        greenSlider.value = newColor.g;
        Slider blueSlider = GameObject.Find("BlueSlider").GetComponent<Slider>();
        blueSlider.value = newColor.b;

        // Simulate the user clicking the "Change color" button
        ClickUIButton("ButtonChangeColor");

        Assert.AreEqual(selectedDomino1.GetColor(), Utility.NormalizedColor(newColor));
        Assert.AreEqual(selectedDomino2.GetColor(), Utility.NormalizedColor(newColor));
        Assert.AreEqual(unselectedDomino.GetColor(), normalizedUnchangedColor);
    }

    /// User can rotate the (programmatically) selected dominos by scrolling the mouse wheel.
    /// Unselected dominos shouldn't change.
    [UnityTest]
    public IEnumerator _Rotates_Selected_Dominos_UI()
    {
        yield return new WaitForFixedUpdate();

        // Create 2 selected dominos
        BH.Selectable selectedDomino1 = Utility.ProgrammaticallyAddDomino();
        Utility.ProgrammaticallySelectDomino(selectedDomino1);
        BH.CustomTransform oldTransform1 = new BH.CustomTransform(selectedDomino1.transform);
        BH.Selectable selectedDomino2 = Utility.ProgrammaticallyAddDomino();
        Utility.ProgrammaticallySelectDomino(selectedDomino2);
        BH.CustomTransform oldTransform2 = new BH.CustomTransform(selectedDomino2.transform);

        // Create 1 unselected domino
        BH.Selectable unselectedDomino = Utility.ProgrammaticallyAddDomino();
        BH.CustomTransform oldTransform3 = new BH.CustomTransform(unselectedDomino.transform);

        // Simulate mouse scroll wheel to rotate dominos
        float simulatedScrollAmt = 0.5f;
        InputManager.SimulateScrollTo(simulatedScrollAmt);

        // Check selected dominos' rotations changed
        Assert.AreNotEqual(oldTransform1.rotation, selectedDomino1.transform.rotation);
        Assert.AreNotEqual(oldTransform2.rotation, selectedDomino2.transform.rotation);

        // Check unselected domino's rotation is the same
        Assert.AreEqual(oldTransform3.rotation, unselectedDomino.transform.rotation);
    }

    //=======================================================
    // Private helpers for this test. Mainly to simulate user input
    //=======================================================
    
    /// Simulates clicking a specified button in the UI.
    /// Only call after SceneManager.LoadScene() is called!
    private void ClickUIButton(string buttonName)
    {
        GameObject buttonObj = GameObject.Find(buttonName);
        Assert.That(buttonObj, Is.Not.Null);
        Button button = buttonObj.GetComponent<Button>();
        button.onClick.Invoke();
    }

    // Sets this.requestedDomino to a new domino added by simulating UI.
    // Assumes the user is in spawning mode (by clicking the button), call ClickUIButton("ButtonSpawn"); to ensure this.
    // This syncs the domino with every relevant game component
    private IEnumerator SimulateUIToAddDominoAt(Vector3 pos)
    {
        // Set up future domino lookup
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        System.Collections.Generic.List<BH.Selectable> oldList = new System.Collections.Generic.List<BH.Selectable>(dominoManager.GetActiveSelectables());

        Debug.Log("old list size: "+oldList.Count);

        // Now add by simulating a complete UI click on the screen
        InputManager.SimulateCursorMoveTo(pos);
        InputManager.SimulateKeyDown("Attack1");
        yield return new WaitForFixedUpdate();
        InputManager.SimulateKeyUp("Attack1");
        yield return new WaitForFixedUpdate();

        // Look up the newly added domino
        System.Collections.Generic.List<BH.Selectable> newList = dominoManager.GetActiveSelectables();
        Debug.Log("new list size: "+newList.Count);
        requestedDomino = newList.Except(oldList).ToList()[0];
    }

    // Disable simulated UI
    private void StopInputSimulations()
    {
        InputManager.DisableCursorSimulation();
        InputManager.DisableKeypressSimulation();
        InputManager.DisableScrollSimulation();
        InputManager.DisableSimulatePointerOverGameObject();
    }
}