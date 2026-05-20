using DobbleGame.Servidor;
using DobbleGame.Utilidades;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DobbleGame
{
    public partial class VentanaCambioContraseña : Window
    {
        private GestionJugadorClient _proxyJugadorClient = new GestionJugadorClient();

        public VentanaCambioContraseña()
        {
            InitializeComponent();
        }

        private void BtnCancelar(object sender, RoutedEventArgs e)
        {
            Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, this);
            this.Close();
        }

        private void BtnActualizarContraseña(object sender, RoutedEventArgs e)
        {
            Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, this);
            ActualizarContraseña();
        }

        private void ActualizarContraseña()
        {
            try
            {
                string contraseñaActual = pbContraseñaActual.Password;
                string nuevaContraseña = pbNuevaContraseña.Password;
                string confirmarNuevaContraseña = pbConfirmarNuevaContraseña.Password;

                if (!ValidarEntradas(contraseñaActual, nuevaContraseña, confirmarNuevaContraseña))
                {
                    return;
                }

                if (!ValidarContraseñaActual(contraseñaActual))
                {
                    return;
                }

                string contraseñaHasheada = Utilidades.EncriptadorContraseña.GenerarHashSHA512(nuevaContraseña);
                var respuestaModificarContraseña = _proxyJugadorClient.ModificarContraseñaUsuario(
                    Dominio.CuentaUsuario.CuentaUsuarioActual.IdCuentaUsuario,
                    contraseñaHasheada);

                if (respuestaModificarContraseña.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }

                if (respuestaModificarContraseña.Resultado)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyJugadorClient, ex, this);
            }
        }

        private bool ValidarEntradas(string contraseñaActual, string nuevaContraseña, string confirmarNuevaContraseña)
        {
            if (Utilidades.Utilidades.EsCampoVacio(contraseñaActual) ||
                Utilidades.Utilidades.EsCampoVacio(nuevaContraseña) ||
                Utilidades.Utilidades.EsCampoVacio(confirmarNuevaContraseña))
            {
                MostrarMensaje(Properties.Resources.lb_CamposVacíos);
                return false;
            }

            if (!Utilidades.Utilidades.EsMismaContraseña(nuevaContraseña, confirmarNuevaContraseña))
            {
                MostrarMensaje(Properties.Resources.lb_ContraseñaNoCoincide);
                return false;
            }

            if (Utilidades.Utilidades.EsMismaContraseña(contraseñaActual, nuevaContraseña))
            {
                MostrarMensaje(Properties.Resources.global_MismaContraseña_);
                return false;
            }

            if (!Utilidades.Utilidades.ValidarContraseña(contraseñaActual) ||
                !Utilidades.Utilidades.ValidarContraseña(nuevaContraseña) ||
                !Utilidades.Utilidades.ValidarContraseña(confirmarNuevaContraseña))
            {
                MostrarMensaje(Properties.Resources.lb_DatosInválidos);
                return false;
            }

            return true;
        }

        private bool ValidarContraseñaActual(string contraseñaActual)
        {
            try
            {
                var respuestaUsuario = _proxyJugadorClient.ValidarContraseña
                    (Dominio.CuentaUsuario.CuentaUsuarioActual.IdCuentaUsuario, 
                    Utilidades.EncriptadorContraseña.GenerarHashSHA512(contraseñaActual));

                if (respuestaUsuario.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return false;
                }

                if (!respuestaUsuario.Resultado)
                {
                    MostrarMensaje(Properties.Resources.lb_ContraseñaActualInvalida);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyJugadorClient, ex, this);
                return false;
            }
        }


        private void MostrarMensaje(string mensaje)
        {
            advertenciaIcono.Visibility = Visibility.Visible;
            lbMensaje.Content = mensaje;
        }

        private void PasswordBox_CambioDeContraseña(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            var textoSugerido = ContraseñaHelper.EncontrarHijoVisual<TextBlock>(passwordBox, "TextoSugerido");
            ContraseñaHelper.ActualizarVisibilidadTextoSugerido(passwordBox, textoSugerido);
        }

    }
}
