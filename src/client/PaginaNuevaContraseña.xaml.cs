using DobbleGame.Utilidades;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DobbleGame
{
    public partial class PaginaNuevaContraseña : Page
    {
        private Servidor.GestionJugadorClient _proxyGestionJugador = new Servidor.GestionJugadorClient();
        private VentanaRecuperarContraseña _marcoPrincipal;
        private string _correo;

        public PaginaNuevaContraseña(VentanaRecuperarContraseña marcoPrincipal, string correo)
        {
            InitializeComponent();
            _correo = correo;
            _marcoPrincipal = marcoPrincipal;
        }

        private void BtnActualizar(object sender, RoutedEventArgs e)
        {
            ActualizarContraseña(_correo);
        }

        private void ActualizarContraseña(string correo)
        {
            try
            {
                string nuevaContraseña = tbNuevaContraseña.Password.Trim();
                string confirmarNuevaContraseña = tbConfirmarContraseña.Password.Trim();

                if (!ValidarCamposContraseña(nuevaContraseña, confirmarNuevaContraseña))
                    return;

                string contraseñaHasheada = Utilidades.EncriptadorContraseña.GenerarHashSHA512(nuevaContraseña);

                var usuario = ObtenerUsuarioPorCorreo(correo);
                if (usuario == null)
                    return;

                if (nuevaContraseña == usuario.Contraseña)
                {
                    MostrarMensaje(Properties.Resources.global_MismaContraseña_);
                    return;
                }

                if (ModificarContraseña(usuario.IdCuentaUsuario, contraseñaHasheada))
                {
                    _marcoPrincipal.Close();
                }
                else
                {
                    MostrarMensaje(Properties.Resources.global_MismaContraseña_);
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionJugador, ex, this);
            }
        }

        private bool ValidarCamposContraseña(string nuevaContraseña, string confirmarNuevaContraseña)
        {
            if (Utilidades.Utilidades.EsCampoVacio(nuevaContraseña) || Utilidades.Utilidades.EsCampoVacio(confirmarNuevaContraseña))
            {
                MostrarMensaje(Properties.Resources.lb_CamposVacíos);
                return false;
            }

            if (nuevaContraseña != confirmarNuevaContraseña)
            {
                MostrarMensaje(Properties.Resources.lb_ContraseñaNoCoincide);
                return false;
            }

            if (!Utilidades.Utilidades.ValidarContraseña(nuevaContraseña))
            {
                MostrarMensaje(Properties.Resources.lb_DatosInválidos);
                return false;
            }

            return true;
        }

        private Servidor.CuentaUsuario ObtenerUsuarioPorCorreo(string correo)
        {
            var respuestaUsuario = _proxyGestionJugador.ObtenerUsuarioPorCorreo(correo);
            if (respuestaUsuario.ErrorBD)
            {
                Utilidades.Utilidades.MostrarVentanaErrorConexionBD(_marcoPrincipal);
                return null;
            }

            return respuestaUsuario.Resultado;
        }

        private bool ModificarContraseña(int idCuentaUsuario, string contraseñaHasheada)
        {
            var respuestaModificarContraseña = _proxyGestionJugador.ModificarContraseñaUsuario(idCuentaUsuario, contraseñaHasheada);

            if (respuestaModificarContraseña.ErrorBD)
            {
                Utilidades.Utilidades.MostrarVentanaErrorConexionBD(_marcoPrincipal);
                return false;
            }

            return respuestaModificarContraseña.Resultado;
        }

        private void MostrarMensaje(string mensaje)
        {
            IconoAdvertencia.Visibility = Visibility.Visible;
            lbMensaje.Content = mensaje;
        }

        private void BtnCancelar(object sender, RoutedEventArgs e)
        {
            _marcoPrincipal.Close();
            _proxyGestionJugador.Close();
        }

        private void PbCambioDeContraseña(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            var textoSugerido = ContraseñaHelper.EncontrarHijoVisual<TextBlock>(passwordBox, "TextoSugerido");
            ContraseñaHelper.ActualizarVisibilidadTextoSugerido(passwordBox, textoSugerido);
        }
    }
}

