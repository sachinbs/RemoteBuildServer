using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Repo;
using MotherBuilder;
using MessagePassing;

namespace TestExecutive
{
    class TestExecutive : Communicator
    {
        public override void messageHandler(CommMessage msg)
        {
            if (msg.type == CommMessage.MessageType.connect)
            {
                Console.WriteLine("\nConnection with Mother Builder established\n");
            }
            else if (msg.type == CommMessage.MessageType.closeReceiver)
            {
                
            }
            else
            {
                switch (msg.command)
                {
                    case "ready":
                        Console.WriteLine("\n In Mother Builder message handler with \"ready\" \n");
                        //readyStatusQ.enQ(msg.from);
                        msg.show();
                        Console.WriteLine("\n In Mother Builder message handler with \"ready\" - Enqueue done\n");
                        break;
                    case "buildRequest":
                        Console.WriteLine("\n In Mother Builder message handler with \"buildRequest\" \n");
                        //buildRequestQ.enQ(msg);
                        msg.show();
                        Console.WriteLine("\n In Mother Builder message handler with \"buildRequest\" - Enqueue done\n");
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            Repo.Repo repo = new Repo.Repo("http://localhost", 8082);
            MotherBuilder.MotherBuilder mbldr = new MotherBuilder.MotherBuilder("http://localhost", 8081, 3);
            //mbldr.StartChildProc();

            repo.sendBR("http://localhost:8082/IPluggableComm", "http://localhost:8081/IPluggableComm");           
        }
    }
}
