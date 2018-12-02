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

    // Called after every test. Destroys the scene
    [TearDown] 
    public void Cleanup()
    {
        Utility.StopInputSimulations();
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
        Utility.ClickUIButton("ButtonSpawn");
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
        Utility.ClickUIButton("ButtonSpawn");
        yield return SimulateUIToAddDominoAt(pos);
        BH.Selectable newDomino = requestedDomino;

        // Simulate right clicking on the domino
        InputManager.SimulateCursorMoveTo(pos);
        yield return new WaitForEndOfFrame();
        yield return Utility.SimulateKeyDown("Attack2");

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
        Utility.ClickUIButton("ButtonDelete");

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
        Utility.ClickUIButton("ButtonSave");

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
        yield return Utility.SimulateKeyDown("Attack1");
        InputManager.SimulateCursorMoveTo(new Vector3(firstDominoPos.x+50, firstDominoPos.y, firstDominoPos.z));
        yield return new WaitForEndOfFrame();
        yield return Utility.SimulateKeyUp("Attack1");

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
        yield return Utility.SimulateKeyDown("Attack1");
        InputManager.SimulateCursorMoveTo(new Vector3(dominoPos.x+50, dominoPos.y, dominoPos.z));
        yield return new WaitForEndOfFrame();
        yield return Utility.SimulateKeyUp("Attack1");
        yield return new WaitForEndOfFrame();

        // Change 5: Delete it via UI by clicking the Delete button.
        Utility.ClickUIButton("ButtonDelete");

        // Time to undo changes by popping them off the history stack.

        // Simulate undo press to undo change 5. Domino should now be present
        yield return Utility.SimulateKeyDown("Undo");
        yield return Utility.SimulateKeyUp("Undo");
        BH.Selectable restoredDomino = dominoManager.GetActiveSelectables()[0];

        // Simulate undo press to undo change 4. Domino should now have its old position
        yield return Utility.SimulateKeyDown("Undo");
        yield return Utility.SimulateKeyUp("Undo");
        Assert.AreEqual(restoredDomino.transform.position, transformBeforeC4.position);

        // Simulate undo press to undo change 3. Domino should have its old rotation
        yield return Utility.SimulateKeyDown("Undo");
        yield return Utility.SimulateKeyUp("Undo");
        Assert.AreEqual(restoredDomino.transform.rotation, transformBeforeC3.rotation);
        
        // Simulate undo press to undo change 2. Domino should have its old color
        yield return Utility.SimulateKeyDown("Undo");
        yield return Utility.SimulateKeyUp("Undo");
        Assert.AreEqual(restoredDomino.GetColor(), colorBeforeC2); 

        // Simulate undo press to undo change 1. Domino should now be deleted (since Change 1 was to add it)
        yield return Utility.SimulateKeyDown("Undo");
        yield return Utility.SimulateKeyUp("Undo");
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);
    }

    /// User can copy and paste the selected dominos.
    /// Pasted dominos can have different positions than the copied ones if placed somewhere else, 
    /// so ignore positions. But colors and rotations should be the same.
    [UnityTest]
    public IEnumerator _Copy_Paste_UI()
    {
        yield return new WaitForFixedUpdate();

        // Make sure we're starting with 0 dominos, so we can keep track of how many dominos are in the world
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        // Turn on spawn mode
        yield return SimulateTogglingSpawnMode();

        // Add 2 selected dominos
        Vector3 firstDominoPos = new Vector3(Screen.width/2f,Screen.height/2f,0);
        yield return SimulateUIToAddDominoAt(firstDominoPos);
        BH.Selectable selectedDomino1 = requestedDomino;
        Utility.ProgrammaticallySelectDomino(selectedDomino1);
        yield return SimulateUIToAddDominoAt(new Vector3(firstDominoPos.x+50,firstDominoPos.y,0));
        BH.Selectable selectedDomino2 = requestedDomino;
        Utility.ProgrammaticallySelectDomino(selectedDomino2);

        // Store the 2 dominos for reference later
        System.Collections.Generic.List<BH.Selectable> oldList 
            = new System.Collections.Generic.List<BH.Selectable>(dominoManager.GetActiveSelectables());

        // Simulate copying them by pressing the copy key
        yield return Utility.SimulateKeyDown("Copy");
        yield return Utility.SimulateKeyUp("Copy");

        // Simulate pasting them. This displays a ghost preview of the pasted dominos that moves with the cursor.
        yield return Utility.SimulateKeyDown("Paste");
        yield return Utility.SimulateKeyUp("Paste");

        // Move the cursor to move the preview, and click to place the pasted dominos at a new location (towards +y-axis)
        InputManager.SimulateCursorMoveTo(new Vector3(firstDominoPos.x, firstDominoPos.y+50, 0));
        yield return new WaitForEndOfFrame();
        yield return Utility.SimulateKeyDown("Attack1");
        yield return Utility.SimulateKeyUp("Attack1");

        // Check that 2 more dominos were added
        System.Collections.Generic.List<BH.Selectable> newList = dominoManager.GetActiveSelectables();
        System.Collections.Generic.List<BH.Selectable> addedDominos = newList.Except(oldList).ToList();
        Assert.AreEqual(addedDominos.Count, 2);

        // Check that each new domino's color and rotation maps to an old domino's. (ugliest way ever, sorry)
        System.Collections.Generic.List<BH.Selectable> oldDominos = newList.Except(addedDominos).ToList();
        bool allDominosCovered = 
            (addedDominos[0].GetColor() == oldDominos[0].GetColor() 
                && addedDominos[0].transform.rotation == oldDominos[0].transform.rotation 
                && addedDominos[1].GetColor() == oldDominos[1].GetColor() 
                && addedDominos[1].transform.rotation == oldDominos[1].transform.rotation) 
            ||
            (addedDominos[1].GetColor() == oldDominos[0].GetColor() 
                && addedDominos[1].transform.rotation == oldDominos[0].transform.rotation 
                && addedDominos[0].GetColor() == oldDominos[1].GetColor() 
                && addedDominos[0].transform.rotation == oldDominos[1].transform.rotation);
        Assert.AreEqual(allDominosCovered, true);
    }

    /// User can right click and drag to create a selectable rectangle that selects all dominos within it.
    [UnityTest]
    public IEnumerator _Mass_Select_UI()
    {
        yield return new WaitForFixedUpdate();

        // Make sure we're starting with 0 dominos, so we can keep track of how many dominos are in the world
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        // Turn on spawn mode
        yield return SimulateTogglingSpawnMode();

        // Add 2 selected dominos near each other
        Vector3 firstDominoPos = new Vector3(Screen.width/2f,Screen.height/2f,0);
        yield return SimulateUIToAddDominoAt(firstDominoPos);
        BH.Selectable domino1 = requestedDomino;
        yield return SimulateUIToAddDominoAt(new Vector3(firstDominoPos.x+50,firstDominoPos.y,0));
        BH.Selectable domino2 = requestedDomino;

        // Simulate a right-click and drag across (almost) entire screen to create a rectangle
        int xPos = 50;
        int yPos = Screen.height-50;
        InputManager.SimulateCursorMoveTo(new Vector3(xPos, yPos, 0)); //top right corner of screen
        yield return new WaitForEndOfFrame();
        yield return Utility.SimulateKeyDown("Attack2");
        int numSteps = 50;
        int stepX = (Screen.width-100)/numSteps;
        int stepY = (Screen.height-100)/numSteps;
        for (int i=0; i<numSteps; i+=1)
        {
            InputManager.SimulateCursorMoveTo(new Vector3(xPos+i*stepX, yPos-i*stepY, 0));
            yield return new WaitForEndOfFrame();
        }
        yield return Utility.SimulateKeyUp("Attack2");

        // Check that the earlier added dominos are now selected
        Assert.AreEqual(domino1.IsSelected(), true);
        Assert.AreEqual(domino2.IsSelected(), true);
    }

    //=======================================================
    // Private helpers for this test. Mainly to simulate user input
    //=======================================================
    
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

    // Simulates toggling spawn mode by pressing the button.
    // (Note: when scene loads, spawn mode is FALSE.)
    private IEnumerator SimulateTogglingSpawnMode()
    {
        Utility.ClickUIButton("ButtonSpawn");
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
        Utility.ClickUIButton("ButtonChangeColor");
        yield return new WaitForEndOfFrame();
    }
}