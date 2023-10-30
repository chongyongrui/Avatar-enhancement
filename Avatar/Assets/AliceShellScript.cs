using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;


public class AliceShellScript : MonoBehaviour
{

    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    


    public void Run()
    {

        try
        {
            string gitBashPath = @"C:\Program Files\Git\bin\bash.exe"; // Adjust the path to your Git Bash executable
            string workingDirectory = @"D:\network images\modified aca-py\demo"; // Specify your working directory
            string gitCommand = "./run user3";
            gitCommand = gitCommand.Replace("\"", "\"\"");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = gitBashPath,
                Arguments = gitCommand,
                WorkingDirectory = workingDirectory,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            // Start the process
            Process process = new Process { StartInfo = psi };
            process.Start();
            Console.WriteLine("Process started");

            // Send the Git Bash command to the process
            process.StandardInput.WriteLine(gitCommand);
            Console.WriteLine("Process started");
            windowMessage.text = "Running ACA-Py instance now...";
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log("ERROR RUNNING BASH   " + ex);
            popupWindow.SetActive(true);
            windowMessage.text = "Error running ACA-Py instance!";
        }
    }

    
}
