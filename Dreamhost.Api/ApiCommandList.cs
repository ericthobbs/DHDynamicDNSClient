using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Dreamhost.Api
{
    public class ApiCommandList
    {
        public object order { get; set; }
        public object args { get; set; }
        public object optargs { get; set; }

        [JsonProperty(PropertyName = "cmd")]
        public string Command { get; set; }
    }
}
