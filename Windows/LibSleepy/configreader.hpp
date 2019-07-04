#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <string>
#include <map>
#include <vector>
#include <xercesc/framework/MemBufInputSource.hpp>


class ConfigReader
{
public:
	ConfigReader(const std::string &filename);

	ConfigReader(const xercesc::MemBufInputSource &buffer);

	~ConfigReader();

	std::string getSetting(const std::string &key);

private:

	std::map<std::string,std::string> keys;
};
