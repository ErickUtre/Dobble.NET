using DobbleGame.Servidor;
using DobbleGame.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DobbleGame
{
    public partial class VentanaGestionarSolicitudesAmistad : Window
    {
        private VentanaMenu _ventanaMenu;

        public VentanaGestionarSolicitudesAmistad(VentanaMenu ventanaMenu)
        {
            _ventanaMenu = ventanaMenu;
            InitializeComponent();
            CargarSolicitudesAmistad();
   
            CallbackManager.Instance.NotificarSolicitudAmistadEvent += NotificarSolicitudAmistad;
        }

        public void NotificarSolicitudAmistad()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    CargarSolicitud();
                });
            }
            catch (Exception)
            {
                Utilidades.Utilidades.MostrarVentanaErrorConexionServidor(this, true);
            }
        }

        private void CargarSolicitudesAmistad()
        {
            var _proxyGestionAmigos = new GestionAmigosClient();
            try
            {
                var respuesta = _proxyGestionAmigos.ObtenerSolicitudesPendientes(Dominio.CuentaUsuario.CuentaUsuarioActual.IdCuentaUsuario);

                if (respuesta.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }

                if (respuesta.Resultado != null && respuesta.Resultado.Length > 0)
                {
                    List<Dominio.Amistad> solicitudesAmistad = respuesta.Resultado
                        .Select(solicitud => new Dominio.Amistad
                        {
                            IdAmistad = solicitud.IdAmistad,
                            EstadoSolicitud = solicitud.EstadoSolicitud,
                            UsuarioPrincipalId = solicitud.UsuarioPrincipalId,
                            UsuarioAmigoId = solicitud.UsuarioAmigoId
                        })
                        .ToList();

                    foreach (var solicitud in solicitudesAmistad)
                    {
                        MostrarNotificacionSolicitud(solicitud);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionAmigos, ex, this);
            }
        }

        public void CargarSolicitud()
        {
            var _proxyGestionAmigos = new GestionAmigosClient();
            try
            {
                var respuesta = _proxyGestionAmigos.ObtenerSolicitud();
                if (respuesta.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }

                if (respuesta.Resultado != null)
                {
                    var amistadDominio = new Dominio.Amistad
                    {
                        IdAmistad = respuesta.Resultado.IdAmistad,
                        EstadoSolicitud = respuesta.Resultado.EstadoSolicitud,
                        UsuarioPrincipalId = respuesta.Resultado.UsuarioPrincipalId,
                        UsuarioAmigoId = respuesta.Resultado.UsuarioAmigoId
                    };

                    MostrarNotificacionSolicitud(amistadDominio);

                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionAmigos, ex, this);
            }

        }

        private void MostrarNotificacionSolicitud(Dominio.Amistad solicitud)
        {
            var _proxyGestionAmigos = new GestionAmigosClient();
            try
            {
                var respuesta = _proxyGestionAmigos.ObtenerUsuario(solicitud.UsuarioPrincipalId);
                if (respuesta.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }
                var cuenta = respuesta.Resultado;

                Dominio.CuentaUsuarioAmigo cuentaUsuarioAmigo = new Dominio.CuentaUsuarioAmigo
                {
                    Usuario = cuenta.Usuario,
                };

                var panelSolicitud = new Border
                {
                    Background = new SolidColorBrush(Colors.Transparent),
                    Padding = new Thickness(10),
                    BorderBrush = new SolidColorBrush(Colors.LightGray),
                    BorderThickness = new Thickness(1)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.25, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.75, GridUnitType.Star) });


                var fotoUsuario = new Image 
                {
                    Width = 70,
                    Height = 70,
                    Stretch = Stretch.UniformToFill,
                    Margin = new Thickness(0, 0, 15, 0)
                };

                byte[] fotoBytes = cuenta.Foto;
                if (fotoBytes != null)
                {
                    fotoUsuario.Source = ConvertirBytesAImagen(fotoBytes);
                }

                fotoUsuario.Clip = new EllipseGeometry
                {
                    Center = new Point(35, 35),
                    RadiusX = 35,
                    RadiusY = 35
                };
                Grid.SetColumn(fotoUsuario, 0);


                var nombreUsuario = new TextBlock  
                {
                    Text = cuentaUsuarioAmigo.Usuario,
                    FontSize = 21,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                Grid.SetColumn(nombreUsuario, 1);


                var stackBotones = new StackPanel  
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var botonAceptar = new Button
                {
                    Content = Properties.Resources.global_Aceptar,
                    Margin = new Thickness(0, 0, 5, 0),
                    Background = new SolidColorBrush(Colors.Green),
                    Foreground = new SolidColorBrush(Colors.White)
                };
                botonAceptar.Click += (s, e) => AceptarSolicitud(solicitud, panelSolicitud, cuentaUsuarioAmigo.Usuario);

                var botonRechazar = new Button
                {
                    Content = Properties.Resources.btn_Rechazar,
                    Background = new SolidColorBrush(Colors.Red),
                    Foreground = new SolidColorBrush(Colors.White)
                };
                botonRechazar.Click += (s, e) => RechazarSolicitud(solicitud, panelSolicitud);

                stackBotones.Children.Add(botonAceptar);
                stackBotones.Children.Add(botonRechazar);

                var contenidoStackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Center
                };
                contenidoStackPanel.Children.Add(nombreUsuario);
                contenidoStackPanel.Children.Add(stackBotones);

                Grid.SetColumn(contenidoStackPanel, 1);

                grid.Children.Add(fotoUsuario);
                grid.Children.Add(contenidoStackPanel);

                panelSolicitud.Child = grid;

                ContenedorNotificaciones.Children.Add(panelSolicitud);

            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionAmigos, ex, this);
            }
        }

        private static ImageSource ConvertirBytesAImagen(byte[] fotoBytes)
        {
            if (fotoBytes == null || fotoBytes.Length == 0)
                return null;

            using (var stream = new MemoryStream(fotoBytes))
            {
                var imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.StreamSource = stream;
                imagen.EndInit();
                return imagen;
            }
        }

        private void AceptarSolicitud(Dominio.Amistad solicitud, Border panelSolicitud, String nombreUsuario)
        {
            var _proxyGestionAmigos = new GestionAmigosClient();
            try
            {
                Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow);

                var respuesta = _proxyGestionAmigos.AceptarSolicitud(solicitud.IdAmistad, nombreUsuario);
                if (respuesta.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }

                if (respuesta.Resultado)
                {
                    ContenedorNotificaciones.Children.Remove(panelSolicitud);
                    _ventanaMenu.ContenedorNotificaciones.Children.Clear();
                    _ventanaMenu.CargarAmistades();
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionAmigos, ex, this);
            }
        }

        private void RechazarSolicitud(Dominio.Amistad solicitud, Border panelSolicitud)
        {
            var _proxyGestionAmigos = new GestionAmigosClient();
            try
            {
                Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow);

                var estaConectado = _proxyGestionAmigos.UsuarioConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario);
                if (!estaConectado.Resultado)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionServidor(this, false);
                    return;
                }

                var respuesta = _proxyGestionAmigos.EliminarAmistad(solicitud.IdAmistad, null, null);

                if (respuesta.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }

                if (respuesta.Resultado)
                {
                    ContenedorNotificaciones.Children.Remove(panelSolicitud);
                }

            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(_proxyGestionAmigos, ex, this);
            }
        }

        private void BtnRegresar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
