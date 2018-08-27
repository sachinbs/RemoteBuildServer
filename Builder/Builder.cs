//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// Builder.cs:     Process (Builder) spawned by the Mother Builder  //
//                 Has the capability to build files to generate    //
//                 libraries                                        //
// **************************************************************** //
// Version:        1.0                                              //
// Date:           12/06/2017                                       //
// Language:       C#.                                              //
// Platform:       Visual Studio Community Edition                  //
//                 HP ENVY, Windows 10                              //
// Application:    CSE681 - Software Modelling and Analysis         //
// Author:         Sachin Basavani Shivashankara                    //
// SUID:           267871645                                        //
// Ph:, email:     (315)751-5895, sbasavan@syr.edu                  //
//////////////////////////////////////////////////////////////////////
//
// Package Information:
// ====================
//         - Builder process spawned by the Mother Builder (prototype)
//             - Sends "ready" status to the mother builder when ready to process the next build request
//             - Communicates with the Repository to receive the test code files
//             - After receving the files, builds them to generate libraries
//         - Communicates with other servers via WCF
// Public Interface:
// =================
//         - public override void messageHandler(CommMessage msg)
//             - Over-ridden function to handle message processing for the child builder
// Required Files:
// ===============
//         - Builder.cs
// Build Command:
// ==============
//         - csc Builder.cs
//         - devenv Builder.csproj
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release
// Version: 2.0
//         - Added building capablities

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using MessagePassing;
using System.Diagnostics;
using System.IO;

namespace Builder
{
    // Builder derived from the common communicator
    class Builder : Communicator
    {
        private static string builderStorageLocation { get; set; }
        private static string repoLogStorageLocation { get; set; } = "../../../Repo/RepoFileStorage/Logs/BuildLogs";
        private static string testersLocation { get; set; } = "../../../TestHarness/TesterStorage/";
        private static string builderPort { get; set; }
        private static string motherBuilderPort { get; set; } = "http://localhost:8081/IMessagePassingComm";
        private static string repoPort { get; set; } = "http://localhost:8082/IMessagePassingComm";
        private static string clientPort { get; set; } = "http://localhost:8083/IMessagePassingComm";
        private static string testHarnessPort { get; set; } = "http://localhost:8084/IMessagePassingComm";
        //private 
        // Constructor:
        //      Initializes a Comm object
        //      Sends a ready status to the mother builder after spawning
        public Builder(string childProcAddr)
        {
            Console.WriteLine("\n ************************************************ \n");
            Console.WriteLine(" Child Builder starting at: {0} ", childProcAddr);
            Console.WriteLine("\n ************************************************ \n");
            commObj = new MessagePassing.Comm(childProcAddr);
            builderPort = childProcAddr + "/IMessagePassingComm";
            sendReadyStat(childProcAddr);
            createDir(childProcAddr);
        }

        private void createDir(string childProcAddr)
        {
            string port = childProcAddr.Substring(childProcAddr.Length - 4);
            string path = Path.Combine("../../../Builder/BuilderStorage/", port);
            builderStorageLocation = Path.GetFullPath(path);
            Console.WriteLine(builderStorageLocation);
            if (!Directory.Exists(builderStorageLocation))
                Directory.CreateDirectory(builderStorageLocation);
        }

        // Sends ready status to the Mother Builder as soon as the child builder is spawned
        private void sendReadyStat(string childProcAddr)
        {
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.to = "http://localhost:8081/IMessagePassingComm";
            sendMsg.from = childProcAddr + "/IMessagePassingComm";
            sendMsg.command = "ready";
            sendMsg.author = "Sachin at " + childProcAddr;
            commObj.postMessage(sendMsg);
            Thread.Sleep(1000);
        }

        // Builds the files
        private bool buildFiles(CommMessage msg)
        {
            ProcessStartInfo proc = new ProcessStartInfo
            {
                FileName = System.IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "csc.exe"),
                WorkingDirectory = msg.path,
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            string opName = msg.author;
            proc.Arguments = "/target:library /out:" + opName + ".dll";
            foreach (string arg in msg.arguments)
            {
                proc.Arguments += " ";
                proc.Arguments += arg;
            }

            Process p = new Process();
            p.StartInfo = proc;
            p.Start();

            bool buildCheck = buildStatus(msg, opName, p);

            p.WaitForExit();

            return buildCheck;
        }

        private bool buildStatus(CommMessage msg, string opName, Process p)
        {
            // For Logging
            string logs = p.StartTime.ToString() + "\n";
            logs += p.ProcessName + "\n";
            logs += "Files for build:\n";
            foreach (string arg in msg.arguments)
            {
                logs += arg;
                logs += "\n";
            }
            logs += p.StandardOutput.ReadToEnd() + "\n";

            string logFileName = opName + ".log";
            string path = Path.Combine(msg.path, logFileName);

            if (!File.Exists(path))
            {
                File.WriteAllText(path, logs);
            }

            Console.WriteLine("\n\n ****  Build log: {0} generated, Sent to the Repository storage area (in Build Logs Dir)  ****\n\n", logFileName);
            commObj.postFile(logFileName, msg.path, 1024, repoPort, repoLogStorageLocation);
            Thread.Sleep(1000);

            // To check for the status of the build
            string err = "error";
            //StringComparison comp = StringComparison.Ordinal;
            bool test = logs.Contains(err);

            return !test;
        }

        // Handling message of msg.command = "buildRequest"
        private void buildRequest(CommMessage msg)
        {
            Console.WriteLine("\n Recevied the following Build Request\n");
            msg.show();
            msg.command = "sendFiles";
            string from = msg.from;
            msg.from = msg.to;
            msg.to = from;
            string path = Path.Combine(builderStorageLocation, msg.author);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            //Console.WriteLine("\nThe destination path is: {0}\n", path);
            msg.path = path;
            commObj.postMessage(msg);
            Thread.Sleep(1000);
        }

        // Handling message of msg.command = "sendLib"
        private void sendLib(CommMessage msg)
        {
            string fileName = msg.author + ".dll";
            string libPath = Path.Combine(builderStorageLocation, msg.author);
            commObj.postFile(fileName, libPath, 1024, msg.to, msg.path);
            Thread.Sleep(500);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.connect);
            sendMsg.from = msg.to;
            sendMsg.to = msg.from;
            sendMsg.command = "libSent";
            sendMsg.author = msg.author;
            sendMsg.arguments = msg.arguments;
            sendMsg.path = msg.path;
            commObj.postMessage(sendMsg);
            Thread.Sleep(1000);
        }

        // Handling message of msg.command = "filesSent"
        private void filesSent(CommMessage msg)
        {
            string[] files = Directory.GetFiles(msg.path, "*.cs");
            Console.WriteLine("\n **Received the following files from {0}**", msg.from);
            foreach (string file in files)
                Console.WriteLine("\n" + Path.GetFileName(file));
            Console.WriteLine("\n The files are placed in: {0}", msg.path);
            bool testStatus = buildFiles(msg);

            //CommMessage sendMsg = new CommMessage(CommMessage.MessageType.connect);
            //sendMsg.from = builderPort;
            //sendMsg.to = clientPort;
            //sendMsg.author = msg.author;
            //sendMsg.command = "Status";
            if (testStatus)
            {
                //sendMsg.arguments.Add("Build successful");
                //commObj.postMessage(sendMsg);

                CommMessage sendMsg1 = new CommMessage(CommMessage.MessageType.connect);
                sendMsg1.from = builderPort;
                sendMsg1.to = testHarnessPort;
                sendMsg1.author = msg.author;
                sendMsg1.command = "libReady";
                sendMsg1.arguments.Add(msg.author + ".dll");
                commObj.postMessage(sendMsg1);
                Thread.Sleep(1000);
            }
            else
            {
                //sendMsg.arguments.Add("Build failed");
                //commObj.postMessage(sendMsg);
            }

            msg.command = "ready";
            msg.from = msg.to;
            msg.to = motherBuilderPort;
            commObj.postMessage(msg);
            Thread.Sleep(1000);
        }

        // Message handler overridden by the child builder
        public override void messageHandler(CommMessage msg)
        {
            // To close the child builder process
            if (msg.type == CommMessage.MessageType.closeReceiver)     
            {
                Console.WriteLine("\n Closing the Child Builder\n");
                CommMessage sendMsg = new CommMessage(CommMessage.MessageType.closeReceiver);
                commObj.close();
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                switch (msg.command)
                {
                    // If the message received is the build request
                    // Request for the files from the repository
                    case "buildRequest":
                        buildRequest(msg);
                        break;
                    case "repo":        // If the message is "repo"
                        Console.WriteLine("\n You have reached the handling for repo\n");
                        break;
                    case "filesSent":   // If the files are received from the repository
                        filesSent(msg);
                        break;
                    case "sendLib":
                        sendLib(msg);
                        break;
                    default:
                        break;
                }
            }
        }

        // Main of the child builder process
        static void Main(string[] args)
        {
            Builder chldBldr = new Builder(args[0]);
            // Start the message handlig thread for the child builder
            chldBldr.messageHandlerThread.Start();
        }
    }
}
