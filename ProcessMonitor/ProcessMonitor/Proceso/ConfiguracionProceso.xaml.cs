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

namespace ProcessMonitor.Proceso
{
    /// <summary>
    /// Interaction logic for ConfiguracionProceso.xaml
    /// </summary>
    public partial class ConfiguracionProceso : Window
    {
        private MonitorEntities db;
        public MainWindow ParentWindow { get; set; }
        private long Id = 0;
        public ConfiguracionProceso()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.Height;
            btnGuardar.Click += BtnGuardar_Click;
            using (db = new MonitorEntities())
            {
                if(db.Procesos.Any())
                {
                    var p = db.Procesos.First();
                    txtNombreProceso.Text = p.vchNombreProceso;
                    txtTiempoEspera.Text = p.vchTiempoEspera;
                    Id = p.id;
                }
                else
                {
                    txtNombreProceso.Text = "notepad";
                    txtTiempoEspera.Text = "10";
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int Segundos = Convert.ToInt32(txtTiempoEspera.Text);
                try
                {
                    using (db = new MonitorEntities())
                    {
                        if (db.Procesos.Any(i => i.id == Id))
                        {
                            var p = db.Procesos.First(i => i.id == Id);
                            p.vchNombreProceso = txtNombreProceso.Text;
                            p.vchTiempoEspera = txtTiempoEspera.Text;
                            db.SaveChanges();
                            ParentWindow.Reload(txtNombreProceso.Text, Convert.ToInt32(txtTiempoEspera.Text));
                            Close();
                        }
                        else
                        {
                            Procesos p = new Procesos();
                            p.vchNombreProceso = txtNombreProceso.Text;
                            p.vchTiempoEspera = txtTiempoEspera.Text;
                            db.Procesos.Add(p);
                            db.SaveChanges();
                            ParentWindow.Reload(txtNombreProceso.Text, Convert.ToInt32(txtTiempoEspera.Text));
                            Close();
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Error en la aplicación: " + exc.Message);
                }
            }
            catch
            {
                MessageBox.Show("El tiempo de espera debe ser una unidad entera de segundos");
            }
            
        }
    }
}
