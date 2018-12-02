using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Test that the mass-select rectangle renders when it's programmatically generated.
public class MassSelectionRectRenders {
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

    /// Test that the mass-select rectangle exists in the world when it's programmatically generated.
    [UnityTest]
    public IEnumerator MassSelectionRect_Renders() {
        // Prepare to draw selection rectangle
        SelectionRectController selectionRectController = GameObject.Find("SelectionRectManager").GetComponent<SelectionRectController>();
        System.Collections.Generic.List<BH.Selectable> junkSel = new System.Collections.Generic.List<BH.Selectable>();

        // Check that selection rectangle isn't active/present in world yet
        Assert.AreEqual(selectionRectController.transform.Find("SelectionRect").gameObject.activeSelf, false);

        // To trigger rectangle render, need to tell its drawing function info about "click and drag"
        int numSteps = 50;
        int maxWidth = Screen.width;
        int step = maxWidth/numSteps;

        // Programmatically trick selection rectangle into thinking it's being "clicked and dragged"
        for (int i=0; i<numSteps; i+=1)
        {
            selectionRectController.AttemptMassSelection(junkSel, new Vector3(i*step, 0, 0), true, false);
            yield return new WaitForEndOfFrame();
        }

        // Check that rectangle is active in world
        Assert.AreEqual(GameObject.Find("SelectionRect").GetComponent<RectTransform>().gameObject.activeSelf, true);

        yield return null;
    }
}
