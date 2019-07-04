#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include "parameters.hpp"
#include "httpclient.hpp"
#include "dnsrecord.hpp"

#include <boost/utility.hpp>
#include <unistd.h>
#include <cstdlib>
#include <cstring>
#include <map>

/*! \brief Main core of DHDynDNSAgent
 *
 */
class Daemon : boost::noncopyable
{
public:

	typedef std::map<std::string,std::string> HttpParameterMap;



	/*! \brief construct a new instance
		\param p processed parameters
	 */
	Daemon(const Parameters &p);

	/*! \brief destructs the daemon */
	~Daemon();

	/*! \brief forks and secures the environment
		\returns true on success, false on failure
	*/
	bool daemonize();

	/*! \brief runs the main loop
		\returns EXIT_SUCCESS on success or EXIT_FAILURE on failure.
	*/
	int runMainLoop();

private:
	pid_t pid;
	pid_t sid;
	const Parameters &params;
	HttpClient client;

	std::string config_hostname;
	std::string config_apihost;
	std::string config_apikey;
	std::string config_externalip;
	std::string config_ipelement;
	std::string config_zone;
	std::string config_interval;

	/*! \brief generate a 64bit UUID
	 *	\returns string form of the generated UUID
	 */
	const std::string generateUUID() const;

	/*! \brief Creates the lock file used to verify that the daemon is running
	 *	\param file path to the lock file
	 *	\param pid process id to write to the file.
	 *	\returns true on success, false on failure
	 */
	const bool createPidFile(const std::string &file, pid_t pid) const;

	/*! \brief Fetch the clients external IP address
	 *	\returns the ip address of the client
	 */
	const std::string fetchExternalIP();

	/*! \brief Get an ip addresses family type
	 *  \brief addr ipv4/ipv6 string representation of an ip address e.g. 192.168.1.1
	 *  \returns "A" for ipv4 and "AAAA" for ipv6
	 */
	const std::string getIPFamilyType(const std::string &addr) const;

	/*! \brief Get the DNS record assocated with domain_name
		\param domain_name name of domain to search for
		\param type record type A or AAAA
		\returns the DNSRecord of the record. bError is true if no record could be found.
	*/
	const DNSRecord executeDNSFetch(const std::string &domain_name,
								const std::string &record_type);

	/*! \brief Deletes a DNS record
		\param domain domain name to delete
		\param value ip address that the record points to
		\param type record type A or AAAA
	*/
	void executeDNSDelete(const std::string &domain,
							const std::string &value,
							const std::string &type);

	/*! \brief Adds a DNS record
		\param domain domain name to add
		\param value ip address to point to
		\param type record type A or AAAA
	*/
	void executeDNSAdd(const std::string &domain,
						const std::string &value,
						const std::string &type);

	/*! \brief builds an http query string
		\param method_name DH API method to call
		\param optional additional key/value pairs to add to the query string.
		\returns built query string
	*/
	const std::string buildQueryString(const std::string method_name,
				const HttpParameterMap &optional =  HttpParameterMap() ) const;

};
