using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Unit tests for Selectable Manager class, which is essentially the backend manager of dominos.
/// Tests are essentially programmatic manipulations of dominos, like adding and saving them.
[TestFixture]
public class SelectableManagerClass
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
        //SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        //Object.Destroy(dominoManager);
    }

    /// Can SelectableManager add a new domino?
    [UnityTest]
    public IEnumerator _Adds_New_Domino_Programmatically()
    {
        yield return new WaitForFixedUpdate();

        // Check that scene started with no dominoes
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        dominoManager.SpawnSelectable();
        
        // Check a new domino has been placed.
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 1);
    }

    /// Can SelectableManager select a domino?
    [UnityTest]
    public IEnumerator _Selects_Domino_Programmatically()
    {
        yield return new WaitForFixedUpdate();

        // Add a domino to test with
        BH.Selectable newDomino = ProgrammaticallyAddDomino();

        ProgrammaticallySelectDomino(newDomino);
        
        Assert.That(newDomino.IsSelected(), Is.True);
    }

    /// Can SelectableManager delete a specified domino?
    [UnityTest]
    public IEnumerator _Deletes_Domino_Programmatically()
    {
        yield return new WaitForFixedUpdate();

        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);

        // Add an unselected domino
        BH.Selectable unselectedDomino = ProgrammaticallyAddDomino();

        // Despawn it
        dominoManager.DespawnSelectable(unselectedDomino);

        // Number of stored dominos should be 0
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);
    }

    /// Can SelectableManager save dominos such that resetting them will discard all changes after the save
    [UnityTest]
    public IEnumerator _Saves_Dominos_Programmatically()
    {
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
        
        // Save current domino transforms
        dominoManager.SaveDataPersistent();

        // Change domino transforms
        foreach (BH.Selectable domino in dominoRefs)
        {
            RandTransformChange(domino.transform);
        }

        // Programmatically reset dominos
        dominoManager.ResetData();

        // Check if original transforms were restored
        for (int i = 0; i < 10; i++)
        {
            BH.Selectable domino = dominoRefs[i];
            Transform expectedTransform = savedTransforms[i];
            Assert.AreEqual(domino.transform, expectedTransform);
        }
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

    // Disable simulated UI
    private void StopInputSimulations()
    {
        InputManager.DisableCursorSimulation();
        InputManager.DisableKeypressSimulation();
    }

    public struct Point {
        public float x;
        public float y;
    }
}