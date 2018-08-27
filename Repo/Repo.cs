//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// Repo.cs:        Acts as a mock repository to send build requests //
//                 to the Mother Builder and test code files to the //
//                 to the Child Builders                            //
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
//         - Acts as a Mock Respository to send Build Requests to the Mother Builder
//         - Recevies build logs and test logs from child builders and test harness
//         - Displays the storage area for the Client
//         - Sends the required files to the child builders on request
//         - Communicates with other servers via WCF
// Public Interface:
// =================
//         - public void sendBR(string repoAddr, string mbldrAddr)
//             - A simple function which hard codes the test code files into the message arguments and posts the message to Mother Builder
// Required Files:
// ===============
//         - Repo.cs
// Build Command:
// ==============
//         - csc Repo.cs
//         - devenv Repo.csproj
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using MessagePassing;
//using MotherBuilder;
using System.IO;
using Navigator;
using BuildRequest;

namespace Repo
{
    // Repo class derived from the common communicator
    public class Repo : Communicator
    {
        private static string repoPort { get; set; } = "http://localhost:8082/IMessagePassingComm";
        private static string motherBuilderPort { get; set; } = "http://localhost:8081/IMessagePassingComm";
        private static string clientPort { get; set; } = "http://localhost:8083/IMessagePassingComm";

        IFileMgr localFileMgr { get; set; } = null;
        Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher =
      new Dictionary<string, Func<CommMessage, CommMessage>>();

        // Constructor
        //      Initializes the comm object and starts the message handling thread
        public Repo(string baseAddress, int port)
        {
            commObj = new MessagePassing.Comm(baseAddress, port);
            base.messageHandlerThread.Start();
            //initializeEnvironment();
            localFileMgr = FileMgrFactory.create(FileMgrType.Local);
            //initializeDispatcher();
        }

        // Gets the list of the code files from the XML file in the XML storage path
        private List<string> getArguments(string xmlFile)
        {
            BuildRequest.BuildRequest br = new BuildRequest.BuildRequest();
            List<string> codeFiles = new List<string>();
            string fileSpec = System.IO.Path.Combine("../../RepoFileStorage/XML", xmlFile);
            fileSpec = System.IO.Path.GetFullPath(fileSpec);
            br.loadXml(fileSpec);
            br.parse("testDriver");
            codeFiles.Add(br.testDriver);
            br.parseList("tested");
            foreach (string file in br.testedFiles)
            {
                codeFiles.Add(file);
            }

            return codeFiles;
        }

        // Handling message of msg.command = "getXmlFiles"
        private void getXmlFiles(CommMessage msg)
        {
            localFileMgr.currentPath = "XML";
            CommMessage sendMsg1 = new CommMessage(CommMessage.MessageType.reply);
            sendMsg1.to = msg.from;
            sendMsg1.from = msg.to;
            sendMsg1.command = "getXmlFiles";
            sendMsg1.arguments = localFileMgr.getFiles().ToList<string>();
            commObj.postMessage(sendMsg1);
            Thread.Sleep(1000);
        }

        // Handling message of msg.command = "getTopDirs"
        private void getTopDirs(CommMessage msg)
        {
            localFileMgr.currentPath = "";
            CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
            reply.to = msg.from;
            reply.from = msg.to;
            reply.command = "getTopDirs";
            reply.arguments = localFileMgr.getDirs().ToList<string>();
            commObj.postMessage(reply);
            Thread.Sleep(1000);
        }

        // Handling message of msg.command = "getTopFiles"
        private void getTopFiles(CommMessage msg)
        {
            localFileMgr.currentPath = "";
            CommMessage sendMsg2 = new CommMessage(CommMessage.MessageType.reply);
            sendMsg2.to = msg.from;
            sendMsg2.from = msg.to;
            sendMsg2.command = "getTopFiles";
            sendMsg2.arguments = localFileMgr.getFiles().ToList<string>();
            commObj.postMessage(sendMsg2);
            Thread.Sleep(1000);
        }

        // Handling message of msg.command = "sendFiles"
        private void sendFiles(CommMessage msg)
        {
            string from = msg.from;
            msg.from = msg.to;
            msg.to = from;
            msg.command = "filesSent";
            Console.WriteLine("\n **Sending the following files to {0}**", msg.to);
            if (msg.arguments.Count > 0)
                Console.Write("\n      ");
            foreach (string arg in msg.arguments)
                Console.Write("{0} ", arg);
            Console.WriteLine("\n");
            Console.WriteLine("\n Sending the files from: {0} [Repository]", Path.GetFullPath("../../RepoFileStorage"));
            foreach (string arg in msg.arguments)
            {
                commObj.postFile(arg, "../../RepoFileStorage", 1024, msg.to, msg.path);
                Thread.Sleep(1000);
            }
            commObj.postMessage(msg);
            Thread.Sleep(1000);
        }

        // Handling message of msg.command = "buildRequest"
        private void buildRequest(CommMessage msg)
        {
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.to = motherBuilderPort;
            sendMsg.from = repoPort;
            sendMsg.command = "buildRequest";
            sendMsg.author = msg.author;
            sendMsg.arguments = getArguments(msg.arguments[0]);
            commObj.postMessage(sendMsg);
            Thread.Sleep(1000);
        }

        // Handling message of msg.command = "buildRequest"
        private void allBuildRequest(CommMessage msg)
        {
            string[] files = Directory.GetFiles("../../RepoFileStorage/XML", "*.xml");
            foreach (string file in files)
            {
                CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
                sendMsg.to = motherBuilderPort;
                sendMsg.from = repoPort;
                sendMsg.command = "buildRequest";
                sendMsg.author = Path.GetFileNameWithoutExtension(file);
                sendMsg.arguments = getArguments(file);
                Console.WriteLine("\n Sending the following build request:\n");
                sendMsg.show();
                commObj.postMessage(sendMsg);
                Thread.Sleep(1000);
            }
        }

        // Overridden mesage handler function
        public override void messageHandler(CommMessage msg)
        {
            switch (msg.command)
            {
                case "buildRequest":
                    buildRequest(msg);
                    break;
                case "mbldr":
                    Console.WriteLine("\n In Mother Builder message handler\n");
                    break;
                case "sendFiles":
                    sendFiles(msg);
                    break;
                case "getTopFiles":
                    getTopFiles(msg);
                    break;
                case "getTopDirs":
                    getTopDirs(msg);
                    break;
                case "getXmlFiles":
                    getXmlFiles(msg);
                    break;
                case "allBuildRequest":
                    allBuildRequest(msg);
                    break;
            }
        }

        // Main process of Repo
        static void Main(string[] args)
        {
            Console.WriteLine("\n *************************************** \n");
            Console.WriteLine(" Starting Repo at: http://localhost:{0} ", 8082);
            Console.WriteLine("\n *************************************** \n");

            // Starting the Repo at "http://localhost:8082"
            Repo repo = new Repo("http://localhost", 8082);
        }
    }
}
