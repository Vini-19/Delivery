using System;
using System.Collections.Generic;
using System.Text;

namespace Delivery.Consumer.Helpers
{
    public class KafKaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;

    }
}
