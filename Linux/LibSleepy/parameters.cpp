/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include "parameters.hpp"

#include <popt.h>

namespace
{
	const std::string CONFIG_FILE = "/etc/dhdns/config.xml";
	const std::string LOCK_FILE   = "/tmp/.dhdnsagent.pid";
	const int         FORCE_START = 0;
	const int         SHOW_VERSION = 0;
	const int         VERBOSE_MODE = 0;
	const int         FORK = 0;
	const int         DROP_PRIVILEGES = 0;
}

Parameters::Parameters()
{
	config_file = nullptr;
	lock_file = nullptr;
	force_start = FORCE_START;
	show_version = SHOW_VERSION;
	verbose_mode = VERBOSE_MODE;
	dont_fork = FORK;
	drop_privileges = DROP_PRIVILEGES;
}

Parameters::~Parameters()
{
	delete config_file;
	delete lock_file;
}

void Parameters::parse(const int argc, const char ** argv)
{
	poptOption optionsTable[] = {
		{ "config-file",	'c',	POPT_ARG_STRING,	static_cast<void*>(&config_file),	0, "configuration file to use", "FILE" },
		{ "lock-file",		'l',	POPT_ARG_STRING,	static_cast<void*>(&lock_file)  ,	0, "lock file", "FILE" },
		{ "force",			'f',	POPT_ARG_NONE,	&force_start,						0, "ignore lock file", NULL},
		{ "version",       0,		POPT_ARG_NONE,	&show_version,						0, "show version information", NULL},
		{ "verbose",		'v',	POPT_ARG_NONE,	&verbose_mode,						0, "output more information", NULL},
		{ "dont-fork",		'd',	POPT_ARG_NONE,	&dont_fork,							0, "Don't Fork", NULL },
		{"drop-privileges",	'p',	POPT_ARG_NONE,	&drop_privileges,					0, "Drop Privileges", NULL},
#if defined(IPCLIENT)
		{ "check-url",   'u', POPT_ARG_STRING, static_cast<void*>(&check_url)  , 0, "ip check url", "URL" }
#endif
		POPT_AUTOHELP
		POPT_TABLEEND
	};

	poptContext ctx = poptGetContext(argv[0],argc,argv,optionsTable,0);

	while( poptGetNextOpt(ctx) > 0 )
	{
		free(poptGetOptArg(ctx));
	}


	poptFreeContext(ctx);
}

const std::string Parameters::configFile() const
{
	if(config_file == nullptr)
		return CONFIG_FILE;
	return config_file;
}

const std::string Parameters::lockFile() const
{
	if(lock_file == nullptr)
		return LOCK_FILE;
	return lock_file;
}

const std::string Parameters::checkURL() const
{
	if(check_url == nullptr)
		return check_url;
	return check_url;
}

bool Parameters::forceStart() const
{
	return (bool)force_start;
}

bool Parameters::showVersion() const
{
	return (bool)show_version;
}

bool Parameters::verbose() const
{
	return (bool)verbose_mode;
}

bool Parameters::noForking() const
{
	return (bool)dont_fork;
}

bool Parameters::dropPrivileges() const
{
	return (bool)drop_privileges;
}
