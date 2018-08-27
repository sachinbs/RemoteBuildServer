//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// TestRequest.cs: Builds and Parese Build Request                  //
//                 Using the demo code (from Help session)          //
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
//         - Creates and parses TestRequest XML messages using XDocument
// Public Interface:
// =================
//         - public void makeRequest()
//             - Builds an xml document
//         - public bool loadXml(string path)
//             - Loads an xml file from the specified path for further processing (like parsing)
//         - public bool saveXml(string path)
//             - Saves an xml into the specifed path
//         - public string parse(string propertyName)
//             - Parses the xml for the propertyName
//         - public List<string> parseList(string propertyName)
//             - Parses the xml for the propertyName. Generates a list
//         - public void genBuildRequest(List<string> buildFiles)
//             - Builds an xml for the files provided in the list

// Required Files:
// ===============
//         - TestRequest.cs
// Build Command:
// ==============
//         - csc TestRequest.cs
//         - devenv TestRequest.csproj
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release (For Project #2)
// Version: 2.0
//         - Modified to create an XML from a list of files

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BuildRequest
{
    // TestRequest class
    public class BuildRequest
    {
        public string author { get; set; } = "";
        public string dateTime { get; set; } = "";
        public string testDriver { get; set; } = "";
        public List<string> testedFiles { get; set; } = new List<string>();
        public XDocument doc { get; set; } = new XDocument();
        private string locBuildRequestPath = "../../../TestRequest/BRFileStorage";

        // Build XML document that represents a test request
        public void makeRequest()
        {
            XElement testRequestElem = new XElement("testRequest");
            doc.Add(testRequestElem);

            XElement authorElem = new XElement("author");
            authorElem.Add(author);
            testRequestElem.Add(authorElem);

            XElement dateTimeElem = new XElement("dateTime");
            dateTimeElem.Add(DateTime.Now.ToString());
            testRequestElem.Add(dateTimeElem);

            XElement testElem = new XElement("test");
            testRequestElem.Add(testElem);

            XElement driverElem = new XElement("testDriver");
            driverElem.Add(testDriver);
            testElem.Add(driverElem);

            foreach (string file in testedFiles)
            {
                XElement testedElem = new XElement("tested");
                testedElem.Add(file);
                testElem.Add(testedElem);
            }
        }
        
        // Load TestRequest from XML file
        public bool loadXml(string path)
        {
            try
            {
                doc = XDocument.Load(path);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }
        }
        
        // Save TestRequest to XML file
        public bool saveXml(string path)
        {
            try
            {
                doc.Save(path);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }
        }
        
        // Parse document for property value
        public string parse(string propertyName)
        {
            string parseStr = doc.Descendants(propertyName).First().Value;
            if (parseStr.Length > 0)
            {
                switch (propertyName)
                {
                    case "author":
                        author = parseStr;
                        break;
                    case "dateTime":
                        dateTime = parseStr;
                        break;
                    case "testDriver":
                        testDriver = parseStr;
                        break;
                    default:
                        break;
                }
                return parseStr;
            }
            return "";
        }
        // Parse document for property list
        public List<string> parseList(string propertyName)
        {
            List<string> values = new List<string>();

            IEnumerable<XElement> parseElems = doc.Descendants(propertyName);

            if (parseElems.Count() > 0)
            {
                switch (propertyName)
                {
                    case "tested":
                        foreach (XElement elem in parseElems)
                        {
                            values.Add(elem.Value);
                        }
                        testedFiles = values;
                        break;
                    default:
                        break;
                }
            }
            return values;
        }

        // Generates a build request from a list of files
        public void genBuildRequest(List<string> buildFiles)
        {
            string buildRequestName = buildFiles[0];
            if (!System.IO.Directory.Exists(locBuildRequestPath))
                System.IO.Directory.CreateDirectory(locBuildRequestPath);
            string fileSpec = System.IO.Path.Combine(locBuildRequestPath, buildRequestName + ".xml");
            fileSpec = System.IO.Path.GetFullPath(fileSpec);
            author = buildRequestName;
            testDriver = buildFiles[1];
            for (int i = 2; i < buildFiles.Count(); i++)
            {
                testedFiles.Add(buildFiles[i]);
            }
            testedFiles.Add("ITest.cs");
            makeRequest();
            saveXml(fileSpec);
        }
    }

    // Test stub for test request class
    class Test_TestRequest
    {
#if (TEST_X)
        static void Main(string[] args)
        {
            Console.Write("\n  Testing TestRequest");
            Console.Write("\n =====================");

            string savePath = "../../test/";
            string fileName = "TestRequest1.xml";

            if (!System.IO.Directory.Exists(savePath))
                System.IO.Directory.CreateDirectory(savePath);
            string fileSpec = System.IO.Path.Combine(savePath, fileName);
            fileSpec = System.IO.Path.GetFullPath(fileSpec);

            BuildRequest tr = new BuildRequest();
            tr.author = "Jim Fawcett";
            tr.testDriver = "td1.cs";
            tr.testedFiles.Add("tf1.cs");
            tr.testedFiles.Add("tf2.cs");
            tr.testedFiles.Add("tf3.cs");
            tr.makeRequest();
            Console.Write("\n{0}", tr.doc.ToString());

            Console.Write("\n  saving to \"{0}\"", fileSpec);
            tr.saveXml(fileSpec);

            Console.Write("\n  reading from \"{0}\"", fileSpec);

            BuildRequest tr2 = new BuildRequest();
            tr2.loadXml(fileSpec);
            Console.Write("\n{0}", tr2.doc.ToString());
            Console.Write("\n");

            tr2.parse("author");
            Console.Write("\n  author is \"{0}\"", tr2.author);

            tr2.parse("dateTime");
            Console.Write("\n  dateTime is \"{0}\"", tr2.dateTime);

            tr2.parse("testDriver");
            Console.Write("\n  testDriver is \"{0}\"", tr2.testDriver);

            tr2.parseList("tested");
            Console.Write("\n  testedFiles are:");
            foreach (string file in tr2.testedFiles)
            {
                Console.Write("\n    \"{0}\"", file);
            }
            Console.Write("\n\n");
        }
    }
#endif
}

