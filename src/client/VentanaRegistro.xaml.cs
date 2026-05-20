using DobbleGame.Servidor;
using DobbleGame.Utilidades;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DobbleGame
{
    public partial class VentanaRegistro : Window 
    {
        public VentanaRegistro()
        {
            InitializeComponent();
        }

        private void BtnRegresar(object sender, RoutedEventArgs e)
        {
            IrMainWindow();
        }

        private void IrMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BtnRegistrarUsuario(object sender, RoutedEventArgs e)
        {
            if (!CamposValidos()) return;

            byte[] foto = CargarFotoDefecto();
            if (foto == null) return;

            string contraseñaHasheada = Utilidades.EncriptadorContraseña.GenerarHashSHA512(pbContraseña.Password);
            string correo = tbCorreo.Text;
            string usuario = tbNombreUsuario.Text;

            RegistrarUsuario(correo, usuario, contraseñaHasheada, foto);
        }

        public void RegistrarUsuario(string correo, string nombreUsuario, string contraseñaHasheada, byte[] foto)
        {
            var proxyJugador = new GestionJugadorClient();
            try
            {
                Servidor.CuentaUsuario cuentaUsuario = new Servidor.CuentaUsuario
                {
                    Correo = correo,
                    Usuario = nombreUsuario,
                    Contraseña = contraseñaHasheada,
                    Foto = foto
                };

                var respuestaCorreo = proxyJugador.ExisteCorreoAsociado(cuentaUsuario.Correo);
                if (respuestaCorreo.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }
                if (respuestaCorreo.Resultado)
                {
                    Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_CorreoExistente);
                    return;
                }

                var respuestaUsuario = proxyJugador.ExisteNombreUsuario(cuentaUsuario.Usuario);
                if (respuestaUsuario.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }
                if (respuestaUsuario.Resultado)
                {
                    Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_UsuarioExistente);
                    return;
                }

                var respuestaRegistro = proxyJugador.RegistrarUsuario(cuentaUsuario);
                if (respuestaRegistro.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }
                if (respuestaRegistro.Resultado)
                {
                    VentanaRegistroExitoso ventanaRegistroExitoso = new VentanaRegistroExitoso
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    ventanaRegistroExitoso.ShowDialog();
                    IrMainWindow();
                }
                else
                {
                    Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_ErrorInesperado);
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(proxyJugador, ex, this);
            }
        }

        private bool CamposValidos()
        {
            if (TieneCamposVacios())
            {
                return false;
            }

            if (!ValidarPatronCorreo())
            {
                return false;
            }

            if (!ValidarContraseña())
            {
                return false;
            }

            if (!ValidarComparacionContraseña())
            {
                return false;
            }

            return true;
        }

        private bool TieneCamposVacios()
        {
            bool hayCaposVacios =
                Utilidades.Utilidades.EsCampoVacio(tbCorreo.Text) ||
                Utilidades.Utilidades.EsCampoVacio(tbNombreUsuario.Text) ||
                Utilidades.Utilidades.EsCampoVacio(pbContraseña.Password) ||
                Utilidades.Utilidades.EsCampoVacio(pbContraseñaConfirmada.Password);

            if (hayCaposVacios)
            {
                Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_CamposVacíos);
                return true;
            }

            return false;
        }

        private bool ValidarContraseña()
        {
            if (!Utilidades.Utilidades.ValidarContraseña(pbContraseña.Password))
            {
                Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_ContraseñaIncorrecta);
                return false;
            }

            return true;
        }

        private bool ValidarComparacionContraseña()
        {
            bool validado = false;
            if (Utilidades.Utilidades.EsMismaContraseña(pbContraseña.Password, pbContraseñaConfirmada.Password))
            {
                validado = true;
            }
            else
            {
                Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_ContraseñaNoCoincide);
            }
            return validado;
        }

        private bool ValidarPatronCorreo()
        {
            bool validado = false;
            
            if (Utilidades.Utilidades.ValidarPatronCorreo(tbCorreo.Text))
            {
                validado = true;
            }
            else
            {
                Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_CorreoInválido);
            }
            
            return validado;
        }

        private void PbCambioDeContraseña(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            var textoSugerido = ContraseñaHelper.EncontrarHijoVisual<TextBlock>(passwordBox, "TextoSugerido");
            ContraseñaHelper.ActualizarVisibilidadTextoSugerido(passwordBox, textoSugerido);
        }

        private byte[] CargarFotoDefecto()
        {
            try
            {
                string rutaProyecto = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string rutaFotoDefecto = System.IO.Path.Combine(rutaProyecto, "Imagenes", "PerfilPorDefecto.jpg");

                if (!File.Exists(rutaFotoDefecto))
                {
                    Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_ErrorInesperado);
                    return new byte[0];
                }

                return File.ReadAllBytes(rutaFotoDefecto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Array.Empty<byte>();
            }
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
