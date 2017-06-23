using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
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
    /// Interaction logic for Destinatarios.xaml
    /// </summary>
    public partial class Destinatarios : Window
    {
        private MonitorEntities db;
        private readonly BackgroundWorker CargaDestinatarios = new BackgroundWorker();
        private List<DestinatarioModel> lst_destinatarios { get; set; } = new List<DestinatarioModel>();        
        public Destinatarios()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.Height;
            btnAgregarDestinatario.Click += BtnAgregarDestinatario_Click;
            CargaDestinatarios.DoWork += CargaDestinatarios_DoWork;
            CargaDestinatarios.RunWorkerCompleted += CargaDestinatarios_RunWorkerCompleted;
            CargaDestinatarios.RunWorkerAsync();
            CollectionViewSource itemCollectionViewSource;
            itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));
            itemCollectionViewSource.Source = lst_destinatarios;
            btnGuardarCambios.Click += BtnGuardarCambios_Click;
        }

        private void BtnGuardarCambios_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var list = (List<DestinatarioModel>)dgDestinatarios.ItemsSource;
                using (db = new MonitorEntities())
                {
                    foreach (var d in list)
                    {
                        var element = db.CorreosDestinatarios.First(i => i.id == d.Id);
                        element.bitActivo = d.Activo;
                    }
                    db.SaveChanges();
                }
                MessageBox.Show("Se han guardado los cambios correctamente", "Datos guardados");
                Close();
            }
            catch(Exception exc)
            {
                MessageBox.Show("Error en la aplicación: " + exc.Message, "Error");
            }
        }

        private void CargaDestinatarios_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (db = new MonitorEntities())
                {
                    lst_destinatarios = (from d in db.CorreosDestinatarios
                                         select new DestinatarioModel
                                         {
                                             Activo = d.bitActivo,
                                             Correo = d.vchCorreo,
                                             Id = d.id,
                                             Nombre = d.vchNombre
                                         }).ToList();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error en la aplicación: " + exc.Message, "Error");
            }
        }
        private void CargaDestinatarios_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dgDestinatarios.ItemsSource = lst_destinatarios;
        }       

        private void BtnAgregarDestinatario_Click(object sender, RoutedEventArgs e)
        {
            if (string.Empty != txtCorreo.Text && string.Empty != txtNombre.Text)
            {
                if (IsValidMail(txtCorreo.Text))
                {
                    using (db = new MonitorEntities())
                    {
                        if(!db.CorreosDestinatarios.Any(i=>i.vchCorreo == txtCorreo.Text))
                        {
                            CorreosDestinatarios cd = new CorreosDestinatarios();
                            cd.vchNombre = txtNombre.Text;
                            cd.vchCorreo = txtCorreo.Text;
                            db.CorreosDestinatarios.Add(cd);
                            db.SaveChanges();
                            CargaDestinatarios.RunWorkerAsync();
                            MessageBox.Show("Se ha agregado al destinatario: " + txtNombre.Text, "Alta exitosa");
                            txtNombre.Text = "";
                            txtCorreo.Text = "";
                        }
                        else
                        {
                            MessageBox.Show("El correo ya está dado de alta", "Error");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("El correo es inválido", "Error");
                }
            }
            else
            {
                MessageBox.Show("Debes llenar los campos para poder agregar el destinatario", "Error");
            }
        }
        public bool IsValidMail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
