using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Renci.SshNet;
using System.Threading;
using System.Threading.Tasks;
using System;

public class SSHScript : MonoBehaviour
{
    public void SSHLogin()
    {

        string gitBashPath = @"C:\Program Files\Git\bin\bash.exe"; // Adjust the path to your Git Bash executable
        string workingDirectory = @"D:\network images\modified aca-py\demo"; // Specify your working directory
        string gitCommand = "./run user3";
        gitCommand = gitCommand.Replace("\"", "\"\"");

        SshClient sshclient = new SshClient("10.255.252.138", "sshuser", "D5taCard");

        try
        {
            sshclient.Connect();

            if (!sshclient.IsConnected)
            {
                Console.WriteLine("Not connected...");
                sshclient.Connect();
            }
            else
            {
                Console.WriteLine("Connected to Host!");
            }

            SshCommand sc = sshclient.CreateCommand("cd /");
            sc.Execute();
            sc = sshclient.CreateCommand("cd D ");
            sc.Execute();
            sc = sshclient.CreateCommand("cd network\\ images/");
            sc.Execute();
            sc = sshclient.CreateCommand("cd modified\\ aca-py/ ");
            sc.Execute();
            sc = sshclient.CreateCommand("cd demo");
            sc.Execute();
            sc = sshclient.CreateCommand("./run user3");
            sc.Execute();
            string answer = sc.Result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        

        //AuthenticationMethod method = new PasswordAuthenticationMethod("Yong Rui Chong", "694218");
       // ConnectionInfo conenction = new ConnectionInfo("192.168.0.102", "Yong Rui Chong", method);
       // var Client = new SshClient(conenction);

        

        //var readCommand = Client.RunCommand("uname -mrs");
        //Console.WriteLine(readCommand.Result);
        //var writeCommand = Client.RunCommand(gitCommand);
        //Thread.Sleep(1000);

    }
    
}
