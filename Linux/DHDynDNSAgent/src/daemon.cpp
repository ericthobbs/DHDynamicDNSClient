#include <iostream>
#include <fstream>
#include <cstdlib>
#include <cstring>
#include <exception>
#include <chrono>
#include <thread>

#include <sys/stat.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>

#include <popt.h>

#include <uuid.h>

#include <syslog.h>
#include <libexplain/fork.h>

#include <xercesc/util/XMLString.hpp>
#include <xercesc/util/PlatformUtils.hpp>
#include <xercesc/framework/MemBufInputSource.hpp>

#include <boost/format.hpp>

#include "daemon.hpp"
#include "configreader.hpp"
#include "dhxmlreader.hpp"
#include "configfileexception.hpp"
#include "httpclientexception.hpp"
#include "malformedxmlexception.hpp"
#include "dhapikeypermissionexception.hpp"
#include "dhapirecordnotfoundexception.hpp"

namespace
{
	static const std::string kUserAgent = "DHDynDNSAgent/1.0 (https://www.badpointer.net)";

	static const std::string kDHApiMethodDnsAdd    = "dns-add_record";
	static const std::string kDHApiMethodDnsList   = "dns-list_records";
	static const std::string kDHApiMethodDnsDelete = "dns-remove_record";
}

Daemon::Daemon(const Parameters &p) : params(p), client(kUserAgent)
{
	int logmask = 0;
	if(params.verbose())
		logmask = setlogmask(LOG_UPTO(LOG_DEBUG));
	else
		logmask = setlogmask(LOG_UPTO(LOG_WARNING));


	openlog(NULL,LOG_PID,LOG_DAEMON);

	xercesc::XMLPlatformUtils::Initialize();
	syslog(LOG_DEBUG, "xerces-c initialized");

	ConfigReader config(params.configFile());

	config_hostname = config.getSetting("Configuration.Domain.HostName");
	config_zone = config.getSetting("Configuration.Domain.Zone");
	
	config_externalip = config.getSetting("Configuration.Settings.ExternalIPCheck");
	config_ipelement = config.getSetting("Configuration.Settings.ExternalIPCheckElement");
	config_interval = config.getSetting("Configuration.Settings.CheckInterval");

	config_apikey = config.getSetting("Configuration.Api.Key");
	config_apihost = config.getSetting("Configuration.Api.HostName");
}

Daemon::~Daemon()
{
	xercesc::XMLPlatformUtils::Terminate();
	syslog(LOG_DEBUG, "Xerces-c terminated");

	closelog();
}

bool Daemon::daemonize()
{
	if(params.noForking())
	{
		syslog(LOG_INFO, "forking disabled. Not forking. STD{IN,OUT,ERR} not closed.");
		return true;
	}

	pid = fork();
	if (pid < 0) {
		syslog(LOG_ERR, "Failed to fork. Error %s", explain_fork());
		exit(EXIT_FAILURE);
	}
	
	if(pid > 0)
	{
		//Parent process 
		//TODO: Change the return value of the method to return an exit code
		exit(EXIT_SUCCESS);
	}

	umask(0);

	sid = setsid();
	if (sid < 0) {
		syslog(LOG_ERR, "Failed to set sid().");
		exit(EXIT_FAILURE);
	}

	//double fork
	pid = fork();
	if (pid < 0)
	{
		syslog(LOG_ERR, "Failed to double fork.");
		exit(EXIT_FAILURE);
	}
	if(pid > 0)
	{
		//Parent Process, Exit.
		//TODO: Change the return value of the method to return an exit code
		exit(EXIT_SUCCESS);
	}

	if ((chdir("/")) < 0) {
		syslog(LOG_ERR, "Failed to chdir(/).");
		exit(EXIT_FAILURE);
	}

	close(STDIN_FILENO);
	close(STDOUT_FILENO);
	close(STDERR_FILENO);

	if( !createPidFile(params.lockFile(),pid) )
		syslog(LOG_ERR,"failed to create lockfile %s",params.lockFile().c_str() );

	return true;
}

const bool Daemon::createPidFile(const std::string &lockfile, pid_t pid) const
{
	syslog(LOG_DEBUG,"lock file: %s, pid: %u", lockfile.c_str(), ( pid == 0 ? getpid() : pid ) );
	std::ofstream strm(lockfile,std::ios::app);
	if(!strm)
		return false;

	strm << (pid == 0 ? getpid() : pid );
	return true;
}

/**! /brief Generate a 36 character guid as a string
 * 
 */
const std::string Daemon::generateUUID() const
{
	static const int UUID_LEN = 36;
	uuid_t uuid;
	uuid_generate(uuid);
	char* output = new char[UUID_LEN+1];
	memset(output, 0, UUID_LEN);
	uuid_unparse(uuid, output);
	std::string guidAsString(output);
	delete [] output;
	return guidAsString;
}

int Daemon::runMainLoop()
{
	bool skip_add = false;
	while(true)
	{
		try
		{
			syslog(LOG_DEBUG, "Begin update cycle");

			//Fetch External IP and record Type
			std::string external_ip = fetchExternalIP();
			
			std::cout << "External IP is: " << external_ip << std::endl;
			syslog(LOG_INFO, "External ip is: %s", external_ip.c_str());

			//Access DH API and query records
			DNSRecord rec = executeDNSFetch(config_hostname,getIPFamilyType(external_ip));

			if(!rec.bError)
			{
				//found existing record, delete it
				std::cout << "Domain " << rec.Record << " found in DNS with value of " << rec.Value << " with a type of " << rec.Type << std::endl;
				syslog(LOG_INFO, "Domain %s found in DNS with a value of %s and a type of %s belonging to account id %s",
								rec.Record.c_str(), rec.Value.c_str(), rec.Type.c_str(), rec.accountID.c_str() );

				if(rec.Value == external_ip)
				{
					syslog(LOG_INFO,"skipping dns update as no change is needed.");
					skip_add = true;
				}
				else
				{
					//Delete existing record
					executeDNSDelete(rec.Record,rec.Value,rec.Type);

					//sleep for 3 second as the DH servers may not have registered the delete yet.
					syslog(LOG_DEBUG, "sleeping for 3 seconds before add after delete");
					sleep(3);
				}
			}

			//add new record
			if(!skip_add)
				executeDNSAdd(config_hostname,external_ip,getIPFamilyType(external_ip));
			skip_add = false;

		}
		catch(HttpClientException const &ex)
		{
			syslog(LOG_ERR, "Network error: %s.", ex.what());
		}
		catch(MalformedXMLException const &ex)
		{
			syslog(LOG_ERR, "Invalid response from server: %s", ex.what() );
		}
		catch(boost::io::format_error const &ex)
		{
			syslog(LOG_ERR, "Internal failure: %s", ex.what() );
		}
		catch(std::exception &ex)
		{
			syslog(LOG_ERR, "Unexpected error: %s", ex.what() );
		}
		catch(DHApiKeyPermissionException &ex)
		{
			syslog(LOG_ERR, "Key does not have enough permission: %s.", ex.what() );
			break;
		}
		catch(DHApiRecordNotFoundException const &ex)
		{
			syslog(LOG_ERR, "Invalid Query String: %s",ex.what() );
			break;
		}


		syslog(LOG_DEBUG,"sleeping for %s seconds.", config_interval.c_str() );
		std::chrono::minutes minutes(std::stoi(config_interval));
		std::this_thread::sleep_for(minutes);

	}
	syslog(LOG_DEBUG, "exiting main update loop.");
	return EXIT_SUCCESS;
}

const std::string Daemon::fetchExternalIP()
{
	std::string response = client.getResponse(config_externalip);
	
	std::string fname = boost::str(boost::format("HTTP Response(%1%)") % config_externalip);

	xercesc::MemBufInputSource ipxml((const XMLByte*)response.c_str(), response.size(),fname.c_str());
	ConfigReader parsed(ipxml);
	return parsed.getSetting(config_ipelement);
}

const std::string Daemon::getIPFamilyType(const std::string &addr) const
{
	addrinfo hint, *info = 0 ;
	memset(&hint, 0, sizeof(hint));
	hint.ai_family = AF_UNSPEC;
	int ret = getaddrinfo(addr.c_str(), 0, &hint, &info);
	std::string family;

	if (ret)
	{
		syslog(LOG_ERR, "getaddrinfo error: %s", gai_strerror(ret));
		throw std::runtime_error(gai_strerror(ret));
	}

	int result = info->ai_family;
	
	freeaddrinfo(info);

	//Only two types exist for ip currently, v4 and v6
	if(result == AF_INET)
		family = "A";
	else if(result == AF_INET6)
		family = "AAAA";
	else
		throw std::runtime_error("unknown AF_INET type.");

	syslog(LOG_DEBUG, "%s is of type %s", addr.c_str() ,family.c_str() );

	return family;
}

const DNSRecord Daemon::executeDNSFetch(const std::string &host_name, 
											const std::string &record_type)
{
	
	std::string qs = buildQueryString(kDHApiMethodDnsList);

	std::string response = client.getResponse(qs);

	std::string fname = boost::str(boost::format("HTTP Response(%1%)") % qs);

	xercesc::MemBufInputSource xml((const XMLByte*)response.c_str(), response.size(),fname.c_str());

	DHXMLReader reader(xml);

	std::vector<DNSRecord> records = reader.getRecords();

	DNSRecord rec;
	rec.bError = true;

	for(DNSRecord &r : records)
	{
		if(r.Record == host_name && r.Type == record_type)
		{
			syslog(LOG_DEBUG, "found matching record: %s", r.Record.c_str());
			return r;
		}
	}

	syslog(LOG_DEBUG, "no matching record found for %s", host_name.c_str());
	return rec;
}

void Daemon::executeDNSDelete(const std::string &domain, 
								const std::string &value, 
								const std::string &type)
{

	HttpParameterMap params;
	typedef std::pair<std::string,std::string> Pair;

	params.insert( Pair( "record",domain ) );
	params.insert( Pair( "value", value  ) );
	params.insert( Pair( "type",  type   ) );

	std::string qs = buildQueryString(kDHApiMethodDnsDelete,params);

	std::string response = client.getResponse(qs);

	std::string fname = boost::str(boost::format("HTTP Response(%1%)") % qs);
	xercesc::MemBufInputSource xml((const XMLByte*)response.c_str(), response.size(),fname.c_str());

	DHXMLReader reader(xml);

	if(!reader.success())
	{
		if(reader.getError() == "this_key_cannot_access_this_cmd")
			throw DHApiKeyPermissionException( reader.getError().c_str() );
		else if(reader.getError() == "no_such_record")
			throw DHApiRecordNotFoundException( reader.getError().c_str() );
		else
			std::runtime_error( reader.getError().c_str() );
	}
}


void Daemon::executeDNSAdd(const std::string &domain, 
							const std::string &value,
							const std::string &type)
{
	HttpParameterMap params;
	typedef std::pair<std::string,std::string> Pair;

	params.insert( Pair( "record",domain ) );
	params.insert( Pair( "value", value  ) );
	params.insert( Pair( "type",  type   ) );

	std::string qs = buildQueryString(kDHApiMethodDnsAdd,params);

	std::string response = client.getResponse(qs);

	std::string fname = boost::str(boost::format("HTTP Response(%1%)") % qs);
	xercesc::MemBufInputSource xml((const XMLByte*)response.c_str(), response.size(),fname.c_str());

	DHXMLReader reader(xml);

	if(!reader.success())
	{
		if(reader.getError() == "this_key_cannot_access_this_cmd")
			throw DHApiKeyPermissionException( reader.getError().c_str() );
		else if(reader.getError() == "no_such_zone")
			throw DHApiRecordNotFoundException( reader.getError().c_str() );
		else
			throw std::runtime_error( reader.getError().c_str() );
	}
}

const std::string Daemon::buildQueryString(const std::string method_name, 
											const HttpParameterMap &optional) const
{
	boost::format fmt = boost::format("%1%/?key=%2%&cmd=%3%&unique_id=%4%&format=xml");
	fmt % config_apihost % config_apikey % method_name % generateUUID();

	auto cit = optional.cbegin();

	std::string qs = fmt.str();

	for(cit; cit != optional.cend(); ++cit)
	{
		qs.append("&" + cit->first + "=" + cit->second);
	}

	syslog(LOG_DEBUG,"query string: %s", qs.c_str());
	return qs;
}
