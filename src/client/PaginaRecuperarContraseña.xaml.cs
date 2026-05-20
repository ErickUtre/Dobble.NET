using System;
using System.Windows;
using System.Windows.Controls;


namespace DobbleGame
{
    public partial class PaginaRecuperarContraseña : Page
    {
        private Servidor.GestionJugadorClient _proxyGestionJugador = new Servidor.GestionJugadorClient();
        private Servidor.GestionCorreosClient _proxyGestionCorreos = new Servidor.GestionCorreosClient();
        private VentanaRecuperarContraseña _marcoPrincipal;

        public PaginaRecuperarContraseña(VentanaRecuperarContraseña marcoPrincipal)
        {
            InitializeComponent();  
            _marcoPrincipal = marcoPrincipal;
        }

        private void BtnEnviarCodigo(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreo.Text.Trim();
            EnviarCodigo(correo);
        }

        private void EnviarCodigo(string correo)
        {
            try
            {
                if (Utilidades.Utilidades.EsCampoVacio(correo))
                {
                    MostrarMensaje(Properties.Resources.lb_CamposVacíos);
                    return;
                }

                if (!ValidarCorreo(correo))
                {
                    MostrarMensaje(Properties.Resources.lb_CorreoNoExiste_);
                    return;
                }

                string codigo = GenerarCodigo();
                var respuesta = _proxyGestionCorreos.EnviarCodigo(correo, codigo);

                if (respuesta.Resultado)
                {
                    PaginaIngresoCodigo paginaIngresoCodigo = new PaginaIngresoCodigo(_marcoPrincipal, correo, codigo);
                    this.NavigationService.Navigate(paginaIngresoCodigo);
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionCorreos, ex, this);
            }
        }

        public static string GenerarCodigo()
        {
            return new Random().Next(100000, 999999).ToString(); 
        }

        public bool ValidarCorreo(string correo)
        {
            try
            {
                var respuesta = _proxyGestionJugador.ExisteCorreoAsociado(correo);

                if (respuesta.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(_marcoPrincipal);
                    return false;
                }

                if (respuesta.Resultado)
                {
                    return true;
                }
                    return false;
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionJugador, ex, this);
                return false;
            }
        }

        private void BtnCancelar(object sender, RoutedEventArgs e)
        {
            _marcoPrincipal.Close();
            _proxyGestionCorreos.Close();
            _proxyGestionJugador.Close();
        }

        private void MostrarMensaje(string mensaje)
        {
            IconoAdvertencia.Visibility = Visibility.Visible;
            lbMensaje.Content = mensaje;
        }
    }
}
