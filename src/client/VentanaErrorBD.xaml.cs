using DobbleGame.Servidor;
using DobbleGame.Utilidades;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DobbleGame
{
    public partial class VentanaErrorBD : Window
    {
        public VentanaErrorBD(string titulo, string mensaje)
        {
            InitializeComponent();
            lbTitulo.Content = titulo;
            tbMensaje.Text = mensaje;
        }

        private async void BtnReintentar_Click(object sender, RoutedEventArgs e)
        {
            await ReintentarConexionBD();
        }

        private async Task ReintentarConexionBD()
        {
            var proxy = new GestionAmigosClient();
            try
            {
                var respuestaUsuario = await proxy.ObtenerSolicitudAsync();
                if (respuestaUsuario.ErrorBD)
                {
                    await Task.Delay(1000);
                    return;
                }
                this.Close();
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(proxy, ex, this);
            }
        }


        private void BtnInicioSesion_Click(object sender, RoutedEventArgs e)
        {
            MainWindow inicioSesion = new MainWindow();
            var proxy = new GestionJugadorClient();
            var proxyUsuario = new GestionAmigosClient();

            bool isLoginWindowOpen = Application.Current.Windows.OfType<MainWindow>().Any();

            try
            {
                if (!isLoginWindowOpen)
                {
                    proxy.CerrarSesionJugador(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Properties.Resources.msg_AbandonoSala);
                    CallbackManager.Instance.Desconectar(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario);
                    proxyUsuario.NotificarDesconexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario);
                }

                proxy.Close();

                foreach (Window window in Application.Current.Windows.OfType<Window>().Where(window => window != inicioSesion))
                {
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepcionErrorConexion(ex, proxy);
            }

            inicioSesion.Show();
        }

    }
}
