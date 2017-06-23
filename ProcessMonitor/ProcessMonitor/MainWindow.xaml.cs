using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ProcessMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dt = new DispatcherTimer();
        Stopwatch sw = new Stopwatch();
        string currentTime = string.Empty;
        bool play = false;
        bool processOk = true;
        public string Proceso { get; set; } = "notepad";
        public int Segundos { get; set; } = 10;
        TimeSpan top;
        BackgroundWorker worker;
        BackgroundWorker CargarGrid;
        private MonitorEntities db;
        public List<Notificador.NotificacionModelo> lst_Notificaciones { get; set; } = new List<Notificador.NotificacionModelo>();
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                using (db = new MonitorEntities())
                {
                    if(db.Procesos.Any())
                    {
                        var configuracionProceso = db.Procesos.First();
                        Proceso = configuracionProceso.vchNombreProceso;
                        Segundos = Convert.ToInt32(configuracionProceso.vchTiempoEspera);
                    }                    
                    dt.Tick += new EventHandler(dt_Tick);
                    dt.Interval = new TimeSpan(0, 0, 0, 0, 100);
                    btnPlay.Click += BtnPlay_Click;
                    play = false;
                    btnPlay.Content = "Iniciar";
                    top = new TimeSpan(0, 0, Segundos);
                    currentTime = string.Format("{0:00}:{1:00}",
                        top.Minutes, top.Seconds);
                    Lbl_Crono.Content = currentTime;
                    LblProceso.Content = Proceso;
                    worker = new BackgroundWorker();
                    worker.DoWork += Worker_DoWork;
                    worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                    mnProceso.Click += MnProceso_Click;
                    btnDestinatarios.Click += BtnDestinatarios_Click;
                    btnConfigurarCorreo.Click += BtnConfigurarCorreo_Click;
                    CargarGrid = new BackgroundWorker();
                    CargarGrid.DoWork += CargarGrid_DoWork;
                    CargarGrid.RunWorkerCompleted += CargarGrid_RunWorkerCompleted;
                    CargarGrid.RunWorkerAsync();
                    sw.Start();
                    dt.Start();
                    play = true;
                    btnPlay.Content = "Detener";
                    mnConfiguracion.Visibility = Visibility.Collapsed;
                }
            }
            catch(Exception exc)
            {
                MessageBox.Show("Error en la aplicación: " + exc.Message);
                Close();
            }            
        }

        private void CargarGrid_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dgNotificaciones.ItemsSource = lst_Notificaciones;
        }

        private void CargarGrid_DoWork(object sender, DoWorkEventArgs e)
        {
            using (db = new MonitorEntities())
            {
                if(db.NotificacionesEnviadas.Any())
                {
                    lst_Notificaciones = (from n in db.NotificacionesEnviadas.ToList()
                                          select new Notificador.NotificacionModelo
                                          {
                                              Correo = n.CorreosDestinatarios.vchCorreo,
                                              Fecha = n.datFechaEnviado.ToString(),
                                              Nombre = n.CorreosDestinatarios.vchNombre                                          
                                          }).ToList();
                }
            }
        }
        private void BtnConfigurarCorreo_Click(object sender, RoutedEventArgs e)
        {
            Notificador.ConfigurarCorreo w = new Notificador.ConfigurarCorreo();
            w.ShowDialog();
        }

        private void BtnDestinatarios_Click(object sender, RoutedEventArgs e)
        {
            Notificador.Destinatarios chdWindow = new Notificador.Destinatarios();
            chdWindow.ShowDialog();
        }
        private void MnProceso_Click(object sender, RoutedEventArgs e)
        {
            Proceso.ConfiguracionProceso chdWindow = new Proceso.ConfiguracionProceso();
            chdWindow.ParentWindow = this;
            chdWindow.ShowDialog();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            sw.Reset();
            Process[] processlist = Process.GetProcesses();
            processOk = false;
            string pid = "";
            foreach(var process in processlist)
            {
                if (process.ProcessName == Proceso)
                {
                    processOk = true;
                    pid = process.Id + "";
                }
            }
            if(!processOk)
            {
                SendMail("ERROR EN EL SERVIDOR", "El proceso " + Proceso + " está inactivo.");
            }
        }
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            currentTime = string.Format("{0:00}:{1:00}",
                top.Minutes, top.Seconds);
            Lbl_Crono.Content = currentTime;
            if(!processOk)
                CargarGrid.RunWorkerAsync();
            sw.Start();
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if(play)
            {
                if (sw.IsRunning)
                {
                    sw.Stop();
                    play = false;
                    btnPlay.Content = "Continuar";
                    mnConfiguracion.Visibility = Visibility.Visible;
                }
            }
            else
            {
                sw.Start();
                dt.Start();
                play = true;
                btnPlay.Content = "Detener";
                mnConfiguracion.Visibility = Visibility.Collapsed;
            }
        }
        private void dt_Tick(object sender, EventArgs e)
        {
            if (sw.IsRunning)
            {
                top = new TimeSpan(0,0, Segundos);
                var r = top - sw.Elapsed;
                currentTime = string.Format("{0:00}:{1:00}",
                r.Minutes, r.Seconds);
                if(r.Seconds >= 0)
                    Lbl_Crono.Content = currentTime;
                if (r.Seconds <= 0)
                {
                    worker.RunWorkerAsync();
                }
            }
        }
        public void Reload(string process, int seconds)
        {
            Proceso = process;
            Segundos = seconds;
            top = new TimeSpan(0, 0, Segundos);
            currentTime = string.Format("{0:00}:{1:00}",
                top.Minutes, top.Seconds);
            Lbl_Crono.Content = currentTime;
            LblProceso.Content = Proceso;
        }

        bool SendMail(string Asunto, string Mensaje)
        {
            bool Enviado = false;
            try
            {
                using (db = new MonitorEntities())
                {
                    if (db.CorreoNotificador.Any())
                    {
                        var conf = db.CorreoNotificador.First();
                        if (db.CorreosDestinatarios.Any(i => i.bitActivo))
                        {
                            SmtpClient client = new SmtpClient();
                            client.Port = Convert.ToInt32(conf.vchPuerto);
                            client.Host = conf.vchHost;
                            client.EnableSsl = true;
                            client.DeliveryMethod = SmtpDeliveryMethod.Network;
                            client.UseDefaultCredentials = false;
                            client.Credentials = new System.Net.NetworkCredential(conf.vchCorreo, conf.vchPassword);
                            MailAddress From = new MailAddress("noreply@fujifilm.mx");
                            MailMessage message = new MailMessage();
                            message.From = From;
                            message.Sender = new MailAddress("noreply@fujifilm.mx", "No Reply");
                            message.ReplyToList.Add(new MailAddress("noreply@fujifilm.mx", "No Reply"));
                            string[] Destinatarios = (from d in db.CorreosDestinatarios where d.bitActivo select d.vchCorreo).ToArray();
                            foreach (string destinatario in Destinatarios)
                                message.To.Add(new MailAddress(destinatario.Trim()));
                            message.Subject = Asunto;
                            string FilePath = string.Empty;
                            List<string> ConjuntoCertificados = new List<string>();
                            message.Body = Mensaje;
                            message.IsBodyHtml = true;
                            client.Send(message);
                            Enviado = true;
                            DateTime fecha = DateTime.Now;
                            foreach(var d in db.CorreosDestinatarios.Where(i=>i.bitActivo))
                            {
                                NotificacionesEnviadas n = new NotificacionesEnviadas();
                                n.datFechaEnviado = fecha;
                                n.id_correo = d.id;
                                n.id_proceso = db.Procesos.First().id;
                                db.NotificacionesEnviadas.Add(n);
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            MessageBox.Show("No existen destinatarios habilitados para notificar", "Error");
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error al intentar enviar el correo: " + exc.Message,"Error");
            }
            return Enviado;
        }
    }
}
