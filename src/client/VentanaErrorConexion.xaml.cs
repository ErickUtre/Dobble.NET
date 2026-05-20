using DobbleGame.Servidor;
using DobbleGame.Utilidades;
using System;
using System.Linq;
using System.Windows;

namespace DobbleGame
{
    public partial class VentanaErrorConexion : Window
    {
        public VentanaErrorConexion()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
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
                    if (!Dominio.CuentaUsuario.CuentaUsuarioActual.EsInvitado)
                    {
                        CallbackManager.Instance.Desconectar(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario);
                        proxyUsuario.NotificarDesconexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario);
                    }
                }

                proxy.Close();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window != inicioSesion)
                    {
                        window.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepcionErrorConexion(ex, proxy);
            }

            inicioSesion.Show();
        }

        public void MostrarDialogo(string titulo, string mensaje)
        {
            lbTitulo.Content = titulo;
            tbMensaje.Text = mensaje;
        }
    }
}
