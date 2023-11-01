using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RestrictedItem : MonoBehaviour
{
    [SerializeField] public string itemCode;
    
    // Start is called before the first frame update
    public bool isRestricted = true;
    public int itemKey;
    

    public string GetItemKey(string playerName)
    {
        string foundKey ;
    
        string IPAddress = userdatapersist.Instance.IPAdd;
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select * from issuedkeys where receiver_hash = '" +playerName+ "' and key_type = '" + itemCode + "';";
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["key_val"] == null)
                            {
                                Debug.Log("(SQL server) no prior private key data found");
                                return null;
                            }
                            else
                            {
                                foundKey = reader["key_val"].ToString();
                                return foundKey;
                            }
                        }
                        reader.Close();
                    }
                    connection.Close();

                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("(SQL Server) Error getting private key " + e);
        }

        return null; ;

    }

    

}
