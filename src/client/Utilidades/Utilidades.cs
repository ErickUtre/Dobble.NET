using DobbleGame.Servidor;
using System;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DobbleGame.Utilidades
{
    public static class Utilidades
    {
        private static readonly Regex CorreoRegex = new Regex(
            @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(2)
        );

        public static bool ValidarContraseña(string contraseña)
        {

            if (contraseña.Contains(" "))
            {
                return false;
            }

            if (contraseña.Length < 8)
            {
                return false;
            }

            if (!contraseña.Any(Char.IsUpper))
            {
                return false;
            }

            if (contraseña.Count(Char.IsDigit) < 2)
            {
                return false;
            }

            return true;
        }

        public static bool EsCampoVacio(string contenido)
        {
            return string.IsNullOrWhiteSpace(contenido);
        }

        public static bool EsMismaContraseña(string contraseña, string contraseñaConfirmacion)
        {
            if (contraseña.Equals(contraseñaConfirmacion))
            {
                return true;
            }
            return false;
        }

        public static bool ValidarPatronCorreo(string correo)
        {
            return CorreoRegex.IsMatch(correo);
        }

        public static System.Windows.Media.Brush StringABrush(string colorString)
        {
            var convertidorBrush = new BrushConverter();
            return (System.Windows.Media.Brush)convertidorBrush.ConvertFromString(colorString);
        }
        public static void MostrarVentanaErrorConexionBD(Window contenedor)
        {
            var ventanaErrorBD = new VentanaErrorBD(
                             Properties.Resources.lb_ErrorConexiónBD,
                             Properties.Resources.lb_MensajeErrorConexiónBD
                         )
            {
                Owner = contenedor,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            ventanaErrorBD.ShowDialog();
        }

        public static void MostrarVentanaErrorConexionServidor(FrameworkElement contenedor, bool estaEnCanal)
        {
            MainWindow inicioSesion = new MainWindow();
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != null && window != inicioSesion && window.IsLoaded)
                        {
                            window.Close();
                        }
                    }
                });

                inicioSesion.Show();
                contenedor = inicioSesion;

                var ventanaErrorConexion = new VentanaErrorConexion
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                if (estaEnCanal)
                {
                    ventanaErrorConexion.MostrarDialogo(Properties.Resources.lb_ErrorConexiónServidor,
                        Properties.Resources.lb_MensajeErrorConexiónServidor);
                }
                else
                {
                    ventanaErrorConexion.MostrarDialogo(Properties.Resources.lb_ErrorConexiónServidor,
                        Properties.Resources.lb_MensajeErrorCanalServidor);


                }

                if (contenedor is Window ventana && ventana.IsLoaded)
                {
                    ventanaErrorConexion.Owner = ventana;
                }
                else if (contenedor is Page pagina && pagina.Parent is Window ventanaPrincipal && ventanaPrincipal.IsLoaded)
                {
                    ventanaErrorConexion.Owner = ventanaPrincipal;
                }
                ventanaErrorConexion.ShowDialog();
            }
            catch (QuotaExceededException ex)
            {
                Registro.Error($"Excepción de QuotaExceededException: {ex.Message}. " +
                                   $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
            }
            catch (Exception ex)
            {
                Registro.Error($"Excepción no manejada: {ex.Message}. " +
                                   $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
            }
        }

        public static void MostrarMensajeStackPanel(StackPanel contenedor, Label lbMensaje, string mensaje)
        {
            contenedor.Visibility = Visibility.Visible;
            lbMensaje.Content = mensaje;
        }

        public static void ManejarExcepciones(ICommunicationObject proxy, Exception ex, FrameworkElement contenedor)
        {
            if (proxy != null)
            {
                try
                {
                    if (proxy.State == CommunicationState.Faulted)
                    {
                        proxy.Abort();
                        Registro.Error($"Proxy en estado 'Faulted'. Se abortó la conexión: {proxy}");
                    }
                    else
                    {
                        proxy.Close();
                        Registro.Informacion($"Proxy cerrado correctamente. Estado: {proxy.State}");
                    }
                }
                catch (Exception proxyEx)
                {
                    proxy.Abort();
                    Registro.Fatal($"Error al intentar cerrar el proxy: {proxyEx.Message}. Proxy abortado");
                }

                if (ex is CommunicationObjectFaultedException)
                {
                    Registro.Error($"Estado del proxy: {proxy.State}. \nExcepción de CommunicationObjectFaultedException: {ex.Message}. " +
                                   $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
                    MostrarVentanaErrorConexionServidor(contenedor, true);
                }
                else if (ex is CommunicationException)
                {
                    Registro.Error($"Estado del proxy: {proxy.State}. \nExcepción de CommunicationException: {ex.Message}. " +
                                   $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
                    MostrarVentanaErrorConexionServidor(contenedor, true);
                }
                else if (ex is TimeoutException)
                {
                    Registro.Error($"Estado del proxy: {proxy.State}. \nExcepción de TimeoutException: {ex.Message}. " +
                                   $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
                    MostrarVentanaErrorConexionServidor(contenedor, true);
                }
                else
                {
                    Registro.Error($"Estado del proxy: {proxy.State}. \nExcepción no manejada: {ex.Message}. " +
                                   $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
                    MostrarVentanaErrorConexionServidor(contenedor, true);
                }
            }
        }


        public static bool EstaConectado(string nombreUsuario, Window ventana)
        {
            var proxy = new GestionAmigosClient();
            try
            {
                var estaConectado = proxy.UsuarioConectado(nombreUsuario);
                if (!estaConectado.Resultado)
                {
                    MostrarVentanaErrorConexionServidor(ventana, false);
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                ManejarExcepciones(proxy, ex, ventana);
                return false;
            }
        }

        public static bool PingConexion(string nombreUsuario, Window ventana)
        {
            var proxyGestionJugador = new GestionJugadorClient();
            try
            {
                var estaConectado = proxyGestionJugador.Ping(nombreUsuario);
                if (!estaConectado)
                {
                    MostrarVentanaErrorConexionServidor(ventana, false);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ManejarExcepciones(proxyGestionJugador, ex, ventana);
                return false;
            }
        }


        public static void ManejarExcepcionErrorConexion(Exception ex, ICommunicationObject proxy)
        {
            Registro.Error($"Estado del proxy: {proxy.State}. \nExcepción: {ex.GetType().Name}: {ex.Message}." +
                           $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}");
            proxy.Abort();
        }
    }
}
