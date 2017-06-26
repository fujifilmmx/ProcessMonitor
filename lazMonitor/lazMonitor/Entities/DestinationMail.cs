using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lazMonitor.Entities
{
    public class DestinationMail
    {
        public DestinationMail(string mail)
        {
            Mail = mail;
            Name = "";
        }
        public string Mail { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
