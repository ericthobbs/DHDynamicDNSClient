#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <exception>
#include <string>

/*! \brief Malformed XML exception
 */
class MalformedXMLException: virtual std::exception
{
public:

	/*! \brief Constructs a new exception
	 *	\param msg message of the exception
	 */
	MalformedXMLException(const std::string &msg);

	/*! \brief get the text of the exception
	 */
	virtual const char* what() const throw();

private:
	std::string errorMsg; //!< \brief message
};
