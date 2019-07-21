/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include "dhsaxhandler.hpp"
#include "malformedxmlexception.hpp"
#include "util.hpp"
#include <syslog.h>

#include <memory>

XERCES_CPP_NAMESPACE_USE

namespace
{
	const std::string DREAMHOST_ROOTNODE = "dreamhost";
	const std::string DREAMHOST_DATANODE = "data";
	const std::string DREAMHOST_RESULTNODE = "result";
	const std::string DREAMHOST_SUCCESS = "success";

	const std::string NODE_ACCOUNTID = "account_id";
	const std::string NODE_COMMENT = "comment";
	const std::string NODE_EDITABLE = "editable";
	const std::string NODE_RECORD = "record";
	const std::string NODE_TYPE = "type";
	const std::string NODE_VALUE = "value";
	const std::string NODE_ZONE = "zone";
}

DHSAXHandler::DHSAXHandler(std::vector<DNSRecord> &rvec, std::string &res, std::string &err) : result(res), error_msg(err), records(rvec), depth(0)
{
	XMLCh *encoding = XMLString::transcode("UTF-8");

	formatter = new XMLFormatter(encoding,0,&target,XMLFormatter::CharEscapes,XMLFormatter::UnRep_Replace);

	XMLString::release(&encoding);

	in_data = false;
}

void DHSAXHandler::startElement(const XMLCh* const uri, const XMLCh* const localname,
									const XMLCh* const qname, const Attributes& attrs)
{
	char *uri_str = XMLString::transcode(uri);
	char *localname_str = XMLString::transcode(localname);
	char *qname_str = XMLString::transcode(qname);

	std::string currentNode(localname_str);

	if(depth.size() > 0)
		if(depth.back() == DREAMHOST_ROOTNODE && currentNode == DREAMHOST_DATANODE)
		in_data = true;

	depth.push_back(localname_str);

	XMLString::release(&uri_str);
	XMLString::release(&localname_str);
	XMLString::release(&qname_str);

}

void DHSAXHandler::fatalError(const SAXParseException& exception)
{
	char* message = XMLString::transcode(exception.getMessage());
	char* systemid = XMLString::transcode(exception.getSystemId());

	syslog(LOG_ERR,"%s: Malformed XML: %s on line %lu column %lu",
			systemid , message, exception.getLineNumber(), exception.getColumnNumber() );
	XMLString::release(&message);
	XMLString::release(&systemid);

	throw MalformedXMLException("Malformed XML document.");
}

void DHSAXHandler::endElement(const XMLCh* const uri, const XMLCh* const localname, const XMLCh* const qname)
{
	char *uri_str = XMLString::transcode(uri);
	char *localname_str = XMLString::transcode(localname);
	char *qname_str = XMLString::transcode(qname);

	std::string currentNode(localname_str);

	XMLString::release(&uri_str);
	XMLString::release(&localname_str);
	XMLString::release(&qname_str);

	std::string currentText((char*)target.getRawBuffer(),target.getLen());
	currentText = filterString(currentText);

	if(in_data)
	{
	if(depth.back() == NODE_ACCOUNTID)
	{
		currentData.accountID = currentText;
	}
	else if(depth.back() == NODE_COMMENT)
	{
		currentData.Comment = currentText;
	}
	else if(depth.back() == NODE_EDITABLE)
	{
		currentData.Editable = currentText;
	}
	else if(depth.back() == NODE_RECORD)
	{
		currentData.Record = currentText;
	}
	else if(depth.back() == NODE_TYPE)
	{
		currentData.Type = currentText;
	}
	else if(depth.back() == NODE_VALUE)
	{
		currentData.Value = currentText;
	}
	else if(depth.back() == NODE_ZONE)
	{
		currentData.Zone = currentText;
	}
	else
	{
		error_msg = currentText;
	}
}

	if(currentNode == DREAMHOST_DATANODE)
	{
		in_data = false;
		records.push_back(currentData);
		currentData.reset();
	}

	if(depth.back() == DREAMHOST_RESULTNODE)
	{
		syslog(LOG_DEBUG,"result from API Call: %s",currentText.c_str());
		result = currentText;
	}

	depth.pop_back();
	target.reset();
}

void DHSAXHandler::characters(const XMLCh* const chars, const XMLSize_t length)
{
	formatter->formatBuf(chars, length, XMLFormatter::NoEscapes);
}

bool DHSAXHandler::hasError() const
{
	return result != DREAMHOST_SUCCESS;
}
