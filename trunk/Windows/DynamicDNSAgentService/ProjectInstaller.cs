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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;


namespace DynamicDNSAgentService
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : System.Configuration.Install.Installer
	{
		private EventLogInstaller eventLogInstaller;

		public ProjectInstaller()
		{
			InitializeComponent();

			eventLogInstaller = new EventLogInstaller();

			eventLogInstaller.Source = Globals.GetLogSourceName();
			eventLogInstaller.Log = Globals.GetLogName();

			Installers.Add(eventLogInstaller);
		}

		protected override void  OnAfterInstall(IDictionary savedState)
		{
			base.OnAfterInstall(savedState);

			string apikey = this.Context.Parameters["APIKEY"];
			string hostname = this.Context.Parameters["HOSTNAME"];
			string zone = this.Context.Parameters["ZONE"];
			string targetdir = this.Context.Parameters["TARGETDIR"];

			if (targetdir == "")
				return;
		
			System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

			doc.Load(targetdir + "DNSConfig.xml");

			System.Xml.XmlNode node = doc.DocumentElement.SelectSingleNode(@"/Configuration/Api/Key");

			if (node != null)
			{
				System.Xml.XmlText KeyNode = doc.CreateTextNode(apikey);
				node.RemoveAll();
				node.AppendChild(KeyNode);
			}

			node = doc.DocumentElement.SelectSingleNode(@"/Configuration/Domain/HostName");

			if (node != null)
			{
				System.Xml.XmlText HostNameNode = doc.CreateTextNode(hostname);
				node.RemoveAll();
				node.AppendChild(HostNameNode);
			}

			node = doc.DocumentElement.SelectSingleNode(@"/Configuration/Domain/Zone");
			
			if (node != null)
			{
				System.Xml.XmlText ZoneNameNode = doc.CreateTextNode(zone);
				node.RemoveAll();
				node.AppendChild(ZoneNameNode);
			}

			doc.Save(targetdir + "DNSConfig.xml");
		}

		protected override void OnCommitted(IDictionary savedState)
		{
			base.OnCommitted(savedState);

			try
			{
				System.Diagnostics.EventLog.CreateEventSource("DynamicDNS Client", "DynamicDNS Client");
			}
			catch (Exception)
			{
				//log already exists
			}

			System.ServiceProcess.ServiceController controller = 
				new System.ServiceProcess.ServiceController(this.serviceInstaller1.ServiceName);

			if(Context.Parameters["STARTUP"] == "1")
				controller.Start();
		}
	}
}
