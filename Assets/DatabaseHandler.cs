//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
////using System.data;
//using Mono.Data.Sqlite;

//public class DatabaseHandler : MonoBehaviour {

//    private string connectionString;

//	// Use this for initialization
//	void Start () {
//        connectionString = "URI=file:" + ApplicationException.dataPath + "/DominoesDB.sqlite";
//        GetSave();
//	}
	
//	// Update is called once per frame
//	void Update () {
		
//	}

//    private void GetSave()
//    {
//        //IDbConnection dbConnection;
//        //dbConnection.Dispose();
//        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
//        {
//            dbConnection.Open();

//            using (IDbCommand dbCmd = dbConnection.CreateCommand())
//            {
//                string sqlQuery = "SELECT rowid, * FROM save_states";
//                dbCmd.CommandText = sqlQuery;

//                using (IDataReader reader = dbCmd.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        Debug.Log(reader.getString(4));
//                    }

//                    dbConnection.Close();
//                    reader.Close();
//                }
//            }
//        }
//    }

//    private void AddUser(string id, string pw, string save_state)
//    {
//        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
//        {
//            dbConnection.Open();

//            using (IDbCommand dbCmd = dbConnection.CreateCommand())
//            {
//                string sqlQuery = "INSERT INTO save_states(user_id,user_pw,save_state) VALUES('" + id + "','" + pw + "','" + save_state + "')";
//                dbCmd.CommandText = sqlQuery;
//                dbCmd.ExecuteScalar();
//                dbConnection.Close();
//            }
//        }
//    }
//}
