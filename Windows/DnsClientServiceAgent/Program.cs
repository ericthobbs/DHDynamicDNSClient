using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace DnsClientServiceAgent
{
    class Program
    {
        static void Main(string[] args)
        {

            HostFactory.Run(x =>                                 //1
            {
                x.Service<DnsManager>(s =>                        //2
                {
                    s.ConstructUsing(name => new TownCrier());     //3
                    s.WhenStarted(tc => tc.Start());              //4
                    s.WhenStopped(tc => tc.Stop());               //5
                });
                x.RunAsLocalSystem();                            //6

                x.SetDescription("Sample Topshelf Host");        //7
                x.SetDisplayName("Stuff");                       //8
                x.SetServiceName("Stuff");                       //9
            });                                                  //10
    }
    }
}
