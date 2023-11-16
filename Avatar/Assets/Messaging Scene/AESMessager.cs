using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Linq;
using System.Text.RegularExpressions;

public class AESMessager : MonoBehaviour
{
    public static AESMessager instance;
    public string username;
    public static byte[] userPublicKey;
    [SerializeField] TMP_InputField ReceiverNameInputField;
    [SerializeField] TMP_InputField MessageString;
    [SerializeField] TMP_Text SentMessages;
    [SerializeField] TMP_Text ReceivedMessages;
    [SerializeField] TMP_Text issuerName;
    public string ReceiverName;
    public string IPAddress;
    public string ledgerUrl;
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    [SerializeField] GameObject ConnectionsPanel;
    [SerializeField] GameObject MessagesPanel;
    private static readonly HttpClient client = new HttpClient();
    public string message;
    public string hashedReceiverUserName;
    public static List<string> messages = new List<string>();
    public bool isFiltered = false;
    private void Start()
    {
        IPAddress = userdatapersist.Instance.IPAdd;
        ledgerUrl = "http://" + IPAddress + ":9000";
        instance = this;
        try
        {
            username = userdatapersist.Instance.verifiedUser;
            issuerName.name = username;
        }
        catch (System.Exception e)
        {
            Debug.Log("Unable to get username from Login page.");
        }
        GetAllMessages();
        InvokeRepeating("GetAllMessages", 10.0f, 10.0f);

    }

    public void ShowAllMessages()
    {
        isFiltered = false;
        GetAllMessages();
    }

    public void GetAllMessages()
    {

        //try
        {
            string transactionsUrl = $"{ledgerUrl}/ledger/domain?query=&type=101"; // Specify the transaction type as "101" for schemas
            HttpResponseMessage response = client.GetAsync(transactionsUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var transactions = JToken.Parse(responseBody)["results"];
                messages.Clear();
                foreach (var transaction in transactions)
                {
                    ParseToString(transaction,"");
                }

                string receivedMessages = "";
                string sentMessages = "";
                int i = 1;
                int j = 1;

                if (messages.Count > 0)
                {
                    foreach (string message in messages)
                    {
                        ParseToMessage(ref receivedMessages, ref sentMessages, ref i, ref j, message);
                    }
                }
                if (i == 1)
                {
                    receivedMessages += "none found" + "\n";
                }
                if (j == 1)
                {
                    sentMessages += "none found" + "\n";
                }

                if (isFiltered == false)
                {
                    SentMessages.text = sentMessages;
                    ReceivedMessages.text = receivedMessages;
                }

            }
            else
            {
                Debug.Log("Error retrieving transactions!");
            }
        }
        

    }

    private void ParseToMessage(ref string receivedMessages, ref string sentMessages, ref int i, ref int j, string message)
    {

        string data = message;
        data = data.Replace("[", "").Replace("]", "");
        data = data.Replace("\"", "");
        data = data.Replace(" ", "");
        int type = Int32.Parse(data.Split(".")[0]);
        string name = data.Split(".")[1];
        string encryptedMessage = data.Split(".")[2];
        string foundAESIV = data.Split(".")[3];
        string AESKey = GetAESKey(name);
        Debug.Log("type is " + type + ", " + encryptedMessage + " is message, " + foundAESIV + " is aesiv " + AESKey);
        try
        {
            string result = DecryptMessage(encryptedMessage, AESKey, foundAESIV);

            if (type == 0 && result != null) // received
            {
                receivedMessages += ("\n" + i + ". From " + name + ": " + result + "\n");
                i++;
            }
            else if (type == 1 && result != null) // sent
            {
                sentMessages += ("\n" + j + ". To " + name + ": " + result + "\n");
                j++;

            }
        }
        catch (Exception e)
        {
            if (type == 0 ) // received
            {
                receivedMessages += ("\n" + i + ". From " + name + ": Error parsing message \n");
                i++;
            }
            else if (type == 1) // sent
            {
                sentMessages += ("\n" + j + ". To " + name + ": Error parsing message \n");
                j++;

            }
        }

    }

    private static void ParseToString(JToken transaction, string keyWordSearch)
    {
        var responseData = transaction["txn"]["data"]["data"];
        var type = responseData["version"];
        var credName = responseData["name"];
        var attributes = responseData["attr_names"];

        if (type.ToString() == "5.0") // found a message
        {
            string target = credName.ToString();
            string receiver = target.Split(".")[0];
            string sender = target.Split(".")[1];
            List<string> data = new List<string>();
            data.Add(attributes[0].ToString());
            data.Add(attributes[1].ToString());
            data.Add(attributes[2].ToString());
            data.Add(attributes[3].ToString());
            data.Add(attributes[4].ToString());
            data.Add(attributes[5].ToString());
            data.Sort();
            string encryptedMessage = data[1].Substring(2) + data[2].Substring(2) + data[3].Substring(2) + data[4].Substring(2) + data[5].Substring(2);
            string AESIV = data[0].Substring(2);


            if (sender == userdatapersist.Instance.verifiedUser && receiver.Contains(keyWordSearch))
            {
                string parsed = "1." + receiver + "." + encryptedMessage + "." + AESIV;
                messages.Add(parsed);
            }
            else if (receiver == userdatapersist.Instance.verifiedUser && receiver.Contains(keyWordSearch))
            {
                string parsed = "0." + sender + "." + encryptedMessage + "." + AESIV;
                messages.Add(parsed);
            }
        }
    }

    public void FilterMessages()
    {
        isFiltered = true;
        string targetUsername = ReceiverNameInputField.text.ToString();
        try
        {
            string transactionsUrl = $"{ledgerUrl}/ledger/domain?query=&type=101"; // Specify the transaction type as "101" for schemas
            HttpResponseMessage response = client.GetAsync(transactionsUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var transactions = JToken.Parse(responseBody)["results"];
                messages.Clear();
                foreach (var transaction in transactions)
                {

                    ParseToString(transaction, targetUsername);
                }

                string receivedMessages = "";
                string sentMessages = "";
                int i = 1;
                int j = 1;

                if (messages.Count > 0)
                {
                    foreach (string message in messages)
                    {

                        ParseToMessage(ref receivedMessages, ref sentMessages, ref i, ref j, message);
                    }

                }
                if (i == 1)
                {
                    receivedMessages += "none found" + "\n";
                }
                if (j == 1)
                {
                    sentMessages += "none found" + "\n";
                }

                SentMessages.text = sentMessages;
                ReceivedMessages.text = receivedMessages;

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

    }


    public void SendData()
    {
  
            EncryptAndSendMessage();
      
    }

    public void EncryptAndSendMessage()
    {

        message = MessageString.text;
        hashedReceiverUserName = ReceiverNameInputField.text.ToString();
        //check if such a connection exists
        Dictionary<string, int> connections = new Dictionary<string, int>();
        DHMessager.instance.GetEstablishedConnections(connections);
        if (connections.ContainsKey(hashedReceiverUserName))
        {

            string keyValue = GetAESKey(hashedReceiverUserName);

            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(keyValue);
            // Ensure the key is 32 bytes long (AES-256)
            if (keyBytes.Length != 32)
            {
                // Pad or truncate the key to 32 bytes
                Array.Resize(ref keyBytes, 32);
            }
            // Your message to be encrypted
            EncryptAndPost(keyBytes, message, 5);

        }
        else
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Connection does not exist!";
        }
        GetAllMessages();
    }

    public void EncryptAndPost(byte[] keyBytes, string stringToEncrypt, int version)
    {
        try
        {
            using (Aes myAes = Aes.Create())
            {
                myAes.Key = keyBytes;
                string invalidPattern = "[^0-9A-Fa-f]";
                myAes.GenerateIV();
                string AESIV = BitConverter.ToString(myAes.IV).Replace("-", string.Empty);
                // Encrypt the string to an array of bytes
                byte[] encrypted = EncryptStringToBytes_Aes(stringToEncrypt, myAes.Key, myAes.IV);

                string encryptedString = BitConverter.ToString(encrypted).Replace("-", string.Empty);





                Debug.Log("encrypted message is " + encryptedString);

                string attribute = encryptedString;
                string schemaName = hashedReceiverUserName + "." + userdatapersist.Instance.verifiedUser;
                //post message to ledger
                PostMessageToLedger(schemaName, attribute, version, AESIV);
            }
            popupWindow.SetActive(true);
            windowMessage.text = "Message Sent!";
        }
        catch (Exception e)
        {
            Debug.Log("Failed at message encryption process" + e.Message);
        }
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

    public string DecryptMessage(string encyrptedString, string keyValue, string foundAESIV)
    {
        //encyrptedString = "3B1141E83052B69E881031F0DBE411C9EFB121EB66B4531141B73CE26BE826BC";
        //AESIV = "43FC7601EF84A2A18E317E2F29FC5D2A";
        //keyValue = "605430400530133053453980900039527052859476385626838272470836173704642956803610946381913979656307099007090924853107517882456801765988013644390716291693597";

        string decryptedMessage = null;
        string invalidPattern = "[^0-9A-Fa-f]";

        // Use Regex.Replace to remove all characters that are not in the valid range.
        string newEncryptedString = Regex.Replace(encyrptedString, invalidPattern, "");
        string newAESIV = Regex.Replace(foundAESIV, invalidPattern, "");

        Debug.Log("newEncryptedString string is :" + newEncryptedString + " newAESIV is " + newAESIV + " and key is " + keyValue);

        byte[] receivedEncryptedMessage = Enumerable.Range(0, newEncryptedString.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(newEncryptedString.Substring(x, 2), 16))
                .ToArray();

        byte[] receivedAESIV = Enumerable.Range(0, newAESIV.Length)
        .Where(x => x % 2 == 0)
        .Select(x => Convert.ToByte(newAESIV.Substring(x, 2), 16))
        .ToArray();


        using (Aes newAes = Aes.Create())
        {
            string newkeyValue = keyValue;

            // Convert the key value from a hexadecimal string to a byte array
            byte[] newkeybuytes = System.Text.Encoding.UTF8.GetBytes(newkeyValue);
            if (newkeybuytes.Length != 32)
            {
                // Pad or truncate the key to 32 bytes
                Array.Resize(ref newkeybuytes, 32);
            }

            newAes.Key = newkeybuytes;
            newAes.IV = receivedAESIV;
            decryptedMessage = DecryptStringFromBytes_Aes(receivedEncryptedMessage, newAes.Key, newAes.IV);

        }

        return decryptedMessage;
    }



    public async void PostMessageToLedger(string schemaName, string attribute, int version, string AESIV)
    {
        try
        {
            //string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
            string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
            string dateTimeSec = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            int chunkSize = attribute.Length / 5;
            string chunk1 = attribute.Substring(0, chunkSize);
            string chunk2 = attribute.Substring(chunkSize, chunkSize);
            string chunk3 = attribute.Substring(2 * chunkSize, chunkSize);
            string chunk4 = attribute.Substring(3 * chunkSize, chunkSize);
            string chunk5 = attribute.Substring(4 * chunkSize);

            using (HttpClient httpClient = new HttpClient())
            {

                // Prepare the JSON payload
                string jsonPayload = $@"{{
                ""attributes"": [
                    ""1.{chunk1}"",  
                    ""2.{chunk2}"",
                    ""3.{chunk3}"",
                    ""4.{chunk4}"",
                    ""5.{chunk5}"",
                    ""0.{AESIV}""
                ],
                ""schema_name"": ""{schemaName}.{dateTimeSec.GetHashCode()}"",
                ""schema_version"": ""{version}.0""
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
        catch (Exception e)
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Failed to post message. Check if ACA-py has loaded.";
        }


    }

    // Function to convert a hexadecimal string to a byte array
    static byte[] HexStringToByteArray(string hex)
    {
        hex = hex.Replace("-", ""); // Remove any dashes if present
        int length = hex.Length / 2;
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        if (bytes.Length != 32)
        {
            // Pad or truncate the key to 32 bytes
            Array.Resize(ref bytes, 32);
        }
        return bytes;
    }


    public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");
        byte[] encrypted;


        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
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
        return encrypted;
    }

    static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {

        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        string plaintext = null;

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }

    public void OpenConnectionsPanel()
    {
        ConnectionsPanel.SetActive(true);
        MessagesPanel.SetActive(false);
    }


}
