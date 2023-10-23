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
using UnityEditor.PackageManager;


public class CredentialIssuer : MonoBehaviour
{

    private static readonly HttpClient client = new HttpClient();
    [SerializeField] private TMP_InputField userIDInputField;
    [SerializeField] private TMP_InputField expiryInputField;
    [SerializeField] private TMP_Text issuerName;
    [SerializeField] private GameObject adminRequests;
    [SerializeField] private TMP_Text requets;
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

        if (userdatapersist.Instance.verifiedUser == "admin")
        {
            adminRequests.SetActive(true);
            GetRequests();
            //InvokeRepeating("GetRequests", 10.0f, 5.0f);
        }
    }



    public void GetRequests()
    {
        string ledgerUrl = "http://" + IPAddress + ":9000";
        List<Tuple<string, string>> requests = new List<Tuple<string, string>>();
        /*try
        {*/
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
                Debug.Log("sender DID is " + senderDID + "of name " + senderAlias + " with a schema version of " + type.ToString());
                string key = senderAlias + type.ToString();
                if (CheckSQLAlreadyAccepted(senderAlias, type.ToString()) == false)
                {
                    if (type.ToString() == "2.1") // found a DH paramter that is to connect to you
                    {
                        string ans = senderAlias + " requests for car access";
                        Tuple<string, string> val = new Tuple<string, string>(senderAlias, ans);
                        Debug.Log(ans);
                        requests.Add(val);
                    }
                    else if (type.ToString() == "2.2")
                    {
                        string ans = senderAlias + " requests for dynamite access";
                        Tuple<string, string> val = new Tuple<string, string>(senderAlias, ans);
                        Debug.Log(ans);
                        requests.Add(val);
                    }
                }

            }

            string pendingRequests = null;
            int i = 1;
            foreach (Tuple<string,string> tuple in requests)
            {
                pendingRequests = pendingRequests + i +". " + tuple.Item2 + "\n" ; 
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


        /*}
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            popupWindow.SetActive(true);
            windowMessage.text = "Error parsing transactions!";
        }*/

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
                        while (reader.Read())
                        {
                            if (reader["key_val"] == null)
                            {
                                Debug.Log("(SQL server) no prior private key data found");
                                return false;
                            }
                            else
                            {
                                return true;
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


    public async void RequestCredential()
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

        if (validInput && type != "ACCOUNT")
        {
            string expiryString = expiryDate.ToString();
            if (expiryString.Length < 8)
            { // DD is single digit
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




    public async void sendReq(string issuer, int credentialID, string userID, int expiry, string expiryString, string type)
    {

        //string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
        string url = "http://" + IPAddress + ":11001/schemas?create_transaction_for_endorser=false";
        int schemaVer1 = 0;
        int schemaVer2 = 0;

        if (type == "ACCOUNT")
        {
            schemaVer1 = 1;
            schemaVer2 = 0;
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
        }
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {


                // Prepare the JSON payload
                string jsonPayload = $@"{{
                ""attributes"": [
                    ""{userID.GetHashCode()}.{expiryString}""                
                ],
                ""schema_name"": ""{credentialID.ToString()}"",
                ""schema_version"": ""{schemaVer1}.{schemaVer2}""
            }}";

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
                    windowMessage.text = "Credential Generation Success! \n Credential ID = " + credentialID;

                    AddCredentialToServer(issuer, credentialID, userID, expiry);
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
                    command.CommandText = "INSERT INTO IssuedCredentials (CredentialID,Issuer,UserID,Expiry,Activated) VALUES (" + credentialID + ",'" + issuer + "','" + userID + "'," + expiry + ", CAST(0 AS bit) )";
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



}
