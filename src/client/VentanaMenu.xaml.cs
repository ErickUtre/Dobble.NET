using DobbleGame.Servidor;
using DobbleGame.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DobbleGame
{
    public partial class VentanaMenu : Window
    {
        private readonly ControlDeUsuarioNotificacion _controlNotificacion;
        public VentanaMenu()
        {
            InitializeComponent();
            InicializarDatos();
            MarcoPrincipal.NavigationService.Navigate(new PaginaMenu());
            _controlNotificacion = new ControlDeUsuarioNotificacion();
            contenedorNotificacion.Content = _controlNotificacion;
        }

        private void InicializarDatos()
        {
            CargarAmistades();
            lbNombreUsuario.Content = Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario;
            ConvertirImagenPerfil(Dominio.CuentaUsuario.CuentaUsuarioActual.Foto);
            CallbackManager.Instance.NotificarCambioEvent += NotificarCambio;
            CallbackManager.Instance.NotificarSalidaEvent += NotificarSalida;

            this.Closing += VentanaMenuCierreAbrupto;
        }

        public void NotificarCambio()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    ContenedorNotificaciones.Children.Clear();
                    CargarAmistades();
                });
            }
            catch (Exception)
            {
                Utilidades.Utilidades.MostrarVentanaErrorConexionServidor(this, true);
            }
        }

        public void NotificarSalida(string nombreUsuario)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    var stackPanel = EncontrarStackPanelPorUsuario(nombreUsuario);
                    if (stackPanel != null)
                    {
                        CambiarEstadoAusente(stackPanel);
                    }
                });
            }
            catch (Exception)
            {
                Utilidades.Utilidades.MostrarVentanaErrorConexionServidor(this, true);
            }
        }

        private StackPanel EncontrarStackPanelPorUsuario(string nombreUsuario)
        {
            foreach (var child in ContenedorNotificaciones.Children)
            {
                if (child is Border border && border.Child is Grid grid)
                {
                    var stackPanel = grid.Children
                        .OfType<StackPanel>()
                        .FirstOrDefault(sp => sp.Children
                            .OfType<TextBlock>()
                            .Any(tb => tb.Text == nombreUsuario));

                    if (stackPanel != null)
                    {
                        return stackPanel;
                    }
                }
            }
            return null;
        }

        private static void CambiarEstadoAusente(StackPanel stackPanel)
        {
            foreach ( var hijo in stackPanel.Children)   
            {
                if(hijo is StackPanel estadoPanel)
                {
                    foreach (var estadoHijo in estadoPanel.Children)
                    {
                        if(estadoHijo is Ellipse circulo){
                            circulo.Fill = Brushes.Red;
                        }
                        else if (estadoHijo is Label estadoTexto)
                        {
                            estadoTexto.Content = Properties.Resources.lb_Ausente;
                        }
                    }
                }
            }
        }


        private void BtnIrPerfil(object sender, RoutedEventArgs e)
        {
            if (MarcoPrincipal.Content is PaginaSala paginasala)
            {
                VentanaModalDecision ventanaModalDecision = new 
                    VentanaModalDecision(Properties.Resources.lb_ConfirmarIrPerfil);
                bool? respuesta = ventanaModalDecision.ShowDialog();

                if (respuesta == true)
                {
                    paginasala.AbandonarSala();
                    IrPaginaPerfil();
                }
                else
                {
                    return;
                }
            }

            if (!(MarcoPrincipal.Content is PaginaPerfil))
            {
                IrPaginaPerfil();
            }
        }

        private void IrPaginaPerfil()
        {
            DoubleAnimation animacionDesvanecimiento = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.5)));
            animacionDesvanecimiento.Completed += (s, a) =>
            {
                PaginaPerfil paginaPerfil = new PaginaPerfil(this);
                MarcoPrincipal.Navigate(paginaPerfil);

                DoubleAnimation animacionAparicion = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5)));
                MarcoPrincipal.BeginAnimation(Frame.OpacityProperty, animacionAparicion);
            };
            MarcoPrincipal.BeginAnimation(Frame.OpacityProperty, animacionDesvanecimiento);
        }

        private void BtnCerrarSesion(object sender, RoutedEventArgs e)
        {
            Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, this);

            VentanaModalDecision ventana = new VentanaModalDecision(Properties.Resources.lb_MensajeCerrarSesion,
                Properties.Resources.btn_CerrarSesión, Properties.Resources.global_Cancelar);
            bool? respuesta = ventana.ShowDialog();

            if (respuesta == true)
            {
                CerrarSesion();

                MainWindow mainWindow = new MainWindow();
                this.Close();
                mainWindow.Show();
            }
        }

        private void VentanaMenuCierreAbrupto(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CerrarSesion();
        }

        private void CerrarSesion()
        {
            var proxyGestionJugador = new GestionJugadorClient();
            var proxyGestionAmigos = new GestionAmigosClient();
            string nombreUsuario = Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario;
            try
            {
                if (!string.IsNullOrEmpty(nombreUsuario))
                {
                    proxyGestionJugador.CerrarSesionJugador(nombreUsuario, Properties.Resources.msg_AbandonoSala);
                    CallbackManager.Instance.Desconectar(nombreUsuario);
                    proxyGestionAmigos.NotificarDesconexion(nombreUsuario);
                    proxyGestionAmigos.NotificarDesconexion(nombreUsuario);
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(proxyGestionJugador, ex, this);
            }
        }

        private void BtnSolicitudesAmistad(object sender, RoutedEventArgs e)
        {
            var proxyGestionAmigos = new GestionAmigosClient();
            Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, this);

            var respuesta = proxyGestionAmigos.ObtenerSolicitudesPendientes(Dominio.CuentaUsuario.CuentaUsuarioActual.IdCuentaUsuario);

            if (respuesta.ErrorBD)
            {
                Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                return;
            }
            VentanaGestionarSolicitudesAmistad ventanaGestionarSolicitudesAmistad = new VentanaGestionarSolicitudesAmistad(this);
            ventanaGestionarSolicitudesAmistad.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ventanaGestionarSolicitudesAmistad.ShowDialog();
        }

        private void BtnAgregarAmistad(object sender, RoutedEventArgs e)
        {
            VentanaEnviarSolicitudAmistad ventanaEnviarSolicitudAmistad = new VentanaEnviarSolicitudAmistad();
            ventanaEnviarSolicitudAmistad.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ventanaEnviarSolicitudAmistad.ShowDialog();
        }

        public void ActualizarNombreUsuario(string nuevoTexto)
        {
            lbNombreUsuario.Content = nuevoTexto;
        }

        public void ConvertirImagenPerfil(byte[] fotoBytes)
        {
            if (fotoBytes == null || fotoBytes.Length == 0)
                return;

            using (var flujoMemoria = new MemoryStream(fotoBytes))
            {
                BitmapImage imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.StreamSource = flujoMemoria;
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.EndInit();
                imagen.Freeze();

                ImagenPerfil.Source = imagen;
            }
        }

        public void CargarAmistades()
        {
            var proxyGestionAmigos = new GestionAmigosClient();
            try
            {
                var respuesta = proxyGestionAmigos.ObtenerAmistades(Dominio.CuentaUsuario.CuentaUsuarioActual.IdCuentaUsuario);

                if (respuesta.ErrorBD)
                {
                    Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                    return;
                }

                if (respuesta.Resultado != null && respuesta.Resultado.Length > 0)
                {
                    List<Dominio.Amistad> amistades = respuesta.Resultado
                        .Select(amistad => new Dominio.Amistad
                        {
                            IdAmistad = amistad.IdAmistad,
                            EstadoSolicitud = amistad.EstadoSolicitud,
                            UsuarioPrincipalId = amistad.UsuarioPrincipalId,
                            UsuarioAmigoId = amistad.UsuarioAmigoId
                        })
                        .ToList();

                    foreach (var amistad in amistades)
                    {
                        if (amistad.UsuarioPrincipalId != Dominio.CuentaUsuario.CuentaUsuarioActual.IdCuentaUsuario)
                        {
                            MostrarAmigo(amistad, true);
                        }
                        else
                        {
                            MostrarAmigo(amistad, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 Utilidades.Utilidades.ManejarExcepciones(proxyGestionAmigos, ex, this);
            }
        }

        private void MostrarAmigo(Dominio.Amistad solicitud, bool esAgeno)
        {
            var proxyGestionAmigos = new GestionAmigosClient();
            try
            {
                Dominio.CuentaUsuarioAmigo cuentaUsuarioAmigo = new Dominio.CuentaUsuarioAmigo
                {
                    Usuario = UsuarioAmigo(solicitud, esAgeno).Usuario,
                    Puntaje = UsuarioAmigo(solicitud, esAgeno).Puntaje,
                    Foto = UsuarioAmigo(solicitud, esAgeno).Foto
                };

                var respuesta = proxyGestionAmigos.UsuarioConectado(cuentaUsuarioAmigo.Usuario);

                var panelSolicitud = new Border
                {
                    Background = new SolidColorBrush(Colors.Transparent),
                    Padding = new Thickness(10),
                    BorderBrush = new SolidColorBrush(Colors.LightGray),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(5)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });

                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                
                var fotoUsuario = new Image   
                {
                    Width = 60,
                    Height = 60,
                    Stretch = Stretch.UniformToFill,
                    Margin = new Thickness(0, 0, 15, 0),
                    Clip = new EllipseGeometry { Center = new Point(30, 30), RadiusX = 30, RadiusY = 30 }
                };
                byte[] fotoBytes = cuentaUsuarioAmigo.Foto;
                if (fotoBytes != null)
                {
                    fotoUsuario.Source = ConvertirBytesAImagen(fotoBytes);
                }
                Grid.SetColumn(fotoUsuario, 0);
                Grid.SetRowSpan(fotoUsuario, 2);
                grid.Children.Add(fotoUsuario);

                 
                var stackNombreEstado = new StackPanel   
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var nombreUsuario = new TextBlock
                {
                    Text = cuentaUsuarioAmigo.Usuario,
                    FontSize = 13,
                    FontWeight = FontWeights.Bold
                };
                stackNombreEstado.Children.Add(nombreUsuario);

                var estado = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                
                var puntosUsuario = new TextBlock    
                {
                    Text = $"Puntos: {cuentaUsuarioAmigo.Puntaje}",
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                Grid.SetColumn(puntosUsuario, 1);
                Grid.SetRow(puntosUsuario, 1);
                grid.Children.Add(puntosUsuario);

                
                var botonesPanel = new StackPanel   
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var botonEliminar = new Button
                {
                    Background = Brushes.Transparent,
                    Foreground = Brushes.Black,
                    BorderBrush = Brushes.Transparent,
                    Padding = new Thickness(5),
                    Margin = new Thickness(0, 0, 5, 0),
                    ToolTip = Properties.Resources.btn_EliminarAmigo
                };
                var imagenEliminar = new Image
                {
                    Source = new BitmapImage(new Uri("Imagenes/BotonEliminarAmigo.png", UriKind.Relative)),
                    Width = 30,
                    Height = 30
                };
                botonEliminar.Content = imagenEliminar;
                botonEliminar.Click += (s, e) => EliminarAmistad(solicitud, panelSolicitud);
                botonesPanel.Children.Add(botonEliminar);


                var circulo = new Ellipse
                {
                    Width = 15,
                    Height = 15,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                estado.Children.Add(circulo);

                var textoEstado = new Label
                {
                    FontSize = 12
                };
                estado.Children.Add(textoEstado);


                if (respuesta.Resultado)   
                {
                    circulo.Fill = Brushes.LightGreen;
                    textoEstado.Content = Properties.Resources.lb_EnLínea;
                }
                else   
                {
                    circulo.Fill = Brushes.Red;
                    textoEstado.Content = Properties.Resources.lb_Ausente;
                }

                stackNombreEstado.Children.Add(estado);
                Grid.SetColumn(stackNombreEstado, 1);
                Grid.SetRow(stackNombreEstado, 0);
                grid.Children.Add(stackNombreEstado);

                Grid.SetColumn(botonesPanel, 2);
                Grid.SetRowSpan(botonesPanel, 2);
                grid.Children.Add(botonesPanel);

                panelSolicitud.Child = grid;

                ContenedorNotificaciones.Children.Add(panelSolicitud);
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(proxyGestionAmigos, ex, this);
            }
        }

        private static DobbleGame.Servidor.CuentaUsuario UsuarioAmigo(Dominio.Amistad solicitud, bool esAgeno)
        {
            var proxyGestionAmigos = new GestionAmigosClient();
            if (esAgeno)
            {
                var respuesta = proxyGestionAmigos.ObtenerUsuario(solicitud.UsuarioPrincipalId);
                var cuenta = respuesta.Resultado;
                return cuenta;
            }
            else
            {
                var respuesta = proxyGestionAmigos.ObtenerUsuario(solicitud.UsuarioAmigoId);
                var cuenta = respuesta.Resultado;
                return cuenta;
            }
        }

        private static ImageSource ConvertirBytesAImagen(byte[] fotoBytes)
        {
            if (fotoBytes == null || fotoBytes.Length == 0)
                return null;

            using (var flujoMemoria = new MemoryStream(fotoBytes))
            {
                var imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.StreamSource = flujoMemoria;
                imagen.EndInit();
                return imagen;
            }
        }

        private void EliminarAmistad(Dominio.Amistad amistad, Border panelSolicitud)
        {
            Utilidades.Utilidades.EstaConectado(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, this);

            VentanaEliminarAmigo ventanaEliminarAmigo = new VentanaEliminarAmigo(this, amistad, panelSolicitud);
            ventanaEliminarAmigo.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ventanaEliminarAmigo.ShowDialog();
        }
    }
}

