/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include "util.hpp"

#include <algorithm>
#include <boost/algorithm/string.hpp>

std::string filterString(const std::string &raw)
{
	std::string output = raw;

	output.erase(std::remove(output.begin(), output.end(), '\n'), output.end());
	boost::algorithm::trim(output);
	return output;
}

