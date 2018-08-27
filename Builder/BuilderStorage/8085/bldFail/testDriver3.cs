//////////////////////////////////////////////////////////////////////
// Project#2:      Core Build Server                                //
//                 To develop core functionality of a Build Server  //
//                                                                  //
// testDriver3.cs: Test Driver for Demo3 package                    //
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
//         - Drives the test for the test codes to be tested
//             - Tests files testCode6, testCode7
//             - The package is configured as a class library
// Required Files:
// ===============
//         - testDriver3.cs
//         - testCode6.cs
//         - testCode7.cs
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
    using ITestInterface;

    public class testDriver3 : ITest
    {
        bool test_testCode6()
        {
            bool result = true;
            int number = 15842;
            int mod = 6;
            int res = 2;
            Console.WriteLine("\n      Testing modulo(number, mod) function");
            Console.WriteLine("\n        Input = 15842 modulo 6");
            testCode6 tc6 = new testCode6();
            Console.WriteLine("\n        Expected output = {0}", res);
            int value = tc6.modulo(number, mod);
            Console.WriteLine("\n        Output = {0}", value);
            if (res != value)
                result = false;

            return result;
        }

        bool test_testCode7()
        {
            bool result = true;
            int power = 15;
            long res = 65536;
            Console.WriteLine("\n      Testing powerOf2(power) function");
            Console.WriteLine("\n        Input = 2 to the power of 15");
            Console.WriteLine("\n        Expected output = {0}", res);
            testCode7 tc7 = new testCode7();
            long value = tc7.powerOf2(power);
            Console.WriteLine("\n        Output = {0}", value);
            if (res != value)
                result = false;

            return result;
        }

        public void say()
        {
            Console.WriteLine("\n   Testing \"testDriver3\" from TestRequest3");
        }

        public bool test()
        {
            bool result1 = test_testCode6();
            bool result2 = test_testCode7();
            return (result1 && result2);
        }
    }
}
