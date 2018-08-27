//////////////////////////////////////////////////////////////////////
// Project#2:      Core Build Server                                //
//                 To develop core functionality of a Build Server  //
//                                                                  //
// testDriver2.cs: Test Driver for Demo2 package                    //
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
//             - Tests files testCode4, testCode5
//             - The package is configured as a class library
// Required Files:
// ===============
//         - testDriver2.cs
//         - testCode4.cs
//         - testCode5.cs
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
    using ITestInterface;

    public class testDriver2 : ITest
    {
        bool test_testCode4()
        {
            bool result = true;
            long baseNumber = 25;
            long power = 12;
            testCode4 tc4 = new testCode4();
            Console.WriteLine("\n      Testing power(basenumber, power) function");
            Console.WriteLine("\n        Input = 25 to the power of 12");
            long res = (long)Math.Pow(baseNumber, power) + 1;
            Console.WriteLine("\n        Expected output = {0}", res);
            long value = tc4.power(baseNumber, power);
            Console.WriteLine("\n        Output = {0}", value);
            if (res != value)
                result = false;
            return result;
        }

        bool test_testCode5()
        {
            bool result = true;
            long number = 9;
            long fact = 181440;
            Console.WriteLine("\n      Testing factorial of a number [fact(number) function]");
            Console.WriteLine("\n        Input = factorial of 9");
            Console.WriteLine("\n        Expected output = {0}", fact);
            testCode5 tc5 = new testCode5();
            long value = tc5.fact(number);
            Console.WriteLine("\n        Output = {0}", value);
            if (fact != value)
                result = false;
            return result;
        }

        public void say()
        {
            Console.WriteLine("\n   Testing \"testDriver2\" from TestRequest2");
        }

        public bool test()
        {
            bool result1 = test_testCode4();
            bool result2 = test_testCode5();
            return (result1 && result2);
        }
    }
}
