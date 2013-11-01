/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include "configsaxhandler.hpp"
#include "malformedxmlexception.hpp"
#include "util.hpp"

#include <iostream>
#include <cstdlib>
#include <algorithm>
#include <memory>
#include <syslog.h>

XERCES_CPP_NAMESPACE_USE

ConfigSAXHandler::ConfigSAXHandler(std::map<std::string, std::string> &kv) : pKeyValues(kv)
{
	XMLCh *encoding = XMLString::transcode("UTF-8");

	formatter = new XMLFormatter(encoding,0,&target,XMLFormatter::CharEscapes,XMLFormatter::UnRep_Replace);

	XMLString::release(&encoding);
}

void ConfigSAXHandler::startElement(const XMLCh* const uri, const XMLCh* const localname,
									const XMLCh* const qname, const Attributes& attrs)
{
	char *uri_str = XMLString::transcode(uri);
	char *localname_str = XMLString::transcode(localname);
	char *qname_str = XMLString::transcode(qname);

	depth.push_back(localname_str);

	XMLString::release(&uri_str);
	XMLString::release(&localname_str);
	XMLString::release(&qname_str);

}

void ConfigSAXHandler::fatalError(const SAXParseException& exception)
{
	char* message = XMLString::transcode(exception.getMessage());
	char* systemid = XMLString::transcode(exception.getSystemId());

	syslog(LOG_ERR,"%s: Malformed XML: %s on line %lu column %lu",
			systemid , message, exception.getLineNumber(), exception.getColumnNumber() );
	XMLString::release(&message);
	XMLString::release(&systemid);

	throw MalformedXMLException("Malformed XML document.");
}

void ConfigSAXHandler::endElement(const XMLCh* const uri, const XMLCh* const localname, const XMLCh* const qname)
{
	char *uri_str = XMLString::transcode(uri);
	char *localname_str = XMLString::transcode(localname);
	char *qname_str = XMLString::transcode(qname);

	std::string keypath;
	std::string currentText;
	for(std::string e : depth )
	{
		keypath += e + ".";
	}
	keypath = keypath.substr(0,keypath.length()-1);

	currentText.append((char*)target.getRawBuffer(),target.getLen());
	currentText = filterString(currentText);

	if(currentText.length() != 0)
	{
		syslog(LOG_DEBUG,"%s(len:%lu)='%s'",keypath.c_str(),
			currentText.length(),currentText.c_str());

		if(pKeyValues.count(keypath) > 0)
		{
			//key exists
			syslog(LOG_WARNING,"key %s already exists with value of %s.",
				keypath.c_str(), currentText.c_str());
		}
		else
		{
			pKeyValues.insert(
				std::pair<std::string,std::string>(keypath,currentText) );
		}
	}

	XMLString::release(&uri_str);
	XMLString::release(&localname_str);
	XMLString::release(&qname_str);

	depth.pop_back();

	target.reset();
}

void ConfigSAXHandler::characters(const XMLCh* const chars, const XMLSize_t length)
{
	formatter->formatBuf(chars, length, XMLFormatter::NoEscapes);
}
