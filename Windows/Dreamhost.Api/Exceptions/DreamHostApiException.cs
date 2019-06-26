using System;
using System.Collections.Generic;
using System.Text;

namespace Dreamhost.Api.Exceptions
{
    public class DreamHostApiException : Exception
    {
        private readonly ApiResult _result;
        public string command { get; set; }
        public IDictionary<string,string> commandParameters { get; set; } = new Dictionary<string, string>();

        public DreamHostApiException(ApiResult apiResult) : base(apiResult.Reason)
        {
            _result = apiResult;
        }

        public string DreamhostErrorCode()
        {
            return _result.Data.ToString();
        }
    }

    public class DnsRecordAlreadyExistsException : DreamHostApiException
    {
        

        public DnsRecordAlreadyExistsException(ApiResult result) : base(result)
        {

        }

        public DnsRecordAlreadyExistsException(ApiResult result, string command, IDictionary<string, string> additionalParameters) : base(result)
        {
            this.command = command;
            commandParameters = additionalParameters;
        }
    }
}
