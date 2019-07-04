#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <string>
#include <map>
#include <vector>
#include <xercesc/framework/MemBufInputSource.hpp>

#include "dnsrecord.hpp"

#include <vector>

class DHXMLReader
{
public:
	DHXMLReader(const xercesc::MemBufInputSource &buffer);

	~DHXMLReader();

	std::vector<DNSRecord> getRecords() const;

	bool success() const;
	const std::string& getError() const;

private:
	std::vector<DNSRecord> records;
	std::string response_message;
	std::string error_msg;
};
