#pragma once
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#include <string>

/*! \file util.hpp
 *  \brief Misc utility methods that do not belong anywhere else.
 */

/*! \brief Removes newlines and trims extra spaces from a string.
 *  \param raw the string to filter out newlines and trim spaces from.
 *  \returns the filtered string
 */
const std::string filterString(const std::string &raw);
