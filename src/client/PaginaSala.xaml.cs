using DobbleGame.Servidor;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DobbleGame
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class PaginaSala : Page, Servidor.IGestionSalaCallback
    {
        private Servidor.IGestionSala proxySala;
        private const int NUMERO_JUGADORES_MINIMOS_INICIO_PARTIDA = 2;
        private readonly SelectorPlantillaJugador _selectorPlantilla;
        private bool _estaListo = false;
        public ObservableCollection<Jugador> UsuariosConectados { get; set; }
        public bool EsAnfitrion { get; set; }
        public string CodigoSala { get; set; }
        private ControlDeUsuarioNotificacion _controlNotificacion;
        private readonly object _bloqueoInterfaz = new object();

        public PaginaSala(bool esAnfitrion, string codigoSala)
        {
            InitializeComponent();
            EsAnfitrion = esAnfitrion;
            CodigoSala = codigoSala;
            _selectorPlantilla = (SelectorPlantillaJugador)this.Resources["SelectorPlantillaJugador"];
            InicializarDatos();
        }

        private void InicializarDatos()
        {
            this.DataContext = this;
            UsuariosConectados = new ObservableCollection<Jugador>();
            btnIniciarPartida.DataContext = UsuariosConectados;
            _selectorPlantilla.IniciarlizarPlantillas();

            Application.Current.Dispatcher.Invoke(() =>
            {
                _controlNotificacion = new ControlDeUsuarioNotificacion();
                contenedorNotificacion.Content = _controlNotificacion;
            });

        }

        public bool IniciarSesionSala()
        {
            if (EsAnfitrion && CodigoSala == null)
            {
                return CrearSala();
            }
            else
            {
                return UnirseASala();
            }
        }

        private bool CrearSala()
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return false;
            }

            InicializarProxySiEsNecesario();

            bool resultado = false;

            try
            {
                CodigoSala = proxySala.GenerarCodigoNuevaSala();
                btnCodigoSala.Content = CodigoSala;

                var usuarioActual = Dominio.CuentaUsuario.CuentaUsuarioActual;

                resultado = proxySala.CrearNuevaSala(usuarioActual.Usuario, CodigoSala);

                if (resultado)
                {
                    proxySala.UnirseASala(usuarioActual.Usuario, CodigoSala, Properties.Resources.msg_UnionSala, EsAnfitrion);
                    proxySala.NotificarUsuarioConectado(CodigoSala);
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
            }

            return resultado;
        }

        private bool UnirseASala()
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return false;
            }

            InicializarProxySiEsNecesario();

            bool resultado = false;

            try
            {
                btnCodigoSala.Content = CodigoSala;
                var usuarioActual = Dominio.CuentaUsuario.CuentaUsuarioActual;
                resultado = proxySala.UnirseASala(usuarioActual.Usuario, CodigoSala, Properties.Resources.msg_UnionSala, EsAnfitrion);
                proxySala.NotificarUsuarioConectado(CodigoSala);
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
            }

            return resultado;
        }

        public void AbandonarSala()
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            InicializarProxySiEsNecesario();

            try
            {
                var usuarioActual = Dominio.CuentaUsuario.CuentaUsuarioActual;
                proxySala.AbandonarSala(usuarioActual.Usuario, CodigoSala, Properties.Resources.msg_AbandonoSala);

                ((ICommunicationObject)proxySala).Close();
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
            }
        }

        public bool HayEspacioEnSala()
        {
            InicializarProxySiEsNecesario();

            try
            {
                return proxySala.HayEspacioSala(CodigoSala);
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
                return false;
            }
        }

        public bool ExisteSala()
        {
            InicializarProxySiEsNecesario();

            try
            {
                return proxySala.ExisteSala(CodigoSala);
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
                return false;
            }
        }

        public bool EsSalaDisponible()
        {
            InicializarProxySiEsNecesario();

            try
            {
                return proxySala.EsSalaDisponible(CodigoSala);
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
                return false;
            }
        }

        private void BtnRegresar(object sender, RoutedEventArgs e)
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            AbandonarSala();
            IrPaginaMenu();
        }

        private void IrPaginaMenu()
        {
            lock (_bloqueoInterfaz)
            {
                Dispatcher.Invoke(() =>
                {
                    DoubleAnimation fadeOutAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.5)));
                    fadeOutAnimation.Completed += (s, a) =>
                    {
                        PaginaMenu paginaMenu = new PaginaMenu();
                        this.NavigationService.Navigate(paginaMenu);

                        AnimateElementsInPaginaMenu(paginaMenu);
                    };
                    this.BeginAnimation(Frame.OpacityProperty, fadeOutAnimation);
                });
            }
        }

        private void AnimateElementsInPaginaMenu(PaginaMenu paginaMenu)
        {
            if (paginaMenu.Content is Panel panel)
            {
                foreach (UIElement element in panel.Children)
                {
                    element.Opacity = 0;

                    DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5)))
                    {
                        BeginTime = TimeSpan.FromMilliseconds(200)
                    };

                    element.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
                }
            }
        }

        private async void BtnEnviarMensaje(object sender, RoutedEventArgs e)
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            string mensaje = tbChat.Text;

            InicializarProxySiEsNecesario();

            try
            {
                if (!String.IsNullOrEmpty(mensaje))
                {
                    await proxySala.EnviarMensajeSalaAsync(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, CodigoSala, mensaje);
                    tbChat.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
            }
        }

        private void EnviarMensajeEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    btnEnviarMensaje.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                catch (Exception ex)
                {
                    Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
                }
            }
        }

        private async void BtnIniciarPartida(object sender, RoutedEventArgs e)
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            try
            {
                if (!EsAnfitrion)
                {
                    _controlNotificacion.MostrarNotificacion(Properties.Resources.msg_DebesSerAnfitrion);
                    return;
                }

                if (UsuariosConectados.Count >= NUMERO_JUGADORES_MINIMOS_INICIO_PARTIDA)
                {
                    await Task.Delay(555);
                    if (await proxySala.TodosLosJugadoresEstanListosAsync(CodigoSala))
                    {
                        IniciarPartida();
                    }
                    else
                    {
                        _controlNotificacion.MostrarNotificacion(Properties.Resources.msg_JugadoresNoListos);
                    }
                }
                else
                {
                    _controlNotificacion.MostrarNotificacion(Properties.Resources.msg_MinimoJugadores);
                }
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
            }
        }

        private void IniciarPartida()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    int numeroJugadoresEsperados = UsuariosConectados.Count;
                    VentanaPartida ventanaPartida = new VentanaPartida(CodigoSala, EsAnfitrion, numeroJugadoresEsperados, Window.GetWindow(this));
                    if (ventanaPartida.IniciarSesionPartida())
                    {
                        proxySala.CambiarVentanaParaTodos(CodigoSala);
                        ventanaPartida.Show();
                        Window.GetWindow(this).Hide();
                    }
                });
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
            }
        }

        private void BtnExpulsar(object sender, RoutedEventArgs e)
        {
            Button boton = sender as Button;

            var jugadorAExpulsar = boton?.DataContext as Jugador;

            if (jugadorAExpulsar != null)
            {
                string mensaje = String.Format(Properties.Resources.lb_MensajeExpulsarJugador, jugadorAExpulsar.Usuario);
                VentanaModalDecision ventanaModalDecision = new VentanaModalDecision(mensaje,
                    Properties.Resources.btn_Expulsar, Properties.Resources.global_Cancelar);
                bool? respuesta = ventanaModalDecision.ShowDialog();

                if (respuesta == true)
                {
                    if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
                    {
                        return;
                    }

                    try
                    {
                        ExpulsarJugador(jugadorAExpulsar.Usuario);
                    }
                    catch (Exception ex)
                    {
                        Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
                    }
                }
            }
        }

        private void ExpulsarJugador(string nombreUsuario)
        {
            InicializarProxySiEsNecesario();

            try
            {
                proxySala.ExpulsarJugador(nombreUsuario, CodigoSala, Properties.Resources.msg_ExpulsionSala);
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
            }
        }

        public void NotificarExpulsionAJugador()
        {
            ((ICommunicationObject)proxySala).Close();
            IrPaginaMenu();
            _controlNotificacion.MostrarNotificacion(Properties.Resources.lb_HasSidoExpulsado);
        }

        public void MostrarMensajeSala(string mensaje)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                tbContenedor.Text += $"{mensaje}{Environment.NewLine}";
                scrollViewer.ScrollToEnd();
            }));
        }

        public void ActualizarUsuariosConectados(Jugador[] usuariosConectados)
        {
            UsuariosConectados.Clear();
            _selectorPlantilla.ReiniciarPlantillas();
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (var usuario in usuariosConectados)
                {
                    Console.WriteLine($"Usuario: {usuario.Usuario}, Puntaje: {usuario.Puntaje}, EsAnfitrion: {usuario.EsAnfitrion}");
                    UsuariosConectados.Add(usuario);
                }
            }));
        }

        public void ConvertirEnAnfitrion(string nombreUsuario)
        {
            EsAnfitrion = true;

            this.Dispatcher.Invoke(() =>
            {
                var usuario = UsuariosConectados.FirstOrDefault(u => u.Usuario == nombreUsuario);

                if (usuario != null)
                {
                    usuario.EsAnfitrion = true;
                }
            });

            _controlNotificacion.MostrarNotificacion(Properties.Resources.msg_EresAnfitrion);
        }

        public void CambiarVentana()
        {
            Dispatcher.Invoke(() =>
            {
                int numeroJugadoresEsperados = UsuariosConectados.Count;
                VentanaPartida ventanaPartida = new VentanaPartida(CodigoSala, EsAnfitrion, numeroJugadoresEsperados, Window.GetWindow(this));
                if (ventanaPartida.IniciarSesionPartida())
                {
                    ventanaPartida.Show();
                    Window.GetWindow(this).Hide();
                }
            });
        }

        private void BtnJugadorListo(object sender, RoutedEventArgs e)
        {
            if (!Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow))
            {
                return;
            }

            if (_estaListo)
            {
                _controlNotificacion.MostrarNotificacion(Properties.Resources.msg_YaEstasListo);
                return;
            }

            try
            {
                proxySala.NotificarJugadorListo(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, CodigoSala);
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones((ICommunicationObject)proxySala, ex, this);
            }
        }

        public void MostrarJugadorListo(string nombreUsuario, bool estaListo)
        {
            this.Dispatcher.Invoke(() =>
            {
                var usuario = UsuariosConectados.FirstOrDefault(u => u.Usuario == nombreUsuario);

                if (usuario != null)
                {
                    if (usuario.Usuario == Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario)
                    {
                        this._estaListo = estaListo;
                    }

                    usuario.EstaListo = estaListo;
                }
            });
        }

        public bool PingSala()
        {
            return true;
        }

        private void BtnCopiarCodigoSala(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton)
            {
                Clipboard.SetDataObject(boton.Content.ToString(), true);
                _controlNotificacion.MostrarNotificacion(Properties.Resources.msg_CodigoCopiado);
            }
        }

        private void InicializarProxy()
        {
            if (proxySala != null && ((ICommunicationObject)proxySala).State == CommunicationState.Opened)
            {
                return;
            }

            if (proxySala != null && ((ICommunicationObject)proxySala).State == CommunicationState.Faulted)
            {
                ((ICommunicationObject)proxySala).Abort();
            }

            var contexto = new InstanceContext(this);
            var factory = new DuplexChannelFactory<IGestionSala>(contexto, "NetTcpBinding_IGestionSala");
            proxySala = factory.CreateChannel();
        }

        private void InicializarProxySiEsNecesario()
        {
            if (proxySala == null || ((ICommunicationObject)proxySala).State != CommunicationState.Opened)
            {
                InicializarProxy();
            }
        }

        private void TbChatTieneElFoco(object sender, RoutedEventArgs e)
        {
            borderContenedor.OpacityMask = null;
        }

        private void TbChatPierdeElFoco(object sender, RoutedEventArgs e)
        {
            borderContenedor.OpacityMask = new LinearGradientBrush
            {
                StartPoint = new Point(0, 4),
                EndPoint = new Point(0, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.Black, 0.5),
                    new GradientStop(Colors.Transparent, 1)
                }
            };
        }

        private void OcultarDialogo(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is TextBox))
            {
                var scope = FocusManager.GetFocusScope(this);
                FocusManager.SetFocusedElement(scope, this as IInputElement);
                Keyboard.ClearFocus();
            }
        }

        private void BtnEnviarMensajeTieneElFoco(object sender, RoutedEventArgs e)
        {
            borderContenedor.OpacityMask = null;
        }

        private void BtnEnviarMensajePierdeElFoco(object sender, RoutedEventArgs e)
        {
            borderContenedor.OpacityMask = new LinearGradientBrush
            {
                StartPoint = new Point(0, 4),
                EndPoint = new Point(0, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.Black, 0.5),
                    new GradientStop(Colors.Transparent, 1)
                }
            };
        }
    }
}