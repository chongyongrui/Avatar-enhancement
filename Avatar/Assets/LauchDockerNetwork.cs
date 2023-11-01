using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LauchDockerNetwork : MonoBehaviour
{
    public void runDocker()
    {


        StartSQLDockerInstanceAsync();
        
        
    }

    public async void StartSQLDockerInstanceAsync()
    {
        //for unity player:
        string composeFilePath = "../../Assets/Main game/Scripts/Database Scripts/";


        //FOR BUILD:
        //string assetsPath = Path.Combine(Application.dataPath, "..", "..");
        //string composeFilePath = assetsPath+"/Avatar/Assets/Main game/Scripts/Database Scripts/";
        UnityEngine.Debug.Log(composeFilePath);
        UnityEngine.Debug.Log("Starting PostgreSQL Server Docker instance now");
        await RunDockerComposeAsync(composeFilePath);
        UnityEngine.Debug.Log("Docker Compose completed.");
        // RunScriptInDirectory(directoryPath, scriptCommand, arguments);
    }

    public async Task RunDockerComposeAsync(string composeFilePath)
    {
        Process process = new Process();

        try
        {
            string composeFile = Path.GetDirectoryName(composeFilePath);
            string currentScriptPath = Assembly.GetExecutingAssembly().Location; // Get the current script file path
            string currentScriptDirectory = Path.GetDirectoryName(currentScriptPath); // Get the directory path of the current script
            string composeFileFullPath = Path.Combine(currentScriptDirectory, composeFile); // Combine the current script directory with the relative compose file path

            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = composeFileFullPath; // Set the working directory to the current script directory
            process.StartInfo.Arguments = $"/k docker-compose up"; // Specify the compose file and command
            UnityEngine.Debug.Log("Directory of process.StartInfo.Arguments: " + process.StartInfo.Arguments);


            process.StartInfo.UseShellExecute = true;

            process.EnableRaisingEvents = true;
            process.Start();

            bool processStarted = await Task.Run(() => process.WaitForExit(Timeout.Infinite));
            UnityEngine.Debug.Log("Process started: " + processStarted);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running docker-compose: {ex.Message}");
        }
        finally
        {
            process.Close();
            // process.Dispose();
        }
    }
}
