/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include "httpclient.hpp"
#include "httpclientexception.hpp"
#include <cstring>
#include <memory>
#include <functional>
#include <cassert>

std::atomic<bool> HttpClient::sCurlHasBeenAllocated(false);

size_t writeCallback(char* contents, size_t size, size_t nmemb, std::string* buffer);

HttpClient::HttpClient(const std::string &user_agent)
{
	if (!sCurlHasBeenAllocated) {
		curl_global_init(CURL_GLOBAL_ALL);
		std::atexit([]{curl_global_cleanup();});
		sCurlHasBeenAllocated = true;
	}

	curl_handle = curl_easy_init();
	curl_easy_setopt(curl_handle, CURLOPT_VERBOSE, 1);
	curl_easy_setopt(curl_handle, CURLOPT_FOLLOWLOCATION, 1);
	curl_easy_setopt(curl_handle, CURLOPT_SSL_ENABLE_ALPN, 0);
	curl_easy_setopt(curl_handle, CURLOPT_USERAGENT, user_agent.c_str());
	curl_easy_setopt(curl_handle, CURLOPT_WRITEFUNCTION, &writeCallback);
}

HttpClient::~HttpClient()
{
	curl_easy_cleanup(curl_handle);
}

std::string HttpClient::getResponse(const std::string &url)
{
	std::string buffer;

	curl_easy_setopt(curl_handle, CURLOPT_WRITEFUNCTION, &writeCallback);
	curl_easy_setopt(curl_handle, CURLOPT_WRITEDATA, &buffer);
	curl_easy_setopt(curl_handle, CURLOPT_URL, url.c_str());

	CURLcode res = curl_easy_perform(curl_handle);

	if(res != CURLE_OK)
		throw HttpClientException(curl_easy_strerror(res));

	curl_easy_reset(curl_handle);
	
	return buffer;
}

size_t writeCallback(char* contents, size_t size, size_t nmemb, std::string* buffer) {
	size_t realsize = size * nmemb;
	if (buffer == NULL) {
		return 0;
	}
	buffer->append(contents, realsize);
	return realsize;
}
