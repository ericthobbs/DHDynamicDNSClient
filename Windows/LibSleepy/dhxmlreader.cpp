/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <iostream>
#include <stdexcept>
#include <memory>

#include "dhxmlreader.hpp"
#include "dhsaxhandler.hpp"

#include <xercesc/sax2/SAX2XMLReader.hpp>
#include <xercesc/sax2/XMLReaderFactory.hpp>
#include <xercesc/sax2/DefaultHandler.hpp>
#include <xercesc/util/XMLString.hpp>

XERCES_CPP_NAMESPACE_USE

DHXMLReader::DHXMLReader(const xercesc::MemBufInputSource &buffer)
{
	std::unique_ptr<SAX2XMLReader> parser(XMLReaderFactory::createXMLReader());
	//parser->setFeature(XMLUni::fgSAX2CoreValidation, true);
	parser->setFeature(XMLUni::fgSAX2CoreNameSpaces, true);

	std::unique_ptr<DefaultHandler> defaultHandler( new DHSAXHandler(records,response_message,error_msg) );
	parser->setContentHandler( std::addressof(*defaultHandler) );
	parser->setErrorHandler( std::addressof(*defaultHandler) );

	try
	{
		parser->parse(buffer);

	}
	catch(const XMLException &ex)
	{
		char *msg = XMLString::transcode(ex.getMessage());
		std::cerr << "XMLException...." << msg << std::endl;
		XMLString::release(&msg);

		throw std::runtime_error("");
	}
	catch(const SAXParseException &ex)
	{
		char *msg = XMLString::transcode(ex.getMessage());
		std::cerr << "SAXParseException...." << msg << std::endl;
		XMLString::release(&msg);

		throw std::runtime_error("");
	}
	catch(std::exception &ex)
	{
		std::cerr << "Whoops...." << ex.what() << std::endl;

		throw std::runtime_error("");
	}
}

DHXMLReader::~DHXMLReader()
{

}

std::vector<DNSRecord> DHXMLReader::getRecords() const
{
	return records;
}


bool DHXMLReader::success() const
{
	if(response_message == "success")
		return true;
	return false;
}

const std::string& DHXMLReader::getError() const
{
	return error_msg;
}
