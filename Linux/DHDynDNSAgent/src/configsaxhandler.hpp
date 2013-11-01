#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <xercesc/sax2/DefaultHandler.hpp>
#include <xercesc/framework/XMLFormatter.hpp>
#include <xercesc/framework/MemBufFormatTarget.hpp>

#include <deque>
#include <string>
#include <map>

class ConfigSAXHandler : public xercesc::DefaultHandler
{
public:

	ConfigSAXHandler(std::map<std::string,std::string> &kv);

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

private:
	std::deque<std::string> depth;
	xercesc::XMLFormatter *formatter;
	xercesc::MemBufFormatTarget target;
	std::map<std::string,std::string> &pKeyValues;
};
