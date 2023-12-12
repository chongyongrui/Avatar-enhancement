using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class AdminTokenControl : MonoBehaviour
{

    [SerializeField] private GameObject MintButton;
    [SerializeField] private GameObject DescriptionHeader;
    [SerializeField] private GameObject DescriptionInput;
    [SerializeField] private TMP_InputField tokenNameInputField;
    [SerializeField] private TMP_InputField tokenDescriptionInputField;
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    [SerializeField] private TMP_Text issuerName;
    public string IPAddress;
    public string ledgerUrl;
    public static AdminTokenControl instance;
    public string username;


    // Start is called before the first frame update
    void Start()
    {
        if (userdatapersist.Instance.verifiedUser != "admin")
        {
            MintButton.SetActive(false);
            DescriptionHeader.SetActive(false);
            DescriptionInput.SetActive(false);
        }


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
    }



    public void MintToken()
    {
        string tokenID = tokenNameInputField.text;
        string tokenDescription = tokenDescriptionInputField.text;
        MintTokenToLedger(tokenID, tokenDescription);

        //TokenControl.instance.GetTokenDataFromLedger();
    }


    public async void CreateTokenOwnsership(string tokenName, string tokenDesc)
    {
        try
        {

            string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";


            using (HttpClient httpClient = new HttpClient())
            {

                // Prepare the JSON payload
                string jsonPayload = $@"{{
                ""attributes"": [
                    ""{tokenDesc}""  
                    
                ],
                ""schema_name"": ""{tokenName}.{username.GetHashCode()}"",
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
                    windowMessage.text = "Minted to ledger!";
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

    public async void MintTokenToLedger(string tokenName, string tokenDesc)
    {
        try
        {

            string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";


            using (HttpClient httpClient = new HttpClient())
            {

                // Prepare the JSON payload
                string jsonPayload = $@"{{
                ""attributes"": [
                    ""{tokenDesc}""  
                    
                ],
                ""schema_name"": ""{tokenName}"",
                ""schema_version"": ""6.0""
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
                    CreateTokenOwnsership(tokenName, tokenDesc);
                    //popupWindow.SetActive(true);
                    //windowMessage.text = "Minted to ledger!";
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
            windowMessage.text = "Failed to mint to ledger. Check if ACA-py has loaded.";
        }
    }

    public void OpenMessagePanel()
    {
        SceneManager.LoadSceneAsync("Messaging");
    }

    public void OpenAdminScene()
    {
        SceneManager.LoadSceneAsync("Admin Panel");
    }
}
