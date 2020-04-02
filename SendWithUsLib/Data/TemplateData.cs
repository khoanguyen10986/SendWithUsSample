using System.Collections.Generic;

namespace SendWithUsLib
{
    public class TemplateData
    {
        public string TemplateId { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public TemplateData()
        {
            Data = new Dictionary<string, object>();
        }
    }
}
