using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;
using System;
using System.Data;
using Mono.Data.Sqlite;

// NOTE: Database is in the Assets folder

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
        // Used for SQLite plugin (for database location)
        private string connectionString;
        
        protected DataManager() { }

        void Awake()
        {
            DontDestroyOnLoad(gameObject); // Want the singleton instance to persist between scenes.
        }

        void Start()
        {
            connectionString = "URI=file:" + Application.dataPath + "/DominoesDB.sqlite";
            
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

            DataManager.Instance.DeleteUser(myUsername);
            // ------------------------------------------- //
        }

        // The only public function with direct access to database
        public void DeleteUser(string id)
        {
            if (!IsUser(id))
            {
                Debug.Log("ERROR: Username not found!");
                return;
            }

            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    //DELETE FROM "save_states" WHERE user_id = id
                    string sqlQuery = "DELETE FROM \"save_states\" WHERE user_id = '" + id + "'";
                    Debug.Log(sqlQuery);
                    dbCmd.CommandText = sqlQuery;
                    dbCmd.ExecuteScalar();
                    dbConnection.Close();
                }
            }
        }

        // Asynchronous function to retrieve data from the database.
        public void GetData(string username, string password, DataReturnStatusDelegate callback)
        {
            Debug.Log("GetData");

            if(password != GetUserPassword(username))
            {
                Debug.Log("Entered password is incorrect!");
                return;
            }

            // Get JSON representing save data from database.
            string jsonData = GetSaveState(username, password); // Getting data will probably be asynchronous, unlike this line of code.

            // Convert JSON into a data object.
            Data data = JsonUtility.FromJson<Data>(jsonData);

            // Call the callback function with the data.
            callback(data, DataManagerStatusCodes.SUCCESS);
        }

        // Asynchronous function to save data from the database.
        // Also creates new user if one 
        public void SaveData(string username, string password, Data data, ReturnStatusDelegate callback)
        {
            Debug.Log("SaveData");

            // Check if username/password entry exists in database.
            if(!IsUser(username))
            {
                Debug.Log("Creating new user with this username");
            }
            else if(password != GetUserPassword(username))
            {
                Debug.Log("Entered password is incorrect!");
                return;
            }

            // Save saveData to the database if the entry exists.
            string jsonData = JsonUtility.ToJson(data); // Convert data to JSON.
            AddOrUpdateUser(username, password, jsonData); // Database access will probably be asynchronous, unlike this line of code.

            // Call the callback function with the return status.
            callback(DataManagerStatusCodes.SUCCESS);
        }
        

        // ---------------------------------- //
        // -------- SQLite FUNCTIONS -------- //
        // ---------------------------------- //
        // Private helper functions to send/retrieve info from database in Assets folder
        
    // Returns true if user with given username id exists, false otherwise
        private bool IsUser(string id)
        {
            //IDbConnection dbConnection;
            //dbConnection.Dispose();

            bool r = false;

            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string sqlQuery = "SELECT rowid, * FROM save_states";
                    dbCmd.CommandText = sqlQuery;

                    using (IDataReader reader = dbCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Debug.Log(reader.GetString(1));
                            if(reader.GetString(1) == id)
                            {
                                r = true;
                            }
                        }

                        dbConnection.Close();
                        reader.Close();
                    }
                }
            }

            return r;
        }


    // Returns password of user
        private string GetUserPassword(string id)
        {
            //IDbConnection dbConnection;
            //dbConnection.Dispose();

            string s = null;

            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string sqlQuery = "SELECT rowid, * FROM save_states";
                    dbCmd.CommandText = sqlQuery;

                    using (IDataReader reader = dbCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Debug.Log(reader.GetString(1));
                            if (reader.GetString(1) == id)
                            {
                                s = reader.GetString(2);
                            }
                        }

                        dbConnection.Close();
                        reader.Close();
                    }
                }
            }

            if (s == null)
            {
                Debug.Log("USER NOT FOUND");
            }
            return s;
        }

    // Returns save state of user
        private string GetSaveState(string id)
        {
            //IDbConnection dbConnection;
            //dbConnection.Dispose();

            string s = null;

            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string sqlQuery = "SELECT rowid, * FROM save_states";
                    dbCmd.CommandText = sqlQuery;

                    using (IDataReader reader = dbCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Debug.Log(reader.GetString(1));
                            if (reader.GetString(1) == id)
                            {
                                s = reader.GetString(3);
                            }
                        }

                        dbConnection.Close();
                        reader.Close();
                    }
                }
            }

            if (s == null)
            {
                Debug.Log("USER NOT FOUND");
            }
            return s;
        }

    // Adds new data to existing user or creates new one
        private void AddOrUpdateUser(string id, string pw, string save_state)
        {
            // TODO: Test if this function properly updates existing users
            if(IsUser(id)) && (pw = GetUserPassword(id))
            {
                Debug.Log("Updating existing user");
                using (IDbConnection dbConnection = new SqliteConnection(connectionString))
                {
                    dbConnection.Open();

                    using (IDbCommand dbCmd = dbConnection.CreateCommand())
                    {
                        string sqlQuery = "UPDATE \"save_states\" SET \"save_state\" = '" + save_state + "' WHERE \"user_id\" = '" + id + "'";
                        Debug.Log(sqlQuery);
                        dbCmd.CommandText = sqlQuery;
                        dbCmd.ExecuteScalar();
                        dbConnection.Close();
                    }
                }
                return;
            }
            else 
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                Debug.Log("Creating new user");
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    //INSERT INTO "save_states"("user_id", "user_pw", "save_state") VALUES('manas', 'kumar', 'SAVEME');
                    string sqlQuery = "INSERT INTO \"save_states\"(\"user_id\",\"user_pw\",\"save_state\") VALUES('" + id + "','" + pw + "','" + save_state + "')";
                    Debug.Log(sqlQuery);
                    dbCmd.CommandText = sqlQuery;
                    dbCmd.ExecuteScalar();
                    dbConnection.Close();
                }
            }
        }
    }
}