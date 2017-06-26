using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lazMonitor.Entities
{
    public class Configuracion
    {
        public string ProcessName { get; set; } = string.Empty;
        public int TimeInterval { get; set; } = 0;
        public string ServerName { get; set; } = string.Empty;

        public string MailSender { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public string Password { get; set; } = string.Empty;
        public List<DestinationMail> Mails { get; set; } = new List<DestinationMail>();
    }
}
