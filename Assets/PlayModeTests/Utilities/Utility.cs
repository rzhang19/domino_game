using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BH;

/// Useful functions for all tests.
/// Only call after SceneManager.LoadScene() is called!
public class Utility : MonoBehaviour {
    
    /// Programmatically adds a domino to SelectableManager and returns it.
    public static Selectable ProgrammaticallyAddDomino()
    {
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        System.Collections.Generic.List<Selectable> oldDominos 
            = new System.Collections.Generic.List<Selectable>(dominoManager.GetActiveSelectables());
        dominoManager.SpawnSelectable();
        System.Collections.Generic.List<Selectable> newDominos = dominoManager.GetActiveSelectables();
        return newDominos.Except(oldDominos).ToList()[0];
    }

    /// Programatically selects the given domino.
    /// Selection is delegated to BuildModeController.
    public static void ProgrammaticallySelectDomino(BH.Selectable domino)
    {   
        BuildModeController buildController = GameObject.Find("BuildModeController").GetComponent<BuildModeController>();
        buildController.Select(domino);
    }

    /// Randomly changes the position, rotation, and local scale of the inputted transform in-place.
    public static void RandTransformChange(Transform baseT)
    {
        baseT.position = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        baseT.Rotate(new Vector3(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)));
        baseT.localScale += new Vector3(Random.Range(-0.5f,0.5f), Random.Range(-0.5f,0.5f), Random.Range(-0.5f,0.5f));
    }

    /// Returns a Color with normalized RGB values of the input Color's.
    /// Normalized from [0, 255] to [0, 1]
    public static Color NormalizedColor(Color orig)
    {
        return new Color(orig.r/255.0f, orig.g/255.0f, orig.b/255.0f);
    }
}
