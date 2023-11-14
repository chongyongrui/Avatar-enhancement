using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Windows;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class testStuff : MonoBehaviour
{
    public int count = 0;
    public static Random rnd = new Random();

    public void test()
    {
        
         sendIssueReqAsync();
            Debug.Log(count);
        
        
        
    }


    public async Task sendIssueReqAsync()
    {

        int num = rnd.Next(99999999);
        string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        try { 

        using (HttpClient httpClient = new HttpClient())
        {

            // Prepare the JSON payload
            string jsonPayload = $@"{{
                ""attributes"": [
                    ""1.asdfasdf"",
                    ""2.asdfasdf""   
                    
                   
                ],
                ""schema_name"": ""lols{count}{num}"",
                ""schema_version"": ""6.9""
            }}";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // Create the request content
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    count++;
                    string responseBody = response.Content.ReadAsStringAsync().Result; // Blocking call
                    Console.WriteLine(responseBody);
                    Debug.Log("posted with count " + count);
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {count}");
                }

                stopwatch.Stop();
                long elapsed_time = stopwatch.ElapsedMilliseconds;
                Console.WriteLine(count + " took " + elapsed_time);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
