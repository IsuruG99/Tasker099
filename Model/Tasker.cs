using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasker099.Model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class Tasker
    {

        public string? Date { get; set; }
        public string? Interval { get; set; }
        public string? IntervalReset { get; set; }
        public string? Name { get; set; }
        public string? ResetDay { get; set; }
        public string? Type { get; set; }
        public bool? Checked { get; set; }
        public string? CheckedTime { get; set; }
        public string? DisplayText { get; internal set; }
        public string? ImgSource { get; set; }
    }
}
