using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitor.Notificador
{
    public class DestinatarioModel
    {
        public long Id { get; set; } = 0;
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public bool Activo { get; set; } = false;
    }
}
