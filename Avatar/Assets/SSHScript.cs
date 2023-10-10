using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Renci.SshNet;
using System.Threading;
using System.Threading.Tasks;
using System;


public class SSHScript : MonoBehaviour
{
    /*
    private NpgsqlConnection connection;

    private void Test()
    {
        // Set your PostgreSQL connection string
        string connString = "Host=localhost;Port=5432;Username=postgres;Password=password;Database=postgres";

        try
        {
            // Create a connection to the PostgreSQL database
            connection = new NpgsqlConnection(connString);

            // Open the database connection
            connection.Open();

            // Check if the connection is open
            if (connection.State == System.Data.ConnectionState.Open)
            {
                Debug.Log("Connected to PostgreSQL database!");
            }
            else
            {
                Debug.LogError("Failed to connect to PostgreSQL database.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error: " + ex.Message);
        }
    }
    */
}
