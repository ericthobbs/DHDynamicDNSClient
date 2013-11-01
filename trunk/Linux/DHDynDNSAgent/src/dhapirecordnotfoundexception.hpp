#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <exception>
#include <string>

class DHApiRecordNotFoundException: virtual std::exception
{
public:

	DHApiRecordNotFoundException(const std::string &msg);

	virtual const char* what() const throw();

private:
	std::string errorMsg;
};
