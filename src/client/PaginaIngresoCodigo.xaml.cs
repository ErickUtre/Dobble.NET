using System;
using System.Windows;
using System.Windows.Controls;


namespace DobbleGame
{
    public partial class PaginaIngresoCodigo : Page
    {
        private Servidor.GestionCorreosClient _proxyGestionCorreos = new Servidor.GestionCorreosClient();
        private VentanaRecuperarContraseña _marcoPrincipal;
        private string _correo;
        private string _codigo;

        public PaginaIngresoCodigo(VentanaRecuperarContraseña marcoPrincipal, string correo, string codigo)
        {
            InitializeComponent();
            _marcoPrincipal = marcoPrincipal;
            _correo = correo;
            _codigo = codigo;
        }

        private void BtnAceptar(object sender, RoutedEventArgs e)
        {
            try
            {
                string codigo = tbCodigoSala.Text.Trim();
                if (Utilidades.Utilidades.EsCampoVacio(codigo))
                {
                    MostrarMensaje(Properties.Resources.lb_CamposVacíos);
                    return;
                }

                if (codigo == _codigo)
                {
                    var paginaNuevaContraseña = new PaginaNuevaContraseña(_marcoPrincipal, _correo);
                    this.NavigationService.Navigate(paginaNuevaContraseña);
                }
                else
                {
                    MostrarMensaje(Properties.Resources.lb_CodigoInvalido);
                }

            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionCorreos, ex, this);
            }
        }

        private void BtnReintentar(object sender, RoutedEventArgs e)
        {
            EnviarCodigo(_correo);
        }

        private void EnviarCodigo(string correo)
        {
            try
            {
                string codigo = new Random().Next(100000, 999999).ToString();
                _codigo = codigo;

                var respuesta = _proxyGestionCorreos.EnviarCodigo(correo, codigo);
                if (respuesta.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(_marcoPrincipal);
                    return;
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionCorreos, ex, this);
            }
        }

        private void BtnCancelar(object sender, RoutedEventArgs e)
        {
            _marcoPrincipal.Close();
            _proxyGestionCorreos.Close();
        }

        private void MostrarMensaje(string mensaje)
        {
            IconoAdvertencia.Visibility = Visibility.Visible;
            lbMensaje.Content = mensaje;
        }
    }
}
