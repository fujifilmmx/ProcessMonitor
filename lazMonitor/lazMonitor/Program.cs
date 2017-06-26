using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace lazMonitor
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        //static void Main()
        //{
        //    ServiceBase[] ServicesToRun;
        //    ServicesToRun = new ServiceBase[]
        //    {
        //        new LazMonitor()
        //    };
        //    ServiceBase.Run(ServicesToRun);
        //}
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                LazMonitor servicio = new LazMonitor();
                servicio.TestStartupAndStop(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                        new LazMonitor()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
