using DobbleGame.Servidor;
using DobbleGame.Utilidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DobbleGame
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Hace que el contenido de CUALQUIER ventana se escale de forma
            // uniforme para adaptarse a la resolución de la pantalla en la que se
            // ejecuta, sin importar el tamaño de esta. Se aplica de forma global
            // a todas las ventanas para no tener que modificar cada XAML.
            EventManager.RegisterClassHandler(
                typeof(Window),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(AdaptarVentanaAResolucion));
        }

        private void AdaptarVentanaAResolucion(object sender, RoutedEventArgs e)
        {
            Window ventana = sender as Window;
            if (ventana == null)
            {
                return;
            }

            // Si ya fue procesada (su contenido es un Viewbox) no se vuelve a envolver.
            FrameworkElement contenido = ventana.Content as FrameworkElement;
            if (contenido == null || contenido is Viewbox)
            {
                return;
            }

            // Tamaño de diseño = el tamaño con el que se creó la ventana en XAML.
            // OJO: en una ventana maximizada, Width/Height ya reflejan el tamaño
            // maximizado; el tamaño de diseño real está en RestoreBounds.
            bool maximizada = ventana.WindowState == WindowState.Maximized;
            double anchoDiseno = ObtenerDimensionDeDiseno(maximizada, ventana.RestoreBounds.Width, ventana.Width, contenido.ActualWidth);
            double altoDiseno = ObtenerDimensionDeDiseno(maximizada, ventana.RestoreBounds.Height, ventana.Height, contenido.ActualHeight);
            if (anchoDiseno <= 0 || altoDiseno <= 0)
            {
                return;
            }

            // El contenido se "congela" a su tamaño de diseño y se mete en un
            // Viewbox, que lo escala uniformemente al tamaño real de la ventana.
            ventana.Content = null;
            contenido.Width = anchoDiseno;
            contenido.Height = altoDiseno;
            ventana.Content = new Viewbox
            {
                Stretch = Stretch.Uniform,
                Child = contenido
            };

            // En las ventanas maximizadas (pantallas de juego) el escalado
            // uniforme puede dejar franjas a los lados; se rellenan con el fondo
            // de la aplicación para que combinen con el tema.
            if (ventana.WindowState == WindowState.Maximized && !ventana.AllowsTransparency)
            {
                ventana.Background = new ImageBrush(
                    new BitmapImage(new Uri("pack://application:,,,/Imagenes/FondoPanal.jpg")))
                {
                    Stretch = Stretch.UniformToFill
                };
            }
        }

        private static double ObtenerDimensionDeDiseno(bool maximizada, double valorRestaurado, double valorVentana, double valorActual)
        {
            // En ventanas maximizadas el tamaño de diseño es el de restauración.
            if (maximizada && !double.IsNaN(valorRestaurado) && valorRestaurado > 0)
            {
                return valorRestaurado;
            }
            if (!double.IsNaN(valorVentana) && valorVentana > 0)
            {
                return valorVentana;
            }
            if (!double.IsNaN(valorRestaurado) && valorRestaurado > 0)
            {
                return valorRestaurado;
            }
            return valorActual;
        }
    }
}
