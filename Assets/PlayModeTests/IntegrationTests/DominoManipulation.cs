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
/// Integration tests between frontend UI and backend SelectableManager/Selectable classes.
[TestFixture]
public class DominoManipulation 
{
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

        // Check that scene started with no dominoes
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        // Now add by simulating UI clicks on the screen
        ClickUIButton("ButtonSpawn");
        InputManager.SimulateCursorMoveTo(new Vector3(Screen.width/2f,Screen.height/2f,0));
        InputManager.SimulateKeyDown("Attack1");
        yield return new WaitForFixedUpdate();

        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 1);

        StopInputSimulations();
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
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        // Add an unselected domino
        BH.Selectable unselectedDomino = Utility.ProgrammaticallyAddDomino();

        // Programmatically create and select 10 dominos
        for (int i = 0; i < 10; i++)
        {
            BH.Selectable newDomino = Utility.ProgrammaticallyAddDomino();
            Utility.ProgrammaticallySelectDomino(newDomino);
        }
        
        ClickUIButton("ButtonDelete");

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

    // Disable simulated UI
    private void StopInputSimulations()
    {
        InputManager.DisableCursorSimulation();
        InputManager.DisableKeypressSimulation();
    }
}