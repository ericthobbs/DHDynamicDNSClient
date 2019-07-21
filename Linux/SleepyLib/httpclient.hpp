#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <string>
#include <atomic>
#include <future>

#include <curl/curl.h>

/*! \brief Simple HTTP client based on cURL.
 *
 *
 *  Simple HTTP client that gets a response from a remote resource such as HTTP.
 */
class HttpClient
{
public:

    /*! \brief Construct a new instance of the Http Client.
     * \param user_agent the user agent string that the client will send upon connecting to an http server.
     */
	HttpClient(const std::string &user_agent);

	~HttpClient();

    /*! \brief Get the response from the Http server
        \param url the query string, e.g. www.badpointer.net/?my_query=23
        \return the response as a string.
        \exception HttpClientException upon error
     */
	std::string getResponse(const std::string &url);

private:
	CURL *curl_handle;  //!< handle to the curl instance used by http client
	static std::atomic<bool> sCurlHasBeenAllocated;
};
