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
    // Workaround; is an alternative return value for helper functions that need to yield to give the game time to process actions, 
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
        Vector3 pos = new Vector3(Screen.width/2f,Screen.height/2f,0);
        ClickUIButton("ButtonSpawn");
        yield return SimulateUIToAddDominoAt(pos);
        BH.Selectable newDomino = requestedDomino;

        // Simulate right clicking on the domino
        InputManager.SimulateCursorMoveTo(pos);
        yield return new WaitForEndOfFrame();
        yield return SimulateKeyDown("Attack2");

        // Check domino was internally selected
        Assert.That(newDomino.IsSelected(), Is.True);
    }

    /// User should click a button to delete all selected dominos.
    [UnityTest]
    public IEnumerator _Deletes_Selected_Dominos_UI()
    {
        yield return new WaitForFixedUpdate();

        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();

        // Make sure we're starting with no dominos in the world
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        // Programmatically create and select 2 dominos
        yield return SimulateTogglingSpawnMode();
        for (int i = 0; i < 2; i++)
        {
            yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+i*50,Screen.height/2f,0));
            BH.Selectable newDomino = requestedDomino;
            Utility.ProgrammaticallySelectDomino(newDomino);
        }

        // Add an unselected domino
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+100,Screen.height/2f,0));
        BH.Selectable unselectedDomino = requestedDomino;
        
        // Delete dominos by clicking button
        ClickUIButton("ButtonDelete");

        // Check that the only domino remaining is the unselected one
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 1);
        Assert.AreEqual(dominoManager.GetActiveSelectables()[0], unselectedDomino);
    }

    /// User should click a button to save all dominos' current transforms (i.e. position, rotation, etc).
    /// These transforms are restored when the domino layout (programmatically) resets.
    [UnityTest]
    public IEnumerator _Saves_Dominos_UI()
    {
        yield return new WaitForFixedUpdate();

        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        System.Collections.Generic.List<BH.Selectable> dominoRefs = new System.Collections.Generic.List<BH.Selectable>();
        System.Collections.Generic.List<Transform> savedTransforms = new System.Collections.Generic.List<Transform>();

        // Programmatically create 2 dominos and save their associated transforms
        yield return SimulateTogglingSpawnMode();
        for (int i = 0; i < 2; i++)
        {
            yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+i*50,Screen.height/2f,0));
            BH.Selectable newDomino = requestedDomino;
            Utility.RandTransformChange(newDomino.transform);
            dominoRefs.Add(newDomino);
            savedTransforms.Add(newDomino.transform);
        }

        // Simulate clicking the Save button
        ClickUIButton("ButtonSave");

        // Change domino transforms randomly
        foreach (BH.Selectable domino in dominoRefs)
        {
            Utility.RandTransformChange(domino.transform);
        }

        // Programmatically reset
        dominoManager.ResetData();

        // Check if original transforms were restored in the domino instances
        for (int i = 0; i < 2; i++)
        {
            BH.Selectable domino = dominoRefs[i];
            Transform expectedTransform = savedTransforms[i];
            Assert.AreEqual(domino.transform, expectedTransform);
        }
    }

    /// User can change the colors of (programmatically) selected dominos by adjusting an RGB slider (UI).
    /// Unselected dominos shouldn't change.
    [UnityTest]
    public IEnumerator _Recolors_Selected_Dominos_UI()
    {
        yield return new WaitForFixedUpdate();

        // Turn on spawn mode
        yield return SimulateTogglingSpawnMode();

        // Add 2 selected dominos
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f,Screen.height/2f,0));
        BH.Selectable selectedDomino1 = requestedDomino;
        Utility.ProgrammaticallySelectDomino(selectedDomino1);
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+50,Screen.height/2f,0));
        BH.Selectable selectedDomino2 = requestedDomino;
        Utility.ProgrammaticallySelectDomino(selectedDomino2);

        // Create 1 unselected domino
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+100,Screen.height/2f,0));
        BH.Selectable unselectedDomino = requestedDomino;
        Color normalizedUnchangedColor = unselectedDomino.GetColor();

        // Generate a new random color different from both dominos' original colors
        Color oldColor1 = selectedDomino1.GetColor();
        Color oldColor2 = selectedDomino2.GetColor();
        Color newColor;
        do {
            newColor = new Color(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255));
        } while (newColor == oldColor1 || newColor == oldColor2);

        // Simulate the user updating the selected dominos' colors by adjusting the RGB slider and clicking "Change Color" button
        yield return SimulateChangeSelectedToColor(newColor);

        // Selected dominos should have the updated color. Unselected domino should have its old color
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

        // Turn on spawn mode
        yield return SimulateTogglingSpawnMode();

        // Add 2 selected dominos
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f,Screen.height/2f,0));
        BH.Selectable selectedDomino1 = requestedDomino;
        Utility.ProgrammaticallySelectDomino(selectedDomino1);
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+50,Screen.height/2f,0));
        BH.Selectable selectedDomino2 = requestedDomino;
        Utility.ProgrammaticallySelectDomino(selectedDomino2);

        // Save the dominos' rotations (stored in their transforms)
        BH.CustomTransform oldTransform1 = new BH.CustomTransform(selectedDomino1.transform);
        BH.CustomTransform oldTransform2 = new BH.CustomTransform(selectedDomino2.transform);

        // Create 1 unselected domino and save its rotation aka transform
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+100,Screen.height/2f,0));
        BH.Selectable unselectedDomino = requestedDomino;
        BH.CustomTransform oldTransform3 = new BH.CustomTransform(unselectedDomino.transform);

        // Simulate mouse scroll wheel to rotate dominos
        float simulatedScrollAmt = 0.5f;
        InputManager.SimulateScrollTo(simulatedScrollAmt);
        yield return new WaitForEndOfFrame();

        // Check selected dominos' rotations changed
        Assert.AreNotEqual(oldTransform1.rotation, selectedDomino1.transform.rotation);
        Assert.AreNotEqual(oldTransform2.rotation, selectedDomino2.transform.rotation);

        // Check unselected domino's rotation is the same
        Assert.AreEqual(oldTransform3.rotation, unselectedDomino.transform.rotation);
    }

    /// User can click and drag selected dominos to change their positions.
    /// Unselected dominos shouldn't change.
    [UnityTest]
    public IEnumerator _Click_And_Drag_Selected_Dominos_UI()
    {
        yield return new WaitForFixedUpdate();

        // Turn on spawn mode
        yield return SimulateTogglingSpawnMode();

        // Add 2 selected dominos
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f,Screen.height/2f,0));
        BH.Selectable selectedDomino1 = requestedDomino;
        Utility.ProgrammaticallySelectDomino(selectedDomino1);
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+50,Screen.height/2f,0));
        BH.Selectable selectedDomino2 = requestedDomino;
        Utility.ProgrammaticallySelectDomino(selectedDomino2);

        // Save the dominos' positions (stored in their transforms)
        BH.CustomTransform oldTransform1 = new BH.CustomTransform(selectedDomino1.transform);
        BH.CustomTransform oldTransform2 = new BH.CustomTransform(selectedDomino2.transform);

        // Create 1 unselected domino and save its position aka transform
        yield return SimulateUIToAddDominoAt(new Vector3(Screen.width/2f+100,Screen.height/2f,0));
        BH.Selectable unselectedDomino = requestedDomino;
        BH.CustomTransform oldTransform3 = new BH.CustomTransform(unselectedDomino.transform);

        // Simulate clicking and dragging the first domino 50 units towards the +x-axis
        Vector3 firstDominoPos = new Vector3(Screen.width/2f,Screen.height/2f,0);
        InputManager.SimulateCursorMoveTo(firstDominoPos);
        yield return new WaitForEndOfFrame();
        InputManager.SimulateKeyDown("Attack1");
        yield return new WaitForEndOfFrame();
        InputManager.SimulateCursorMoveTo(new Vector3(firstDominoPos.x+50, firstDominoPos.y, firstDominoPos.z));
        yield return new WaitForEndOfFrame();
        InputManager.SimulateKeyUp("Attack1");
        yield return new WaitForEndOfFrame();

        // Check all selected dominos' positions have changed. Unselected domino has old position.
        Assert.AreNotEqual(oldTransform1.position, selectedDomino1.transform.position);
        Assert.AreNotEqual(oldTransform2.position, selectedDomino2.transform.position);
        Assert.AreEqual(oldTransform3.position, unselectedDomino.transform.position);
    }

    /// User can undo the changes in color, rotation, position, addition, and deletion of a domino.
    /// Changes are stored as a stack.
    /// Note that this function assumes all UI changes correctly change domino, which is covered in other integration tests.
    [UnityTest]
    public IEnumerator _Undo_UI()
    {
        yield return new WaitForFixedUpdate();

        // Make sure we're starting with 0 dominos, so we can easily access the domino we'll add
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        // Turn on spawn mode
        yield return SimulateTogglingSpawnMode();

        // Change 1: Add a domino via UI
        Vector3 dominoPos = new Vector3(Screen.width/2f,Screen.height/2f,0);
        yield return SimulateUIToAddDominoAt(dominoPos);
        BH.Selectable selectedDomino = requestedDomino;

        // Select it
        Utility.ProgrammaticallySelectDomino(selectedDomino);

        // Change 2: Change it to a new, random color via UI
        Color colorBeforeC2 = selectedDomino.GetColor();
        Color newColor;
        do {
            newColor = new Color(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255));
        } while (newColor == colorBeforeC2);
        yield return SimulateChangeSelectedToColor(newColor);

        // Change 3: Change its rotation via UI by simulating mouse scroll wheel
        CustomTransform transformBeforeC3 = new CustomTransform(selectedDomino.transform);
        float simulatedScrollAmt = 0.5f;
        InputManager.SimulateScrollTo(simulatedScrollAmt);
        yield return new WaitForEndOfFrame();

        // Change 4: Change its position via UI by simulating a click and drag 50 units towards the +x-axis
        CustomTransform transformBeforeC4 = new CustomTransform(selectedDomino.transform);
        InputManager.SimulateCursorMoveTo(dominoPos);
        yield return new WaitForEndOfFrame();
        InputManager.SimulateKeyDown("Attack1");
        yield return new WaitForEndOfFrame();
        InputManager.SimulateCursorMoveTo(new Vector3(dominoPos.x+50, dominoPos.y, dominoPos.z));
        yield return new WaitForEndOfFrame();
        InputManager.SimulateKeyUp("Attack1");
        yield return new WaitForEndOfFrame();

        // Change 5: Delete it via UI by clicking the Delete button.
        ClickUIButton("ButtonDelete");

        // Time to undo changes by popping them off the history stack.

        // Simulate undo press to undo change 5. Domino should now be present
        yield return SimulateKeyDown("Undo");
        yield return SimulateKeyUp("Undo");
        BH.Selectable restoredDomino = dominoManager.GetActiveSelectables()[0];

        // Simulate undo press to undo change 4. Domino should now have its old position
        yield return SimulateKeyDown("Undo");
        yield return SimulateKeyUp("Undo");
        Assert.AreEqual(restoredDomino.transform.position, transformBeforeC4.position);

        // Simulate undo press to undo change 3. Domino should have its old rotation
        yield return SimulateKeyDown("Undo");
        yield return SimulateKeyUp("Undo");
        Assert.AreEqual(restoredDomino.transform.rotation, transformBeforeC3.rotation);
        
        // Simulate undo press to undo change 2. Domino should have its old color
        yield return SimulateKeyDown("Undo");
        yield return SimulateKeyUp("Undo");
        Assert.AreEqual(restoredDomino.GetColor(), colorBeforeC2); 

        // Simulate undo press to undo change 1. Domino should now be deleted (since Change 1 was to add it)
        yield return SimulateKeyDown("Undo");
        yield return SimulateKeyUp("Undo");
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);
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

    /// Simulates pressing down the specified key. See InputManager for valid strings/names of keys.
    /// Forces a delay until the end of the frame before continuing, so keypress can be processed properly
    private IEnumerator SimulateKeyDown(string key)
    {
        InputManager.SimulateKeyDown(key);
        yield return new WaitForEndOfFrame();
    }

    /// Simulates releasing the specified key. See InputManager for valid strings/names of keys.
    /// Forces a delay until the end of the frame before continuing, so keypress can be processed properly
    private IEnumerator SimulateKeyUp(string key)
    {
        InputManager.SimulateKeyUp(key);
        yield return new WaitForEndOfFrame();
    }

    // Sets this.requestedDomino to a new domino added by simulating UI.
    // Assumes the user is in spawning mode (by clicking the button), call ClickUIButton("ButtonSpawn"); to ensure this.
    // This syncs the domino with every relevant game component
    private IEnumerator SimulateUIToAddDominoAt(Vector3 pos)
    {
        // Set up future domino lookup
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        System.Collections.Generic.List<BH.Selectable> oldList = new System.Collections.Generic.List<BH.Selectable>(dominoManager.GetActiveSelectables());

        // Now add by simulating a complete UI click on the screen
        InputManager.SimulateCursorMoveTo(pos);
        yield return new WaitForEndOfFrame();
        yield return SimulateKeyDown("Attack1");
        yield return SimulateKeyUp("Attack1");

        // Look up the newly added domino
        System.Collections.Generic.List<BH.Selectable> newList = dominoManager.GetActiveSelectables();
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

    // Simulates toggling spawn mode by pressing the button.
    // (Note: when scene loads, spawn mode is FALSE.)
    private IEnumerator SimulateTogglingSpawnMode()
    {
        ClickUIButton("ButtonSpawn");
        yield return new WaitForEndOfFrame();
        InputManager.SimulatePointerOverGameObject();
        yield return new WaitForEndOfFrame();
    }

    // Simulates changing selected dominos to the target color by adjusting the RGB slider and clicking the Change button. 
    private IEnumerator SimulateChangeSelectedToColor(Color newColor)
    {
        // Simulate the user updating the RGB slider to the new color
        Slider redSlider = GameObject.Find("RedSlider").GetComponent<Slider>();
        redSlider.value = newColor.r;
        Slider greenSlider = GameObject.Find("GreenSlider").GetComponent<Slider>();
        greenSlider.value = newColor.g;
        Slider blueSlider = GameObject.Find("BlueSlider").GetComponent<Slider>();
        blueSlider.value = newColor.b;

        // Simulate the user clicking the "Change color" button
        ClickUIButton("ButtonChangeColor");
        yield return new WaitForEndOfFrame();
    }
}