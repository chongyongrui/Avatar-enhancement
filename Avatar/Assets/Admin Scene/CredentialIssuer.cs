using System;
using System.Collections;
using System.Collections.Generic;
using Npgsql;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using I18N.Common;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.SceneManagement;
using Org.BouncyCastle.Asn1.Crmf;

public class CredentialIssuer : MonoBehaviour
{

    private static readonly HttpClient client = new HttpClient();
    [SerializeField] private TMP_InputField userIDInputField;
    [SerializeField] private TMP_InputField expiryInputField;
    [SerializeField] private TMP_Text issuerName;
    [SerializeField] private GameObject adminRequests;
    [SerializeField] private GameObject keyPanel;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    [SerializeField] private TMP_Dropdown dropDown;
    private string IPAddress;
    private string issuer;
    bool validInput = false;
    public string ledgerUrl;

    // Start is called before the first frame update
    void Start()
    {
        IPAddress = userdatapersist.Instance.IPAdd;
        ledgerUrl = "http://" + IPAddress + ":9000";
        issuer = userdatapersist.Instance.verifiedUser;
        issuerName.text = issuer;
        if (userdatapersist.Instance.verifiedUser != "admin")
        {
            keyPanel.SetActive(true);
            
        }
        string myKeys = GetSQLKeys();
        keyText.text = myKeys;
    }


    /// <summary>
    /// Reads the ledger to see if there are any credentials that can be added to the users wallet
    /// </summary>
    /// <param name="receiverID"> the user's username</param>
    /// <param name="type">the code of the type of crednetial to check for</param>
    public async void GetCredDef(string receiverID, string type)
    {


        //check if the receiverID matches the system logs

        if (VerifyID(receiverID))
        {

            //Get ledger params
            //string attributes = GetAttributes(tranactionID);
            string attributes = (receiverID + type).GetHashCode().ToString();
            Debug.Log("receiverID is " + receiverID);
            Debug.Log("attributes is " + attributes);

            string value = null;

            string transactionsUrl = $"{ledgerUrl}/ledger/domain?query=&type=102"; // Specify the transaction type as "102" for cred def
            HttpResponseMessage response = client.GetAsync(transactionsUrl).Result;



            if (response.IsSuccessStatusCode)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var transactions = JToken.Parse(responseBody)["results"];
                bool found = false;
                foreach (var transaction in transactions)
                {
                    //Debug.Log(transaction.ToString());
                    if (transaction["txn"]["data"]["data"]["primary"]["r"] != null)
                    {
                        var responseData = transaction["txn"]["data"]["data"]["primary"]["r"];


                        if (responseData[attributes] != null)
                        {
                            found = true;
                            string val = responseData[attributes].ToString();
                            Debug.Log("Generating Key");
                            GenerateKey(val, type);
                        }
                    }

                }

                if (found == true)
                {
                    popupWindow.SetActive(true);
                    windowMessage.text = "Received a credential \n";
                }

            }

            else
            {
                Debug.Log("Error retrieving transactions!");
            }
        }




    }


    public bool VerifyID(string receiverID)
    {
        string adminConString = "Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;";
        NpgsqlConnection con = new NpgsqlConnection(adminConString);
        LoginController.instance.CreateTables();
        bool isMatch = false;
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(adminConString))
            {

                connection.Open();

                using (var command = connection.CreateCommand())
                {

                    command.CommandText = "select * from userdata WHERE username_hash = '"+ userdatapersist.Instance.verifiedUser.GetHashCode()+"' and userid_hash = '"+receiverID.GetHashCode()+ "';";
                    

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        isMatch = reader.Read();
                        reader.Close();
                    }
                }
                connection.Close();
                

            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL Server) Error inserting key!  " + e);

        }
        return isMatch;
    }

    /// <summary>
    /// Checks if there are any new credentials of all types issued to the user 
    /// </summary>

    public void GetKeys()
    {
        string receiverID = userIDInputField.text;
        GetCredDef(receiverID, "2.1");
        GetCredDef(receiverID, "2.2");
        string myKeys = GetSQLKeys();
        keyText.text = myKeys;
    }

    /// <summary>
    /// Gets all available keys the users has from his local PSQL wallet
    /// </summary>
    /// <returns>string of all keys strored on the PSQL wallet </returns>

    public string GetSQLKeys()
    {
        string myKeys = null;
        string username = userdatapersist.Instance.verifiedUser;
        string password = userdatapersist.Instance.verifiedPassword;
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;User Id= " + username + ";Password=" + password + ";Database=" + username + "wallet;"))
            {

                connection.Open();

                using (var command = connection.CreateCommand())
                {

                    command.CommandText = "SELECT * FROM other_keys;";
                    int i = 1;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string keyType = reader["key_type"].ToString();
                            if (keyType == "2.1")
                            {
                                myKeys = myKeys + i + ". CAR\n"; 
                            }else if (keyType == "2.2")
                            {
                                myKeys = myKeys + i + ". DYNAMITE\n";
                            }
                            i++;
                            
                        }
                        reader.Close();
                    }
                }

                connection.Close();

                return myKeys;
            }
        }
        catch (Exception e)
        {
            Debug.Log("(SQL server) Error updating requests");
        }
        return null;
    }


    /// <summary>
    /// Creates a new credential in the form of a key by encrypting the public key params with the private AES key
    /// 
    /// </summary>
    /// <param name="keyParams">the public key params to encrypt</param>
    /// <param name="type">the code of the type of credential that will be created</param>
    public void GenerateKey(string keyParams, string type)
    {
        //generate key using AES with ledger params and AES key
        string AESKey = GetAESKey("admin");

        byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(AESKey);
        if (keyBytes.Length != 32)
        {
            // Pad or truncate the key to 32 bytes
            Array.Resize(ref keyBytes, 32);
        }

        string AESIV = keyParams.Substring(0, 32);
        string encyrptedString = keyParams.Substring(32, 32);
        string invalidPattern = "[^0-9A-Fa-f]";
        string newEncryptedString = Regex.Replace(encyrptedString, invalidPattern, "");
        string newAESIV = Regex.Replace(AESIV, invalidPattern, "");

        byte[] receivedEncryptedMessage = Enumerable.Range(0, newEncryptedString.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(newEncryptedString.Substring(x, 2), 16))
                .ToArray();

        byte[] receivedAESIV = Enumerable.Range(0, newAESIV.Length)
        .Where(x => x % 2 == 0)
        .Select(x => Convert.ToByte(newAESIV.Substring(x, 2), 16))
        .ToArray();
        string encryptedKey = null;


        try
        {
            using (Aes newAes = Aes.Create())
            {
                newAes.Key = keyBytes;
                newAes.IV = receivedAESIV;
                byte[] encrypted = EncryptStringToBytes_Aes(newEncryptedString, newAes.Key, newAes.IV);
                encryptedKey = BitConverter.ToString(encrypted).Replace("-", string.Empty);
                SQLAddKey(encryptedKey, type);
                popupWindow.SetActive(true);
                windowMessage.text = "Key Issue Success! \n";
            }

        }
        catch (Exception e)
        {
            Debug.Log("Failed at message encryption process" + e.Message);
        }



    }


    /// <summary>
    /// Adds the key to the PSQL wallet of the user
    /// </summary>
    /// <param name="encryptedKey">the encrypted key value</param>
    /// <param name="type">the code of the type of key </param>
    public void SQLAddKey(string encryptedKey, string type)
    {
        string username = userdatapersist.Instance.verifiedUser;
        string password = userdatapersist.Instance.verifiedPassword;
        NpgsqlConnection con = new NpgsqlConnection("Server=localhost;Port=5432;User Id= " + username + ";Password=" + password + ";Database=" + username + "wallet;");
        //LoginController.instance.CreateNewDB();
        LoginController.instance.CreateTables();
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;User Id= " + username + ";Password=" + password + ";Database=" + username + "wallet;"))
            {

                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO other_keys (key_type,key_val) SELECT '" + type + "','" + encryptedKey + "' WHERE NOT EXISTS ( SELECT 1 FROM other_keys WHERE key_type = '" + type+ "');";
                    command.ExecuteNonQuery();
                    Debug.Log("(SQL server) key added with id: " + encryptedKey + " of type " + type);
                }
                connection.Close();


            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL Server) Error inserting key!  " + e);

        }
    }

    /// <summary>
    /// Checks if the user has an established connection with the admin
    /// </summary>
    /// <returns>boolean representing the presence of a connection with the admin</returns>

    public bool CheckAdminConnection()
    {
        string username = userdatapersist.Instance.verifiedUser;
        string password = userdatapersist.Instance.verifiedPassword;
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;User Id= " + username + ";Password=" + password + ";Database=" + username + "wallet;"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select * from aes_keys where receiver_hash = 'admin';";
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {

                        if (reader.Read() == false)
                        {
                            connection.Close();

                            return false;
                        }
                        connection.Close();
                        return true;


                    }

                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("(SQL Server) Error getting private key " + e);
        }

        return false;

    }


    /// <summary>
    /// Posts to a ledger a schema representing the request for a certian type of credential
    /// </summary>

    public async void RequestCredential()
    {

        //Get ID and expiry from input fields
        string userID = userIDInputField.text;
        int expiryDate = -1;
        string type = dropDown.captionText.text;
        validInput = true;
        //check if date in the valid format
        
        if (CheckAdminConnection() == true && AdminCredentialIssuer.instance.MatchUserID(userdatapersist.Instance.verifiedUser, userID))
        {
            if (validInput && type != "ACCOUNT")
            {
                string expiryString = expiryDate.ToString();
                if (expiryString.Length < 8)
                {
                    expiryString = "0" + expiryString;
                }
                int CredentialID = (userID + issuer + expiryInputField.text).GetHashCode();
                sendReq(issuer, CredentialID, userID, expiryDate, expiryString, type);
            }
            else if (validInput && type == "ACCOUNT")
            {
                popupWindow.SetActive(true);
                windowMessage.text = "Type should not be \"Account\"";
            }
            else
            {
                popupWindow.SetActive(true);
                windowMessage.text = "Invalid input";
            }
        }
        else
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Check for invalid inputs!";
        }



    }

    /// <summary>
    /// Creates and post a invitation token to the ledger in the form of a schema
    /// </summary>

    public async void GenerateCredential()
    {

        //Get ID and expiry from input fields
        string userID = userIDInputField.text;
        int expiryDate = -1;
        string type = dropDown.captionText.text;

        //check if date in the valid format
        try
        {
            expiryDate = Int32.Parse(expiryInputField.text);
            DateTime date = DateTime.ParseExact(expiryInputField.text, "ddmmyyyy", CultureInfo.InvariantCulture);
            Debug.Log("Input date is " + date);
            validInput = true;
        }
        catch (Exception)
        {
            Console.WriteLine($"Unable to parse '{expiryInputField.text}'");
            popupWindow.SetActive(true);
            windowMessage.text = "Expiry Date is in the wrong format!";
        }

        if (validInput && type == "ACCOUNT")
        {
            string expiryString = expiryDate.ToString();
            if (expiryString.Length < 8)
            { // DD is single digit
                expiryString = "0" + expiryString;
            }
            int CredentialID = (userID + issuer + expiryInputField.text).GetHashCode();
            sendReq(issuer, CredentialID, userID, expiryDate, expiryString, type);
        }
        else
        {
            if (userdatapersist.Instance.verifiedUser != "admin")
            {
                popupWindow.SetActive(true);
                windowMessage.text = "You are not admin!";
            }
            else
            {
                //VerifyCredentialRequest();
            }
        }


    }

    /// <summary>
    /// Encrypts a string using AES
    /// </summary>
    /// <param name="plainText">the string to be encrypted</param>
    /// <param name="Key">the AES key used to encrypt the string</param>
    /// <param name="IV">the AES initialisation vector to encrypt the string</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>


    static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");
        byte[] encrypted;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }


    /// <summary>
    /// Retrieves the AES key for a given connection from PSQL wallet
    /// </summary>
    /// <param name="connectionName"> the username of the AES key associated with</param>
    /// <returns></returns>

    public string GetAESKey(string connectionName)
    {
        string username = userdatapersist.Instance.verifiedUser;
        string password = userdatapersist.Instance.verifiedPassword;
        string foundKey = null;
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;User Id= " + username + ";Password=" + password + ";Database=" + username + "wallet;"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM AES_Keys WHERE receiver_hash = '" + connectionName + "';";
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["key_val"] == null)
                            {
                                Debug.Log("(SQL server) no prior private key data found");

                            }
                            else
                            {
                                foundKey = reader["key_val"].ToString();
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

        return foundKey;
    }


    /// <summary>
    /// Writes a request to the ledger
    /// </summary>
    /// <param name="issuer">the username wring the request</param>
    /// <param name="credentialID">the value of the credential that will be written to the ledger</param>
    /// <param name="userID"></param>
    /// <param name="expiry"></param>
    /// <param name="expiryString"></param>
    /// <param name="type"></param>


    public async void sendReq(string issuer, int credentialID, string userID, int expiry, string expiryString, string type)
    {

        //string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
        string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
        int schemaVer1 = 0;
        int schemaVer2 = 0;
        string jsonPayload = null;

        if (type == "ACCOUNT")
        {
            schemaVer1 = 1;
            schemaVer2 = 0;
            jsonPayload = $@"{{
                ""attributes"": [
                    ""{userID.GetHashCode()}.{expiryString}""                
                ],
                ""schema_name"": ""{credentialID.ToString()}"",
                ""schema_version"": ""{schemaVer1}.{schemaVer2}""
            }}";
        }
        else
        {
            schemaVer1 = 2;
            if (type == "CAR")
            {
                schemaVer2 = 1;
            }
            else if (type == "DYNAMITE")
            {
                schemaVer2 = 2;
            }
            else
            {
                schemaVer2 = 3;
            }
            jsonPayload = $@"{{
                ""attributes"": [
                    ""{(userID + schemaVer1 + "." + schemaVer2).GetHashCode()}""                
                ],
                ""schema_name"": ""{credentialID.ToString()}"",
                ""schema_version"": ""{schemaVer1}.{schemaVer2}""
            }}";
        }
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {


                // Prepare the JSON payload


                // Set headers
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // Create the request content
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                    popupWindow.SetActive(true);
                    if (schemaVer1 == 1)
                    {
                        windowMessage.text = "Credential Generation Success! \n Credential ID = " + credentialID;
                        AddCredentialToServer(issuer, credentialID, userID, expiry);
                    }
                    else
                    {
                        windowMessage.text = "Request Success! \nTxn ID: " + credentialID;
                    }



                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }
        }
        catch (Exception)
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Error posting! Check if ACA-py has loaded!";
        }


    }

    public bool AddCredentialToServer(string issuer, int credentialID, string userID, int expiry)
    {
        string adminConString = "Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;";
        NpgsqlConnection con = new NpgsqlConnection(adminConString);
        //LoginController.instance.CreateNewDB();
        LoginController.instance.CreateTables();

        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(adminConString))
            {

                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO IssuedCredentials (CredentialID,Issuer,UserID,Expiry,Activated) VALUES (" + credentialID + ",'" + issuer + "','" + userID.GetHashCode() + "'," + expiry + ", CAST(0 AS bit) )";
                    command.ExecuteNonQuery();
                    Debug.Log("(SQL server) credential added with id: " + credentialID + " by user " + issuer);
                }
                connection.Close();


            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL Server) Error inserting credential!  " + e);

        }
        return false;
    }

    public void OpenMessagePanel()
    {
        SceneManager.LoadSceneAsync("Messaging");
    }



}
