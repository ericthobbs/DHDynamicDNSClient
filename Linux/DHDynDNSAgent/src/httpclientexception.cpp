/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include "httpclientexception.hpp"

HttpClientException::HttpClientException(const std::string &msg)
{
	errorMsg = msg;
}

const char* HttpClientException::what() const throw()
{
	return errorMsg.c_str();
}
