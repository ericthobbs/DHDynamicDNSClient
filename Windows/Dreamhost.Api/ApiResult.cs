using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dreamhost.Api
{
    public class ApiResult
    {
        public string Result { get; set; }

        /// <summary>
        /// Dynamic Data - whats in here depends on the api call and the result.
        /// </summary>
        public dynamic Data { get; set; }

        public string Reason { get; set; }
    }

    public class AvailableCommandData
    {
        public string cmd { get; set; }
        public object order { get; set; }
        public object args { get; set; }
        public object optargs { get; set; }
    }

}
