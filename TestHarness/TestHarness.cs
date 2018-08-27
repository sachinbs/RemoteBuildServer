//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// TestHarness.cs: Executes the libraries                           //
//                 Using the dllLoaderDemo project provided by      //
//                 Prof. Jim Fawcett as reference                   //
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
//         - Executes the libraries supplied by the Build Server
// Public Interface:
// =================
//         - public void initTesting(string file)
//             - Inititates testing of the dll's
//             - Loads and runs the dll's in sequence
//             - Checks for test drivers implementing ITest interface
//         - string loadAndExerciseTesters(string file, string author)
//             - Load assemblies from testersLocation and run their tests
//         - bool runSimulatedTest(Type t, Assembly asm, string author)
//             - Run tester t from assembly asm
//         - public void initTesting(string file)
//             - Inititates testing of the dll's
// Required Files:
// ===============
//         - TestHarness.cs
// Build Command:
// ==============
//         - csc TestHarness.cs
//         - devenv TestHarness.csproj
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release (For Project #2)
// Version: 2.0
//         - Modified to work with Message Passing and Comm

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using MessagePassing;

namespace TestHarness
{
    //Test Harness class
    public class TestHarness : Communicator
    {
        private static string testersLocation { get; set; } = "../../../TestHarness/TesterStorage/";
        private static string repoLogStorageLocation { get; set; } = "../../../Repo/RepoFileStorage/Logs/TestLogs/";
        private static string testerPort { get; set; }
        private static string repoPort { get; set; } = "http://localhost:8082/IMessagePassingComm";
        private static string clientPort { get; set; } = "http://localhost:8083/IMessagePassingComm";
        private StringBuilder logs = new StringBuilder();
        public List<string> testLogFiles { get; set; } = new List<string>();
        private int count { get; set; } = 0;

        public TestHarness(string baseAddress, int port)
        {
            commObj = new MessagePassing.Comm(baseAddress, port);
            base.messageHandlerThread.Start();
            testerPort = baseAddress + port + "/IMessagePassingComm";
        }

        // Initiates testing of the libraries
        public void initTesting(string file)
        {
            logs.Clear();
            string author = Path.GetFileNameWithoutExtension(file);
            string result = loadAndExerciseTesters(file, author);

            Console.Write("\n  {0}", result);
        }

        // Library binding error event handler
        static Assembly LoadFromComponentLibFolder(object sender, ResolveEventArgs args)
        {
            Console.Write("\n  called binding error event handler");
            string folderPath = testersLocation;
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        // Load assemblies from testersLocation and run their tests
        string loadAndExerciseTesters(string file, string author)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromComponentLibFolder);
            try
            {
                string filePath = Path.GetFullPath(file);
                Assembly asm = Assembly.LoadFile(filePath);
                string fileName = Path.GetFileName(filePath);
                Console.Write("\n   <------Loaded \"{0}\"------>", fileName);
                logs.Append("\n   <------Loaded \"" + fileName + "\"------>");
                // Exercise each tester found in assembly
                Type[] types = asm.GetTypes();
                foreach (Type t in types)
                {
                    // If type supports ITest interface then run test
                    if (t.IsClass && t.GetInterface("ITestInterface.ITest", true) != null)
                    {
                        if (!runSimulatedTest(t, asm, author))
                        {
                            Console.Write("\n  Test {0} failed to run", t.ToString());
                            logs.Append("\n  Test " + t.ToString() + " failed to run");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n Exception Raised!!");
                Console.Write("\n--{0}--", ex.Message);
                return ex.Message;
            }
            return "<------Simulated Testing completed------>";
        }

        // Run tester t from assembly asm
        bool runSimulatedTest(Type t, Assembly asm, string author)
        {
            try
            {
                Console.Write("\n    <------Attempting to create instance of {0}------>\n", t.ToString());
                object obj = asm.CreateInstance(t.ToString());

                // Announce test
                MethodInfo method = t.GetMethod("say");
                if (method != null)
                    method.Invoke(obj, new object[0]);

                // Run test
                bool status = false;
                method = t.GetMethod("test");
                if (method != null)
                    status = (bool)method.Invoke(obj, new object[0]);

                Func<bool, string> act = (bool pass) =>
                {
                    if (pass)
                        return "passed";
                    return "failed";
                };
                Console.Write("\n    <------Test {0}------>\n", act(status));
                logs.Append("\n    <------Test " + act(status) + "------>\n");
                //// To send notification
                //CommMessage sendMsg = new CommMessage(CommMessage.MessageType.connect);
                //sendMsg.from = testerPort;
                //sendMsg.to = clientPort;
                //sendMsg.author = author;
                //sendMsg.command = "Status";
                //if (act(status) == "passed")
                //    sendMsg.arguments.Add("Test Passed");
                //else
                //    sendMsg.arguments.Add("Test Failed");
                //commObj.postMessage(sendMsg);
            }
            catch (Exception ex)
            {
                Console.Write("\n  test failed with message \"{0}\"", ex.Message);
                logs.Append("\n  test failed with message \"" + ex.Message + "\"");
                return false;
            }
            return true;
        }

        // Message Handler for Test Harness
        public override void messageHandler(CommMessage msg)
        {
            switch(msg.command)
            {
                case "libReady":
                    msg.command = "sendLib";
                    string from = msg.from;
                    msg.from = msg.to;
                    msg.to = from;
                    msg.path = testersLocation;
                    commObj.postMessage(msg);
                    Thread.Sleep(1000);
                    break;
                case "libSent":
                    Console.WriteLine("\n Received the following library");
                    string fileName = msg.author + ".dll";
                    Console.WriteLine("\n {0}", fileName);
                    string path = Path.Combine(testersLocation, fileName);
                    initTesting(path);
                    string logOp = logs.ToString();
                    string logFileName = msg.author + ".log";
                    string logPath = Path.Combine(testersLocation, logFileName);
                    if (!File.Exists(logPath))
                    {
                        File.WriteAllText(logPath, logOp);
                    }
                    Console.WriteLine("\n\n ****  Test log: {0} generated, Sent to the Repository storage area (in Test Logs Dir)  ****\n\n", logFileName);
                    commObj.postFile(logFileName, testersLocation, 1024, repoPort, repoLogStorageLocation);
                    Thread.Sleep(1000);
                    // Add sending notification to the Client
                    break;
            }
        }

        // Extract name of current directory without its parents ---------
        string GuessTestersParentDir()
        {
            string dir = Directory.GetCurrentDirectory();
            int pos = dir.LastIndexOf(Path.DirectorySeparatorChar);
            string name = dir.Remove(0, pos + 1).ToLower();
            if (name == "debug")
                return "../..";
            else
                return ".";
        }

        // Helper function for getTestLogFiles function
        private void getTestLogFilesHelper(string path, string pattern)
        {
            string[] tempFiles = Directory.GetFiles(path, pattern);
            for (int i = 0; i < tempFiles.Length; ++i)
            {
                tempFiles[i] = Path.GetFullPath(tempFiles[i]);
            }
            testLogFiles.AddRange(tempFiles);

            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                getTestLogFilesHelper(dir, pattern);
            }
        }

        // Gets the list of all files in the storagePath directory with the matching pattern (Here, .log files)
        public void getTestLogFiles(string pat = "*.log")
        {
            testLogFiles.Clear();
            getTestLogFilesHelper(testersLocation, pat);
        }

        // Sends all the generated dll's to the destination location
        public void sendLogFiles()
        {
            foreach (string file in testLogFiles)
            {
                try
                {
                    Console.Write("\n   Sending \"{0}\" to \"{1}\"", Path.GetFileName(file), Path.GetFullPath(repoLogStorageLocation));
                    string destSpec = Path.Combine(repoLogStorageLocation, Path.GetFileName(file));
                    File.Copy(file, destSpec, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n Exception Raised!!");
                    Console.Write("\n--{0}--", ex.Message);
                    return;
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine(" *********************************************** \n");
            Console.WriteLine(" Starting Test Harness at: http://localhost:{0} ", 8084);
            Console.WriteLine(" *********************************************** \n");

            // Starting the Test Harness at "http://localhost:8084"
            TestHarness testHarness = new TestHarness("http://localhost", 8084);
        }
    }
}
