//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// NavigatorClient.xaml.cs: Provides the logic for GUI              //
//                    Developed using "NavigatorClientServer"       //
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
//         - Provides the logic for the GUI. It provides facilities for: 
//             - Viewing the contents of repository remotely
//             - Making selections on the GUI to generate a Build Request
//             - To test the XMLs present in the Repository (displayed remotely)
//             - To close the Mother Builder and its spawned processes
//             - To view Build and Test notifications
//             - To view Build and Test logs (remotely)
// Public Interface:
// =================
//         - public void getTopFiles()
//             - Gets the list of all files and Directories in the current directory
// Required Files:
// ===============
//         - NavigatorClient.xaml.cs
//         - NavigatorClient.xaml
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release
/*
 * Package Operations:
 * -------------------
 * This package defines WPF application processing by the client.  The client
 * displays a local FileFolder view, and a remote FileFolder view.  It supports
 * navigating into subdirectories, both locally and in the remote Server.
 * 
 * It also supports viewing local files.
 * 
 * Maintenance History:
 * --------------------
 * ver 2.1 : 26 Oct 2017
 * - relatively minor modifications to the Comm channel used to send messages
 *   between NavigatorClient and NavigatorServer
 * ver 2.0 : 24 Oct 2017
 * - added remote processing - Up functionality not yet implemented
 *   - defined NavigatorServer
 *   - added the CsCommMessagePassing prototype
 * ver 1.0 : 22 Oct 2017
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
//using MessagePassingComm;
using MessagePassing;
using BuildRequest;

namespace Navigator
{
    public partial class MainWindow : Window
    {
        private IFileMgr fileMgr { get; set; } = null;  // note: Navigator just uses interface declarations
        Comm comm { get; set; } = null;
        private static string clientPort { get; set; } = "http://localhost:8083/IMessagePassingComm";
        private static string repoPort { get; set; } = "http://localhost:8082/IMessagePassingComm";
        private static string mBldrPort { get; set; } = "http://localhost:8081/IMessagePassingComm";
        private static string xmlStorageLocation { get; set; } = "../../../Repo/RepoFileStorage/XML/";
        private string locBuildRequestPath = "../../../TestRequest/BRFileStorage";
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;
        string currentItemText = "";
        int currentItemIndex;

        public MainWindow()
        {
            Thread.Sleep(500);
            InitializeComponent();
            //initializeEnvironment();
            fileMgr = FileMgrFactory.create(FileMgrType.Local); // uses Environment
            comm = new Comm("http://localhost", 8083);
            getTopFiles();
            getXmlFiles();
            initializeMessageDispatcher();
            initializeMessageDispatcher1();
            //Thread.Sleep(1000);
            automate();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
        }

        public void getXmlFiles()
        {
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.command = "getXmlFiles";
            sendMsg.author = "Sachin_Client";
            sendMsg.from = clientPort;
            sendMsg.to = repoPort;
            comm.postMessage(sendMsg);
            Thread.Sleep(1000);
        }

        // Define how to process each message command
        void initializeMessageDispatcher()
        {
            // load repoFiles listbox with files from root

            messageDispatcher["getTopFiles"] = (CommMessage msg) =>
            {
                repoFiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    repoFiles.Items.Add(file);
                }
            };
            // load repoDirs listbox with dirs from root

            messageDispatcher["getTopDirs"] = (CommMessage msg) =>
            {
                repoDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    repoDirs.Items.Add(dir);
                }
            };
            // load repoFiles listbox with files from folder

            messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>
            {
                repoFiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    repoFiles.Items.Add(file);
                }
            };
            // load repoDirs listbox with dirs from folder

            messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>
            {
                repoDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    repoDirs.Items.Add(dir);
                }
            };
        }

        // Define how to process each message command
        private void initializeMessageDispatcher1()
        {
            messageDispatcher["getXmlFiles"] = (CommMessage msg) =>
            {
                viewBldReq.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    viewBldReq.Items.Add(fileName);
                }
            };
        }

        // Define processing for GUI's receive thread
        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();
                //msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }
        
        // Shut down comm when the main window closes
        private void Window_Closed(object sender, EventArgs e)
        {
            comm.close();

            // The step below should not be nessary, but I've apparently caused a closing event to 
            // hang by manually renaming packages instead of getting Visual Studio to rename them.

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        //----< not currently being used >-------------------------------

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        //----< show files and dirs in root path >-----------------------

        public void getTopFiles()
        {
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.command = "getTopFiles";
            sendMsg.author = "Sachin_Client";
            sendMsg.from = clientPort;
            sendMsg.to = repoPort;
            comm.postMessage(sendMsg);
            Thread.Sleep(1000);
            CommMessage sendMsg2 = new CommMessage(CommMessage.MessageType.request);
            sendMsg2.command = "getTopDirs";
            sendMsg2.author = "Sachin_Client";
            sendMsg2.from = clientPort;
            sendMsg2.to = repoPort;
            comm.postMessage(sendMsg2);
            Thread.Sleep(1000);
        }
        
        //----< move to parent directory and show files and subdirs >----

        private void localUp_Click(object sender, RoutedEventArgs e)
        {
            if (fileMgr.currentPath == "")
                return;
            fileMgr.currentPath = fileMgr.pathStack.Peek();
            fileMgr.pathStack.Pop();
            getTopFiles();
        }
        
        //----< move into remote subdir and display files and subdirs >--
        /*
         * - sends messages to server to get files and dirs from folder
         * - recv thread will create Action<CommMessage>s for the UI thread
         *   to invoke to load the repoFiles and repoDirs listboxs
         */
        private void repoDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = clientPort;
            msg1.to = repoPort;
            msg1.command = "moveIntoFolderFiles";
            msg1.arguments.Add(repoDirs.SelectedValue as string);
            comm.postMessage(msg1);
            Thread.Sleep(1000);
        }

        private void viewBldReq_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = viewBldReq.SelectedValue as string;
            string fileEntries = @"../../../Repo/RepoFileStorage/XML/";
            try
            {
                string path = System.IO.Path.Combine(fileEntries, fileName);
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

        private void bldLogsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Could not implement due to time constraint
        }

        private void tstLogsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Could not implement due to time constraint
        }

        private void addFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                currentItemText = repoFiles.SelectedValue.ToString();
                currentItemIndex = repoFiles.SelectedIndex;

                toBeBld.Items.Add(currentItemText);
            }
            catch (Exception Ex)
            {
                string msg = Ex.Message;
            }
        }

        private void topFiles_Click(object sender, RoutedEventArgs e)
        {
            fileMgr.currentPath = "";
            getTopFiles();
        }

        private void prevDir_Click(object sender, RoutedEventArgs e)
        {
            // Could not implement due to time constraint
        }

        private void removFiles_Click(object sender, RoutedEventArgs e)
        {
            if (toBeBld.SelectedValue != null)
            {
                try
                {
                    // Find the right item and it's value and index
                    currentItemText = toBeBld.SelectedValue.ToString();
                    currentItemIndex = toBeBld.SelectedIndex;

                    toBeBld.Items.RemoveAt(toBeBld.Items.IndexOf(toBeBld.SelectedItem));
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }
        }

        private void genXml_Click(object sender, RoutedEventArgs e)
        {
            if (toBeBld.Items.Count != 0)
            {
                List<string> buildFilesList = new List<string>();
                //string TimeslotItems = "";
                string stamp = DateTime.Now.TimeOfDay.ToString();
                stamp = stamp.Replace(":", "_");
                stamp = stamp.Replace(".", "_");
                string buildRequestName = "Sachin_" + stamp;
                buildFilesList.Add(buildRequestName);
                foreach (string item in toBeBld.Items)
                {
                    buildFilesList.Add(item.ToString());
                }
                BuildRequest.BuildRequest br = new BuildRequest.BuildRequest();
                br.genBuildRequest(buildFilesList);
                toBeBld.Items.Clear();
                comm.postFile(buildRequestName + ".xml", locBuildRequestPath, 1024, repoPort, xmlStorageLocation);
                Thread.Sleep(500);
                CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
                sendMsg.command = "getXmlFiles";
                sendMsg.author = "Sachin_Client";
                sendMsg.from = clientPort;
                sendMsg.to = repoPort;
                comm.postMessage(sendMsg);
                Thread.Sleep(1000);
            }
        }

        private void testXml_Click(object sender, RoutedEventArgs e)
        {
            if (viewBldReq.SelectedValue != null)
            {
                currentItemText = viewBldReq.SelectedValue.ToString();
                CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
                sendMsg.from = clientPort;
                sendMsg.to = repoPort;
                sendMsg.command = "buildRequest";
                sendMsg.author = System.IO.Path.GetFileNameWithoutExtension(currentItemText);
                sendMsg.arguments.Add(currentItemText);
                comm.postMessage(sendMsg);
                Thread.Sleep(1000);
            }
        }

        public void automate()
        {
            Console.WriteLine("\n Testing all the XMLs from the Repository XML storage area\n");
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = clientPort;
            sendMsg.to = repoPort;
            sendMsg.command = "allBuildRequest";
            sendMsg.author = "Sachin";
            sendMsg.arguments.Add("");
            comm.postMessage(sendMsg);
            Thread.Sleep(1000);
        }

        public void testAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (string item in viewBldReq.Items)
            {
                CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
                sendMsg.from = clientPort;
                sendMsg.to = repoPort;
                sendMsg.command = "buildRequest";
                sendMsg.author = System.IO.Path.GetFileNameWithoutExtension(item);
                sendMsg.arguments.Add(item);
                comm.postMessage(sendMsg);
                Thread.Sleep(1000);
            }
        }

        private void killProc_Click(object sender, RoutedEventArgs e)
        {
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.closeReceiver);
            sendMsg.from = clientPort;
            sendMsg.to = mBldrPort;
            sendMsg.author = "Sachin_Client";
            sendMsg.command = "killProc";
            comm.postMessage(sendMsg);
            Thread.Sleep(1000);
        }

        private void Status_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Could not implement due to time constraint
        }
    }
}
