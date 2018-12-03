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
        USERNAME_TAKEN,
        USERNAME_NOT_FOUND,
        WRONG_CREDENTIALS
    }
    
    public delegate void DataReturnStatusDelegate(Data data, DataManagerStatusCodes err);
    public delegate void ReturnStatusDelegate(DataManagerStatusCodes success);

    public class DataManager : Singleton<DataManager>
    {
        protected DataManager() { }
        
        // Used for SQLite plugin (for database location)
        private string connectionString;

        string _currentUsername = "";
        string _currentPassword = "";

        void Awake()
        {
            DontDestroyOnLoad(gameObject); // Want the singleton instance to persist between scenes.
        }

        void Start()
        {
            connectionString = "URI=file:" + Application.dataPath + "/DominoesDB.sqlite";

            //// ------ Example usage of DataManager ------- //
            //string myUsername = "BobbyJoe2003";
            //string myPassword = "meowmeow";
            //Data myData = new Data();
            
            //DataManager.Instance.SaveData(myUsername, myPassword, (err) => {
            //    if (err != DataManagerStatusCodes.SUCCESS)
            //        Debug.LogError("Couldn't register user!");
            //});
            
            //DataManager.Instance.SaveData(myUsername, myPassword, myData, (err) => {
            //    if (err != DataManagerStatusCodes.SUCCESS)
            //        Debug.LogError("Couldn't save data!");
            //});
            
            //DataManager.Instance.GetData(myUsername, myPassword, (data, err) => {
            //    if (err != DataManagerStatusCodes.SUCCESS)
            //        Debug.LogError("Couldn't retrieve data!");
            //    else
            //        Debug.Log("Retrieved data: " + data);
            //});

            //DataManager.Instance.DeleteUser(myUsername);
            //// ------------------------------------------- //
        }
        
        /// <summary>
        /// Deletes the user with given id.
        /// </summary>
        /// <param name="id">The identifier of the user.</param>
        /// <param name="callback">The callback that takes the data as an argument.</param>
        public void DeleteUser(string id, ReturnStatusDelegate callback)
        {
            if (!IsUser(id))
            {
                callback(DataManagerStatusCodes.USERNAME_NOT_FOUND);
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
        
        /// <summary>Asynchronous function to retrieve data from the database.</summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="callback">The callback that takes the data as an argument.</param>
        public void GetData(string username, string password, DataReturnStatusDelegate callback)
        {
            // Check credentials.
            if (!IsUser(username) || password != GetUserPassword(username))
            {
                Debug.Log("Entered password is incorrect!");
                callback(null, DataManagerStatusCodes.WRONG_CREDENTIALS);
                return;
            }
            
            string jsonData = GetSaveState(username);
            Data data = JsonUtility.FromJson<Data>(jsonData);

            if (data != null)
            {
                _currentUsername = username;
                _currentPassword = password;
                callback(data, DataManagerStatusCodes.SUCCESS);
            }
            else
            {
                // Corrupted data. Rewrite with new data. (might not be the best thing to do here, but w/e)
                Data freshData = new Data();
                string freshJsonData = JsonUtility.ToJson(freshData);
                UpdateUser(username, password, freshJsonData);
                callback(data, DataManagerStatusCodes.DATABASE_ERROR);
            }
        }
        
        /// <summary>Wrapper for GetData that uses the currently loaded user.</summary>
        /// <param name="callback">The callback that takes the data as an argument.</param>
        public void GetData(DataReturnStatusDelegate callback)
        {
            GetData(_currentUsername, _currentPassword, callback);
        }
        
        /// <summary>Asynchronous function to save data to the database.</summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="data">The data to be saved.</param>
        /// <param name="callback">The callback that takes an error code as an argument.</param>
        public void SaveData(string username, string password, Data data, ReturnStatusDelegate callback)
        {
            // Check if username/password entry exists in database.
            if (!IsUser(username) || password != GetUserPassword(username))
            {
                callback(DataManagerStatusCodes.WRONG_CREDENTIALS);
                return;
            }

            // Save saveData to the database if the entry exists.
            string jsonData = JsonUtility.ToJson(data);
            UpdateUser(username, password, jsonData);
            callback(DataManagerStatusCodes.SUCCESS);
        }

        /// <summary>Wrapper for GetData that uses the currently loaded user.</summary>
        /// <param name="callback">The callback that takes an error code as an argument.</param>
        public void SaveData(Data data, ReturnStatusDelegate callback)
        {
            SaveData(_currentUsername, _currentPassword, data, callback);
        }

        /// <summary>Asynchronous function to register a user in the database.</summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="callback">The callback that takes an error code as an argument.</param>
        public void RegisterUser(string username, string password, ReturnStatusDelegate callback)
        {
            // Make a user with fresh data.
            Data freshData = new Data();
            string jsonData = JsonUtility.ToJson(freshData);
            if (AddUser(username, password, jsonData))
            {
                callback(DataManagerStatusCodes.SUCCESS);
            }
            else
            {
                callback(DataManagerStatusCodes.USERNAME_TAKEN);
            }
        }

        // ---------------------------------- //
        // -------- SQLite FUNCTIONS -------- //
        // ---------------------------------- //
        // Private helper functions to send/retrieve info from database in Assets folder
        
        // Returns true if user with given username id exists, false otherwise
        bool IsUser(string userId)
        {
            bool ret = false;

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
                            if(reader.GetString(1) == userId)
                                ret = true;
                        }

                        dbConnection.Close();
                        reader.Close();
                    }
                }
            }

            return ret;
        }


        // Returns password of user.
        // Returns null if no userId exists.
        string GetUserPassword(string userId)
        {
            string ret = null;

            // Make a new connection.
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    // Retrieve every (rowid, row) tuple possible.
                    string sqlQuery = "SELECT rowid, * FROM save_states";
                    dbCmd.CommandText = sqlQuery;

                    // Scan every tuple looking for the matching rowid.
                    using (IDataReader reader = dbCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetString(1) == userId)
                            {
                                ret = reader.GetString(2);
                                // break; ???
                            }
                        }

                        dbConnection.Close();
                        reader.Close();
                    }
                }
            }
            
            return ret;
        }
        
        // Returns save state of user
        string GetSaveState(string id)
        {
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
        bool UpdateUser(string id, string pw, string save_state)
        {
            // TODO: Test if this function properly updates existing users
            if (IsUser(id) && pw == GetUserPassword(id))
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

                return true;
            }
            else
            {
                return false; // Incorrect credentials.
            }
        }

        bool AddUser(string id, string pw, string save_state)
        {
            // Not a new user.
            if (IsUser(id))
                return false; // Username taken.

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

            return true;
        }
    }
}
