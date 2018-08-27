//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// SenderReceiver.cs: Provides the sender and receiver classes for  //
//                    communication                                 //
//                    Using the demo code                           //
//                    provided by Prof. Jim Fawcett as reference    //
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
//         - Provides Sender and Receiver class for communication
//         - Sender
//             - Opens a proxy to the to the endpoint
//         - Receiver
//             - Creates a host and listens for a channel
// Public Interface:
// =================
//         - Sender
//             - public bool connect(string baseAddress, int port)
//                 - Tries to connect and create a channel to the endpoint
//             - public void close()
//                 - Closes a channel
//             - public bool postFile(string fileName, string storagePath, long blockSize, string toAddress)
//                 - Posts files to the given address through a channel. File transfer is through blocks
//         - Receiver
//             - public void start(string baseAddress, int port)
//                 - Starts a host (receiver)
//             - public void postMessage(CommMessage msg)
//                 - Sender proxies call this message to enqueue for processing
//             - public CommMessage getMessage()
//                 - Retrieves a message sent by the sender
//             - public void close()
//                 - Closes the host
//             - public bool openFileForWrite(string name)
//                 - Opens a file for storing incoming file blocks
//             - public bool writeFileBlock(byte[] block)
//                 - Writes an incoming file block to storage
//             - public void closeFile()
//                 - Closes newly uploaded file
// Required Files:
// ===============
//         - SenderReceiver.cs
//         - IMessagePassing.cs
// Build Command:
// ==============
//         - csc SenderReceiver.cs IMessagePassing.cs
//         - devenv MessagePassing.csproj
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release
// Version: 2.0
//         - Modified close() function of Receiver

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading;

using SWTools;
using System.IO;

namespace MessagePassing
{
    // Receiver class which provides the service instance
    class Receiver : IMessagePassing
    {
        public static SWTools.BlockingQueue<CommMessage> rcvQ { get; set; } = null;
        ServiceHost commHost = null;
        FileStream fs = null;
        string lastError = "";

        // Constructor
        public Receiver()
        {
            if (rcvQ == null)
                rcvQ = new SWTools.BlockingQueue<CommMessage>();
        }

        // Starts listening on the specified endpoint
        public void start(string baseAddress, int port)
        {
            //string address = baseAddress + ":" + port.ToString() + "/IPluggableComm";
            string address = baseAddress + ":" + port.ToString() + "/IMessagePassingComm"; 
            createCommHost(address);
        }

        // Overloaded function: Starts listening on the specified endpoint (Used by child builders)
        public void start(string baseAddress)
        {
            //string address = baseAddress + "/IPluggableComm";
            string address = baseAddress + "/IMessagePassingComm";
            createCommHost(address);
        }

        // Creates a host on the specified endpoint
        public void createCommHost(string address)
        {
            try
            {
                WSHttpBinding binding = new WSHttpBinding();
                Uri baseAddress = new Uri(address);
                commHost = new ServiceHost(typeof(Receiver), baseAddress);
                commHost.AddServiceEndpoint(typeof(IMessagePassing), binding, baseAddress);
                commHost.Open();
            }
            catch(Exception ex)
            {
                Console.WriteLine("\n Exception Raised!!");
                Console.WriteLine("\n Port already in use");
                Console.WriteLine(ex.Message);
            }
        }

        // Enqueue a message for transmission to a Receiver
        public void postMessage(CommMessage msg)
        {
            msg.threadId = Thread.CurrentThread.ManagedThreadId;
            rcvQ.enQ(msg);
        }

        // Retrieve a message sent by a Sender instance
        public CommMessage getMessage()
        {
            CommMessage msg = rcvQ.deQ();
            if (msg.type == CommMessage.MessageType.closeReceiver)
            {
                close();
            }
            if (msg.type == CommMessage.MessageType.connect)
            {
                msg = rcvQ.deQ();  // discarding the connect message
            }
            return msg;
        }

        // Close ServiceHost
        public void close()
        {
            //Console.WriteLine("closing receiver - please wait");
            //commHost.Close();
            Console.Write("\n  closing receiver - please wait");
            commHost.Close();
            (commHost as IDisposable).Dispose();

            Console.Write("\n  commHost closed");
        }

        // Called by Sender's proxy to open file on Receiver
        public bool openFileForWrite(string destAddr, string name)
        {
            try
            {
                //string writePath = Path.Combine("../../../ServiceFileStorage", name);
                string writePath = Path.Combine(destAddr, name);
                fs = File.OpenWrite(writePath);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }

        //  Writes a block received from Sender instance
        public bool writeFileBlock(byte[] block)
        {
            try
            {
                fs.Write(block, 0, block.Length);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }

        // Closes Receiver's uploaded file
        public void closeFile()
        {
            fs.Close();
        }
    }

    // Sender class which creates a channel for the specified endpoint
    class Sender
    {
        private IMessagePassing channel;
        private ChannelFactory<IMessagePassing> factory = null;
        private SWTools.BlockingQueue<CommMessage> sndQ = null;
        private string fromAddress = "";
        private string toAddress = "";
        Thread sndThread = null;
        int tryCount = 0, maxCount = 10;
        string lastError = "";
        string lastUrl = "";

        // Constructor
        //      Attaches and starts a thread to fuction which redirects the message to the specified endpoint
        public Sender(string baseAddress, int listenPort)
        {
            fromAddress = baseAddress + listenPort.ToString() + "/IMessagePassingComm";
            sndQ = new SWTools.BlockingQueue<CommMessage>();
            sndThread = new Thread(threadProc);
            sndThread.Start();
        }

        // Overloaded contructor to take one argument (Used by child builders)
        public Sender(string baseAddress)
        {
            fromAddress = baseAddress + "/IMessagePassingComm";
            sndQ = new SWTools.BlockingQueue<CommMessage>();
            sndThread = new Thread(threadProc);
            sndThread.Start();
        }

        // Creates a proxy with interface of remote instance
        public void createSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            factory = new ChannelFactory<IMessagePassing>(binding, address);
            channel = factory.CreateChannel();
        }

        // Returns a new channel for the specified endpoint
        public IMessagePassing createNewSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            factory = new ChannelFactory<IMessagePassing>(binding, address);
            channel = factory.CreateChannel();
            return channel;
        }

        // Attempts to connect to Receiver instance
        public bool connect(string baseAddress, int port)
        {
            toAddress = baseAddress + ":" + port.ToString() + "/IMessagePassingComm";
            return connect(toAddress);
        }

        // Attempts to connect to recevier instance for a fixed number of times
        public bool connect(string toAddress)
        {
            int timeToSleep = 500;
            try
            {
                createSendChannel(toAddress);
            }
            catch(Exception ex)
            {
                Console.WriteLine("\n Exception Raised!!");
                Console.WriteLine("\n Port already in use");
                Console.WriteLine(ex.Message);
            }

            CommMessage connectMsg = new CommMessage(CommMessage.MessageType.connect);
            while (true)
            {
                try
                {
                    channel.postMessage(connectMsg);
                    tryCount = 0;
                    return true;
                }
                catch (Exception ex)
                {
                    if (++tryCount < maxCount)
                    {
                        Console.WriteLine("failed to connect - waiting to try again");
                        Thread.Sleep(timeToSleep);
                    }
                    else
                    {
                        Console.WriteLine("failed to connect - quitting");
                        lastError = ex.Message;
                        return false;
                    }
                }
            }
        }

        // Closes Sender's proxy
        public void close()
        {
            if (factory != null)
                factory.Close();
        }

        // Main processing for receive thread
        //      Sends thread dequeues the received message and posts it to the specified endpoint
        void threadProc()
        {
            while (true)
            {
                CommMessage msg = sndQ.deQ();
                if (msg.type == CommMessage.MessageType.closeSender)
                {
                    break;
                }
                if (msg.to == lastUrl)
                {
                    channel.postMessage(msg);
                }
                else
                {
                    close();
                    if (!connect(msg.to))
                    {
                        return;
                    }
                    lastUrl = msg.to;
                    channel.postMessage(msg);
                }
            }
        }

        // Enqueues the message into the queue
        public void postMessage(CommMessage msg)
        {
            sndQ.enQ(msg);
        }

        // Uploads file to Receiver instance
        public bool postFile(string toPath, string fileName)
        {
            FileStream fs = null;
            long bytesRemaining;

            try
            {
                string path = Path.Combine("../../../ClientFileStorage", fileName);
                fs = File.OpenRead(path);
                bytesRemaining = fs.Length;
                channel.openFileForWrite(toPath, fileName);
                while (true)
                {
                    long bytesToRead = Math.Min(1024, bytesRemaining);
                    byte[] blk = new byte[bytesToRead];
                    long numBytesRead = fs.Read(blk, 0, (int)bytesToRead);
                    bytesRemaining -= numBytesRead;

                    channel.writeFileBlock(blk);
                    if (bytesRemaining <= 0)
                        break;
                }
                channel.closeFile();
                fs.Close();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
            return true;
        }

        // Helper function which tries to send the file block through the specified channel
        private bool postFileTry(string fileName, string storagePath, long blockSize, IMessagePassing channel, string toPath)
        {
            FileStream fs = null;
            long bytesRemaining;
            try
            {
                string path = Path.Combine(storagePath, fileName);
                fs = File.OpenRead(path);
                bytesRemaining = fs.Length;
                channel.openFileForWrite(toPath, fileName);
                while (true)
                {
                    long bytesToRead = Math.Min(blockSize, bytesRemaining);
                    byte[] blk = new byte[bytesToRead];
                    long numBytesRead = fs.Read(blk, 0, (int)bytesToRead);
                    bytesRemaining -= numBytesRead;

                    channel.writeFileBlock(blk);
                    if (bytesRemaining <= 0)
                        break;
                }
                channel.closeFile();
                fs.Close();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
            return true;
        }

        // Uploads file to Receiver instance
        public bool postFile(string fileName, string storagePath, long blockSize, string toAddress, string toPath)
        {
            if (toAddress == lastUrl)
            {
                return (postFileTry(fileName, storagePath, blockSize, channel, toPath));
            }
            else
            {
                close();
                try
                {
                    IMessagePassing newChannel = createNewSendChannel(toAddress);
                    lastUrl = toAddress;
                    return (postFileTry(fileName, storagePath, blockSize, newChannel, toPath));
                }
                catch(Exception ex)
                {
                    Console.WriteLine("\n Exception Raised!!");
                    Console.WriteLine("\n Port already in use");
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
    }

#if (TEST_SENDERRECEIVER)
    public class Test_SenderReceiver
    {
        // Test stub for SenderReceiver
        static void Main(string[] args)
        {
            Receiver rcvr = new Receiver();
            rcvr.start("http://localhost", 8083);
            Sender sndr = new Sender("http://localhost", 8083);

            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.command = "show";
            sndMsg.author = "Sachin_test_SR";
            sndMsg.to = "http://localhost:8083/IPluggableComm";
            sndMsg.from = "http://localhost:8083/IPluggableComm";

            sndr.postMessage(sndMsg);

            CommMessage rcvMsg;
            rcvMsg = rcvr.getMessage();
            rcvMsg.show();
            rcvMsg = rcvr.getMessage();
            rcvMsg.show();
        }
    }
#endif
}