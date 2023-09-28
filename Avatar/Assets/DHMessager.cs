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
using static UnityEngine.UI.Image;

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
        using (Aes myAes = Aes.Create())
        {

            // Encrypt the string to an array of bytes.
            byte[] encrypted = EncryptStringToBytes_Aes(message, myAes.Key, myAes.IV);
            string encryptedString = Convert.ToBase64String(encrypted);
            Debug.Log("receiver user is " + hashedReceiverUserName + " and the encryted message is " + encryptedString);
            postMessageToLedger(encryptedString, hashedReceiverUserName);
            // Decrypt the bytes to a string.
            //string roundtrip = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);

            //Display the original data and the decrypted data.
            Console.WriteLine("Original:   {0}", message);
            //Console.WriteLine("Round Trip: {0}", roundtrip);
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

    static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
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
