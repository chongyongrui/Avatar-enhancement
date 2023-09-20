using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static DHMessager;
using TMPro;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Globalization;
using UnityEditor.PackageManager;

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



        using (ECDiffieHellmanCng user = new ECDiffieHellmanCng())
        {
            user.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            user.HashAlgorithm = CngAlgorithm.Sha256;
            userPublicKey = user.PublicKey.ToByteArray();
            //todo post the public key to the ledger as string
            string schemaName = username + ".publickey";
            var str = System.Text.Encoding.UTF8.GetString(userPublicKey);
            sharePublicKey(schemaName,str);
        }


    }

    private void GetMyMessages()
    {
        //todo check for messages every x seconds
        List<string> messages = new List<string>();
        messages = GetMessages(username, ledgerUrl);
        bobKey = DeriveKeyMaterial(CngKey.Import(Alice.alicePublicKey, CngKeyBlobFormat.EccPublicBlob));
        foreach (string message in messages)
        {
            ReceiveMessage();
        }

    }


    public void SendMessage(ECDiffieHellmanCng user)
    {

        //todo get bobs public key from ledger/from text input
        ReceiverName = ReceiverNameInputField.text;
        string myMessage = MessageString.text;
        string receiverKeyString = GetPublicKey(ReceiverName, ledgerUrl);
        byte[] receiverByteArray = Encoding.UTF8.GetBytes(receiverKeyString); ;
        CngKey receiverKey = CngKey.Import(receiverByteArray, CngKeyBlobFormat.EccPublicBlob);
        byte[] myKey = user.DeriveKeyMaterial(receiverKey);
        byte[] encryptedMessage = null;
        byte[] iv = null;
        Send(myKey, myMessage, out encryptedMessage, out iv, ReceiverName);
        //bob.Receive(encryptedMessage, iv);

    }

    private void Send(byte[] key, string secretMessage, out byte[] encryptedMessage, out byte[] iv, string ReceiverName)
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

            //todo post the ciphertext to blockchain ledger
            ReceiverName = ReceiverName + ".Message";
            string encryptedMessageString = Encoding.UTF8.GetString(encryptedMessage);
            postMessageToLedger(ReceiverName, encryptedMessageString);
        }
    }

    public void ReceiveMessage(byte[] mykey, byte[] iv)
    {

        byte[] encryptedMessage = null; 

        //get encryptedMessage from ledger

        using (Aes aes = new AesCryptoServiceProvider())
        {
            aes.Key = mykey;
            aes.IV = iv;
            // Decrypt the message
            using (MemoryStream plaintext = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(encryptedMessage, 0, encryptedMessage.Length);
                    cs.Close();
                    string message = Encoding.UTF8.GetString(plaintext.ToArray());
                    Console.WriteLine("FOUND MESSAGE:  " + message);
                }
            }
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
                ""schema_version"": ""3.0""
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









    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>



    class Alice
    {
        public static byte[] alicePublicKey;

        public static void Main(string[] args)
        {
            using (ECDiffieHellmanCng alice = new ECDiffieHellmanCng())
            {

                alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                alice.HashAlgorithm = CngAlgorithm.Sha256;
                alicePublicKey = alice.PublicKey.ToByteArray();
                Bob bob = new Bob();
                CngKey bobKey = CngKey.Import(bob.bobPublicKey, CngKeyBlobFormat.EccPublicBlob);
                byte[] aliceKey = alice.DeriveKeyMaterial(bobKey);
                byte[] encryptedMessage = null;
                byte[] iv = null;
                Send(aliceKey, "Secret message", out encryptedMessage, out iv);
                bob.Receive(encryptedMessage, iv);
            }
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
    }
    public class Bob
    {
        public byte[] bobPublicKey;
        private byte[] bobKey;
        public Bob()
        {
            using (ECDiffieHellmanCng bob = new ECDiffieHellmanCng())
            {

                bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                bob.HashAlgorithm = CngAlgorithm.Sha256;
                bobPublicKey = bob.PublicKey.ToByteArray();
                bobKey = bob.DeriveKeyMaterial(CngKey.Import(Alice.alicePublicKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }

        public void Receive(byte[] encryptedMessage, byte[] iv)
        {

            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = bobKey;
                aes.IV = iv;
                // Decrypt the message
                using (MemoryStream plaintext = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedMessage, 0, encryptedMessage.Length);
                        cs.Close();
                        string message = Encoding.UTF8.GetString(plaintext.ToArray());
                        Console.WriteLine(message);
                    }
                }
            }
        }
    }
}
