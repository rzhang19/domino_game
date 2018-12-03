using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BH;

/// Test that DataManager class correctly has database interactions.
[TestFixture]
public class DataManagerInteractions {
    string _username = "test", _password = "123";

    // Load scene to spawn DataManager singleton instance.
    // Would register user here, but need to allow yield return type to give DB time, but Init() won't allow this.
    [SetUp]
    public void Init()
    { 
        SceneManager.LoadScene("SpectatorMode");
    }

    /// Called after every test. Clears DB
    [TearDown] 
    public void Cleanup() {
        RemoveUser(_username);
    }

    /// Test that we can update a user's domino layout data for a correct username-pw combo
    [UnityTest]
    public IEnumerator Saves_Authenticated_Users_Data_Persistently() {
        yield return new WaitForFixedUpdate();
        
        // Attempt user creation. Throw error if this fails
        yield return AttemptUserCreation(_username, _password);

        BH.Data _data = CreateNewData();

        // Check that update with new data reports a success. Throw error if fails
        DataManager.Instance.SaveData(_username, _password, _data, (err) => {
            if (err != DataManagerStatusCodes.SUCCESS)
                Debug.LogError("Couldn't save data!");
            else
                Debug.Log("Saved data! N I C E");
        });
        yield return new WaitForEndOfFrame();

        // Retrieve data to make sure it was updated
        BH.Data newData = null;
        DataManager.Instance.GetData(_username, _password, (data, err) =>
            {
                switch (err)
                {
                    case DataManagerStatusCodes.SUCCESS:
                        newData = data;
                        break;
                    default:
                        newData = null;
                        break;
                }
             });
        yield return new WaitForEndOfFrame();
        Assert.AreEqual(newData.ToString(), _data.ToString());

        yield return null;
    }
    
    /// Test that we do not update a user's domino layout data for an incorrect username-pw combo
    [UnityTest]
    public IEnumerator Reject_Unauthenticated_Users_Data_Persistently() {
        yield return new WaitForFixedUpdate();

        // Attempt user creation. Throw error if this fails
        yield return AttemptUserCreation(_username, _password);

        BH.Data _data = CreateNewData();

        // Update with new data using bad username-pw combo & make sure the update is rejected
        bool dataSaved = true; //should change
        DataManager.Instance.SaveData(_username, _password+"makesPasswordWrong", _data, (err) => {
            if (err != DataManagerStatusCodes.SUCCESS)
                dataSaved = false;
            else
                dataSaved = true;
        });
        yield return new WaitForEndOfFrame();
        Assert.AreEqual(dataSaved, false);
        

        // Retrieve data to make sure it wasn't updated to _data
        BH.Data newData = null;
        DataManager.Instance.GetData(_username, _password, (data, err) =>
            {
                switch (err)
                {
                    case DataManagerStatusCodes.SUCCESS:
                        newData = data;
                        break;
                    default:
                        newData = null;
                        break;
                }
             });
        yield return new WaitForEndOfFrame();
        Assert.AreNotEqual(newData, _data);
    }

    // Create new user, replacing old if any. Throw error if this fails
    IEnumerator AttemptUserCreation(string username, string password) {
        // Get rid of old, if any
        yield return RemoveUser(_username);

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
            }
        );

        // Give DB some time
        yield return new WaitForEndOfFrame();
    }

    // Remove user
    IEnumerator RemoveUser(string username) {
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

    BH.Data CreateNewData() {
        // Create new data
        SelectableManager dominoManager = GameObject.Find("SelectableManager").GetComponent<SelectableManager>();
        BH.Selectable newSelectable = dominoManager.SpawnSelectable();
        return new Data(new BH.Selectable[]{ newSelectable });
    }
}
