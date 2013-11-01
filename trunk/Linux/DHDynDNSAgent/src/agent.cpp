#include <iostream>
#include <fstream>
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

/*! \file main.cpp
 *  \brief program entry point
 */

#include <cstring>
#include <cstdlib>
#include <unistd.h>
#include <boost/lexical_cast.hpp>

#include <sys/stat.h>
#include <sys/types.h>
#include <signal.h>

#include <curl/curl.h>
#include <xercesc/util/XercesVersion.hpp>

#include "daemon.hpp"
#include "configfileexception.hpp"


/*! \brief check if the daemon is already running
 *  \param path to the lock/pid file
 *  \returns true if the daemon is already running or false if not.
 */
bool childIsRunning(const std::string &lock_file);

/*! \brief print version information to a ostream
 *  \param output stream to print to
 *  \returns the outputstream containing the output
 */
std::ostream& showVersion(std::ostream &os);

/*! \brief main function
    \param argc count of arguments
    \param argv array of arguments
    \returns exit code
*/
int main(int argc, char** argv)
{
	Parameters p;
	p.parse(argc,const_cast<const char**>(argv));

	if(p.showVersion())
	{
		showVersion(std::cout);
		return EXIT_SUCCESS;
	}

	try
	{
		Daemon daemon(p);

		if( childIsRunning( p.lockFile() ) )
		{
			std::cerr << argv[0] << " (" << getpid() << ") Error - agent is already running" << std::endl;
			return EXIT_FAILURE;
		}

		if( daemon.daemonize() )
		{
			return daemon.runMainLoop();
		}
		else
		{
			std::cerr << argv[0] << " (" << getpid()  << ") Failed to fork child process. Exiting." << std::endl;
			return EXIT_FAILURE;
		}
	}
	catch(ConfigFileException &ex)
	{
		std::cerr << "Failed to read configuration file, error is: " << ex.what() << std::endl;
		return EXIT_FAILURE;
	}
}

bool childIsRunning(const std::string &lock_file)
{
	struct stat s;
	int success = stat(lock_file.c_str(),&s);
	if(success == -1)
	{
		return false;
	}
	else
	{
		std::ifstream lockfilestrm;
		std::string pidstr;

		lockfilestrm.open(lock_file.c_str());
		lockfilestrm.seekg(0,std::ios::end);
		pidstr.resize(lockfilestrm.tellg());
		lockfilestrm.seekg(0,std::ios::beg);
		lockfilestrm.read(&pidstr.front(), pidstr.size());
		lockfilestrm.close();

		if(pidstr.empty())
		{
			std::cerr << "invalid lock file, ignoring..." << std::endl;
			return false;
		}

		int pid = 0;
		try
		{
			//Remove spaces and letters
			pidstr.erase(remove_if(pidstr.begin(),pidstr.end(), isspace), pidstr.end());
			pidstr.erase(remove_if(pidstr.begin(),pidstr.end(), isalpha), pidstr.end());
			pid = boost::lexical_cast<int>(pidstr.c_str());
		}
		catch(boost::bad_lexical_cast const &ex)
		{
			std::cerr << "Invalid lock file, ignoring: \"" << pidstr << "\""  << std::endl;
			return false;
		}

		//check if the process exists
		success = kill(pid,0);
		if(success == -1)
		{
			std::cerr << "process does not exist. lock file is stale. Ignoring." << std::endl;
			return false;
		}
	}
	return true;
}

std::ostream& showVersion(std::ostream &out)
{
	out << "Compiled on " << __DATE__ << " at " << __TIME__ <<  std::endl
		<< "(c) 2013 Eric Hobbs / www.badpointer.net" << std::endl
		<< "CURL version....: " << curl_version() << std::endl
		<< "Xerces version..: " << gXercesFullVersionStr << std::endl;
	return out;
}
