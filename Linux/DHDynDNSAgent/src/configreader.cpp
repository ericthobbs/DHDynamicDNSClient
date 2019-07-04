/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <iostream>
#include <stdexcept>
#include <memory>

#include "configreader.hpp"
#include "configfileexception.hpp"
#include "configsaxhandler.hpp"

#include <xercesc/sax2/SAX2XMLReader.hpp>
#include <xercesc/sax2/XMLReaderFactory.hpp>
#include <xercesc/sax2/DefaultHandler.hpp>
#include <xercesc/util/XMLString.hpp>

XERCES_CPP_NAMESPACE_USE

ConfigReader::ConfigReader(const std::string &file)
{
	std::unique_ptr<SAX2XMLReader> parser(XMLReaderFactory::createXMLReader());
	parser->setFeature(XMLUni::fgSAX2CoreValidation, true);
	parser->setFeature(XMLUni::fgSAX2CoreNameSpaces, true);

	std::unique_ptr<DefaultHandler> defaultHandler(new ConfigSAXHandler( keys ));
	parser->setContentHandler( std::addressof(*defaultHandler) );
	parser->setErrorHandler( std::addressof(*defaultHandler) );

	try
	{
		parser->parse(file.c_str());
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


ConfigReader::ConfigReader(const xercesc::MemBufInputSource &buffer)
{
	std::unique_ptr<SAX2XMLReader> parser(XMLReaderFactory::createXMLReader());
	parser->setFeature(XMLUni::fgSAX2CoreValidation, true);
	parser->setFeature(XMLUni::fgSAX2CoreNameSpaces, true);

	std::unique_ptr<DefaultHandler> defaultHandler(new ConfigSAXHandler( keys ));
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

ConfigReader::~ConfigReader() = default;

std::string ConfigReader::getSetting(const std::string &var)
{
	if(keys.count(var) > 0)
		return std::string(keys[var]);
	return std::string();
}
