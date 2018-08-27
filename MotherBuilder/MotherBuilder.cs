//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// MotherBuilder.cs: Spawns the child builders to assist in building//
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
//         - Spawns number of child builders to assist in building files
//         - Maintains ready status queue and build request queue to dispatch the recevied build requests to the child builders which are available
//         - Communicates with other servers via WCF
// Public Interface:
// =================
//         - public override void messageHandler(CommMessage msg)
//             - Over-ridden function to handle message processing for the mother builder
// Required Files:
// ===============
//         - MotherBuilder.cs
// Build Command:
// ==============
//         - csc MotherBuilder.cs
//         - devenv MotherBuilder.csproj
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePassing;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace MotherBuilder
{
    // Mother Builder which derives from the common communicator
    public class MotherBuilder : Communicator
    {
        // Ready Status Queue
        private SWTools.BlockingQueue<string> readyStatusQ = null;
        // Build Request Queue
        private SWTools.BlockingQueue<CommMessage> buildRequestQ = null;
        private string toAddress = "";
        private int numProcess;
        // Holds the list of addresses of child processes
        private List<string> childProcessAddr = new List<string>();

        // Constructor:
        //      Initializes a comm for mother builder
        //      Starts the message handling thread for getMessage
        //      Spawns the initialized number of child builders and stores their addresses
        //      Starts the buildRequestDispatcher thread
        public MotherBuilder(string baseAddress, int port, int numProc)
        {
            commObj = new MessagePassing.Comm(baseAddress, port);
            buildRequestQ = new SWTools.BlockingQueue<CommMessage>();
            readyStatusQ = new SWTools.BlockingQueue<string>();

            base.messageHandlerThread.Start();

            numProcess = numProc;
            int childPortStart = 8085;
            updateChildProcPorts(baseAddress, childPortStart, numProc);
            spawnProcess(childProcessAddr, numProcess);

            messageHandlerThread = new Thread(buildRequestDispatcher);
            messageHandlerThread.Start();
        }

        // Takes care of sending build requests to the child builders based on availablity of the child builders (from "ready" status)
        private void buildRequestDispatcher()
        {
            while (true)
            {
                CommMessage msg = buildRequestQ.deQ();
                string childBuilderAddr = readyStatusQ.deQ();
                // Sends message when both the messages are dequeued
                msg.to = childBuilderAddr;
                Console.WriteLine("\n Sending the following Build Request to: {0} child builder", msg.to);
                msg.show();
                commObj.postMessage(msg);
                Thread.Sleep(1000);
            }
        }

        // Spawns the child builder processes
        private void spawnProcess(List<string> childProcessAddr, int numProcess)
        {
            for (int i = 0; i < numProcess; i++)
            {
                Process proc = new Process();
                // Process that will be spawned
                string fileName = "..\\..\\..\\Builder\\bin\\debug\\Builder.exe";
                string absFileSpec = Path.GetFullPath(fileName);

                try
                {
                    // Spawning the process at the port given by childProcessAddr[i]
                    Process.Start(fileName, childProcessAddr[i]);
                }
                catch (Exception ex)
                {
                    Console.Write("\n  {0}", ex.Message);
                }
            }
        }

        // Maintains a list of addresses of the child builders
        private void updateChildProcPorts(string baseAddress, int childPortStart, int numProc)
        {
            for (int i = childPortStart; i < childPortStart + numProc; i++)
            {
                toAddress = baseAddress + ":" + i.ToString();
                childProcessAddr.Add(toAddress);
            }
        }

        // messageHandler function overridden by mother builder
        public override void messageHandler(CommMessage msg)
        {
            // To close the child builders
            if (msg.type == CommMessage.MessageType.closeReceiver)
            {
                for (int i = 0; i < numProcess; i++)
                {
                    CommMessage sendMsg = new CommMessage(CommMessage.MessageType.closeReceiver);
                    sendMsg.from = "http://localhost:8081/IMessagePassingComm";
                    sendMsg.to = childProcessAddr[i] + "/IMessagePassingComm";
                    sendMsg.command = "killProc";
                    sendMsg.author = "Sachin";
                    //sendMsg.type = CommMessage.MessageType.closeReceiver;
                    commObj.postMessage(sendMsg);
                    Thread.Sleep(1000);
                }
                Thread.Sleep(10000);
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                switch (msg.command)
                {
                    // Enqueues the address of the child builder that sent the ready status
                    case "ready":
                        readyStatusQ.enQ(msg.from);
                        break;
                    // Enqueues the build request sent by the repo
                    case "buildRequest":
                        buildRequestQ.enQ(msg);
                        break;
                }
            }
        }

        // Main process of the Mother Builder
        static void Main(string[] args)
        {
            Console.WriteLine("\n ************************************************ \n");
            Console.WriteLine(" Starting MotherBuilder at: http://localhost:{0} ", 8081);
            Console.WriteLine("\n ************************************************ \n");
            int numProcess = Int32.Parse(args[0]);
            Console.WriteLine(" Number of child builders spawned: {0}", numProcess);

            // Starting Mother Builder at "http://localhost:8081"
            MotherBuilder mb = new MotherBuilder("http://localhost", 8081, numProcess);
        }

    }
}
