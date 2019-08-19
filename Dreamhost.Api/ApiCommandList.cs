using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Dreamhost.Api
{
    public class ApiCommandList
    {
        [JsonProperty(PropertyName = "order")]
        public object Order { get; set; }

        [JsonProperty(PropertyName = "args")]
        public object Arguments { get; set; }

        [JsonProperty(PropertyName = "optargs")]
        public object OptionalArguments { get; set; }

        [JsonProperty(PropertyName = "cmd")]
        public string Command { get; set; }
    }
}
