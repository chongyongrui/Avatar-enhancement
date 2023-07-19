using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using UnityEngine;
using System.Data.SqlClient;
using System.Configuration;

public class SQLConnection : MonoBehaviour
{
   void Start()
        {

        //DisplayWeapons();


        //   SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

        //builder.DataSource = "<your_server.database.windows.net>"; 
        //builder.UserID = "";            
        //builder.Password = "<your_password>";     
        //builder.InitialCatalog = "<your_database>";
        //builder.ConnectionString = "data source=DESKTOP-2P23NMB;initial catalog=AvatarProject;trusted_connection=true";
        /*
        System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
        builder["Data Source"] = "DESKTOP-2P23NMB";
        builder["integrated Security"] = true;
        builder["Initial Catalog"] = "AvatarProject";
        Console.WriteLine(builder.ConnectionString);
        */
        /*
        SqlConnection conn = new SqlConnection();
        conn.ConnectionString =
          "Data Source=DESKTOP-2P23NMB;" +
          "Initial Catalog=AvatarProject;" +
          "User id=DESKTOP-2P23NMB\\userAdmin;" +
          "Password=;";
        conn.Open();
        */
        //string connectionString = "data source=DESKTOP-2P23NMB;initial catalog=AvatarProject;Integrated Security=true";
         //SqlConnection con = new SqlConnection(builder.ConnectionString);
         SqlConnection conn = new SqlConnection("data source =.;initial catalog=master;integrated security=true;");
        conn.Open();
        string query = "SELECT * FROM  weaponInventoryList";
        SqlCommand sqlCommand = new SqlCommand(query, conn);
        SqlDataReader reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            string output = "Output playerID = " + reader.GetValue(0) + " and weaponID = " + reader.GetValue(1);
            Debug.Log(output);
        }



        }


    public void DisplayWeapons()
    {
       // "Data Source=DESKTOP-2P23NMB;Initial Catalog=AvatarProject;User ID=DESKTOP-2P23NMB\\userAdmin;Password=";
        string connectionString = "Data Source=DESKTOP-2P23NMB;Initial Catalog=AvatarProject;Integrated Security=true";
       //string connectionString = "Data Source=DESKTOP-2P23NMB;Initial Catalog=AvatarProject;User ID=DESKTOP-2P23NMB\\userAdmin;Password=";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {

            connection.Open();


            using (var command = connection.CreateCommand())
            {

                command.CommandText = "SELECT * FROM weaponInventoryList;";

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Debug.Log("Player ID: " + reader["playerID"] + " \tWeapon ID: " + reader["weaponID"] );
                    }
                    reader.Close();
                }
            }

            connection.Close();


        }
    }
}

