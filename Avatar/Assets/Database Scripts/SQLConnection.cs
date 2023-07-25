using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using UnityEngine;

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
    void Start()
    {
        //create the table
        


        string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        SqlConnection con = new SqlConnection(connstring);
        try {
            con.Open();
            Debug.Log("SQL server connection successful!");
            SQLServerConnected = true;
            CreateDB(connstring);

            DisplayWeapons();

            con.Close();

        }
        catch(Exception e) {
            Debug.Log("ERROR SQL server connection unsuccessful!");
        }
        
        
    }

    public void DisplayWeapons()
    {
        string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        using (SqlConnection connection = new SqlConnection(connstring))
        {

            connection.Open();


            using (var command = connection.CreateCommand())
            {

                command.CommandText = "SELECT * FROM weapons;";

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


    private void Awake()
    {
        instance = this;
    }

   
   

    public void CreateDB(string connstring)
    {

        //create the db connection
        using (SqlConnection connection = new SqlConnection(connstring))
        {

            connection.Open();

            //set up objeect called command to allow db control
            using (var command = connection.CreateCommand())
            {

                //sql statements to execute
                command.CommandText = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'userdata')BEGIN  CREATE TABLE userdata ( playerid INT, name VARCHAR(20))END;";
                command.ExecuteNonQuery();
                command.CommandText = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'weapons')BEGIN  CREATE TABLE weapons ( playerid INT, weaponid INT, quantity INT) END;";
                command.ExecuteNonQuery();
                command.CommandText = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'playerlocation')BEGIN  CREATE TABLE playerlocation ( playerid INT, x INT, y INT, z INT) END;";
                command.ExecuteNonQuery();
            }

            connection.Close();
        }
    }


    public void AddWeapon(int playerID, int weaponid, int quantity)
    {
        string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        using (SqlConnection connection = new SqlConnection(connstring))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {

                //correct format is " INSERT INTO weapons (playerid,weponid,quantity) VALUES (playerid, weaponid, quantity); "
                command.CommandText = "INSERT INTO weapons (playerid,weaponid,quantity) VALUES (" + playerID + "," + weaponid + "," + quantity + ");";
                command.ExecuteNonQuery();
                Debug.Log("(SQL server) Weapon added with id: " + weaponid + " to player with ID = " + playerID);
            }

            connection.Close();
        }

    }

    public void RemoveWeapon(int playerID, int weaponid, int quantity)
    {
        string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        using (SqlConnection connection = new SqlConnection(connstring))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {

                //correct format is " DELETE FROM weapons WHERE playerid = player ANS weaponid = id; "
                command.CommandText = "DELETE FROM weapons WHERE playerid = " + playerID + " AND weaponid = " + weaponid + " ;";
                command.ExecuteNonQuery();

            }

            connection.Close();
        }
        Debug.Log("(SQL server) Weapon added with id: " + weaponid);
    }

    



    public void UpdatePlayerLocation(int playerID, int x, int y, int z)
    {
        string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        bool dataFound = false;
        using (SqlConnection connection = new SqlConnection(connstring))
        {

            connection.Open();


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
                    command.CommandText = "INSERT INTO playerlocation (playerid,x,y,z) VALUES (" + playerID + "," + x + "," + y + "," + z + ");";
                    command.ExecuteNonQuery();
                }
            }
            connection.Close();


        }
    }

    public Item[] GetStartingItems(int playerID)
    {
        string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        List<Item> items = new List<Item>();
        using (SqlConnection connection = new SqlConnection(connstring))
        {

            connection.Open();


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

        Item[] startingItems = items.ToArray();
        return startingItems;
    }

    public int[] getStartingLocation(int playerID)
    {
        string connstring = "Server=DESKTOP-2P23NMB;database=AvatarProject;Trusted_Connection=True;";
        int[] startingCoordinates = new int[3];
        using (SqlConnection connection = new SqlConnection(connstring))
        {

            connection.Open();


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
        }
        return newItem;
    }



}

