using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Tests that a domino renders when a backend domino is programmatically created by SelectableManager.
public class DominoRenders {
    
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

    // Domino should be active in world after a domino is created
    [UnityTest]
    public IEnumerator _New_Domino_Renders() {
        // Programmatically create domino
        BH.Selectable newDomino = Utility.ProgrammaticallyAddDomino();

        // Wait for world re-render
        yield return new WaitForEndOfFrame();

        // Check that domino exists in world
        newDomino.name = newDomino.GetInstanceID().ToString();
        Assert.AreNotEqual(GameObject.Find(newDomino.name), null);
    }
}
