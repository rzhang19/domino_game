using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Unit test for Selectable class. For operations on dominos themselves
public class Selectable {
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
    
    /// Calling select() and deselect() on domino should switch its selection appropriately
    [UnityTest]
    public IEnumerator _Toggles_Domino_Selection()
    {
        yield return new WaitForFixedUpdate();

        // Add a domino to manipulate
        BH.Selectable newSel = Utility.ProgrammaticallyAddDomino();

        // Toggle selection to update domino's selection status
        Assert.AreEqual(newSel.IsSelected(), false);
        newSel.Select();
        Assert.AreEqual(newSel.IsSelected(), true);
        newSel.Deselect();
        Assert.AreEqual(newSel.IsSelected(), false);
    }
}
