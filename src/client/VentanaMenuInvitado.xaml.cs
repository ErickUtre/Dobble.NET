using DobbleGame.Servidor;
using System.Windows;

namespace DobbleGame
{
    public partial class VentanaMenuInvitado : Window
    {
        public VentanaMenuInvitado()
        {
            InitializeComponent();
            Inicializar();
        }

        private void Inicializar()
        {
            MarcoPrincipal.NavigationService.Navigate(new PaginaMenu());
            ControlDeUsuarioNotificacion controlNotificacion = new ControlDeUsuarioNotificacion();
            this.gridPrincipal.Children.Add(controlNotificacion);
            this.Closing += VentanaMenuCierreAbrupto;
        }

        private void BtnCerrarSesion(object sender, RoutedEventArgs e)
        {
            VentanaModalDecision ventana = new VentanaModalDecision(Properties.Resources.lb_MensajeCerrarSesion,
                Properties.Resources.btn_CerrarSesión, Properties.Resources.global_Cancelar);
            bool? respuesta = ventana.ShowDialog();

            if (respuesta == true)
            {
                CerrarSesion();

                MainWindow mainWindow = new MainWindow();
                this.Close();
                mainWindow.Show();
            }
        }

        private void VentanaMenuCierreAbrupto(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CerrarSesion();
        }

        private void CerrarSesion()
        {
            var proxyGestionJugador = new GestionJugadorClient();

            try
            {
                string nombreUsuario = Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario;
                if (!string.IsNullOrEmpty(nombreUsuario))
                {
                    proxyGestionJugador.CerrarSesionJugador(nombreUsuario, Properties.Resources.msg_AbandonoSala);
                }
            }
            catch (System.Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(proxyGestionJugador, ex, this);
            }
        }
    }
}
