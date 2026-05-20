using System.Windows;
using System.Windows.Input;

namespace DobbleGame
{
    public partial class VentanaIngresoCodigoSala : Window
    {
        public PaginaSala Sala { get; set; }
        public VentanaIngresoCodigoSala()
        {
            InitializeComponent();
        }

        private void BtnAceptar(object sender, RoutedEventArgs e)
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            var codigoSala = tbCodigoSala.Text;

            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_CamposVacíos);
                return;
            }

            var paginaSala = new PaginaSala(false, codigoSala);

            if (!paginaSala.ExisteSala())
            {
                Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_SalaInexistente);
                return;
            }

            if (!paginaSala.EsSalaDisponible())
            {
                Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_SalaEnPartida);
                return;
            }

            if (!paginaSala.HayEspacioEnSala())
            {
                Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_SalaLlena);
                return;
            }

            if (!paginaSala.IniciarSesionSala())
            {
                Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_ErrorInesperado);
                return;
            }

            Sala = paginaSala;
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancelar(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OcultarDialogo(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource != panelMensaje && panelMensaje.Visibility == Visibility.Visible)
            {
                panelMensaje.Visibility = Visibility.Hidden;
            }
        }
    }
}
