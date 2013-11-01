using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace DynamicDNSAgentService
{
	public class Globals
	{
		public static String GetLogSourceName()
		{
			return "DynamicDNSAgentService";
		}

		public static String GetLogName()
		{
			return "DDNS Client";
		}
	}

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
			{ 
				new DNSAgentService() 
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
