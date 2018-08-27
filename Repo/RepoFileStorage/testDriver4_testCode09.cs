//////////////////////////////////////////////////////////////////////
// Project#2:      Core Build Server                                //
//                 To develop core functionality of a Build Server  //
//                                                                  //
// testCode1.cs:   Test code for Demo1 package                      //
// **************************************************************** //
// Version:        1.0                                              //
// Date:           09/27/2017                                       //
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
//         - Test code to be tested
//             - Used by Test Driver 1
// Required Files:
// ===============
//         - testDriver1.cs
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo1
{
    class testCode1
    {
        public string truncate(string inString, int n)
        {
            string result;
            if (0 <= n && n < inString.Count())
                result = inString.Substring(0, n);
            else
                result = inString;
            return result;
        }
        public string expand(string inString, int n)
        {
            int m = Math.Abs(n);
            if (m < inString.Count())
                return truncate(inString, m);
            string expansion = new string('.', m - inString.Count());
            if (0 < n)
            {
                return inString + expansion;
            }
            else
            {
                return expansion + inString;
            }
        }
    }
}
