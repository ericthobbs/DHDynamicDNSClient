using System;
using System.Collections.Generic;
using System.Text;

namespace Dreamhost.Api.Exceptions
{
    public class DreamHostApiException : Exception
    {
        private readonly ApiResult _result;

        public DreamHostApiException(ApiResult apiResult) : base(apiResult.Reason)
        {
            _result = apiResult;
        }

        public string DreamhostErrorCode()
        {
            return _result.Data.ToString();
        }
    }
}
