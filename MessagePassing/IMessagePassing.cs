//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// IMessagePassing.cs: Provides the service interface for           //
//                     message passing communication                //
//                     Using the demo code provided by              //
//                     Prof. Jim Fawcett as reference               //
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
//         - Provides the service interface for message passing communication
// Required Files:
// ===============
//         - IMessagePassing.cs
// Build Command:
// ==============
//         - csc IMessagePassing.cs
//         - devenv MessagePassing.csproj
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading;

namespace MessagePassing
{
    using Command = String;             // Command is key for message dispatching, e.g., Dictionary<Command, Func<bool>>
    using EndPoint = String;            // string is (ip address or machine name):(port number)
    using Argument = String;
    using ErrorMessage = String;

    // Provides the service contract
    [ServiceContract(Namespace = "MessagePassing")]
    public interface IMessagePassing
    {
        // To pass messages around
        [OperationContract(IsOneWay = true)]
        void postMessage(CommMessage msg);

        // To receive the messages
        CommMessage getMessage();

        // To support sending file in blocks
        [OperationContract]
        bool openFileForWrite(string addr, string name);

        // To open the file block to write
        [OperationContract]
        bool writeFileBlock(byte[] block);

        // To close files after writing
        [OperationContract(IsOneWay = true)]
        void closeFile();
    }

    // Provides the data contract
    // The messages that gets passed around
    [DataContract]
    public class CommMessage
    {
        // Provides type
        public enum MessageType
        {
            [EnumMember]
            connect,           // initial message sent on successfully connecting
            [EnumMember]
            request,           // request for action from receiver
            [EnumMember]
            reply,             // response to a request
            [EnumMember]
            closeSender,       // close down client
            [EnumMember]
            closeReceiver      // close down server for graceful termination
        }

        // Constructor requires message type
        public CommMessage(MessageType mt)
        {
            type = mt;
        }

        // Data members
        // All serializable public properties
        [DataMember]
        public MessageType type { get; set; } = MessageType.connect;

        [DataMember]
        public string to { get; set; }

        [DataMember]
        public string from { get; set; }

        [DataMember]
        public string author { get; set; }

        [DataMember]
        public Command command { get; set; }

        [DataMember]
        public List<Argument> arguments { get; set; } = new List<Argument>();

        [DataMember]
        public string path { get; set; }

        [DataMember]
        public int threadId { get; set; } = Thread.CurrentThread.ManagedThreadId;

        [DataMember]
        public ErrorMessage errorMsg { get; set; } = "no error";

        // To display the message
        public void show()
        {
            Console.Write("\n  Message:");
            Console.Write("\n    MessageType : {0}", type.ToString());
            Console.Write("\n    To          : {0}", to);
            Console.Write("\n    From        : {0}", from);
            Console.Write("\n    Author      : {0}", author);
            Console.Write("\n    Command     : {0}", command);
            Console.Write("\n    Arguments   :");
            if (arguments.Count > 0)
                Console.Write("\n      ");
            foreach (string arg in arguments)
                Console.Write("{0} ", arg);
            Console.Write("\n    ThreadId    : {0}", threadId);
            Console.Write("\n    ErrorMessage    : {0}\n", errorMsg);
        }
    }
}
