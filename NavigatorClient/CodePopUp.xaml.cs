//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
//                                                                  //
// CodePopUp.xaml.cs: Displays text file source in response to      //
//                    double-click                                  //
//                    Using the demo code (from Help session)       //
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

///////////////////////////////////////////////////////////////////////////////
// CodePopUp.xaml.cs - Displays text file source in response to double-click //
// ver 1.0                                                                   //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017           //
///////////////////////////////////////////////////////////////////////////////

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
using System.Windows.Shapes;

namespace Navigator
{
  public partial class CodePopUp : Window
  {
    public CodePopUp()
    {
      InitializeComponent();
    }
  }
}
