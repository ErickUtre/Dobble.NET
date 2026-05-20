using System;
using System.Windows;
using DobbleGame.Servidor;

namespace DobbleGame
{
    public partial class VentanaCambioNombre : Window
    {
        private GestionJugadorClient _proxyGestionJugadorClient = new GestionJugadorClient();
        private PaginaPerfil _paginaPerfil;
        private VentanaMenu _ventanaMenu;
        public VentanaCambioNombre(PaginaPerfil paginaPerfil, VentanaMenu ventanaMenu)
        {
            InitializeComponent();
            _paginaPerfil = paginaPerfil;
            _ventanaMenu = ventanaMenu;
        }

        private void BtnActualizarUsuario(object sender, RoutedEventArgs e)
        {
            Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, this);
            GuardarCambioNombre();
            _paginaPerfil.ActualizarNombreUsuario(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario);
            _ventanaMenu.ActualizarNombreUsuario(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario);
        }

        private void GuardarCambioNombre()
        {
            try
            {
                string nuevoNombre = tbNuevoNombre.Text.Trim();

                if (Utilidades.Utilidades.EsCampoVacio(nuevoNombre))
                {
                    MostrarMensaje(Properties.Resources.lb_CamposVacíos);
                    return;
                }

                var respuestaUsuario = _proxyGestionJugadorClient.ExisteNombreUsuario(nuevoNombre);
                if (respuestaUsuario.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }
                if (respuestaUsuario.Resultado)
                {
                    MostrarMensaje(Properties.Resources.lb_UsuarioExistente);
                    return;
                }

                var respuestaModificarUsuario = _proxyGestionJugadorClient.ModificarNombreUsuario
                    (Dominio.CuentaUsuario.CuentaUsuarioActual.IdCuentaUsuario, nuevoNombre);
                if (respuestaModificarUsuario.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }

                if (respuestaModificarUsuario.Resultado)
                {
                    Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario = nuevoNombre;
                    Dominio.CuentaUsuario.CuentaUsuarioActual = new Dominio.CuentaUsuario
                    {
                        IdCuentaUsuario = Dominio.CuentaUsuario.CuentaUsuarioActual.IdCuentaUsuario,
                        Usuario = nuevoNombre,
                        Correo = Dominio.CuentaUsuario.CuentaUsuarioActual.Correo,
                        Contraseña = Dominio.CuentaUsuario.CuentaUsuarioActual.Contraseña,
                        Foto = Dominio.CuentaUsuario.CuentaUsuarioActual.Foto,
                        Puntaje = Dominio.CuentaUsuario.CuentaUsuarioActual.Puntaje,
                        Estado = true,
                    };

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionJugadorClient, ex, this);
            }
        }

        private void BtnCancelar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MostrarMensaje(string mensaje)
        {
            advertenciaIcono.Visibility = Visibility.Visible;
            lbMensaje.Content = mensaje;
        }
    }
}
