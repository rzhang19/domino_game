using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

namespace BH
{
    public enum DataManagerStatusCodes
    {
        SUCCESS,
        DATABASE_ERROR,
        USERNAME_TAKEN
    }
    
    public delegate void DataReturnStatusDelegate(Data data, DataManagerStatusCodes err);
    public delegate void ReturnStatusDelegate(DataManagerStatusCodes success);

    public class DataManager : Singleton<DataManager>
    {
        protected DataManager() { }

        void Start()
        {
            // ------ Example usage of DataManager ------- //
            string myUsername = "BobbyJoe2003";
            string myPassword = "meowmeow";
            Data myData = new Data();
            
            DataManager.Instance.RegisterUser(myUsername, myPassword, (err) => {
                if (err != DataManagerStatusCodes.SUCCESS)
                    Debug.LogError("Couldn't register user!");
            });
            
            DataManager.Instance.SaveData(myUsername, myPassword, myData, (err) => {
                if (err != DataManagerStatusCodes.SUCCESS)
                    Debug.LogError("Couldn't save data!");
            });
            
            DataManager.Instance.GetData(myUsername, myPassword, (data, err) => {
                if (err != DataManagerStatusCodes.SUCCESS)
                    Debug.LogError("Couldn't retrieve data!");
                else
                    Debug.Log("Retrieved data: " + data);
            });
            // ------------------------------------------- //
        }

        // Asynchronous function to retrieve data from the database.
        public void GetData(string username, string password, DataReturnStatusDelegate callback)
        {
            Debug.Log("GetData");

            // Get JSON representing save data from database.
            string jsonData = GetJSONDataThatsTotallyFromTheDatabase(username, password); // Getting data will probably be asynchronous, unlike this line of code.

            // Convert JSON into a data object.
            Data data = JsonUtility.FromJson<Data>(jsonData);

            // Call the callback function with the data.
            callback(data, DataManagerStatusCodes.SUCCESS);
        }

        // Asynchronous function to save data from the database.
        public void SaveData(string username, string password, Data data, ReturnStatusDelegate callback)
        {
            Debug.Log("SaveData");

            // Check if username/password entry exists in database.
            bool entryExists = EntryExistsInTheDatabase(username, password); // Database access will probably be asynchronous, unlike this line of code.

            // Save saveData to the database if the entry exists.
            string jsonData = JsonUtility.ToJson(data); // Convert data to JSON.
            SaveJSONDataToTheVeryRealDatabase(username, password, jsonData); // Database access will probably be asynchronous, unlike this line of code.

            // Call the callback function with the return status.
            callback(DataManagerStatusCodes.SUCCESS);
        }
        
        // Asynchronous function to register a username/password entry
        public void RegisterUser(string username, string password, ReturnStatusDelegate callback)
        {
            Debug.Log("RegisterUser");

            // Check if username exists in database.
            bool userExists = UsernameExistsInTheDatabase(username); // Database access will probably be asynchronous, unlike this line of code.

            // If user does not exist, create a new entry in the database.
            string jsonData = JsonUtility.ToJson(new Data()); // Create fresh save data.
            SaveJSONDataToTheVeryRealDatabase(username, password, jsonData); // Database access will probably be asynchronous, unlike this line of code.
            
            // Call the callback function with the return status.
            callback(DataManagerStatusCodes.SUCCESS);
        }




        // ---------------------------------- //
        // --------- FAKE FUNCTIONS --------- //
        // ---------------------------------- //

        // Delete this when there's an actual way to retrieve data!
        public string GetJSONDataThatsTotallyFromTheDatabase(string username, string password)
        {
            return JsonUtility.ToJson(new Data());
        }

        // Delete this when there's an actual way to check entries!
        public bool EntryExistsInTheDatabase(string username, string password)
        {
            return true;
        }

        // Delete this when there's an actual way to check usernames!
        public bool UsernameExistsInTheDatabase(string username)
        {
            return true;
        }
        
        // Delete this when there's an actual way to save data!
        public void SaveJSONDataToTheVeryRealDatabase(string username, string password, string data)
        {
            return;
        }
    }
}
