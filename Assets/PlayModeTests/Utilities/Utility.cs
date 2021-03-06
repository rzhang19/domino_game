﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using NUnit.Framework;
using BH;
using TWM.UI;

/// Useful functions for all tests.
/// Only call after SceneManager.LoadScene() is called!
public class Utility : MonoBehaviour {
    
    /// Programmatically adds a domino to SelectableManager and returns it.
    public static BH.Selectable ProgrammaticallyAddDomino()
    {
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        System.Collections.Generic.List<BH.Selectable> oldDominos 
            = new System.Collections.Generic.List<BH.Selectable>(dominoManager.GetActiveSelectables());
        dominoManager.SpawnSelectable();
        System.Collections.Generic.List<BH.Selectable> newDominos = dominoManager.GetActiveSelectables();
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

    // Disable simulated UI
    public static void StopInputSimulations()
    {
        InputManager.DisableCursorSimulation();
        InputManager.DisableKeypressSimulation();
        InputManager.DisableScrollSimulation();
        InputManager.DisableSimulatePointerOverGameObject();
    }

    /// Simulates clicking a specified button in the UI.
    /// Only call after SceneManager.LoadScene() is called!
    public static void ClickUIButton(string buttonName)
    {
        GameObject buttonObj = GameObject.Find(buttonName);
        UIButton button = buttonObj.GetComponent<UIButton>();
        if (buttonName == "ButtonSpawn" || buttonName == "ButtonRandomColor")
        {
            button.OnToggleOnInvoke();
        }
        else
        {
            button.OnButtonUpInvoke();
        }
        //button.onClick.Invoke();
    }

    /// Simulates pressing down the specified key. See InputManager for valid strings/names of keys.
    /// Forces a delay until the end of the frame before continuing, so keypress can be processed properly
    public static IEnumerator SimulateKeyDown(string key)
    {
        InputManager.SimulateKeyDown(key);
        yield return new WaitForEndOfFrame();
    }

    /// Simulates releasing the specified key. See InputManager for valid strings/names of keys.
    /// Forces a delay until the end of the frame before continuing, so keypress can be processed properly
    public static IEnumerator SimulateKeyUp(string key)
    {
        InputManager.SimulateKeyUp(key);
        yield return new WaitForEndOfFrame();
    }

    /// Programmatically creates an account with the given username and password.
    /// Only involves DataManager class; as low level as you can get.
    public static IEnumerator Register(string username, string password)
    {
        DataManager.Instance.RegisterUser(username, password, (err) =>
        {
            switch (err)
            {
                case DataManagerStatusCodes.SUCCESS:
                    Debug.Log("Successfully registered new user " + username + "!");
                    break;
                case DataManagerStatusCodes.DATABASE_ERROR:
                    Debug.LogError("Database error occured!");
                    break;
                case DataManagerStatusCodes.USERNAME_TAKEN:
                    Debug.LogError("Username already taken!");
                    break;
                default:
                    Debug.LogError("Unknown error occured!");
                    break;
            }
        });
        yield return new WaitForEndOfFrame();
    }

    // Remove user silently, mainly to reinitialize DB
    public static IEnumerator RemoveUser(string username) {
        DataManager.Instance.DeleteUser(username, (err) =>
            {
                switch (err)
                {
                    case DataManagerStatusCodes.USERNAME_NOT_FOUND:
                        Debug.Log("No user found. But thats ok");
                        break;
                    default:
                        Debug.Log("Deletion success");
                        break;
                }
            }
        );

        // Give DB some time
        yield return new WaitForEndOfFrame();
    }
}
