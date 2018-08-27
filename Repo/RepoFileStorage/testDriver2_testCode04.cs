//////////////////////////////////////////////////////////////////////
// Project#2:      Core Build Server                                //
//                 To develop core functionality of a Build Server  //
//                                                                  //
// testCode5.cs:   Test code for Demo2 package                      //
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
//             - Used by Test Driver 2
// Required Files:
// ===============
//         - testDriver5.cs
// Maintenance Information:
// ========================
// Version: 1.0
//         - First release

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo2
{
    class testCode5
    {
        public long fact(long number)
        {
            long result = 1;
            for (long i = number; i > 0; i--)
                result *= i;
            return result;
        }
    }
}
