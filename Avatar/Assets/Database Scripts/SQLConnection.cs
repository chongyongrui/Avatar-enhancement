using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using UnityEngine;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Drawing;

public class SQLConnection : MonoBehaviour
{
   void Start()
        {
        var builder = new System.Data.SqlClient.SqlConnectionStringBuilder
        {
            DataSource = "localhost\\MSSQLSERVER01",
            InitialCatalog = "AvatarProject",
            PersistSecurityInfo = false,
            IntegratedSecurity = true

        };
        string connstring = "Server=localhost\\MSSQLSERVER01;Database=AvatarProject;Trusted_Connection=True;";
        SqlConnection con = new SqlConnection(connstring);
        con.Open();


        string query = "SELECT * FROM  weaponInventoryList";
        SqlCommand sqlCommand = new SqlCommand(query, con);
        SqlDataReader reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            string output = "Output playerID = " + reader.GetValue(0) + " and weaponID = " + reader.GetValue(1);
            Debug.Log(output);
        }
        con.Close();
        //ConnectToDatabase();
        //CloseConnection();
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
        /*

        SqlConnection conn = new SqlConnection("Server=localhost\\MSSQLSERVER01;Database=master;Trusted_Connection=True;");
       conn.Open();
       string query = "SELECT * FROM  weaponInventoryList";
       SqlCommand sqlCommand = new SqlCommand(query, conn);
       SqlDataReader reader = sqlCommand.ExecuteReader();
       while (reader.Read())
       {
           string output = "Output playerID = " + reader.GetValue(0) + " and weaponID = " + reader.GetValue(1);
           Debug.Log(output);
       }

       */

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

    private string connectionString = "Data Source=localhost\\MSSQLSERVER01;Database=AvatarProject;Trusted_Connection=True;";
    private SqlConnection connection;
    
    public void ConnectToDatabase()
    {
        connection = new SqlConnection(connectionString);
        try
        {
            connection.Open();
            Debug.Log("Connected to the database.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error connecting to the database: " + ex.Message);
        }
    }

    public void CloseConnection()
    {
        if (connection != null && connection.State != ConnectionState.Closed)
        {
            connection.Close();
            Debug.Log("Connection closed.");
        }
    }

}

