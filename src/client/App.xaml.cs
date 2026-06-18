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
using System.Windows.Navigation;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

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

            // Las ventanas maximizadas que alojan páginas (menú, perfil, sala) en
            // un Frame tienen el problema de que cada página pinta su PROPIO fondo,
            // que no coincide ni en escala ni en color con el de la ventana. Por eso
            // se traslada el fondo de la página a la ventana CADA VEZ que se navega,
            // para que cubra toda la pantalla (incluidas las franjas del escalado
            // uniforme) sin dejar marcos de otro color.
            EventManager.RegisterClassHandler(
                typeof(Frame),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(EngancharNavegacionDeFrame));
        }

        private void EngancharNavegacionDeFrame(object sender, RoutedEventArgs e)
        {
            Frame frame = sender as Frame;
            if (frame == null)
            {
                return;
            }

            // Navigated se dispara de forma fiable en cada navegación del Frame.
            frame.Navigated -= FrameNavego;
            frame.Navigated += FrameNavego;

            // La página inicial puede haberse navegado antes de este enganche.
            AdaptarFondoPaginaAVentana(frame, frame.Content);
        }

        private void FrameNavego(object sender, NavigationEventArgs e)
        {
            AdaptarFondoPaginaAVentana(sender as Frame, e.Content);
        }

        private void AdaptarFondoPaginaAVentana(Frame frame, object contenidoNavegado)
        {
            if (frame == null)
            {
                return;
            }

            Page pagina = contenidoNavegado as Page;
            FrameworkElement raizPagina = pagina != null
                ? pagina.Content as FrameworkElement
                : contenidoNavegado as FrameworkElement;
            if (raizPagina == null)
            {
                return;
            }

            // Solo aplica a ventanas maximizadas (a pantalla completa), donde el
            // escalado uniforme deja franjas arriba y abajo que hay que rellenar.
            Window ventana = Window.GetWindow(frame);
            if (ventana == null || ventana.WindowState != WindowState.Maximized || ventana.AllowsTransparency)
            {
                return;
            }

            // El fondo de la página se mueve a la ventana (que cubre toda la
            // pantalla) y la raíz de la página queda transparente. Así el mismo
            // fondo, a una sola escala, llena el contenido y las franjas del
            // letterbox, evitando los marcos de otro color (p. ej. el panal azul
            // detrás del panal café de la sala).
            Brush fondoPagina = ObtenerFondo(raizPagina);
            if (fondoPagina == null || EsTransparente(fondoPagina))
            {
                return;
            }

            EstablecerFondo(raizPagina, Brushes.Transparent);
            ventana.Background = fondoPagina;
        }

        private static bool EsTransparente(Brush brocha)
        {
            SolidColorBrush solido = brocha as SolidColorBrush;
            return solido != null && solido.Color.A == 0;
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

            // En ventanas maximizadas, para que el escalado uniforme NO deje
            // franjas visibles, se mueve el fondo del propio contenido a la
            // ventana (así cubre toda la pantalla) y solo se escala el primer
            // plano. De este modo el fondo (panal) llena la pantalla sin marcos.
            if (maximizada && !ventana.AllowsTransparency)
            {
                Brush fondoContenido = ObtenerFondo(contenido);
                if (fondoContenido != null)
                {
                    EstablecerFondo(contenido, Brushes.Transparent);
                    ventana.Background = fondoContenido;
                }
            }

            ventana.Content = new Viewbox
            {
                Stretch = Stretch.Uniform,
                Child = contenido
            };
        }

        private static Brush ObtenerFondo(FrameworkElement elemento)
        {
            Panel panel = elemento as Panel;
            if (panel != null)
            {
                return panel.Background;
            }
            Border borde = elemento as Border;
            if (borde != null)
            {
                return borde.Background;
            }
            Control control = elemento as Control;
            if (control != null)
            {
                return control.Background;
            }
            return null;
        }

        private static void EstablecerFondo(FrameworkElement elemento, Brush brocha)
        {
            Panel panel = elemento as Panel;
            if (panel != null)
            {
                panel.Background = brocha;
                return;
            }
            Border borde = elemento as Border;
            if (borde != null)
            {
                borde.Background = brocha;
                return;
            }
            Control control = elemento as Control;
            if (control != null)
            {
                control.Background = brocha;
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
