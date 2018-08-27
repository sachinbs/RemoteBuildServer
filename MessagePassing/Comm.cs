//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// Comm.cs:        Provides the base for all the servers to         //
//                 communicate                                      //
//                 Using the demo code                              //
//                 provided by Prof. Jim Fawcett as reference       //
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
//         - Provides an instance of Sender and Recevier to establish communication between servers
//             - Provides functionalities for sending and receiving messages
//             - Provides functionalities for sending and receiving files
//         - Provides the communication between servers
// Public Interface:
// =================
//         - public void postMessage()
//             - Sends message to remote communication
//         - public void getMessages()
//             - Retrieve messages from remote communication
//         - public bool postFile(string filename, string storagePath, long blockSize, string toAddress)
//             - Posts a block of file to the specified endpoint
// Required Files:
// ===============
//         - Comm.cs
//         - SenderReceiver.cs
//         - IMessagePassing.cs
// Build Command:
// ==============
//         - csc Comm.cs SenderReceiver.cs IMessagePassing.cs
//         - devenv MessagePassing.csproj
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release
// Version: 2.0
//         - Added close(), which is to be called before shutting down any process

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagePassing
{
    // Comm class: which provides the base for all the servers to communicate
    public class Comm
    {
        // Receiver and Sender of Comm
        private Receiver rcvr = null;
        private Sender sndr = null;

        // Constructor- Starts listening on specified endpoint
        public Comm(string baseAddress, int port)
        {
            rcvr = new Receiver();
            rcvr.start(baseAddress, port);
            sndr = new Sender(baseAddress, port);
        }

        // Overloaded Constructor- Starts listening on specified endpoint
        public Comm(string baseAddress)
        {
            rcvr = new Receiver();
            rcvr.start(baseAddress);
            sndr = new Sender(baseAddress);
        }

        // Shutdown of Comm
        public void close()
        {
            Console.Write("\n  Comm closing");
            rcvr.close();
            sndr.close();
        }

        // Post message to remote Comm
        public void postMessage(CommMessage msg)
        {
            sndr.postMessage(msg);
        }

        // Retrieve message from remote Comm
        public CommMessage getMessage()
        {
            return rcvr.getMessage();
        }

        // Called by remote Comm to upload file
        public bool postFile(string toPath, string filename)
        {
            return sndr.postFile(toPath, filename);
        }

        // Overloaded constructor with parameters to create a new channel if required
        public bool postFile(string filename, string storagePath, long blockSize, string toAddress, string toPath)
        {
            return sndr.postFile(filename, storagePath, blockSize, toAddress, toPath);
        }

#if(TEST_TESTCOMM)
        // Test stub for Comm
        static void Main(string[] args)
        {
            Comm comm = new Comm("http://localhost", 8084);
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);

            csndMsg.command = "show";
            csndMsg.author = "Sachin_test_Comm";
            csndMsg.to = "http://localhost:8084/IPluggableComm";
            csndMsg.from = "http://localhost:8084/IPluggableComm";

            comm.postMessage(csndMsg);

            CommMessage crcvMsg = comm.getMessage();
            crcvMsg.show();
            crcvMsg = comm.getMessage();
            crcvMsg.show();
        }
#endif
    }
}