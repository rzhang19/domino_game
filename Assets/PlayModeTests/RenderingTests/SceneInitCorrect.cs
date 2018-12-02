using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using BH;

public class SceneInitCorrect 
{
    // Check that all important components exist and are properly initialized in the scene.
    [UnityTest]
    public IEnumerator _New_Scene_Properly_Initializes()
    {   
        SceneManager.LoadScene("SpectatorMode");
        yield return new WaitForFixedUpdate();

        Scene scene = SceneManager.GetActiveScene();
        Assert.AreEqual(scene.name, "SpectatorMode");

        GameObject interactSurface1Obj = GameObject.Find("InteractSurface1");
        Assert.That(interactSurface1Obj, Is.Not.Null);

        GameObject interactSurface2Obj = GameObject.Find("InteractSurface2");
        Assert.That(interactSurface2Obj, Is.Not.Null);

        GameObject dominoManagerObj = GameObject.Find("SelectableManager");
        Assert.That(dominoManagerObj, Is.Not.Null);
        SelectableManager dominoManager = dominoManagerObj.GetComponent<SelectableManager>();
        Assert.AreEqual(dominoManager.GetActiveSelectables().Count, 0);
        
        GameObject controllerManagerObj = GameObject.Find("ControllerManager");
        Assert.That(controllerManagerObj, Is.Not.Null);
        
        GameObject buildControllerObj = GameObject.Find("BuildModeController");
        Assert.That(buildControllerObj, Is.Not.Null);

        GameObject spectatorControllerObj = GameObject.Find("SpectatorModeController");
        Assert.That(spectatorControllerObj, Is.Not.Null);

        GameObject freeflyCanvasObj = GameObject.Find("FreeFlyCanvas");
        Assert.That(freeflyCanvasObj, Is.Not.Null);
        Assert.That(freeflyCanvasObj.GetComponent<Canvas>().isActiveAndEnabled, Is.False);

        GameObject buildCanvasObj = GameObject.Find("BuildModeCanvas");
        Assert.That(buildCanvasObj, Is.Not.Null);
        Assert.That(buildCanvasObj.GetComponent<Canvas>().isActiveAndEnabled, Is.True);
        
        GameObject spectatorCanvasObj = GameObject.Find("SpectatorModeCanvas");
        Assert.That(spectatorCanvasObj, Is.Not.Null);
        Assert.That(spectatorCanvasObj.GetComponent<Canvas>().isActiveAndEnabled, Is.False);
    }
}