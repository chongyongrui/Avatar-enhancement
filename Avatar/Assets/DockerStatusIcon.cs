using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Data.SqlClient;
using Npgsql;




public class DockerStatusIcon : MonoBehaviour
{
    public bool SQLServerConnection = false;
    public static DockerStatusIcon instance;
    [SerializeField] private TMP_InputField IPAddressInputField;


    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        TestSQLConnection();
        InvokeRepeating("TestSQLConnection", 0.3f, 3.0f);
    }
    // Update is called once per frame
    void Update()
    {
        Image image = this.GetComponent<Image>();
        if (SQLServerConnection)
        {
            image.color = Color.green;
            
        }
        else
        {
            image.color = Color.red;
            

        }
    }

    public void TestSQLConnection()
    {
        if (!SQLServerConnection)
        {
            string hostName = Dns.GetHostName();
            string IPAddress = IPAddressInputField.text;
            if (IPAddress != "")
            {
                //string adminConString = "Data Source=" + IPAddress + ";Initial Catalog=master;User ID=sa;Password=D5taCard;";
                string adminConString = "Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;";
                Npgsql.NpgsqlConnection con = new Npgsql.NpgsqlConnection(adminConString);
                try
                {
                    con.Open();
                    Debug.Log("SQL server connection successful!");
                    con.Close();
                    SQLServerConnection = true;
                }
                catch (Exception e)
                {
                    Debug.Log("ERROR SQL server connection unsuccessful!");
                    SQLServerConnection = false;
                }

            }

        }

    }
}
 