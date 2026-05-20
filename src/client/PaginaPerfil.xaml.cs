using DobbleGame.Servidor;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DobbleGame
{
    public partial class PaginaPerfil : Page
    {
        private GestionJugadorClient _proxyGestionJugador = new GestionJugadorClient();
        private VentanaMenu _ventanaMenu;

        public PaginaPerfil(VentanaMenu ventanaMenu)
        {
            InitializeComponent();
            InicializarDatos();
            _ventanaMenu = ventanaMenu; 
        }

        private void InicializarDatos()
        {
            var respuestaObtenerPuntaje = _proxyGestionJugador.ObtenerPuntosUsuario(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario);
            if (respuestaObtenerPuntaje.ErrorBD)
            {
                Utilidades.Utilidades.MostrarVentanaErrorConexionBD(_ventanaMenu);
            }
            if (respuestaObtenerPuntaje.Resultado != null)
            {
                lbPuntaje.Content = String.Format(Properties.Resources.lb_Puntaje, respuestaObtenerPuntaje.Resultado);
            }
            lbCorreoElectronico.Content = Dominio.CuentaUsuario.CuentaUsuarioActual.Correo;
            lbNombreUsuario.Content = Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario;
            ConvertirImagenPerfil(Dominio.CuentaUsuario.CuentaUsuarioActual.Foto);
        }

        private void BtnRegresar(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animacionDesvanecimiento = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.5)));
            animacionDesvanecimiento.Completed += (s, a) =>
            {
                PaginaMenu paginaMenu = new PaginaMenu();
                this.NavigationService.Navigate(paginaMenu);

                AnimarElementos(paginaMenu);
            };
            this.BeginAnimation(Frame.OpacityProperty, animacionDesvanecimiento);
        }

        private void AnimarElementos(PaginaMenu paginaMenu)
        {
            if (paginaMenu.Content is Panel panel)
            {
                foreach (UIElement elemento in panel.Children)
                {
                    elemento.Opacity = 0;

                    DoubleAnimation animacionAparicion = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5)))
                    {
                        BeginTime = TimeSpan.FromMilliseconds(200) 
                    };

                    elemento.BeginAnimation(UIElement.OpacityProperty, animacionAparicion);
                }
            }
        }

        private void BtnCambiarUsuario(object sender, RoutedEventArgs e)
        {
            Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, _ventanaMenu);

            VentanaCambioNombre ventanaCambioNombre = new VentanaCambioNombre(this, _ventanaMenu);
            ventanaCambioNombre.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ventanaCambioNombre.ShowDialog();
        }

        private void BtnCambiarContraseña(object sender, RoutedEventArgs e)
        {
            Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, _ventanaMenu);

            VentanaCambioContraseña ventanaCambioContraseña = new VentanaCambioContraseña();
            ventanaCambioContraseña.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ventanaCambioContraseña.ShowDialog();
        }

        private void BtnActualizarFoto(object sender, RoutedEventArgs e)
        {
            Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, _ventanaMenu);

            OpenFileDialog dialogo = new OpenFileDialog();

            dialogo.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg";
            dialogo.Title = "Selecciona una imagen";

            dialogo.ShowDialog();
            string rutaArchivo = dialogo.FileName;

            if (!string.IsNullOrEmpty(rutaArchivo) && File.Exists(rutaArchivo))
            {
                if (!EsArchivoImagen(rutaArchivo))
                {
                    MostrarMensaje(Properties.Resources.lb_FormatoInválido);
                    return;
                }

                FileInfo fileInfo = new FileInfo(rutaArchivo);

                if (fileInfo.Length > 10 * 1024)
                {
                MostrarMensaje(Properties.Resources.lb_FormatoInválido);
                return;
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(rutaArchivo);
                bitmap.EndInit();

                GuardarFotoPerfil(rutaArchivo, bitmap);                          
            }
        }

        private void GuardarFotoPerfil(String rutaImagen, BitmapImage bitmap)
        {
            try
            {
                byte[] foto = File.ReadAllBytes(rutaImagen);

                var respuestaModificarFoto = _proxyGestionJugador.ModificarFotoUsuario
                    (Dominio.CuentaUsuario.CuentaUsuarioActual.IdCuentaUsuario, foto);

                var ventanaPrincipal = Window.GetWindow(this);

                if (respuestaModificarFoto.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(ventanaPrincipal);
                    return;
                }
                if (respuestaModificarFoto.Resultado)
                {
                    Dominio.CuentaUsuario.CuentaUsuarioActual.Foto = foto;
                    ImagenPerfil.Source = bitmap;
                    _ventanaMenu.ConvertirImagenPerfil(Dominio.CuentaUsuario.CuentaUsuarioActual.Foto);
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionJugador, ex, this);
            }
        }

        private static bool EsArchivoImagen(string rutaArchivo)
        {
            try
            {
                using (var imagen = System.Drawing.Image.FromFile(rutaArchivo))
                {
                    return true;
                }
            }
            catch (OutOfMemoryException)
            {
                return false;
            }
        }

        private void ConvertirImagenPerfil(byte[] fotoBytes)
        {
            if (fotoBytes == null || fotoBytes.Length == 0)
                return;

            using (var ms = new MemoryStream(fotoBytes))
            {
                BitmapImage imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.StreamSource = ms;
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.EndInit();
                imagen.Freeze();

                ImagenPerfil.Source = imagen;
            }
        }

        public void ActualizarNombreUsuario(string nuevoTexto)
        {
            lbNombreUsuario.Content = nuevoTexto;
        }

        private void MostrarMensaje(string mensaje)
        {
            panelMensaje.Visibility = Visibility.Visible;
            lbMensaje.Content = mensaje;
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
