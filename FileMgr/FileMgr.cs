//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// FileMgr.cs:     Provides file and directory handling for         //
//                 navigation                                       //
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
//         - Performs file and directory handling for navigation
//             - Finds files and folders at the root path and in any subdirectory in the tree rooted at that path
//             - Defines IFileMgr interface, FileMgrFactory, and LocalFileMgr classes
//             - Clients use the FileMgrFactory to create an instance bound toan interface reference
// Public Interface:
// =================
//         - public IEnumerable<string> getFiles()
//             - Get names of all files in current directory
//         - public IEnumerable<string> getDirs()
//             - Get names of all subdirectories in current directory
// Required Files:
// ===============
//         - FileMgr.cs
// Build Command:
// ==============
//         - csc FileMgr.cs
//         - devenv FileMgr.csproj
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release
// Version: 2.0
//         - Moved all Environment definitions into an Environment project

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MessagePassing;

namespace Navigator
{
    public enum FileMgrType { Local, Remote }

    
    // NavigatorClient uses only this interface and factory
    public interface IFileMgr
    {
        IEnumerable<string> getFiles();
        IEnumerable<string> getDirs();
        bool setDir(string dir);
        Stack<string> pathStack { get; set; }
        string currentPath { get; set; }
    }

    public class FileMgrFactory
    {
        static public IFileMgr create(FileMgrType type)
        {
            if (type == FileMgrType.Local)
                return new LocalFileMgr();
            else
                return null;
        }
    }

    // Concrete class for managing local files
    public class LocalFileMgr : IFileMgr
    {
        public string currentPath { get; set; } = "";
        public Stack<string> pathStack { get; set; } = new Stack<string>();

        public LocalFileMgr()
        {
            pathStack.Push(currentPath);  // stack is used to move to parent directory
        }

        // Get names of all files in current directory
        public IEnumerable<string> getFiles()
        {
            List<string> files = new List<string>();
            string path = Path.Combine("../../../Repo/RepoFileStorage", currentPath);
            string absPath = Path.GetFullPath(path);
            files = Directory.GetFiles(path).ToList<string>();
            for (int i = 0; i < files.Count(); ++i)
            {
                files[i] = Path.Combine(currentPath, Path.GetFileName(files[i]));
            }
            return files;
        }
        
        // Get names of all subdirectories in current directory
        public IEnumerable<string> getDirs()
        {
            List<string> dirs = new List<string>();
            string path = Path.Combine("../../../Repo/RepoFileStorage", currentPath);
            dirs = Directory.GetDirectories(path).ToList<string>();
            for (int i = 0; i < dirs.Count(); ++i)
            {
                string dirName = new DirectoryInfo(dirs[i]).Name;
                dirs[i] = Path.Combine(currentPath, dirName);
            }
            return dirs;
        }

        // Sets value of current directory - not used
        public bool setDir(string dir)
        {
            if (!Directory.Exists(dir))
                return false;
            currentPath = dir;
            return true;
        }
    }

    // Test-stub for File Manager
    class TestFileMgr
    {
        static void Main(string[] args)
        {
            IFileMgr localFileMgr = FileMgrFactory.create(FileMgrType.Local);

            List<string> List1 = new List<string>();
            List<string> List2 = new List<string>();

            List1 = localFileMgr.getFiles().ToList<string>();
            List2 = localFileMgr.getFiles().ToList<string>();

            Console.WriteLine("Displaying the list of files\n");
            foreach (string file in List1)
            {
                Console.WriteLine("{0}\n", file);
            }

            Console.WriteLine("Displaying the list of directories\n");
            foreach (string file in List2)
            {
                Console.WriteLine("{0}\n", file);
            }
        }
    }
}
