/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include "httpclient.hpp"
#include "httpclientexception.hpp"
#include <cstring>
#include <memory>

std::atomic<int> HttpClient::s_use_count(0);

HttpClient::HttpClient(const std::string &user_agent)
{
    if(s_use_count == 0)
        curl_global_init(CURL_GLOBAL_ALL);
    s_use_count++;

	curl_handle = curl_easy_init();
	curl_easy_setopt(curl_handle, CURLOPT_USERAGENT, user_agent.c_str());
}

HttpClient::~HttpClient()
{
    s_use_count--;
	curl_easy_cleanup(curl_handle);

	if(s_use_count == 0)
        curl_global_cleanup();
}

const std::string HttpClient::getResponse(const std::string &url)
{
	std::string memory;

	auto del = [](char * p) { curl_free(p); };

	std::unique_ptr<char, decltype(del)>
		safe_url( curl_easy_escape(curl_handle,url.c_str(), url.size()), del );

	curl_easy_setopt(curl_handle, CURLOPT_URL, url.c_str() );
	curl_easy_setopt(curl_handle, CURLOPT_WRITEFUNCTION, HttpClient::WriteCallback);
	curl_easy_setopt(curl_handle, CURLOPT_WRITEDATA, (void*)&memory);
	curl_easy_setopt(curl_handle, CURLOPT_FORBID_REUSE, 1);

	CURLcode res = curl_easy_perform(curl_handle);

	if(res != CURLE_OK)
		throw HttpClientException(curl_easy_strerror(res));

	return memory;
}

size_t HttpClient::WriteCallback(void *contents, size_t size, size_t nmemb, void *userp)
{
	std::string &mem = *static_cast<std::string*>(userp);
	std::string buffer;
	buffer.resize(size*nmemb);

	memcpy(&buffer.front(), (char**)contents, size * nmemb );

	mem.append(buffer);

	return size * nmemb;
}
