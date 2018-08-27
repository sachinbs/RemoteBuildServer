//////////////////////////////////////////////////////////////////////
// Project#4:      Remote Build Server                              //
//                 An automated tool that builds test libraries     //
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

1. Client, Repo, Mother Builder, Test Harness are started in parallel
2. Mother Builder spawns the number of processes (child builders) specified in the command-line arguments (Default number of processes: 2)
3. To demonstrate the requirements, Client sends a message to the Repo to send the XMLs in its storage area for building and testing
4. There are 3 types of XML. a. Build Pass and Test Pass
							 b. Build Pass and Test Fail
							 c. Build Fail
5. Repo forwards the Build request to the Mother Builder
6. And the Mother Builder sends the build requests to the available child builder (depending on the ready message)
7. After build, logs are sent to the Repo. And if build is successful, sends a message to the Test Harness saying the library is ready
8. Test Harness responds back requesting for files. And the child builder sends it to the Test Harness for testing
9. Test Harness after receiving the library, loads it, runs it and logs the result. Sends the result to the Repo


Storage Locations:
- Repository: ./Repo/RepoFileStorage/
- Builder: ./Builder/BuilderStorage/
- Client: ./TestRequest/BRFileStorage/
- Test Harness: ./TestHarness/TesterStorage/
