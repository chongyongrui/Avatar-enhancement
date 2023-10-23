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
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Agreement;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1.Cms;
using System.ComponentModel.Design;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;
using UnityEngine.Windows;
using System.Linq;
using Org.BouncyCastle.Utilities.IO.Pem;
using Npgsql;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Unity.Collections.LowLevel.Unsafe;

public class DHMessager : MonoBehaviour
{

    public static DHMessager instance;
    public string username;
    public static byte[] userPublicKey;
    [SerializeField] public TMP_InputField ReceiverNameInputField;
    //[SerializeField] public TMP_InputField MessageString;
    public string ReceiverName;
    public string IPAddress;
    public string ledgerUrl;
    [SerializeField] GameObject popupWindow;
    [SerializeField] GameObject ConnectionsPanel;
    [SerializeField] GameObject MessagesPanel; 
    [SerializeField] TMP_Text windowMessage;
    [SerializeField] TMP_Text receivedInvites;
    [SerializeField] TMP_Text acceptedInvites;
    [SerializeField] TMP_Text connections;
    [SerializeField] TMP_Text issuerName;
    private static readonly HttpClient client = new HttpClient();
    public static byte[] alicePublicKey;
    public string message;
    public string hashedReceiverUserName;
    private DHParameters dhParameters;
    private AsymmetricCipherKeyPair keyPair;
    public static DHParameters dHParameters;
    public static Dictionary<string, int> addressBook = new Dictionary<string, int>();




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
        issuerName.text = username;
        UpdateInvtersInvitees();
        InvokeRepeating("UpdateInvtersInvitees", 10.0f, 5.0f);

        
    }


    public void AShareParams()
    {

        try
        {
            hashedReceiverUserName = ReceiverNameInputField.text.ToString();

            // to-do: check if such a connection is pending or not


            var generator = new DHParametersGenerator();
            generator.Init(512, 10, new SecureRandom());
            DHParameters param = generator.GenerateParameters();
            PostDHParametersAsString(param, hashedReceiverUserName + "-" + username + ".params");
            // Generate key pair for Party A
            var keyGen1 = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp1 = new DHKeyGenerationParameters(new SecureRandom(), param);
            keyGen1.Init(kgp1);
            AsymmetricCipherKeyPair A = keyGen1.GenerateKeyPair();
            string stringDHStaticKeyPairPartyA = GetPublicKey(A);
            Debug.Log("person A's public key is " + stringDHStaticKeyPairPartyA);
            PostStaticPublicKey(stringDHStaticKeyPairPartyA, hashedReceiverUserName + "-" + username + ".A");  //post public static key

            //save private key to wallet for this communication link
            string privateKey = GetPrivateKey(A);
            AddDHPrivateKeySQL(hashedReceiverUserName, privateKey);
            UpdateInvtersInvitees();
            popupWindow.SetActive(true);
            windowMessage.text = "Successfully send invitation!" ;
        }
        catch (Exception e)
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Failed to send invitation" + e;
        }
        


    }

    public void AddDHPrivateKeySQL(string receiver, string key)
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
                    command.CommandText = "INSERT into DH_Private_Keys VALUES ('" + receiver + "'," + key + ");";
                    command.ExecuteNonQuery();
                    Debug.Log("(SQL server) DH_Private_Keys added with id: " + receiver + " with key val  " + key);
                }
                connection.Close();


            }
        }
        catch (Exception e)
        {
            Debug.Log("(SQL Server) Error adding private key " + e);
        }
    }

    public string GetDHPrivateKeySQL(string receiver) {

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
                    command.CommandText = "SELECT * FROM dh_private_keys WHERE receiver_hash = '" + receiver + "';";
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

    public void UpdateInvtersInvitees()
    {
        
        GetEstablishedConnections(addressBook);  //value is 3
        GetDHAcceptedInvites(addressBook,username, ledgerUrl);  //value is 2
        GetDHSentInvites(addressBook,username, ledgerUrl);  //value is 0
        GetDHInvites(addressBook,username, ledgerUrl); //value is 1
       
        //parse hash table into the different strings lists
        int i = 1;
        int j = 1;
        int k = 1;


        string receivedInvitations = "";
        string acceptedInviteesResult = "";
        string sentInvitesResult = "";

        foreach (string key in addressBook.Keys)
        {
            int value = addressBook[key];
            if (value == 3)
            {
                sentInvitesResult += ("\n" + i + ". " + key + "\n");
                i++;
            }
            else if (value == 2)
            {
                acceptedInviteesResult += ("\n" + j + ". " + key + "\n");
                j++;
            }else if (value == 1)
            {
                receivedInvitations += ("\n" + k + ". " + key + "\n");
                k++;
            }else if (value == 0)
            {
                sentInvitesResult += ("\n" + i + ". (pending) " + key + "\n");
                i++;
            }
        }

        if (i == 1)
        {
            sentInvitesResult += "none found";
        }
        if (j == 1)
        {
            acceptedInviteesResult += "none found";
        }
        if (k == 1)
        {
            receivedInvitations += "none found";
        }


        receivedInvites.text = receivedInvitations;
        acceptedInvites.text = acceptedInviteesResult;
        connections.text = sentInvitesResult;

    }

    public void GetEstablishedConnections(Dictionary<string,int>addressBook)
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
                    command.CommandText = "SELECT * FROM AES_keys;";
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            Debug.Log("(SQL server) Coonection found with id " + reader["receiver_hash"]);

                            addressBook.Add(reader["receiver_hash"].ToString(), 3);
                               
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

       
    }

    public void BGetParamsAndCalcSecret()
    {

        try
        {
            string hashedInviterUserName = ReceiverNameInputField.text.ToString();
            DHParameters foundDHParams = GetDHParams(username + "-" + hashedInviterUserName + ".params", ledgerUrl);
            var keyGen2 = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp2 = new DHKeyGenerationParameters(new SecureRandom(), foundDHParams);
            keyGen2.Init(kgp2);
            AsymmetricCipherKeyPair B = keyGen2.GenerateKeyPair();

            string StaticKeyString = GetStaticKeyString(username + "-" + hashedInviterUserName + ".A", ledgerUrl);
            Debug.Log("Found StaticKeyString of A by B is " + StaticKeyString);

            // B calc
            var importedKey = new DHPublicKeyParameters(new BigInteger(StaticKeyString), foundDHParams);
            var internalKeyAgreeB = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgreeB.Init(B.Private);
            string stringDHStaticKeyPairPartyB = GetPublicKey(B);
            Debug.Log("person B's public key is " + stringDHStaticKeyPairPartyB);
            PostStaticPublicKey(stringDHStaticKeyPairPartyB, username + "-" + hashedInviterUserName + ".B");  //post public static key
            BigInteger Bans = internalKeyAgreeB.CalculateAgreement(importedKey);
            Debug.Log("B ans is " + Bans.ToString());

            //add to local wallet
            AddAESKeyToWallet(hashedInviterUserName, Bans);
            UpdateInvtersInvitees();
            popupWindow.SetActive(true);
            windowMessage.text = "Successfully confirmed connection!";
        }
        catch( Exception e)
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Failed to confirm invitation" + e;
        }
        


    }

    public void ACalculateSecret()
    {

        try
        {
            hashedReceiverUserName = ReceiverNameInputField.text.ToString();
            DHParameters foundDHParams = GetDHParams(hashedReceiverUserName + "-" + username + ".params", ledgerUrl);
            string StaticKeyString = GetStaticKeyString(hashedReceiverUserName + "-" + username + ".B", ledgerUrl);
            Debug.Log("Found StaticKeyString of B by A is " + StaticKeyString);
            var importedKeyA = new DHPublicKeyParameters(new BigInteger(StaticKeyString), foundDHParams);
            var internalKeyAgreeA = AgreementUtilities.GetBasicAgreement("DH");
            AsymmetricCipherKeyPair A = GetKeyPairFromPrivateKeyString(foundDHParams, GetDHPrivateKeySQL(hashedReceiverUserName));
            internalKeyAgreeA.Init(A.Private);
            BigInteger Aans = internalKeyAgreeA.CalculateAgreement(importedKeyA);
            Debug.Log("A ans is " + Aans.ToString());

            //add to local wallet 
            AddAESKeyToWallet(hashedReceiverUserName, Aans);
            UpdateInvtersInvitees();
            popupWindow.SetActive(true);
            windowMessage.text = "Successfully established connection!";
        }
        catch (Exception e )
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Failed to establish invitation" + e;
        }
        

    }

    public void AddAESKeyToWallet(string connectionName, BigInteger keyVal)
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
                    command.CommandText = "INSERT into AES_Keys VALUES ('" + connectionName+ "'," + keyVal +");";
                    command.ExecuteNonQuery();
                    Debug.Log("(SQL server) AESKey added with id: " + connectionName + " with key val  " + keyVal);
                }
                connection.Close();


            }
        }
        catch (Exception e)
        {
            Debug.Log("(SQL Server) Error adding key " + e);
        }





    }

    // This returns A
    public string GetPublicKey(AsymmetricCipherKeyPair keyPair)
    {
        var dhPublicKeyParameters = keyPair.Public as DHPublicKeyParameters;
        if (dhPublicKeyParameters != null)
        {
            return dhPublicKeyParameters.Y.ToString();
        }
        throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
    }

    // This returns a
    public string GetPrivateKey(AsymmetricCipherKeyPair keyPair)
    {
        var dhPrivateKeyParameters = keyPair.Private as DHPrivateKeyParameters;
        if (dhPrivateKeyParameters != null)
        {
            return dhPrivateKeyParameters.X.ToString();
        }
        throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
    }

    public AsymmetricCipherKeyPair GetKeyPairFromPrivateKeyString(DHParameters P, string privateKeyString){
    try
    {
        // Parse the string and convert it to a BigInteger
        BigInteger xValue = new BigInteger(privateKeyString);

        // Create DH key parameters
        var dhParameters = P; // Replace with your actual DH parameters

        // Create DH private key parameters
        var dhPrivateKeyParameters = new DHPrivateKeyParameters(xValue, dhParameters);

        // Create a DH public key from the private key
        var dhPublicKeyParameters = new DHPublicKeyParameters(dhPrivateKeyParameters.X.ModPow(dhParameters.G, dhParameters.P), dhParameters);

        // Create an AsymmetricCipherKeyPair
        var keyPair = new AsymmetricCipherKeyPair(dhPublicKeyParameters, dhPrivateKeyParameters);

        return keyPair;
    }
    catch (Exception ex)
    {
        // Handle any parsing or conversion errors
        throw new ArgumentException("Invalid private key string", ex);
    }
}


    public static string PublicKeyToString(AsymmetricKeyParameter publicKey)
    {
        SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
        byte[] publicKeyBytes = publicKeyInfo.GetDerEncoded();

        // Convert the public key bytes to a Base64-encoded string
        string publicKeyString = Convert.ToBase64String(publicKeyBytes);

        return publicKeyString;
    }

    public static AsymmetricKeyParameter PublicStaticKeyFromString(string publicKeyBase64)
    {
        try
        {
            // Decode the Base64 string to get the byte array
            byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);

            // Parse the byte array as a SubjectPublicKeyInfo structure
            Asn1InputStream asn1Stream = new Asn1InputStream(publicKeyBytes);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfo.GetInstance(asn1Stream.ReadObject());

            // Extract the public key parameters
            AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(publicKeyInfo);

            return publicKey;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating public key from string: {ex.Message}");
            return null;
        }
    }

    

    public async void PostStaticPublicKey(string input, string schemaName)
    {
        try
        {
            //string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
            string url = "http://" + IPAddress + ":11001/schemas?create_transaction_for_endorser=false";



            List<string> result = new List<string>();

            for (int i = 0; i < input.Length; i += 250)
            {
                int length = Math.Min(250, input.Length - i);
                result.Add(input.Substring(i, length));
            }

            if (result.Count == 1)
            {
                Debug.Log("Count is 1, " + result[0]);
                using (HttpClient httpClient = new HttpClient())
                {

                    // Prepare the JSON payload
                    string jsonPayload = $@"{{
                ""attributes"": [
                    ""1.{result[0]}""      
                    
                   
                ],
                ""schema_name"": ""{schemaName}"",
                ""schema_version"": ""3.2""
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
                    }
                    else
                    {
                        Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                    }
                }
            }

        } catch(Exception ex)
        {
            Debug.Log(ex.Message);
            popupWindow.SetActive(true);
            windowMessage.text = "Failed to post to ledger! Ensure ACA-Py agent has loaded!";
        }
        
    }



    public void GetDHInvites(Dictionary<string, int> addressBook,string username, string ledgerUrl)
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
                    var attributes = responseData["attr_names"];

                    if (type.ToString() == "3.1" ) // found a DH paramter that is to connect to you
                    {
                        string target = credName.ToString();
                        string[] words = target.Split("-");
                        string sender = words[1].Split(".")[0];
                        string stage = words[1].Split(".")[1];
                        string receiver = words[0];
                        
                        if (receiver == username)
                        {
                            string[] vals = words[1].Split(".");
                            if (!addressBook.ContainsKey(vals[0]))
                            {
                                addressBook.Add(vals[0], 1);
                            }
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
            popupWindow.SetActive(true);
            windowMessage.text = "Error parsing transactions!";
        }

    }


    public void GetDHAcceptedInvites(Dictionary<string, int> addressBook, string username, string ledgerUrl)
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
                    var attributes = responseData["attr_names"];

                    if (type.ToString() == "3.2") // found a DH paramter that is to connect to you
                    {

                        string target = credName.ToString();
                        string[] words = target.Split("-");
                        string sender = words[1].Split(".")[0];
                        string stage = words[1].Split(".")[1];
                        string receiver = words[0];
                        Debug.Log("finding accepted invites sender and receiver of params is " + sender + " to " + receiver + "in state " + stage);
                        if (sender == username && stage == "B")
                        {
                            if (!addressBook.ContainsKey(words[0]))
                            {
                                addressBook.Add(words[0], 2);
                            }
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
            popupWindow.SetActive(true);
            windowMessage.text = "Error parsing transactions!";
        }

        

    }

    public void GetDHSentInvites(Dictionary<string, int> addressBook, string username, string ledgerUrl)
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
                    var attributes = responseData["attr_names"];

                    if (type.ToString() == "3.1") // found a DH paramter that is to connect to you
                    {
                        string target = credName.ToString();
                        string[] words = target.Split("-");
                        string sender = words[1].Split(".")[0];
                        string receiver = words[0];
                        Debug.Log("finding sent invites sender and receiver of params is " + sender + " to " + receiver);
                        if (sender == username)
                        {
                            if (!addressBook.ContainsKey(words[0]))
                            {
                                addressBook.Add(words[0], 0);
                            }
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
            popupWindow.SetActive(true);
            windowMessage.text = "Error parsing transactions!";
        }

       
    }

    public DHParameters GetDHParams(string username, string ledgerUrl)
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
                    var attributes = responseData["attr_names"];

                    if (type.ToString() == "3.1" && credName.ToString() == username) // found a DH paramter that is to connect to you
                    {
                        Debug.Log("The found attributes are " + attributes.ToString());
                        DHParameters foundDHParams = ExtractDHParameters(attributes.ToString());
                        return foundDHParams;
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

        return null;

    }


    public string GetStaticKeyString(string username, string ledgerUrl)
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
                    var attributes = responseData["attr_names"];

                    if (type.ToString() == "3.2" && credName.ToString() == username)
                    {
                        Debug.Log("The found static attributes 1 are " + attributes[0].ToString());
                        string[] stringsToSort = { attributes[0].ToString() };

                        // Sort the strings by their first character
                        var sortedStrings = stringsToSort.OrderBy(str => str[0]);
                        string foundKey = "";
                        foreach (var str in sortedStrings)
                        {
                            string cutString = str.Substring(2);
                            foundKey += cutString;
                        }
                        Debug.Log("Found static key = " + foundKey);

                        return foundKey;
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

        return null;

    }


    public static DHParameters ExtractDHParameters(string input)
    {
        // Define regular expressions to match each parameter
        string pPattern = @"P:(\d+)";
        string gPattern = @"G:(\d+)";
        string qPattern = @"Q:(\d+)";
        string mPattern = @"M:(\d+)";
        string lPattern = @"L:(\d+)";
        string jPattern = @"J:(\d+)";

        // Match each parameter using the regular expressions
        Match pMatch = Regex.Match(input, pPattern);
        Match gMatch = Regex.Match(input, gPattern);
        Match qMatch = Regex.Match(input, qPattern);
        Match mMatch = Regex.Match(input, mPattern);
        Match lMatch = Regex.Match(input, lPattern);
        Match jMatch = Regex.Match(input, jPattern);



        BigInteger p = pMatch.Success ? new BigInteger(pMatch.Groups[1].Value) : BigInteger.Zero;
        BigInteger g = gMatch.Success ? new BigInteger(gMatch.Groups[1].Value) : BigInteger.Zero;
        BigInteger q = qMatch.Success ? new BigInteger(qMatch.Groups[1].Value) : BigInteger.Zero;
        int m = mMatch.Success ? int.Parse(mMatch.Groups[1].Value) : 0;
        int l = lMatch.Success ? int.Parse(lMatch.Groups[1].Value) : 0;
        BigInteger j = jMatch.Success ? new BigInteger(jMatch.Groups[1].Value) : BigInteger.Zero;

        Debug.Log("P: " + p.ToString() + "G: " + g.ToString() + " Q: " + q.ToString() + "J: " + j.ToString()
            + "M: " + m.ToString() + "L: " + l.ToString());

        return new DHParameters(p, g, q, m, l, j, null); // Adjust as needed

    }

    public async void PostDHParametersAsString(DHParameters dhParams, string schemaName)
    {
        if (dhParams == null)
        {
            throw new ArgumentNullException(nameof(dhParams));
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("DH Parameters:");
        sb.AppendLine($"P (Prime Modulus): {dhParams.P}");
        sb.AppendLine($"G (Generator): {dhParams.G}");
        sb.AppendLine($"Q (Factor): {dhParams.Q}");
        sb.AppendLine($"J (Subgroup Factor): {dhParams.J}");
        sb.AppendLine($"M: {dhParams.M}");
        sb.AppendLine($"L: {dhParams.L}");

        Debug.Log(sb.ToString());

        //string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
        string url = "http://" + IPAddress + ":11001/schemas?create_transaction_for_endorser=false";
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {

                // Prepare the JSON payload
                string jsonPayload = $@"{{
                ""attributes"": [
                    ""P:{dhParams.P}"", 
                    ""G:{dhParams.G}"",
                    ""Q:{dhParams.Q}"",
                    ""J:{dhParams.J}"",
                    ""M:{dhParams.M}"",
                    ""L:{dhParams.L}""
                ],
                ""schema_name"": ""{schemaName}"",
                ""schema_version"": ""3.1""
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
                    windowMessage.text = "Posted DH parameters to ledger!";


                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }
        }catch (Exception ex) { }
        


    }

    public void OpenMessagePanel()
    {
        ConnectionsPanel.SetActive(false);
        MessagesPanel.SetActive(true);
    }




}
