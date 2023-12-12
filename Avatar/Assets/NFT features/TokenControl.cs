using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Xml;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class TokenControl : MonoBehaviour
{

    [SerializeField] private TMP_InputField tokenNameInputField;
    [SerializeField] private TMP_InputField tokenDescriptionInputField;
    [SerializeField] private TMP_InputField tokenReceiverInputField;
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    public string IPAddress;
    public string ledgerUrl;
    public static TokenControl instance;
    private static readonly HttpClient client = new HttpClient();
    private static Dictionary<string, string> tokenDataMap = new Dictionary<string, string>();
    private static Dictionary<string, string> tokenOwners = new Dictionary<string, string>();
    public  List<string> myTokenList = new List<string>();
 
    [SerializeField] private TMP_Text MyTokens;
    [SerializeField] private TMP_Text AllTokenData;

    // Start is called before the first frame update
    void Start()
    {
        IPAddress = userdatapersist.Instance.IPAdd;
        ledgerUrl = "http://" + IPAddress + ":9000";
        instance = this;
        ShowTokenData();
        InvokeRepeating("ShowTokenData", 10.0f, 10.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void TransferToken()
    {
        string tokenID = tokenNameInputField.text;
        string receiverName = tokenReceiverInputField.text;
        if (!myTokenList.Contains(tokenID))
        {
            popupWindow.SetActive(true);
            windowMessage.text = "You do not own token with id: " + tokenID;
        }
        else
        {
            TokenLedgerTransfer(tokenID,  receiverName);
            ShowTokenData();
        }
    }

    public async void TokenLedgerTransfer(string tokenID, string receiverName)
    {
        string tokenDesc = tokenDataMap[tokenID];
        try
        {
            string dateTimeSec = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";


            using (HttpClient httpClient = new HttpClient())
            {

                // Prepare the JSON payload
                string jsonPayload = $@"{{
                ""attributes"": [
                    ""{tokenDesc}""  
                    
                ],
                ""schema_name"": ""{tokenID}.{receiverName.GetHashCode()}.{dateTimeSec.GetHashCode()}"",
                ""schema_version"": ""6.1""
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
                    windowMessage.text = "Transferred token!";
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
            windowMessage.text = "Failed to post to ledger. Check if ACA-py has loaded.";
        }
    }

    public void GetMyTokens()
    {
        foreach (var kvp in tokenOwners)
        {
            if (kvp.Value == userdatapersist.Instance.verifiedUser.GetHashCode().ToString())
            {
                myTokenList.Add(kvp.Key);
            }
        }
    }

    public void GetTokenDataFromLedger()
    {

        string transactionsUrl = $"{ledgerUrl}/ledger/domain?query=&type=101"; // Specify the transaction type as "101" for schemas
        HttpResponseMessage response = client.GetAsync(transactionsUrl).Result;

        if (response.IsSuccessStatusCode)
        {
            string responseBody = response.Content.ReadAsStringAsync().Result;
            var transactions = JToken.Parse(responseBody)["results"];
            tokenDataMap.Clear();
            tokenOwners.Clear();
            myTokenList.Clear();
            foreach (var transaction in transactions)
            {
                ParseToTokenData(transaction, "");
            }

        }
        else
        {
            Debug.Log("Error retrieving transactions!");
        }
        GetMyTokens();
    }

    public void ShowTokenData()
    {
        GetTokenDataFromLedger();
        ParseDataUI();
    }



    public void ParseDataUI()
    {
        string myTokens = "";
        string allTokenData = "";
        int i = 0;
        int j = 0;

        foreach (string token in myTokenList)
        {
            string tokenDesc = tokenDataMap[token];
            i++;
            myTokens = myTokens + i + ". " + token + ": " + tokenDesc + " \n";
        }


        foreach (var kvp in tokenOwners)
        {
            string tokenId = kvp.Key;
            string ownerHash = kvp.Value;
            string tokenDesc = tokenDataMap[tokenId];
            j++;
            allTokenData = allTokenData + j + ". " + tokenId + " (" + tokenDesc +"):  "+ ownerHash + " \n";
        }

        if (i == 0)
        {
            myTokens = "None found";
        }

        if (j == 0)
        {
            allTokenData = "None found";
        }

        MyTokens.text = myTokens;
        AllTokenData.text = allTokenData;

    }

    private static void ParseToTokenData(JToken transaction, string keyWordSearch)
    {

        var responseData = transaction["txn"]["data"]["data"];
        var type = responseData["version"];
        var credName = responseData["name"];
        var attributes = responseData["attr_names"][0];

        try
        {
            if (type.ToString() == "6.0") // found a token
            {
                string tokenID = credName.ToString();
                string tokenData = attributes.ToString();
                tokenDataMap.Add(tokenID, tokenData);
            }


            else if (type.ToString() == "6.1")
            {
                string[] parts = credName.ToString().Split('.');
                string tokenID = parts[0];
                string ownwerHash = parts[1];
                if (tokenOwners.ContainsKey(tokenID))
                {
                    tokenOwners.Remove(tokenID);
                }

                tokenOwners.Add(tokenID, ownwerHash);

            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        

    }

}



