using Newtonsoft.Json.Linq;

namespace SharedModels
{
    public class SessionKeyJsonValue
    {
        public string Key { get; set; }
        public JObject JsonValue { get; set; }
    }
}
