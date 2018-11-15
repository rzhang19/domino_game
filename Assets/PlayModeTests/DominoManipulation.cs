using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using BH;

public class AddDomino 
{
    /// User should click a button to add a domino that's tracked by the SelectableManager class.
    [UnityTest]
    public IEnumerator _Adds_New_Domino()
    {
        SceneManager.LoadScene("SpectatorMode");
        yield return new WaitForFixedUpdate();

        // Check that scene started with no dominoes
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        ClickUIButton("ButtonAdd");
        
        // Check a new domino has been placed.
        // (Not checking default position/rotation etc. since we'll let user change that later)
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 1);
    }

    /// User should be able to select a single domino. Important for other tests!
    /// Todo: switch selection from programmatic to mouse input
    [UnityTest]
    public IEnumerator _Selects_Domino()
    {
        SceneManager.LoadScene("SpectatorMode");
        yield return new WaitForFixedUpdate();

        BH.Selectable newDomino = ProgrammaticallyAddDomino();
        ProgrammaticallySelectDomino(newDomino);
        
        Assert.That(newDomino.IsSelected(), Is.True);
    }

    /// User should click a button to delete all selected dominos.
    [UnityTest]
    public IEnumerator _Deletes_Selected_Dominos()
    {
        SceneManager.LoadScene("SpectatorMode");
        yield return new WaitForFixedUpdate();

        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        // Add an unselected domino
        BH.Selectable unselectedDomino = ProgrammaticallyAddDomino();

        // Programmatically create and select 10 dominos
        for (int i = 0; i < 10; i++)
        {
            BH.Selectable newDomino = ProgrammaticallyAddDomino();
            ProgrammaticallySelectDomino(newDomino);
        }
        
        ClickUIButton("ButtonDelete");

        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 1);
        Assert.AreEqual(dominoManager.GetActiveSelectables()[0], unselectedDomino);
    }

    /// User should click a button to save all dominos' current transforms (i.e. position, rotation, etc).
    /// These transforms are restored when the domino layout resets.
    /// Todo: trigger layout reset thru keypress input, e.g. switching to spectator mode and back
    [UnityTest]
    public IEnumerator _Saves_Dominos()
    {
        SceneManager.LoadScene("SpectatorMode");
        yield return new WaitForFixedUpdate();

        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        System.Collections.Generic.List<BH.Selectable> dominoRefs = new System.Collections.Generic.List<BH.Selectable>();
        System.Collections.Generic.List<Transform> savedTransforms = new System.Collections.Generic.List<Transform>();

        // Create random domino transforms that we'll save
        for (int i = 0; i < 10; i++)
        {
            BH.Selectable newDomino = ProgrammaticallyAddDomino();
            RandTransformChange(newDomino.transform);
            dominoRefs.Add(newDomino);
            savedTransforms.Add(newDomino.transform);
        }
        ClickUIButton("ButtonSave");

        // Change domino transforms
        foreach (BH.Selectable domino in dominoRefs)
        {
            RandTransformChange(domino.transform);
        }

        dominoManager.ResetLayout();

        // Check if original transforms were restored
        for (int i = 0; i < 10; i++)
        {
            BH.Selectable domino = dominoRefs[i];
            Transform expectedTransform = savedTransforms[i];
            Assert.AreEqual(domino.transform, expectedTransform);
        }
    }

    /// User should change the colors of selected dominos by adjusting an RGB slider.
    [UnityTest]
    public IEnumerator _Recolors_Selected_Dominos()
    {
        SceneManager.LoadScene("SpectatorMode");
        yield return new WaitForFixedUpdate();

        // Create 2 selected dominos
        BH.Selectable selectedDomino1 = ProgrammaticallyAddDomino();
        ProgrammaticallySelectDomino(selectedDomino1);
        BH.Selectable selectedDomino2 = ProgrammaticallyAddDomino();
        ProgrammaticallySelectDomino(selectedDomino2);

        // Create 1 unselected domino
        BH.Selectable unselectedDomino = ProgrammaticallyAddDomino();
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

        Assert.AreEqual(selectedDomino1.GetColor(), NormalizedColor(newColor));
        Assert.AreEqual(selectedDomino2.GetColor(), NormalizedColor(newColor));
        Assert.AreEqual(unselectedDomino.GetColor(), normalizedUnchangedColor);
    }
    

    //=======================================================
    // Private helpers for this test
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

    /// Programmatically adds a domino to SelectableManager and returns it.
    /// Only call after SceneManager.LoadScene() is called!
    private BH.Selectable ProgrammaticallyAddDomino()
    {
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        System.Collections.Generic.List<BH.Selectable> oldDominos 
            = new System.Collections.Generic.List<BH.Selectable>(dominoManager.GetActiveSelectables());
        dominoManager.SpawnSelectable();
        System.Collections.Generic.List<BH.Selectable> newDominos = dominoManager.GetActiveSelectables();
        Assert.AreEqual(newDominos.Count - oldDominos.Count, 1); // sanity check
        return newDominos.Except(oldDominos).ToList()[0];
    }

    /// Programatically selects the given domino.
    /// Selection is delegated to BuildModeController.
    private void ProgrammaticallySelectDomino(BH.Selectable domino)
    {   
        BuildModeController buildController = GameObject.Find("BuildModeController").GetComponent<BuildModeController>();
        buildController.Select(domino);
    }

    /// Randomly changes the position, rotation, and local scale of the inputted transform in-place.
    private void RandTransformChange(Transform baseT)
    {
        baseT.position = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        baseT.Rotate(new Vector3(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)));
        baseT.localScale += new Vector3(Random.Range(-0.5f,0.5f), Random.Range(-0.5f,0.5f), Random.Range(-0.5f,0.5f));
    }

    /// Returns a Color with normalized RGB values of the input Color's.
    /// Normalized from [0, 255] to [0, 1]
    private Color NormalizedColor(Color orig)
    {
        return new Color(orig.r/255.0f, orig.g/255.0f, orig.b/255.0f);
    }
}