using DobbleGame.Extensiones;
using DobbleGame.Utilidades;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DobbleGame
{
    public partial class MainWindow : Window
    {
        private Servidor.GestionAmigosClient _proxyGestionAmigos = new Servidor.GestionAmigosClient();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void IniciarSesion()
        {
            if (!Utilidades.Utilidades.EsCampoVacio(tbUsuario.Text) && !Utilidades.Utilidades.EsCampoVacio(pbContraseña.Password))
            {
                var proxyGestionJugador = new Servidor.GestionJugadorClient();

                try
                {
                    var respuestaInicioSesion = 
                        proxyGestionJugador.IniciarSesionJugador
                        (tbUsuario.Text, Utilidades.EncriptadorContraseña.GenerarHashSHA512(pbContraseña.Password));

                    if (respuestaInicioSesion.ErrorBD)
                    {
                        Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                        return;
                    }
                    else if (!respuestaInicioSesion.Exitoso)
                    {
                        Utilidades.Utilidades.MostrarMensajeStackPanel(panelMensaje, lbMensaje, Properties.Resources.lb_SesiónActiva);
                    }
                    else if (respuestaInicioSesion.Resultado != null)
                    {
                        var cuentaInicioSesion = respuestaInicioSesion.Resultado;
                        Dominio.CuentaUsuario.CuentaUsuarioActual = new Dominio.CuentaUsuario
                        {
                            IdCuentaUsuario = cuentaInicioSesion.IdCuentaUsuario,
                            Usuario = cuentaInicioSesion.Usuario,
                            Correo = cuentaInicioSesion.Correo,
                            Contraseña = cuentaInicioSesion.Contraseña,
                            Foto = cuentaInicioSesion.Foto,
                            Puntaje = cuentaInicioSesion.Puntaje,
                            Estado = true,
                            EsInvitado = cuentaInicioSesion.EsInvitado
                        };

                        CallbackManager.Instance.Conectar(cuentaInicioSesion.Usuario);

                        _proxyGestionAmigos.NotificarCambios(cuentaInicioSesion.Usuario);

                        VentanaMenu ventanaMenu = new VentanaMenu();
                        this.Close();                     
                        ventanaMenu.Show();
                    }
                    else
                    {
                        MostrarMensaje(Properties.Resources.lb_ErrorInicioSesión);
                    }
                }
                catch (Exception ex)
                {
                    Utilidades.Utilidades.ManejarExcepciones(proxyGestionJugador, ex, this);
                }
            }
            else
            {
                MostrarMensaje(Properties.Resources.lb_CamposVacíos);
            }
        }

        private void BtnEntrarMenuInvitado(object sender, EventArgs e)
        {
            var proxyGestionJugador = new Servidor.GestionJugadorClient();

            try
            {
                string nombreInvitado = String.Format(Properties.Resources.lb_Invitado, new Random().Next(10000, 100000));
                var respuestaInicioSesion = proxyGestionJugador.IniciarSesionInvitado(nombreInvitado, CargarFotoDefecto());

                if (respuestaInicioSesion.Resultado != null)
                {
                    var cuentaInvitado = respuestaInicioSesion.Resultado;
                    Dominio.CuentaUsuario.CuentaUsuarioActual = new Dominio.CuentaUsuario
                    {
                        Usuario = cuentaInvitado.Usuario,
                        Foto = cuentaInvitado.Foto,
                        EsInvitado = cuentaInvitado.EsInvitado
                    };

                    VentanaMenuInvitado ventanaMenuInvitado = new VentanaMenuInvitado();
                    this.Close();
                    ventanaMenuInvitado.Show();
                    
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(proxyGestionJugador, ex, this);
            }
        }

        private void BtnEntrarMenu(object sender, RoutedEventArgs e)
        {
            IniciarSesion();
        }

        private void ClicCrearCuentaTf(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            VentanaRegistro ventanaRegistro = new VentanaRegistro();
            ventanaRegistro.Show();
            this.Close();
        }

        private void ClicRecuperarContraseñaTf(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            VentanaRecuperarContraseña ventanaRecuperarContraseña = new VentanaRecuperarContraseña();
            ventanaRecuperarContraseña.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ventanaRecuperarContraseña.ShowDialog();
        }

        private void BtnCambioIdioma(object sender, RoutedEventArgs e)
        {
            string idiomaEspañol = "es-MX";
            string idiomaIngles = "en-US";

            if (Thread.CurrentThread.CurrentUICulture.Name == idiomaEspañol)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(idiomaIngles);
                
            } 
            else
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(idiomaEspañol);
            }

            ActualizarIdiomaIU();
        }

        private void ActualizarIdiomaIU()
        {
            this.Title = Properties.Resources.global_IniciarSesión;
            lbDobble.Content = Properties.Resources.lb_Dobble_NET;
            lbIniciarSesion.Content = Properties.Resources.global_IniciarSesión;
            TextBoxExtensiones.SetTextoSugerido(tbUsuario, Properties.Resources.global_Usuario);
            PasswordBoxExtensiones.SetTextoSugerido(pbContraseña, Properties.Resources.lb_Contraseña);
            tbCrearCuenta.Text = Properties.Resources.lb_CrearCuenta;
            tbContraseñaOlvidada.Text = Properties.Resources.lb_ContraseñaOlvidada;
            btnEntrar.Content = Properties.Resources.btn_Entrar;
            btnEntrarComoInvitado.Content = Properties.Resources.btn_EntrarComoInvitado;
            btnCambioIdioma.Content = Properties.Resources.btn_CambioIdioma;
        }

        private void MostrarMensaje(string mensaje)
        {
            panelMensaje.Visibility = Visibility.Visible;
            lbMensaje.Content = mensaje;
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
                return new byte[0];
            }
        }

        private void PasswordBox_CambioDeContraseña(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            var textoSugerido = ContraseñaHelper.EncontrarHijoVisual<TextBlock>(passwordBox, "TextoSugerido");
            ContraseñaHelper.ActualizarVisibilidadTextoSugerido(passwordBox, textoSugerido);
        }

        private void Window_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource != panelMensaje && panelMensaje.Visibility == Visibility.Visible)
            {
                panelMensaje.Visibility = Visibility.Hidden;
            }
        }
    }
}
