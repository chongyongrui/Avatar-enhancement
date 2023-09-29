using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;


public class DHMessager : MonoBehaviour
{

    public static DHMessager instance;
    public string username;
    public static byte[] userPublicKey;
    [SerializeField] public TMP_InputField ReceiverNameInputField;
    [SerializeField] public TMP_InputField MessageString;
    public string ReceiverName;
    public string IPAddress;
    public string ledgerUrl;
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    private static readonly HttpClient client = new HttpClient();
    public static byte[] alicePublicKey;



    private void Start()
    {
        IPAddress = userdatapersist.Instance.IPAdd;
        ledgerUrl = "http://" + IPAddress + ":9000";
        instance = this;
        try
        {
            username = userdatapersist.Instance.verifiedUser;
        }
        catch (System.Exception e)
        {
            Debug.Log("Unable to get username from Login page.");
        }


    }

    public void EncryptAndSend()
    {
        string message = MessageString.text;
        string hashedReceiverUserName = ReceiverNameInputField.text.ToString();
        string encryptedString;

        
    }


    private static void Send(byte[] key, string secretMessage, out byte[] encryptedMessage, out byte[] iv)
    {
        using (Aes aes = new AesCryptoServiceProvider())
        {
            aes.Key = key;
            iv = aes.IV;

            // Encrypt the message
            using (MemoryStream ciphertext = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                byte[] plaintextMessage = Encoding.UTF8.GetBytes(secretMessage);
                cs.Write(plaintextMessage, 0, plaintextMessage.Length);
                cs.Close();
                encryptedMessage = ciphertext.ToArray();
            }
        }
    }

    



    private void GetMyMessages()
    {
        //todo check for messages every x seconds
        List<string> messages = new List<string>();
        messages = GetMessages(username, ledgerUrl);
        //bobKey = DeriveKeyMaterial(CngKey.Import(Alice.alicePublicKey, CngKeyBlobFormat.EccPublicBlob));
        foreach (string message in messages)
        {
         //   ReceiveMessage();
        }

    }


    

    public async void sharePublicKey(string schemaName, string attribute)
    {

        //string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
        string url = "http://" + IPAddress + ":11001/schemas?create_transaction_for_endorser=false";

        using (HttpClient httpClient = new HttpClient())
        {

            // Prepare the JSON payload
            string jsonPayload = $@"{{
                ""attributes"": [
                    ""{attribute}""                
                ],
                ""schema_name"": ""{schemaName}"",
                ""schema_version"": ""2.0""
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
                windowMessage.text = "Posted to ledger!";

                
            }
            else
            {
                Console.WriteLine($"Request failed with status code: {response.StatusCode}");
            }
        }

    }

    public async void postMessageToLedger(string schemaName, string attribute)
    {

        //string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
        string url = "http://" + IPAddress + ":11001/schemas?create_transaction_for_endorser=false";

        using (HttpClient httpClient = new HttpClient())
        {

            // Prepare the JSON payload
            string jsonPayload = $@"{{
                ""attributes"": [
                    ""{attribute}""                
                ],
                ""schema_name"": ""{schemaName}"",
                ""schema_version"": ""2.0""
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
                windowMessage.text = "Posted to ledger!";


            }
            else
            {
                Console.WriteLine($"Request failed with status code: {response.StatusCode}");
            }
        }

    }

    public string GetPublicKey(string username, string ledgerUrl)
    {
        
        try
        {
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



                    if (type.ToString() == "2.0")
                    {
                        string foundUserName = credName.ToString();
                        string[] words = foundUserName.Split('.');
                        foundUserName = words[0];

  

                        string foundPublicKey = Convert.ToString(responseData["attr_names"][0]);
                        
                        if (string.Compare(foundUserName,username) == 0)
                        {
                            return foundPublicKey;
                        }

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
        }

        popupWindow.SetActive(true);
        windowMessage.text = "username is not valid!";
        return null;
    }


    public List<String> GetMessages(string username, string ledgerUrl)
    {
        List<String> messages = new List<String>();
        try
        {
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



                    if (type.ToString() == "3.0")
                    {
                        string foundUserName = credName.ToString();
                        string[] words = foundUserName.Split('.');
                        foundUserName = words[0];

                        string encryptedMessage = Convert.ToString(responseData["attr_names"][0]);

                        if (string.Compare(foundUserName, username) == 0)
                        {
                           messages.Add(encryptedMessage);
                        }

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
        }

        
        return messages;
    }
}
