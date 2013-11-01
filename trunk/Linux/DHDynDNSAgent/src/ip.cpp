#include <iostream>
#include <fstream>
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

/*! \file ip.cpp
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

#include "configfileexception.hpp"



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
	}
}

std::ostream& showVersion(std::ostream &out)
{
	out << "Compiled on " << __DATE__ << " at " << __TIME__ <<  std::endl
		<< "(c) 2013 Eric Hobbs / www.badpointer.net" << std::endl
		<< "CURL version....: " << curl_version() << std::endl
		<< "Xerces version..: " << gXercesFullVersionStr << std::endl;
	return out;
}
