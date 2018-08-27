//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// Communicator.cs: Provides the base from which all the servers    //
//                  derive from to perform communication            //
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
//         - Abstract class that all the servers in the federation derive from
//             - Provides a comm object to provide communication
//             - Creates a thread to process incoming messages
//             - Provides an abstract message handler function which derived classes implement as required
// Public Interface:
// =================
//         - abstract public void messageHandler(CommMessage msg);
//             - Overridable function which the dervide classes implements
// Required Files:
// ===============
//         - Communicator.cs
//         - Comm.cs
// Build Command:
// ==============
//         - csc Comm.cs Communicator.cs
//         - devenv Communicator.csproj
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SWTools;

namespace MessagePassing
{
    // Base class from which all the servers in the federation derive from
    //      Provides a separate thread to handle incoming messages to every server
    //      Provides an overridable message Handler function to any server in a federation that inherits this class
    abstract public class Communicator
    {
        protected Thread messageHandlerThread = null;
        protected Comm commObj = null;

        // Constructor which creates a new thread for the message handler function
        public Communicator()
        {
            this.messageHandlerThread = new Thread(processMessage);
        }

        // Function that dequeues the message from the receiver queue and calls the message handler function
        void processMessage()
        {
            while (true)
            {
                CommMessage msg = commObj.getMessage();
                messageHandler(msg);
            }
        }

        // Overridable message handler function
        abstract public void messageHandler(CommMessage msg);
    }
}