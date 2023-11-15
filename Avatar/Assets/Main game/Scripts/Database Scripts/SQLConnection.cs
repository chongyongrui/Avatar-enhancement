using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using System.Net;
using Npgsql;
using UnityEngine.UI;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEngine.Rendering.PostProcessing;


public class SQLConnection : MonoBehaviour
{

    public bool SQLServerConnected = false;
    public Item[] startingItems;
    public static SQLConnection instance;
    private int playerID = 0;
    [SerializeField] private Item Ak47Item;
    [SerializeField] private Item dynamiteItem;
    [SerializeField] private Item M4Item;
    [SerializeField] private Item SMGItem;
    [SerializeField] private Item smokeGrenadeItem;
    [SerializeField] private Item grenadeItem;
    [SerializeField] private Item backpackItem;
    [SerializeField] Sprite NewtorkStatusIcon;
    bool initialConfigSuccess = false;
    public string adminConString;
    public string userConString;
    public string IPAddress;
    void Start()
    {
        //get IP address of computer
        try{
            if (LoginController.instance.IPAddress != null)
        {
            IPAddress = userdatapersist.Instance.IPAdd;
        }
        else
        {
            string hostName = Dns.GetHostName();
            IPAddress = Dns.GetHostEntry(hostName).AddressList[1].ToString();
        }
        }catch (Exception e ){
            Debug.Log(e.ToString());
        }
        
       
        

    }

    private void Update()
    {
        if (!initialConfigSuccess)
        {
            string adminConString = "Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;";
            
            try
            {
                NpgsqlConnection con = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;");
                //LoginController.instance.CreateNewDB();
                con.Open();
                Debug.Log("SQL server initial connection successful!");
                
                LoginController.instance.CreateTables();

                
                
                ConfigureUserConnectionString(userdatapersist.Instance.verifiedUser, userdatapersist.Instance.verifiedPassword);
               

                con.Close();


                CreateWalletNewUserAccount(userdatapersist.Instance.verifiedUser, userdatapersist.Instance.verifiedPassword);
                NpgsqlConnection con2 = new NpgsqlConnection("Server=" + IPAddress + ";Port=5432;User Id=" + userdatapersist.Instance.verifiedUser +"; Password=" + userdatapersist.Instance.verifiedPassword +" ; Database=" + userdatapersist.Instance.verifiedUser + "wallet; ");
                //LoginController.instance.CreateNewDB();
                con.Open();
                Debug.Log("SQL wallet initial connection successful!");
                SQLServerConnected = true;
                



                ConfigureUserConnectionString(userdatapersist.Instance.verifiedUser, userdatapersist.Instance.verifiedPassword);


                con.Close();

            }
            catch (Exception e)
            {
                Debug.Log("ERROR SQL server connection unsuccessful!   " + e );
                SQLServerConnected = false;
            }

            initialConfigSuccess = true;

        }
    }

    public void ConfigureUserConnectionString(string username, string password)
    {
        string userConString = "Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;";
        Debug.Log("user con string is : " + userConString);
    }


    public void CreateWalletNewUserAccount(string username, string password)
    {

        Debug.Log("Creating wallet account now... ");
        string connstring = "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=postgres;";
        //string connstring = "Data Source=192.168.56.1;Initial Catalog=AvatarProject;User ID=user;Password=user;";
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connstring))
            {

                connection.Open();

                using (var command = connection.CreateCommand())
                {


                    command.CommandText = "CREATE DATABASE " + username + "wallet;";

                    command.ExecuteNonQuery();
                }
                using (var command = connection.CreateCommand())
                {

                    command.CommandText = "DO $do$ BEGIN IF EXISTS( SELECT FROM pg_catalog.pg_roles WHERE  rolname = '" + username + "') " +
                        "THEN RAISE NOTICE 'Role \"" + username + "\" already exists. Skipping.'; ELSE CREATE ROLE " + username + " LOGIN PASSWORD '" + password + "';" +
                        " GRANT ALL PRIVILEGES ON DATABASE " + username + "wallet TO " + username + "; END IF; END $do$; ";

                    command.ExecuteNonQuery();
                }

                connection.Close();


            }

        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL server) Error creating new wallet account:  " + e);
        }
        CreateWalletTables();
    }

    public void CreateWalletTables()
    {
        string connstring = "Server = localhost; Port = 5432; User Id = postgres; Password = password; Database = " + userdatapersist.Instance.verifiedUser + "wallet; ";
        try
        {
            //create the db connection
            using (NpgsqlConnection connection = new NpgsqlConnection(connstring))
            {

                connection.Open();
                //set up objeect called command to allow db control
                using (var command = connection.CreateCommand())
                {

                    //sql statements to execute
                    command.CommandText = "CREATE TABLE IF NOT EXISTS AES_Keys (receiver_hash VARCHAR(20) UNIQUE NOT NULL, key_val varchar(500));";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS DH_Private_Keys (receiver_hash VARCHAR(20) UNIQUE NOT NULL, key_val varchar(500));";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS other_keys ( key_type varchar(20) UNIQUE NOT NULL,  key_val varchar(500));";
                    command.ExecuteNonQuery();
                    command.CommandText = "GRANT ALL ON AES_Keys TO " + userdatapersist.Instance.verifiedUser + ";";
                    command.ExecuteNonQuery();
                    command.CommandText = "GRANT ALL ON Other_Keys TO " + userdatapersist.Instance.verifiedUser + ";";
                    command.ExecuteNonQuery();
                    command.CommandText = "GRANT ALL ON DH_Private_Keys TO " + userdatapersist.Instance.verifiedUser + ";";
                    command.ExecuteNonQuery();

                }

                connection.Close();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL Server) Error creating new wallet database" + e);
        }

    }





    public void TestConnection()
    {
        string userConString = "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=postgres;";
        NpgsqlConnection con = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;");
        
            Debug.Log("Testing SQL Server connection");
            try
            {
                con.Open();
                SQLServerConnected = true;
                con.Close();
            }
            catch (Exception e)
            {
                Debug.Log("Failed to connect to SQL server");
            }
            

        
    }

    public void DisplayWeapons()
    {
        //string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        //string connstring = "Data Source=10.255.253.29;Initial Catalog=AvatarProject;User ID=SuperAdmin;Password=SuperAdmin;";
        //string connstring = "Data Source=192.168.56.1;Initial Catalog=AvatarProject;User ID=user;Password=user;";
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;"))
            {

                connection.Open();
                SQLServerConnected = true;

                using (var command = connection.CreateCommand())
                {

                    command.CommandText = "SELECT * FROM weapons;";

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Debug.Log("Player ID: " + reader["playerID"] + " \tWeapon ID: " + reader["weaponID"]);
                        }
                        reader.Close();
                    }
                }

                connection.Close();


            }
        }catch (Exception e)
        {
            Debug.Log("(SQL server) Error displaying weapons from SQL server");
            SQLServerConnected = false;
        }
        
    }


    private void Awake()
    {
        instance = this;
    }

   
   

    
    

    public void AddWeapon(int playerID, int weaponid, int quantity)
    {
        //string connstring = "Data Source=10.255.253.29;Initial Catalog=master;User ID=SuperAdmin;Password=SuperAdmin;"; 
        //string connstring = "Data Source=192.168.56.1;Initial Catalog=AvatarProject;User ID=user;Password=user;";
        //string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";

        try
        {


            using (NpgsqlConnection connection = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;"))
            {
                connection.Open();
                SQLServerConnected = true;
                using (var command = connection.CreateCommand())
                {

                    //correct format is " INSERT INTO weapons (playerid,weponid,quantity) VALUES (playerid, weaponid, quantity); "
                    command.CommandText = "INSERT INTO weapons (playerid,weaponid,quantity) VALUES (" + playerID + "," + weaponid + "," + quantity + ");";
                    command.ExecuteNonQuery();
                    Debug.Log("(SQL server) Weapon added with id: " + weaponid + " to player with ID = " + playerID);
                }

                connection.Close();
            }
        } catch (Exception e)
        {
            Debug.Log("(SQL Server) Error adding weapon into DB " + e);
            SQLServerConnected = false;
        }

    }

    public void RemoveWeapon(int playerID, int weaponid, int quantity)
    {
        //string connstring = "Data Source=10.255.253.29;Initial Catalog=AvatarProject;User ID=SuperAdmin;Password=SuperAdmin;"; 
        //string connstring = "Data Source=192.168.56.1;Initial Catalog=AvatarProject;User ID=user;Password=user;";
        //string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";

        try
        {


            using (NpgsqlConnection connection = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;"))
            {
                connection.Open();
                SQLServerConnected = true;
                using (var command = connection.CreateCommand())
                {

                    //correct format is " DELETE FROM weapons WHERE playerid = player ANS weaponid = id; "
                    command.CommandText = "DELETE FROM weapons WHERE playerid = " + playerID + " AND weaponid = " + weaponid + " ;";
                    command.ExecuteNonQuery();

                }

                connection.Close();
            }
            Debug.Log("(SQL server) Weapon added with id: " + weaponid);
        } catch (Exception e)
        {
            Debug.Log("(SQL Server) Error removing weapon from DB " + e);
            SQLServerConnected = false;
        }
    }

    



    public void UpdatePlayerLocation(int playerID, int x, int y, int z)
    {
        //string connstring = "Data Source=10.255.253.29;Initial Catalog=AvatarProject;User ID=SuperAdmin;Password=SuperAdmin;"; 
        //string connstring = "Data Source=192.168.56.1;Initial Catalog=AvatarProject;User ID=user;Password=user;"; 
        //string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        bool dataFound = false;

        try
        {


            using (NpgsqlConnection connection = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;"))
            {

                connection.Open();

                SQLServerConnected = true;
                using (var command = connection.CreateCommand())
                {

                    command.CommandText = "SELECT * FROM playerlocation WHERE playerid = " + playerID + ";";

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["playerID"] == null)
                            {
                                Debug.Log("(SQL server) no prior location data found");
                                dataFound = false;

                            }
                            else
                            {
                                dataFound = true;
                            }
                        }
                        reader.Close();
                    }

                    if (dataFound)
                    {

                        //update the players location while ensuring playerid is correct
                        //  UPDATE userdata SET x = x, SET y = y, SET z = z WHERE playerid = playerid;
                        command.CommandText = "UPDATE playerlocation SET x = " + x + ", y = " + y + ", Z = " + z + " WHERE playerid = " + playerID + ";";
                        command.ExecuteNonQuery();
                        //Debug.Log("player location is now : x=" + x + " y= " + y + " z= " + z);


                    }
                    else if (!dataFound)
                    {
                        command.CommandText = "INSERT INTO playerlocation (playerid,x,y,z) VALUES (" + playerID + "," + x + "," + y + "," + z + "); " +
                            "INSERT INTO weapons(playerid, weaponid, quantity) VALUES( " + playerID + ",7,1);";
                            
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();


            }
        } catch (Exception e)
        {
            Debug.Log("(SQL Server) Error updating player location " + e);
            SQLServerConnected = false;
        }
    }

    public Item[] GetStartingItems(int playerID)
    {
        //string connstring = "Data Source=10.255.253.29;Initial Catalog=AvatarProject;User ID=SuperAdmin;Password=SuperAdmin;";
        //string connstring = "Data Source=192.168.56.1;Initial Catalog=AvatarProject;User ID=user;Password=user;"; 
        //string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        List<Item> items = new List<Item>();
        try
        {


            using (NpgsqlConnection connection = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;"))
            {

                connection.Open();
                SQLServerConnected = true;
                

                using (var command = connection.CreateCommand())
                {

                    command.CommandText = "SELECT * FROM weapons WHERE playerid =" + playerID + ";";

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Debug.Log("(SQL server) Item found with id " + reader["weaponid"]);
                            Item newItem = HashToItem((int)reader["weaponid"]);
                            Debug.Log("(SQL server) Item found with name " + newItem.name);
                            items.Add(newItem);
                        }
                        reader.Close();
                    }
                }

                connection.Close();


            }
        } catch (Exception e)
        {
            Debug.Log("(SQL Server) Error getting starting items " + e);
            SQLServerConnected = false;
        }

        Item[] startingItems = items.ToArray();
        return startingItems;
    }

    public int[] getStartingLocation(int playerID)
    {
        //string connstring = "Data Source=10.255.253.29;Initial Catalog=AvatarProject;User ID=SuperAdmin;Password=SuperAdmin;"; 
        //string connstring = "Data Source=192.168.56.1;Initial Catalog=AvatarProject;User ID=user;Password=user;"; 
        //string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        int[] startingCoordinates = new int[3];
        try
        {


            using (NpgsqlConnection connection = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;"))
            {

                connection.Open();
                SQLServerConnected = true;

                using (var command = connection.CreateCommand())
                {

                    command.CommandText = "SELECT * FROM playerlocation WHERE playerid =" + playerID + ";";

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            startingCoordinates[0] = (int)reader["x"];
                            startingCoordinates[1] = (int)reader["y"];
                            startingCoordinates[2] = (int)reader["z"];

                        }
                        reader.Close();
                    }
                }

                connection.Close();


            }
        }
        catch (Exception e)
        {
            Debug.Log("(SQL Server) Error getting starting player location " + e);
            SQLServerConnected = false;
        }


        return startingCoordinates;
    }

    public Item HashToItem(int weaponid)
    {
        Item newItem = new Item();
        switch (weaponid)
        {
            case 1:
                newItem = Ak47Item;
                break;
            case 2:
                newItem = dynamiteItem;
                break;
            case 3:
                newItem = M4Item;
                break;
            case 4:
                newItem = SMGItem;
                break;
            case 5:
                newItem = smokeGrenadeItem;
                break;
            case 6:
                newItem = grenadeItem;
                break;
            case 7:
                newItem = backpackItem;
                break;
        }
        return newItem;
    }

    



}

