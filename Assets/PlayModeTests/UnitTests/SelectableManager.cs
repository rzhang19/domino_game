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
        BH.Selectable newDomino = Utility.ProgrammaticallyAddDomino();

        Utility.ProgrammaticallySelectDomino(newDomino);
        
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
        BH.Selectable unselectedDomino = Utility.ProgrammaticallyAddDomino();

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
            BH.Selectable newDomino = Utility.ProgrammaticallyAddDomino();
            Utility.RandTransformChange(newDomino.transform);
            dominoRefs.Add(newDomino);
            savedTransforms.Add(newDomino.transform);
        }
        
        // Save current domino transforms. (Local save, so not linked to login)
        dominoManager.SaveDataLocal();

        // Change domino transforms
        foreach (BH.Selectable domino in dominoRefs)
        {
            Utility.RandTransformChange(domino.transform);
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
}