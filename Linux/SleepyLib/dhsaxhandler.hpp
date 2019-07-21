#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <xercesc/sax2/DefaultHandler.hpp>
#include <xercesc/framework/XMLFormatter.hpp>
#include <xercesc/framework/MemBufFormatTarget.hpp>

#include "dnsrecord.hpp"

#include <deque>
#include <string>
#include <vector>

class DHSAXHandler : public xercesc::DefaultHandler
{
public:

	DHSAXHandler(std::vector<DNSRecord> &records, std::string &result, std::string &error);

	void startElement(const XMLCh* const uri,
						const XMLCh* const localname,
						const XMLCh* const qname,
						const xercesc::Attributes& attrs);

	void fatalError(const xercesc::SAXParseException&);

	void endElement(const XMLCh* const uri,
					const XMLCh* const localname,
					const XMLCh* const qname);

	void characters(const XMLCh* const chars,
					const XMLSize_t length);

	bool hasError() const;

private:
	std::deque<std::string> depth{};
	xercesc::XMLFormatter *formatter;
	xercesc::MemBufFormatTarget target;

	bool in_data;

	std::string &result;
	std::string &error_msg;

	std::vector<DNSRecord> &records;
	DNSRecord currentData;
};
