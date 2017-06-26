using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Net.Mail;

namespace lazMonitor
{
    public partial class LazMonitor : ServiceBase
    {
        public LazMonitor()
        {
            InitializeComponent();
            if (!EventLog.SourceExists("MySource"))
            {
                EventLog.CreateEventSource("MySource", "MyNewLog");
            }
            evLog = new EventLog();
            evLog.Source = "MySource";
            evLog.Log = "MyNewLog";
        }
        public Entities.Configuracion configuracion { get; set; } = new Entities.Configuracion();
        public bool processOk = false;
        private bool LoadConfiguracion()
        {
            bool Success = false;
            try
            {
                configuracion.Host = ConfigurationManager.AppSettings["Host"].ToString();
                configuracion.MailSender = ConfigurationManager.AppSettings["MailSender"].ToString();
                configuracion.Password = ConfigurationManager.AppSettings["Password"].ToString();
                configuracion.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"].ToString());
                configuracion.ProcessName = ConfigurationManager.AppSettings["ProcessName"].ToString();
                configuracion.ServerName = ConfigurationManager.AppSettings["ServerName"].ToString();
                configuracion.TimeInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TimeInterval"].ToString());
                foreach (var correo in ConfigurationManager.AppSettings["Destinataries"].ToString().Split(','))
                {
                    configuracion.Mails.Add(new Entities.DestinationMail(correo));
                }
                Success = true;
            }
            catch(Exception exc)
            {
                evLog.WriteEntry("Error al cargar la configuración: " + exc.Message, EventLogEntryType.Error);
            }
            return Success;
        }
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);
        protected override void OnStart(string[] args)
        {
            evLog.WriteEntry("Iniciando servicio");
            if(!LoadConfiguracion())
            {
                configuracion.Host = "smtp.gmail.com";
                configuracion.Mails.Add(new Entities.DestinationMail("noname@fujifilm.com.mx"));
                configuracion.MailSender = "noname@fujifilm.com.mx";
                configuracion.Password = "password";
                configuracion.Port = 587;
                configuracion.ProcessName = "notepad";
                configuracion.ServerName = "UNKNOWN";
                configuracion.TimeInterval = 300000;
            }
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 50000 + configuracion.TimeInterval;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // Set up a timer to trigger every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = configuracion.TimeInterval;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //evLog.WriteEntry("Monitoreando el proceso: " + configuracion.ProcessName, EventLogEntryType.Information);
            Process[] processlist = Process.GetProcesses();
            processOk = false;
            string pid = "";
            foreach (var process in processlist)
            {
                if (process.ProcessName == configuracion.ProcessName)
                {
                    processOk = true;
                    pid = process.Id + "";
                }
            }
            if (!processOk)
            {
                if(SendMail("ERROR EN EL SERVIDOR: " + configuracion.ServerName, "El proceso " + configuracion.ProcessName + " está inactivo."))
                    evLog.WriteEntry("Se ha enviado una notificación sobre la inactividad del proceso " + configuracion.ProcessName, EventLogEntryType.Warning);
            }
        }
        protected override void OnStop()
        {
            evLog.WriteEntry("Servicio detenido.");
        }
        protected override void OnContinue()
        {
            base.OnContinue();
            LoadConfiguracion();
            evLog.WriteEntry("El servicio continua.");
        }
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };
        /// <summary>
        /// Método para enviar el correo
        /// </summary>
        /// <param name="Asunto"></param>
        /// <param name="Mensaje"></param>
        /// <returns></returns>
        bool SendMail(string Asunto, string Mensaje)
        {
            bool Enviado = false;
            try
            {
                SmtpClient client = new SmtpClient();
                client.Port = configuracion.Port;
                client.Host = configuracion.Host;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(configuracion.MailSender, configuracion.Password);
                MailAddress From = new MailAddress("noreply@fujifilm.mx");
                MailMessage message = new MailMessage();
                message.From = From;
                message.Sender = new MailAddress("noreply@fujifilm.mx", "No Reply");
                message.ReplyToList.Add(new MailAddress("noreply@fujifilm.mx", "No Reply"));
                foreach (var destinatario in configuracion.Mails)
                    message.To.Add(new MailAddress(destinatario.Mail.Trim()));
                message.Subject = Asunto;
                string FilePath = string.Empty;
                List<string> ConjuntoCertificados = new List<string>();
                message.Body = Mensaje;
                message.IsBodyHtml = true;
                client.Send(message);
                Enviado = true;
            }
            catch (Exception exc)
            {
                evLog.WriteEntry("Error al intentar enviar el correo: " + exc.Message, EventLogEntryType.Error);
            }
            return Enviado;
        }
        #region Test
        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }
        #endregion
    }
}
