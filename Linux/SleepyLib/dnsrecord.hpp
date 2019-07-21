#pragma once
#include <string>

/*! \brief Represents a Dreamhost DNS record
 */
struct DNSRecord
{
public:
	std::string accountID;	//!< \brief AccountID of account that owns this record.
	std::string Comment;	//!< \brief optional comment that is attached to the record.
	std::string Editable;	//!< \brief value that represents if the record is editable by the user.
	std::string Record;		//!< \brief record/domain name e.g. www.example.com
	std::string Type;		//!< \brief type of record e.g. A/MX/AAA/TXT
	std::string Value;		//!< \brief value of record e.g. 127.0.0.1
	std::string Zone;		//!< \brief zone that the record belongs to.
	bool bError = false;	//!< \brief error state.

	DNSRecord()
	{
		reset();;
	}

	/*! \brief clears the record */
	void reset()
	{
		accountID = "";
		Comment = "";
		Editable = "";
		Record = "";
		Type = "";
		Value = "";
		Zone = "";
	}
};
