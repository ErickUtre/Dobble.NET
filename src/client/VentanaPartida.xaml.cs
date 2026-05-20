using DobbleGame.Servidor;
using DobbleGame.Utilidades;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DobbleGame
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class VentanaPartida : Window, Servidor.IGestionPartidaCallback
    {
        private Servidor.IGestionPartida _proxyGestionPartida;
        private DispatcherTimer _cronometro;
        private int _contador;
        private SelectorPlantillaJugadorPartida _selectorPlantilla;
        private readonly Window _ventana;
        private readonly int _numeroJugadoresEsperados;
        private bool _partidaIniciada = false;
        private bool _permitirCierreInesperado = true;
        private readonly string _codigoSala;
        public ObservableCollection<Jugador> JugadoresEnPartida { get; set; }
        public bool EsAnfitrion { get; set; }

        public VentanaPartida(string codigoSala, bool esAnfitrion, int numeroJugadoresEsperados, Window ventana)
        {
            InitializeComponent();
            InicializarDatos();
            _codigoSala = codigoSala;
            EsAnfitrion = esAnfitrion;
            _numeroJugadoresEsperados = numeroJugadoresEsperados;
            _ventana = ventana;
        }

        private void InicializarDatos()
        {
            this.Closing += VentanaMenuCierreAbrupto;
            this.DataContext = this;
            JugadoresEnPartida = new ObservableCollection<Jugador>();
            JugadoresEnPartida.CollectionChanged += JugadoresEnPartidaIniciarPartida;
            _selectorPlantilla = (SelectorPlantillaJugadorPartida)this.Resources["SelectorPlantillaJugadorPartida"];
        }

        private void VentanaMenuCierreAbrupto(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_permitirCierreInesperado)
            {
                var proxyGestionJugador = new GestionJugadorClient();
                string nombreUsuario = Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario;

                try
                {
                    _proxyGestionPartida.AbandonarPartida(nombreUsuario, _codigoSala);
                    proxyGestionJugador.CerrarSesionJugador(nombreUsuario, Properties.Resources.msg_AbandonoSala);
                }
                catch (Exception ex)
                {
                    Utilidades.Utilidades.ManejarExcepciones(proxyGestionJugador, ex, this);
                }

                if (!Dominio.CuentaUsuario.CuentaUsuarioActual.EsInvitado)
                {
                    var proxyGestionAmigos = new GestionAmigosClient();
                    try
                    {
                        if (!string.IsNullOrEmpty(nombreUsuario))
                        {
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
            }
        }

        public bool PingPartida()
        {
            return true;
        }

        private void BtnRegresar(object sender, RoutedEventArgs e)
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            InicializarProxySiEsNecesario();
            _permitirCierreInesperado = false;
            VentanaModalDecision ventanaModalDecision = new VentanaModalDecision(Properties.Resources.lb_MensajeAbandonarPartida);
            bool? respuesta = ventanaModalDecision.ShowDialog();

            if (respuesta == true)
            {
                try
                {
                    var usuarioActual = Dominio.CuentaUsuario.CuentaUsuarioActual;

                    if (_proxyGestionPartida.AbandonarPartida(usuarioActual.Usuario, _codigoSala))
                    {
                        ((ICommunicationObject)_proxyGestionPartida).Close();
                        IrPaginaMenu();
                    }
                }
                catch (Exception ex)
                {
                    Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)_proxyGestionPartida, ex, this);
                }
            }
        }

        private void IrPaginaMenu()
        {
            Dispatcher.Invoke(() =>
            {
                PaginaMenu paginaMenu = new PaginaMenu();
                if (_ventana is VentanaMenu ventanaMenu)
                {
                    ventanaMenu.MarcoPrincipal.NavigationService.Navigate(paginaMenu);
                }
                else if (_ventana is VentanaMenuInvitado ventanaMenuInvitado)
                {
                    ventanaMenuInvitado.MarcoPrincipal.NavigationService.Navigate(paginaMenu);
                }

                _ventana.Show();
                this.Close();
            });
        }

        public bool IniciarSesionPartida()
        {
            if (EsAnfitrion)
            {
                return CrearPartida();
            }
            else
            {
                return UnirseAPartida();
            }
        }

        private bool CrearPartida()
        {
            InicializarProxySiEsNecesario();

            bool resultado = false;

            try
            {
                resultado = _proxyGestionPartida.CrearNuevaPartida(_codigoSala);

                if (resultado)
                {
                    _proxyGestionPartida.UnirJugadorAPartida(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, _codigoSala);
                    _proxyGestionPartida.NotificarActualizacionDeJugadoresEnPartida(_codigoSala);
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)_proxyGestionPartida, ex, this);
            }

            return resultado;
        }

        private bool UnirseAPartida()
        {
            InicializarProxySiEsNecesario();

            bool resultado = false;

            try
            {
                _proxyGestionPartida.UnirJugadorAPartida(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, _codigoSala);
                _proxyGestionPartida.NotificarActualizacionDeJugadoresEnPartida(_codigoSala);
                resultado = true;
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)_proxyGestionPartida, ex, this);
            }

            return resultado;
        }



        private void JugadoresEnPartidaIniciarPartida(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!_partidaIniciada && JugadoresEnPartida.Count == _numeroJugadoresEsperados)
            {
                _partidaIniciada = true;
                if (EsAnfitrion)
                {
                    try
                    {
                        _proxyGestionPartida.NotificarInicioPartida(_codigoSala);
                    }
                    catch (Exception ex)
                    {
                        Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)_proxyGestionPartida, ex, this);
                    }
                }
            }
        }

        public void ActualizarJugadoresEnPartida(Jugador[] jugadoresConectados)
        {
            JugadoresEnPartida.Clear();
            _selectorPlantilla.ReiniciarPlantillasPartida();
            this.Dispatcher.Invoke(() =>
            {
                foreach (var jugador in jugadoresConectados)
                {
                    JugadoresEnPartida.Add(jugador);
                }
            });
        }

        public void ActualizarPuntosEnPartida(string nombreUsuario, int puntosEnPartida)
        {
            this.Dispatcher.Invoke(() =>
            {
                var jugadorEnPartida = JugadoresEnPartida.FirstOrDefault(j => j.Usuario == nombreUsuario);

                if (jugadorEnPartida != null)
                {
                    jugadorEnPartida.PuntosEnPartida = puntosEnPartida;
                }
            });
        }

        private async void BtnValidarIcono(object sender, RoutedEventArgs e)
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            if (sender is Button boton && boton.Content is Image imagenIcono && imagenIcono.Source != null)
            {
                string rutaIcono = imagenIcono.Source.ToString();
                try
                {
                    await _proxyGestionPartida.ValidarCartaAsync(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, rutaIcono, _codigoSala);
                }
                catch (Exception ex)
                {
                    Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)_proxyGestionPartida, ex, this);
                }
            }
        }


        public async void AsignarCarta(Carta carta)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                Icono1.Source = new BitmapImage(new Uri(carta.Iconos[0].Ruta));
                Icono2.Source = new BitmapImage(new Uri(carta.Iconos[1].Ruta));
                Icono3.Source = new BitmapImage(new Uri(carta.Iconos[2].Ruta));
                /*Icono4.Source = new BitmapImage(new Uri(carta.Iconos[3].Ruta));
                Icono5.Source = new BitmapImage(new Uri(carta.Iconos[4].Ruta));
                Icono6.Source = new BitmapImage(new Uri(carta.Iconos[5].Ruta));
                Icono7.Source = new BitmapImage(new Uri(carta.Iconos[6].Ruta));
                Icono8.Source = new BitmapImage(new Uri(carta.Iconos[7].Ruta));*/
            });
        }


        public async void AsignarCartaCentral(Carta cartaCentral, int cartasRestantes)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                tblCartasRestantes.Text = String.Format(Properties.Resources.lb_CartasRestantes, cartasRestantes);
                IconoCentral1.Source = new BitmapImage(new Uri(cartaCentral.Iconos[0].Ruta));
                IconoCentral2.Source = new BitmapImage(new Uri(cartaCentral.Iconos[1].Ruta));
                IconoCentral3.Source = new BitmapImage(new Uri(cartaCentral.Iconos[2].Ruta));
                /*IconoCentral4.Source = new BitmapImage(new Uri(cartaCentral.Iconos[3].Ruta));
                IconoCentral5.Source = new BitmapImage(new Uri(cartaCentral.Iconos[4].Ruta));
                IconoCentral6.Source = new BitmapImage(new Uri(cartaCentral.Iconos[5].Ruta));
                IconoCentral7.Source = new BitmapImage(new Uri(cartaCentral.Iconos[6].Ruta));
                IconoCentral8.Source = new BitmapImage(new Uri(cartaCentral.Iconos[7].Ruta));*/
            });
        }


        public async void BloquearCarta()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                Button[] botones = { boton1, boton2, boton3, boton4, boton5, boton6, boton7, boton8 };

                foreach (var boton in botones)
                {
                    var triangulo = (Polygon)boton.Template.FindName("triangulo", boton);
                    triangulo.Fill = new SolidColorBrush(Colors.IndianRed);
                }

                cartaJugador.IsEnabled = false;
            });
        }

        public async void DesbloquearCarta()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                Button[] botones = { boton1, boton2, boton3, boton4, boton5, boton6, boton7, boton8 };

                foreach (var boton in botones)
                {
                    var triangulo = (Polygon)boton.Template.FindName("triangulo", boton);
                    triangulo.Fill = new SolidColorBrush(Colors.White);
                }

                cartaJugador.IsEnabled = true;
            });
        }

        public void IniciarPartida()
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            IniciarContador();
        }

        private void IniciarContador()
        {
            _contador = 6;
            _cronometro = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _cronometro.Tick += Cronometro_Tick;
            _cronometro.Start();
        }

        private void Cronometro_Tick(object sender, EventArgs e)
        {
            if (_contador > 0)
            {
                if (_contador != 1)
                {
                    int contadorTexto = _contador - 1;
                    tblTexto.Text = contadorTexto.ToString();
                    _contador--;
                }
                else
                {
                    tblTexto.FontSize = 300;
                    tblTexto.Text = Properties.Resources.lb_Dobble;
                    _contador--;
                }
            }
            else
            {
                _cronometro.Stop();
                cartaCentral.Visibility = Visibility.Visible;
                tblTexto.Visibility = Visibility.Collapsed;

                if (EsAnfitrion)
                {
                    try
                    {
                        _proxyGestionPartida.NotificarDistribucionCartas(_codigoSala);
                    }
                    catch (Exception ex)
                    {
                        Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)_proxyGestionPartida, ex, this);
                    }
                }

                btnRegresar.IsEnabled = true;
                btnRegresar.Opacity = 1;
            }
        }

        public async void FinalizarPartida()
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            btnRegresar.Visibility = Visibility.Collapsed;
            tblCartasRestantes.Visibility = Visibility.Collapsed;
            cartaCentral.Visibility = Visibility.Collapsed;
            tblTexto.FontSize = 200;
            tblTexto.Text = Properties.Resources.lb_FinDelJuego;
            tblTexto.Visibility = Visibility.Visible;
            cartaJugador.Visibility = Visibility.Collapsed;
            controlJugadores.Visibility = Visibility.Collapsed;
            await AnimacionFinalizarPartida();
        }

        public async Task RegistrarPuntosDelJugador()
        {
            Jugador jugador = JugadoresEnPartida.FirstOrDefault(j => j.Usuario == Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario);

            if (jugador != null && !Dominio.CuentaUsuario.CuentaUsuarioActual.EsInvitado)
            {
                bool exito = false;

                while (!exito)
                {
                    try
                    {
                        var respuestaRegistroPuntos = await Task.Run(() =>
                        _proxyGestionPartida.GuardarPuntosJugador(jugador.Usuario, jugador.PuntosEnPartida));

                        if (respuestaRegistroPuntos.ErrorBD)
                        {
                            Utilidades.Utilidades.MostrarVentanaErrorConexionBD(this);
                            continue;
                        }

                        exito = true;
                    }
                    catch (Exception ex)
                    {
                        Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)_proxyGestionPartida, ex, this);
                    }

                }
            }

            if (jugador != null && Dominio.CuentaUsuario.CuentaUsuarioActual.EsInvitado)
            {
                try
                {
                    await _proxyGestionPartida.GuardarPuntosJugadorInvitadoAsync(jugador.Usuario, jugador.PuntosEnPartida);
                }
                catch (Exception ex)
                {
                    Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)_proxyGestionPartida, ex, this);
                }
            }
        }

        private async Task AnimacionFinalizarPartida()
        {
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1)
            };
            tblTexto.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);

            DoubleAnimation fontSizeAnimation = new DoubleAnimation
            {
                From = 50,
                To = 200,
                Duration = TimeSpan.FromSeconds(1)
            };

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            fontSizeAnimation.Completed += (s, e) => tcs.SetResult(true);

            tblTexto.BeginAnimation(System.Windows.Controls.TextBlock.FontSizeProperty, fontSizeAnimation);

            await tcs.Task;

            var contador = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            TaskCompletionSource<bool> timerTcs = new TaskCompletionSource<bool>();
            contador.Tick += async (sender, args) =>
            {
                contador.Stop();
                tblTexto.Visibility = Visibility.Collapsed;
                await RegresarASala();
                timerTcs.SetResult(true);
            };
            contador.Start();

            await timerTcs.Task;
        }


        private async Task RegresarASala()
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            List<Jugador> jugadoresOrdenadosPorPuntos = JugadoresEnPartida.OrderByDescending(j => j.PuntosEnPartida).ToList();

            VentanaMarcadorPartida ventanaMarcadorPartida = new VentanaMarcadorPartida(jugadoresOrdenadosPorPuntos);
            bool? resultado = ventanaMarcadorPartida.ShowDialog();

            if (resultado == true)
            {
                try
                {
                    if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
                    {
                        return;
                    }
                    await RegistrarPuntosDelJugador();
                    _permitirCierreInesperado = false;
                    await _proxyGestionPartida.RegresarASalaAsync(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, _codigoSala);
                }
                catch (Exception ex)
                {
                    Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)_proxyGestionPartida, ex, this);
                }

                this.Close();
                IrASala();
            }
        }

        private void IrASala()
        {
            Dispatcher.Invoke(() =>
            {
                PaginaSala paginaSala = new PaginaSala(EsAnfitrion, _codigoSala);
                if (paginaSala.IniciarSesionSala())
                {
                    if (_ventana is VentanaMenu ventanaMenu)
                    {
                        ventanaMenu.MarcoPrincipal.NavigationService.Navigate(paginaSala);
                    }
                    else if (_ventana is VentanaMenuInvitado ventanaMenuInvitado)
                    {
                        ventanaMenuInvitado.MarcoPrincipal.NavigationService.Navigate(paginaSala);
                    }
                    _ventana.Show();
                }
            });
        }

        public void ConvertirEnAnfitrionDesdePartida()
        {
            EsAnfitrion = true;
        }

        private void InicializarProxy()
        {
            if (_proxyGestionPartida != null && ((ICommunicationObject)_proxyGestionPartida).State == CommunicationState.Opened)
            {
                return;
            }

            if (_proxyGestionPartida != null && ((ICommunicationObject)_proxyGestionPartida).State == CommunicationState.Faulted)
            {
                ((ICommunicationObject)_proxyGestionPartida).Abort();
            }

            var contexto = new InstanceContext(this);
            var fabrica = new DuplexChannelFactory<IGestionPartida>(contexto, "NetTcpBinding_IGestionPartida");
            _proxyGestionPartida = fabrica.CreateChannel();
        }

        private void InicializarProxySiEsNecesario()
        {
            if (_proxyGestionPartida == null || ((ICommunicationObject)_proxyGestionPartida).State != CommunicationState.Opened)
            {
                InicializarProxy();
            }
        }

    }
}