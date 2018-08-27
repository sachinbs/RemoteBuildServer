//////////////////////////////////////////////////////////////////////
// Project#2:      Core Build Server                                //
//                 To develop core functionality of a Build Server  //
//                                                                  //
// testCode6.cs:   Test code for Demo3 package                      //
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
//             - Used by Test Driver 3
// Required Files:
// ===============
//         - testDriver6.cs
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo3
{
    class testCode6
    {
        public int modulo(int number, int mod)
        {
            int rem = number % mod;

            return rem;
        }
    }
}
