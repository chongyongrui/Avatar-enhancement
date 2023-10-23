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
    }



    

    


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
        if (CheckAdminConnection() == true )
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
            windowMessage.text = "Create connection with admin before requesting!";
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
                    if (schemaVer1 == 1)
                    {
                        windowMessage.text = "Credential Generation Success! \n Credential ID = " + credentialID;
                        AddCredentialToServer(issuer, credentialID, userID, expiry);
                    }
                    else
                    {
                        windowMessage.text = "Request Success!" + credentialID;
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
