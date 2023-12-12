using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEditor.PackageManager;
using UnityEngine;

public class NFTCheck : MonoBehaviour
{
    public string IPAddress;
    public string ledgerUrl;
    private static Dictionary<string, string> tokenDataMap = new Dictionary<string, string>();
    private static Dictionary<string, string> tokenOwners = new Dictionary<string, string>();
    public List<string> myTokenList = new List<string>();
    private static readonly HttpClient client = new HttpClient();
    public static NFTCheck instance; 
    void Start()
    {
        IPAddress = userdatapersist.Instance.IPAdd;
        ledgerUrl = "http://" + IPAddress + ":9000";
        instance = this;
        GetTokenDataFromLedger();
        InvokeRepeating("GetTokenDataFromLedger", 10.0f, 10.0f);
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
