/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include "dhapirecordnotfoundexception.hpp"


DHApiRecordNotFoundException::DHApiRecordNotFoundException(const std::string &msg)
{
	errorMsg = msg;
}

const char* DHApiRecordNotFoundException::what() const throw()
{
	return errorMsg.c_str();
}
