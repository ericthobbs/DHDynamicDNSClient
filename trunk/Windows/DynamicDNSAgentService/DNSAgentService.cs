/* ***** BEGIN LICENSE BLOCK ***** 
 * Version: MPL 1.1 
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License 
 * for the specific language governing rights and limitations under the 
 * License. 
 * 
 * The Initial Developer of the Original Code is 
 * Eric Hobbs - www.badpointer.net. 
 * All Rights Reserved. 
 * 
 * ***** END LICENSE BLOCK ***** */ 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;

/*
 * Basic Dynamic DNS service agent.
 */
namespace DynamicDNSAgentService
{

	/* SERVICE_STATUS structure. See MSDN for complete information */
	[StructLayout(LayoutKind.Sequential)]
	public struct SERVICE_STATUS
	{
		public int serviceType;
		public int currentState;
		public int controlsAccepted;
		public int win32ExitCode;
		public int serviceSpecificExitCode;
		public int checkPoint;
		public int waitHint;
	}

	/* Possible Service States */
	public enum State
	{
		SERVICE_STOPPED = 0x00000001,
		SERVICE_START_PENDING = 0x00000002,
		SERVICE_STOP_PENDING = 0x00000003,
		SERVICE_RUNNING = 0x00000004,
		SERVICE_CONTINUE_PENDING = 0x00000005,
		SERVICE_PAUSE_PENDING = 0x00000006,
		SERVICE_PAUSED = 0x00000007,
	}

	public partial class DNSAgentService : ServiceBase
	{
		private static ManualResetEvent pause = new ManualResetEvent(true);

		//UserAgent for the service
		private const String USER_AGENT = @"DynamicDNSAgentService/1.0 (http://www.badpointer.net)";

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern int SetServiceStatus(IntPtr hServiceStatus, ref SERVICE_STATUS lpServiceStatus);
		private SERVICE_STATUS myServiceStatus;

		private Thread workerThread = null;

		// END Service Gluecode

		/// <summary>
		/// Dreamhost DNS Record
		/// </summary>
		private struct DNSRecord
		{
			/// <summary>
			/// Numeric ID of the account 
			/// </summary>
			public String accountID;

			/// <summary>
			/// User Comment
			/// </summary>
			public String Comment;

			/// <summary>
			/// Boolean
			/// </summary>
			public String Editable;

			/// <summary>
			/// The domain name
			/// </summary>
			public String Record;

			/// <summary>
			/// Type of record: A = ipv4, AAAA=ipv6, MX=mail, TXT=TEXT, etc
			/// </summary>
			public String Type;

			/// <summary>
			/// The value of the record (for A/AAAA this is the ip address)
			/// </summary>
			public String Value;

			/// <summary>
			/// DNS Zone
			/// </summary>
			public String Zone;
		};

		private const int LOG_PARSE_ERROR = 1;
		private const short LOG_CONFIG_CATEGORY = 1;

		private const String ConfigurationFile = "DNSConfig.xml";

		/* Configuration settings, read from the xml configuration file */
		private String ApiKey;
		private String ApiHostName;
		private int CheckInterval;
		private String ExternalIPCheck;
		private String ExternalIPCheckXPath;
		private String HostName;
		private String Zone;

		/* current state */
		private DateTime LastChecktime;

		public DNSAgentService()
		{
			InitializeComponent();
			
			AutoLog = true;

			SystemEventLog.Source = Globals.GetLogSourceName();
			SystemEventLog.Log = Globals.GetLogName();

			LastChecktime = DateTime.FromBinary(0);
		}

		protected override void OnStart(string[] args)
		{
			myServiceStatus.currentState = (int)State.SERVICE_START_PENDING;
			SetServiceStatus(ServiceHandle, ref myServiceStatus);
			ParseConfigurationFile(ConfigurationFile);

			//Start the worker thread if its not started
			if ((workerThread == null) || ((workerThread.ThreadState & (System.Threading.ThreadState.Unstarted | System.Threading.ThreadState.Stopped)) != 0))
			{
				workerThread = new Thread(new ThreadStart(ServiceWorkerMethod));
				workerThread.Start();
				SystemEventLog.WriteEntry("Starting worker thread...");
			}
			else
			{
				SystemEventLog.WriteEntry("Worker Thread already started....");
			}
			myServiceStatus.currentState = (int)State.SERVICE_RUNNING;
			SetServiceStatus(ServiceHandle, ref myServiceStatus);
		}

		protected override void OnStop()
		{
			myServiceStatus.currentState = (int)State.SERVICE_STOP_PENDING;
			SetServiceStatus(ServiceHandle, ref myServiceStatus);
			// New in .NET Framework version 2.0.
			this.RequestAdditionalTime(4000);
			// Signal the worker thread to exit.
			if ((workerThread != null) && (workerThread.IsAlive))
			{
				pause.Reset();
				Thread.Sleep(5000);
				workerThread.Abort();
			}
			// Indicate a successful exit.
			this.ExitCode = 0;
			myServiceStatus.currentState = (int)State.SERVICE_STOPPED;
			SetServiceStatus(ServiceHandle, ref myServiceStatus);
		}

		protected override void OnContinue()
		{
			if ((workerThread != null) &&
				((workerThread.ThreadState &
				 (System.Threading.ThreadState.Suspended | System.Threading.ThreadState.SuspendRequested)) != 0))
			{
				pause.Set();
				myServiceStatus.currentState = (int)State.SERVICE_RUNNING;
				SetServiceStatus(ServiceHandle, ref myServiceStatus);
				SystemEventLog.WriteEntry("Resumed Thread");
			}
		}

		protected override void OnPause()
		{
			// Pause the worker thread.
			myServiceStatus.currentState = (int)State.SERVICE_PAUSE_PENDING;
			SetServiceStatus(ServiceHandle, ref myServiceStatus);

			if ((workerThread != null) &&
				(workerThread.IsAlive) &&
				((workerThread.ThreadState &
				 (System.Threading.ThreadState.Suspended | System.Threading.ThreadState.SuspendRequested)) == 0))
			{
				pause.Reset();
				myServiceStatus.currentState = (int)State.SERVICE_PAUSED;
				SetServiceStatus(ServiceHandle, ref myServiceStatus);
				SystemEventLog.WriteEntry("Paused Thread");
			}
		}

		//
		//----------------------Begin Custom-----------------
		//

		/// <summary>
		/// Worker Thread function.
		/// </summary>
		public void ServiceWorkerMethod()
		{
			try
			{
				do
				{
					//We have not yet checked.
					if (LastChecktime.ToBinary() == 0)
					{
						LastChecktime = DateTime.Now.AddMinutes(CheckInterval);
						SystemEventLog.WriteEntry("Init Check Interval. First check at " + LastChecktime);
						DoCheckAndUpdate();
					}

					if(LastChecktime <= DateTime.Today)
					{
						LastChecktime = DateTime.Now.AddMinutes(CheckInterval);
						SystemEventLog.WriteEntry("Performing check/update..." + LastChecktime);
						DoCheckAndUpdate();
					}

					Thread.Sleep(1);
					// Block if the service is paused or is shutting down.
					pause.WaitOne();
					
				}
				while(true);
			}
			catch (ThreadAbortException)
			{
				// Another thread has signalled that this worker
				// thread must terminate.  Typically, this occurs when
				// the main service thread receives a service stop 
				// command.

				// Write a trace line indicating that the worker thread
				// is exiting.  Notice that this simple thread does
				// not have any local objects or data to clean up.
				EventLog.WriteEntry("Thread abort signaled - " + DateTime.Now.ToLongTimeString());
			}
			EventLog.WriteEntry("Exiting the service worker thread - " + DateTime.Now.ToLongTimeString());
		}

		private void DoCheckAndUpdate()
		{
			if (ApiKey == "" || HostName == "" || ApiHostName == "")
			{
				SystemEventLog.WriteEntry("invalid configuration file.");
				return;
			}

			String ExternalIP = "";
			try
			{
				String RecordType = "A";
				ExternalIP = fetchExternalIP();

				try
				{
					if (System.Net.IPAddress.Parse(ExternalIP).AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
						RecordType = "AAAA";
				}
				catch (Exception)
				{
					//Ok,
				}

				DNSRecord record = ExecuteDNSFetch(RecordType);

				if (record.Value == ExternalIP)
				{
					SystemEventLog.WriteEntry(String.Format("Not updating record {0} as it already has it is already up to date {1}",HostName,ExternalIP));
					return;
				}

				//if zone is set, check to make sure it matches.
				if (record.Zone != Zone && Zone.Length > 0)
				{
					SystemEventLog.WriteEntry("Error: Zones do not match. Not Updating.");
					return;
				}

				if (record.Editable == "1")
				{
					ExecuteDNSDelete(record.Record, record.Value, record.Type);
				}
				else if (record.Editable == "0")
				{
					SystemEventLog.WriteEntry(String.Format("Cannot update DNS Record {0}={2}; Type={1} as it is not editable. You must select another domain to use.", record.Record, record.Type, record.Value));
					return;
				}
				ExecuteDNSAdd(record.Record, record.Value, RecordType);
			}
			catch (DNSEntryNotFound)
			{
				ExecuteDNSAdd(HostName, ExternalIP);
			}
			catch (Exception ex)
			{
				SystemEventLog.WriteEntry("DNS update error:" + ex.Message);
			}
		}

		/// <summary>
		/// Parses and loads the configuration file into memory.
		/// </summary>
		/// <param name="xmlconfigfile">filename of the configuration file.</param>
		/// <exception cref="XMLException">If the configuration file cannot be parsed (e.g. invalid)</exception>
		private void ParseConfigurationFile(String xmlconfigfile)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load( System.IO.Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + xmlconfigfile);
			}
			catch (XmlException ex)
			{
				SystemEventLog.WriteEntry(String.Format("Failed to parse the xml configuration file '{0}'. The error was: {1}\nStack Trace: {3}", xmlconfigfile, ex.Message, ex.StackTrace),
					EventLogEntryType.Error, LOG_PARSE_ERROR, LOG_CONFIG_CATEGORY);
				throw ex;
			}
			catch (FileNotFoundException ex)
			{
				SystemEventLog.WriteEntry(String.Format("Failed to find the xml configuration file '{0}'. The error was: {1}\nStack Trace: {3}", xmlconfigfile, ex.Message, ex.StackTrace),
					EventLogEntryType.Error, LOG_PARSE_ERROR, LOG_CONFIG_CATEGORY);
				throw ex;
			}

			XmlNode node = doc.DocumentElement.SelectSingleNode("/Configuration/Api/Key");
			ApiKey = node.InnerText;

			node = doc.DocumentElement.SelectSingleNode("/Configuration/Api/HostName");
			ApiHostName = node.InnerText;

			node = doc.DocumentElement.SelectSingleNode("/Configuration/Settings/ExternalIPCheck");
			ExternalIPCheck = node.InnerText;

			node = doc.DocumentElement.SelectSingleNode("/Configuration/Settings/ExternalIPCheckXPath");
			ExternalIPCheckXPath = node.InnerText;

			node = doc.DocumentElement.SelectSingleNode("/Configuration/Settings/CheckInterval");
			try
			{
				CheckInterval = int.Parse(node.InnerText);
				if (CheckInterval == 0)
					CheckInterval = 60;
			}
			catch (Exception ex)
			{
				SystemEventLog.WriteEntry(String.Format("Failed to parse the xml configuration file '{0}'. The error was: {1}\nStack Trace: {3}", xmlconfigfile, ex.Message, ex.StackTrace),
					EventLogEntryType.Error, LOG_PARSE_ERROR, LOG_CONFIG_CATEGORY);
				CheckInterval = 60;
			}

			node = doc.DocumentElement.SelectSingleNode("/Configuration/Domain/HostName");
			HostName = node.InnerText;

			node = doc.DocumentElement.SelectSingleNode("/Configuration/Domain/Zone");
			Zone = node.InnerText;
		}

		private long GenerateUUID()
		{
			return DateTime.Now.ToBinary();
		}

		/// <summary>
		/// Retreives the clients external ip address
		/// </summary>
		/// <returns>the hosts external ip address if possible, otherwise it returns null</returns>
		private String fetchExternalIP()
		{
			try
			{
				System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(String.Format("{0}", ExternalIPCheck));
				req.Method = "GET";
				req.UserAgent = USER_AGENT;
				//req.Timeout = 10000;
				System.Net.WebResponse response = req.GetResponse();
				XmlDocument doc = new XmlDocument();
				doc.Load(response.GetResponseStream());

				XmlNode ip = doc.DocumentElement.SelectSingleNode(ExternalIPCheckXPath);
				if (ip != null)
				{
					SystemEventLog.WriteEntry(String.Format("External IP address is: {0}",ip.InnerText));
					return ip.InnerText;
				}
			}
			catch (Exception ex)
			{
				SystemEventLog.WriteEntry("Failed to connect to the external ip service. Error was " + ex.Message);
				return null;
			}
			return null;
		}

		/// <summary>
		/// Retreives the current DNS information for the host
		/// </summary>
		/// <returns>A DNS Record that matches the information from the configuration file if such a record exists.</returns>
		/// <exception cref="DNSEntryNotFound">Thrown if the requested record does not exist.</exception>
		/// <exception cref="IOException">Thrown if there is a xml problem but the document parsed ok.</exception>
		/// <exception cref="Exception">Thrown if there is a dreamhost api issue.</exception>
		private DNSRecord ExecuteDNSFetch(String RecordType)
		{
			try
			{
				String RequestString = String.Format("{0}?key={1}&cmd={2}&unique_id={3}&format={4}&editable=0&type={5}", 
					ApiHostName, ApiKey, "dns-list_records", GenerateUUID(), "xml", RecordType);
				System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(RequestString);
				req.Method = "GET";
				req.UserAgent = USER_AGENT;
				System.Net.WebResponse response = req.GetResponse();

				XmlDocument doc = new XmlDocument();
				doc.Load(response.GetResponseStream());

				if (doc.DocumentElement == null)
				{
					SystemEventLog.WriteEntry("failed to parse the data stream.");
					throw new System.IO.IOException("Web Request Data stream error");
				}

				XmlNode node = doc.DocumentElement.SelectSingleNode(@"result");
				if (node == null || node.InnerText == "error")
				{
					node = doc.DocumentElement.SelectSingleNode(@"data");
					throw new Exception("Dreamhost API call failed. Error: " + (node == null? "Unknown" : node.InnerText) );
				}

				node = doc.DocumentElement.SelectSingleNode("data[record = \"" + HostName + "\"]");

				//Only need to return a structure if the record exists.
				if (node != null)
				{
					DNSRecord record;

					XmlNode recordNode = node.SelectSingleNode("account_id");
					if (recordNode == null)
					{
						throw new Exception("Invalid Response from API Server");
					}

					record.accountID = recordNode.InnerText;

					recordNode = node.SelectSingleNode("comment");
					record.Comment = recordNode.InnerText;

					recordNode = node.SelectSingleNode("editable");
					record.Editable = recordNode.InnerText;

					recordNode = node.SelectSingleNode("record");
					record.Record = recordNode.InnerText;

					recordNode = node.SelectSingleNode("type");
					record.Type = recordNode.InnerText;

					recordNode = node.SelectSingleNode("value");
					record.Value = recordNode.InnerText;

					recordNode = node.SelectSingleNode("zone");
					record.Zone = recordNode.InnerText;

					return record;
				}
			}
			catch (Exception ex)
			{
				SystemEventLog.WriteEntry(String.Format("Failed to download DNS Information. Error was: {0}",ex.Message));
				throw ex;
			}
			throw new DNSEntryNotFound("No DNS information found.");
		}

		private void ExecuteDNSDelete(String domain, String value, String type = "A")
		{
			try
			{
				String RequestString = String.Format("{0}?key={1}&cmd={2}&unique_id={3}&format={4}&type={5}&record={6}&value={7}",
					ApiHostName, ApiKey, "dns-remove_record", GenerateUUID(), "xml", type,domain,value);
				System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(RequestString);
				req.Method = "GET";
				req.UserAgent = USER_AGENT;
				System.Net.WebResponse response = req.GetResponse();

				XmlDocument doc = new XmlDocument();
				doc.Load(response.GetResponseStream());

				if (doc.DocumentElement == null)
				{
					SystemEventLog.WriteEntry("failed to parse the data stream.");
					return;
				}

				XmlNode node = doc.DocumentElement.SelectSingleNode(@"result");
				if (node == null || node.InnerText == "error")
				{
					node = doc.DocumentElement.SelectSingleNode(@"data");
					throw new Exception("Dreamhost API call failed. Error: " + (node == null ? "Unknown" : node.InnerText));
				}
				else if (node != null || node.InnerText == "sucess")
				{
					node = doc.DocumentElement.SelectSingleNode(@"data");
					if (node.InnerText != "record_removed")
					{
						SystemEventLog.WriteEntry("Unknown result from API: " + node.InnerText);
					}
					SystemEventLog.WriteEntry("DNS remove Success: " + node.InnerText);
				}
			}
			catch (Exception ex)
			{
				SystemEventLog.WriteEntry("exception: " + ex.Message);
			}
		}

		private void ExecuteDNSAdd(String domain, String value, String type = "A")
		{
			try
			{
				String RequestString = String.Format("{0}?key={1}&cmd={2}&unique_id={3}&format={4}&type={5}&record={6}&value={7}",
					ApiHostName, ApiKey, "dns-add_record", GenerateUUID(), "xml", type, domain, value);
				System.Net.WebRequest req = System.Net.HttpWebRequest.Create(RequestString);
				req.Method = "GET";
				System.Net.WebResponse response = req.GetResponse();

				XmlDocument doc = new XmlDocument();
				doc.Load(response.GetResponseStream());

				if (doc.DocumentElement == null)
				{
					SystemEventLog.WriteEntry("failed to parse the data stream.");
					return;
				}

				XmlNode node = doc.DocumentElement.SelectSingleNode(@"result");
				if (node == null || node.InnerText == "error")
				{
					node = doc.DocumentElement.SelectSingleNode(@"data");
					throw new Exception("Dreamhost API call failed. Error: " + (node == null ? "Unknown" : node.InnerText));
				}
				else if (node != null || node.InnerText == "sucess")
				{
					node = doc.DocumentElement.SelectSingleNode(@"data");
					if (node.InnerText != "record_added")
					{
						SystemEventLog.WriteEntry("Unknown result from API: " + node.InnerText);
					}
					SystemEventLog.WriteEntry("DNS add Success: " + node.InnerText);
				}

			}
			catch (Exception ex)
			{
				SystemEventLog.WriteEntry("Failed to add DNS record. Error is: " + ex.Message);
			}
		}
	}
}
