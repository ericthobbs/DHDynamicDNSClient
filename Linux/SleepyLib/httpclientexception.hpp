#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <exception>
#include <string>

/*! \brief HTTP Client Exception
 *
 */
class HttpClientException: virtual std::exception
{
public:

	/*! \brief Constructs a new http client exception
	 *  \param msg the error message
	 */
	HttpClientException(const std::string &msg);

	/*! \brief message
	 */
	virtual const char* what() const throw();

private:
	std::string errorMsg; //!< \brief error message
};
