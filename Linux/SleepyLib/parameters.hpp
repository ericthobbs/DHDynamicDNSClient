#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <string>
#include <boost/utility.hpp>

/*! \brief Simple Command Line Parser that uses popt
 */
class Parameters : boost::noncopyable
{
public:

	Parameters();   //!< constructs a new instance of the parser
	~Parameters();

    //! \brief parses the arguments passed from the command line.
    //! \param argc count of arguments
    //! \param argv array of arguments
	void parse(const int argc, const char **argv);

    //! \brief path to the configuration file
	const std::string configFile() const;

	//! \brief path to the lock file
	const std::string lockFile() const;

	//! \brief check url
	const std::string checkURL() const;

	//! \brief if true, user wants to ignore the lock file
	bool forceStart() const;

	//! \brief user wants to see the version information
	bool showVersion() const;

	//! \brief display extra information if true
	bool verbose() const;

	//! \brief do not fork the daemon.
	bool noForking() const;

	bool dropPrivileges() const;

private:
	char *config_file;  //!< \brief holds the path to the configuration file
	char *lock_file;    //!< \brief holds the path to the lock file
	char *check_url;	//!< \brief holds the url to the ip check script.
	int force_start;	//!< \brief	user wants to force start the daemon
	int show_version;	//!< \brief user wants to only show version/copyright info
	int verbose_mode;	//!< \brief dump extra information
	int dont_fork;		//!< \brief no forking
	int drop_privileges;//!< \brief drop user privileges
};
