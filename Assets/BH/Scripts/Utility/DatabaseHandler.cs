using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class DatabaseHandler : MonoBehaviour
{
    private string connectionString;


// Use this for initialization
    void Start()
    {
        connectionString = "URI=file:" + Application.dataPath + "/DominoesDB.sqlite";

        AddUser("manas", "kumar", "SAVE1");
        AddUser("manas", "kumar", "SAVE1");
        AddUser("banas", "kumar", "SAVE2");
        AddUser("lanas", "kumar", "SAVE3");
        Debug.Log(IsUser("manas"));
        Debug.Log(IsUser("banas"));
        Debug.Log(GetUserPassword("manas"));
        Debug.Log(GetUserPassword("banas"));
        Debug.Log(GetSaveState("lanas"));
        DeleteUser("manas");
        DeleteUser("banas");
        DeleteUser("lanas");
        DeleteUser("lanas");
    }


// Update is called once per frame
    void Update(){ }


// Checks if username exists in database
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


    private void AddUser(string id, string pw, string save_state)
    {
        if(IsUser(id))
        {
            Debug.Log("ERROR: Username is already taken!");
            return;
        }

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
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


    private void DeleteUser(string id)
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


}
