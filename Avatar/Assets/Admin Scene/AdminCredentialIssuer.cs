using Docker.DotNet;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Newtonsoft.Json.Linq;
using Npgsql;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Windows;
using Input = UnityEngine.Input;

public class AdminCredentialIssuer : MonoBehaviour
{
    // Start is called before the first frame update
    private static readonly HttpClient client = new HttpClient();
    [SerializeField] private TMP_InputField userIDInputField;
    [SerializeField] private TMP_InputField expiryInputField;
    [SerializeField] private TMP_Text issuerName;
    [SerializeField] private GameObject userRequest;
    [SerializeField] private GameObject adminGenerate;
    [SerializeField] private GameObject adminIssue;
    [SerializeField] private GameObject adminReceiverFields;
    [SerializeField] private TMP_InputField receiverNameInputField;
    [SerializeField] private TMP_Text requets;
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    [SerializeField] private TMP_Dropdown dropDown;
    [SerializeField] Camera cam;
    private string IPAddress;
    private string issuer;
    bool validInput = false;
    public string ledgerUrl;
    public static AdminCredentialIssuer instance;
    // Start is called before the first frame update
    void Start()
    {
        IPAddress = userdatapersist.Instance.IPAdd;
        ledgerUrl = "http://" + IPAddress + ":9000";
        issuer = userdatapersist.Instance.verifiedUser;
        issuerName.text = issuer;
        instance = this;
        if (userdatapersist.Instance.verifiedUser != "admin")
        {

            userRequest.SetActive(true);
            adminGenerate.SetActive(false);
            adminIssue.SetActive(false);
            adminReceiverFields.SetActive(false);
            this.gameObject.SetActive(false);
            InvokeRepeating("GetRequests", 10.0f, 5.0f);
        }
        GetRequests();
    }

    public void GetRequests()
    {
        string ledgerUrl = "http://" + IPAddress + ":9000";
        Dictionary<string, string> requests = new Dictionary<string, string>();
        string transactionsUrl = $"{ledgerUrl}/ledger/domain?query=&type=101"; // Specify the transaction type as "101" for schemas
        HttpResponseMessage response = client.GetAsync(transactionsUrl).Result;

        if (response.IsSuccessStatusCode)
        {
            string responseBody = response.Content.ReadAsStringAsync().Result;
            var transactions = JToken.Parse(responseBody)["results"];

            foreach (var transaction in transactions)
            {

                var responseData = transaction["txn"]["data"]["data"];
                var type = responseData["version"];
                var credName = responseData["name"];
                var attributes = responseData["attr_names"];
                var data = transaction["reqSignature"]["values"][0];
                JObject jsonObject = JObject.Parse(data.ToString());
                string senderDID = (string)jsonObject["from"];
                string senderAlias = GetAlias(senderDID.ToString());
                string key = senderAlias + type.ToString();

                if (CheckSQLAlreadyAccepted(senderAlias, type.ToString()) == false)
                {

                    if (type.ToString() == "2.1" && !requests.ContainsKey(key)) // found a DH paramter that is to connect to you
                    {
                        string ans = senderAlias + " requests for car access";
                        requests.Add(key, ans);
                    }
                    else if (type.ToString() == "2.2" && !requests.ContainsKey(key))
                    {
                        string ans = senderAlias + " requests for dynamite access";
                        requests.Add(key, ans);
                    }

                }

            }

            requests = RemoveIssuedRequests(requests);

            string pendingRequests = null;
            int i = 1;
            foreach (var kvp in requests)
            {
                pendingRequests = pendingRequests + i + ". " + kvp.Value + "\n";
                i++;
            }
            if (requests == null)
            {
                pendingRequests = "None found";
            }
            requets.text = pendingRequests;
        }
        else
        {
            Debug.Log("Error retrieving transactions!");
        }

    }



    public Dictionary<string, string> RemoveIssuedRequests(Dictionary<string, string> requests)
    {
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;"))
            {

                connection.Open();

                using (var command = connection.CreateCommand())
                {

                    command.CommandText = "SELECT * FROM issuedkeys;";

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Debug.Log("Player ID: " + reader["receiver_hash"] + " \tWeapon ID: " + reader["key_type"]);
                            string target = reader["receiver_hash"].ToString() + reader["key_type"].ToString();
                            if (requests.ContainsKey(target))
                            {
                                requests.Remove(target);
                            }
                        }
                        reader.Close();
                    }
                }

                connection.Close();


            }
        }
        catch (Exception e)
        {
            Debug.Log("(SQL server) Error updating requests");
        }




        return requests;
    }

    public bool CheckSQLAlreadyAccepted(string user, string type)
    {

        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection("Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM IssuedKeys WHERE receiver_hash = '" + user + "'AND key_type = '" + type + "' ;";
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {

                        if (reader.Read() == false)
                        {
                            connection.Close();
                            return false;
                        }

                        while (reader.Read())
                        {
                            Debug.Log(reader["key_val"] + "is what is found");
                            if (reader["key_val"] == null)
                            {
                                Debug.Log("(SQL server) no prior private key data found");
                                connection.Close();
                                return false;
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

        return true;

    }

    public string GetAlias(string DID)
    {
        string ans = null;
        string ledgerUrl = "http://" + IPAddress + ":9000";


        string transactionsUrl = $"{ledgerUrl}/ledger/domain?query=&type=1"; // Specify the transaction type as "101" for schemas
        HttpResponseMessage response = client.GetAsync(transactionsUrl).Result;

        if (response.IsSuccessStatusCode)
        {
            string responseBody = response.Content.ReadAsStringAsync().Result;
            var transactions = JToken.Parse(responseBody)["results"];

            foreach (var transaction in transactions)
            {

                var responseData = transaction["txn"]["data"];
                var foundDID = responseData["dest"];
                var foundAlias = responseData["alias"];

                if (DID == foundDID.ToString()) // found a DH paramter that is to connect to you
                {
                    return foundAlias.ToString();
                }
            }

        }
        else
        {
            Debug.Log("Error retrieving transactions!");
        }

        return ans;
    }

    public void AdminIssueCredential()
    {

        //Get ID and expiry from input fields
        string userID = userIDInputField.text;
        string receiverName = receiverNameInputField.text;
        int expiryDate = -1;
        string type = dropDown.captionText.text;

        //check if date in the valid format
        try
        {
            expiryDate = Int32.Parse(expiryInputField.text);
            DateTime date = DateTime.ParseExact(expiryInputField.text, "ddmmyyyy", CultureInfo.InvariantCulture);
            validInput = true;
        }
        catch (Exception)
        {
            Console.WriteLine($"Unable to parse '{expiryInputField.text}'");
            popupWindow.SetActive(true);
            windowMessage.text = "Expiry Date is in the wrong format!";
        }

        if (MatchUserID(receiverName, userID))
        {
            if (validInput && type == "CAR")
            {
                //get transaction ID using userID
                string transactionID = GetTransactionID(userID, "2.1");
                Debug.Log("Got transaction ID = " + transactionID);
                string expiryString = expiryDate.ToString();
                if (expiryString.Length < 8)
                { // DD is single digit
                    expiryString = "0" + expiryString;
                }
                if (transactionID != null)
                {
                    Debug.Log("Sending Issue request...");
                    sendIssueReq(transactionID, "2.1", userID, receiverName);
                }

            }
            else if (type == "DYNAMITE")
            {
                string transactionID = GetTransactionID(userID, "2.2");
                string expiryString = expiryDate.ToString();
                if (expiryString.Length < 8)
                { // DD is single digit
                    expiryString = "0" + expiryString;
                }
                if (transactionID != null)
                {
                    sendIssueReq(transactionID, "2.2", userID, receiverName);
                }
            }
            else
            {
                popupWindow.SetActive(true);
                windowMessage.text = "Invalid type!";
            }
        }
        else
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Invalid user ID / Receiver name!";
        }


    }

    public bool MatchUserID(string username, string userID)
    {
        string adminConString = "Server=" + IPAddress + ";Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;";
        NpgsqlConnection con = new NpgsqlConnection(adminConString);
        Debug.Log("Authenticating token with SQL server...");

        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(adminConString))
            {

                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM userdata WHERE userid_hash = " + userID.GetHashCode() + ";";

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["username_hash"].ToString() == username.GetHashCode().ToString())
                            {
                                return true;
                            }

                        }
                    }

                    connection.Close();

                }

                return false;

            }
        }

        catch (Exception e)
        {
        }

        return false;
    }


    public string GetTransactionID(string userID, string target)
    {
        string attribute = (userID + target).GetHashCode().ToString();
        Debug.Log("looking for filter = " + attribute);
        string value = null;
        try
        {
            string transactionsUrl = $"{ledgerUrl}/ledger/domain?query=" + attribute + "&type=101"; // Specify the transaction type as "101" for schemas
            HttpResponseMessage response = client.GetAsync(transactionsUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var transactions = JToken.Parse(responseBody)["results"];

                foreach (var transaction in transactions)
                {

                    var responseData = transaction["txn"]["data"]["data"];
                    var type = responseData["version"];
                    var transactionID = transaction["txnMetadata"]["txnId"];

                    Debug.Log(type.ToString() + "found type");
                    if (type.ToString() == target) // found the correct request
                    {
                        value = transactionID.ToString();
                        Debug.Log("found transaction id of " + value);
                    }
                }

            }
            else
            {
                Debug.Log("Error retrieving transactions!");
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            popupWindow.SetActive(true);
            windowMessage.text = "Error parsing transactions!";
        }

        return value;
    }

    public string GetAttributes(string transactionID)
    {
        string value = null;
        try
        {
            string transactionsUrl = $"{ledgerUrl}/ledger/domain?query=" + transactionID + "&type=101"; // Specify the transaction type as "101" for schemas
            HttpResponseMessage response = client.GetAsync(transactionsUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var transactions = JToken.Parse(responseBody)["results"];

                foreach (var transaction in transactions)
                {

                    var responseData = transaction["txn"]["data"]["data"];
                    var attributes = responseData["attr_names"];
                    string val = attributes.ToString();
                    string invalidPattern = "[^0-9A-Fa-f.-]";
                    // Use Regex.Replace to remove all characters that are not in the valid range.
                    val = Regex.Replace(val, invalidPattern, "");
                    return val;
                }

            }
            else
            {
                Debug.Log("Error retrieving transactions!");
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            popupWindow.SetActive(true);
            windowMessage.text = "Error parsing transactions!";
        }

        return null;
    }




    public void sendIssueReq(string transactionID, string type, string userID, string receiverName)
    {
        popupWindow.SetActive(true);
        windowMessage.text = "Processing...";

        string url = "http://localhost:11001/credential-definitions";

        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string jsonPayload = $@"{{
                 ""revocation_registry_size"": 1000,
                 ""schema_id"": ""{transactionID}"",
                 ""support_revocation"": false,
                 ""tag"": ""default""
             }}";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = httpClient.PostAsync(url, content).Result; // Blocking call

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result; // Blocking call
                    Console.WriteLine(responseBody);
                    popupWindow.SetActive(true);
                    windowMessage.text = "Post Cred-def Success! \n";
                    GetCredDef(userID, type, receiverName);
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }
        }
        catch (Exception e)
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Error posting! Check if ACA-py has loaded! " + e;
        }
    }


    public void GetCredDef(string receiverID, string type, string receiverName)
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
                        GenerateKey(val, receiverName, type);
                        popupWindow.SetActive(true);
                        windowMessage.text = "Success";
                        break;
                    }
                }

            }

            if (found == false)
            {
                popupWindow.SetActive(true);
                windowMessage.text = "Unable to find issued cred_def \n";
            }

        }

        else
        {
            Debug.Log("Error retrieving transactions!");
        }
    }


    public void GenerateKey(string keyParams, string receiver, string type)
    {
        //generate key using AES with ledger params and AES key
        string AESKey = GetAESKey(receiver);

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
                SQLAddKey(encryptedKey, receiver, type);
                popupWindow.SetActive(true);
                windowMessage.text = "Key Issue Success! \n";
            }

        }
        catch (Exception e)
        {
            Debug.Log("Failed at message encryption process" + e.Message);
        }



    }


    public void SQLAddKey(string encryptedKey, string receiver, string type)
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
                    command.CommandText = "INSERT INTO issuedkeys (receiver_hash,key_type,key_val) VALUES ('" + receiver + "','" + type + "','" + encryptedKey + "' )";
                    command.ExecuteNonQuery();
                    Debug.Log("(SQL server) key added with id: " + encryptedKey + " for user " + receiver);
                }
                connection.Close();


            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL Server) Error inserting key!  " + e);

        }
    }

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


}
