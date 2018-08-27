//////////////////////////////////////////////////////////////////////
// Project#2:      Core Build Server                                //
//                 To develop core functionality of a Build Server  //
//                                                                  //
// testDriver1.cs: Test Driver for Demo1 package                    //
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
//             - Tests files testCode1, testCode2, testCode3
//             - The package is configured as a class library
// Required Files:
// ===============
//         - testDriver1.cs
//         - testCode1.cs
//         - testCode2.cs
//         - testCode3.cs
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
    using ITestInterface;

    public class testDriver1 : ITest
    {
        bool test_testCode1()
        {
            bool result = true;
            testCode1 tc1 = new testCode1();
            Console.WriteLine("\n      Testing truncate() function");
            Console.WriteLine("\n        Input = 123456 and truncate three digits");
            Console.WriteLine("\n        Expected output = 123");
            string value = tc1.truncate("123456", 3);
            Console.WriteLine("\n        Output = {0}", value);
            if (value != "123")
                result = false;
            Console.WriteLine("\n      Testing expand() function");
            Console.WriteLine("\n        Input = 123 and expand to six digits");
            Console.WriteLine("\n        Expected output = 123...");
            value = tc1.expand("123", 6);
            Console.WriteLine("\n        Output = {0}", value);
            if (value != "123...")
                result = false;
            
            return result;
        }

        bool test_testCode2()
        {
            bool result = true;
            testCode2 tc2 = new testCode2();
            Console.WriteLine("\n      Testing vowel() function");
            Console.WriteLine("\n        Expected output = aeiou");
            string value = tc2.vowels();
            Console.WriteLine("\n        Output = {0}", value);
            if (value != "aeiou")
                result = false;
            return result;
        }

        bool test_testCode3()
        {
            bool result = true;
            testCode3 tc3 = new testCode3();
            Console.WriteLine("\n      Testing concatenate(str1, str2) function");
            string str1 = "Concatenating ";
            string str2 = "strings";
            string catString = "Concatenating strings";
            Console.WriteLine("\n        Expected output = {0}", catString);
            string resString = tc3.concatenate(str1, str2);
            Console.WriteLine("\n        Output = {0}", resString);
            if (resString != catString)
                result = false;
            return result;
        }

        public void say()
        {
            Console.WriteLine("\n   Testing \"testDriver1\" from TestRequest1");
        }

        public bool test()
        {
            bool result1 = test_testCode1();
            bool result2 = test_testCode2();
            bool result3 = test_testCode3();

            return (result1 && result2 && result3);
        }
    }
}
