using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IFLEGameLauncher.Model
{
    public class ActiveUserResponse
    {
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
