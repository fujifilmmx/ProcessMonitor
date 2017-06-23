using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProcessMonitor.Notificador
{
    /// <summary>
    /// Interaction logic for ConfigurarCorreo.xaml
    /// </summary>
    public partial class ConfigurarCorreo : Window
    {
        public ConfigurarCorreo()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.Height;
            LoadMail();
            btnGuardar.Click += BtnGuardar_Click;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (db = new MonitorEntities())
                {
                    if (db.CorreoNotificador.Any())
                    {
                        var correo = db.CorreoNotificador.First();
                        correo.vchCorreo = txtCorreo.Text;
                        correo.vchHost = txtHost.Text;
                        correo.vchPassword = txtPassword.Password;
                        correo.vchPuerto = txtPuerto.Text;
                    }
                    else
                    {
                        CorreoNotificador correo = new CorreoNotificador();
                        correo.vchCorreo = txtCorreo.Text;
                        correo.vchHost = txtHost.Text;
                        correo.vchPassword = txtPassword.Password;
                        correo.vchPuerto = txtPuerto.Text;
                        db.CorreoNotificador.Add(correo);
                    }
                    db.SaveChanges();
                    MessageBox.Show("La información se ha guardado correctamente", "Guardado");
                    Close();
                }
            }
            catch(Exception exc)
            {
                MessageBox.Show("Error en la aplicación: " + exc.Message, "Error");
            }
        }

        private MonitorEntities db;
        private void LoadMail()
        {
            try
            {
                using (db = new MonitorEntities())
                {
                    if (db.CorreoNotificador.Any())
                    {
                        var correo = db.CorreoNotificador.First();
                        txtCorreo.Text = correo.vchCorreo;
                        txtHost.Text = correo.vchHost;
                        txtPassword.Password = correo.vchPassword;
                        txtPuerto.Text = correo.vchPuerto;
                    }
                }
            }
            catch(Exception exc)
            {
                MessageBox.Show("Error en la aplicación LoadMail(): " + exc.Message, "Error");
            }
        }
    }
}
