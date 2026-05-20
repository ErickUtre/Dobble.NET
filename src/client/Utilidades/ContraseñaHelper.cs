using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DobbleGame.Utilidades
{
    public static class ContraseñaHelper
    {
        public static void ActualizarVisibilidadTextoSugerido(PasswordBox passwordBox, TextBlock textoSugerido)
        {
            if (textoSugerido != null)
            {
                textoSugerido.Visibility = string.IsNullOrEmpty(passwordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static T EncontrarHijoVisual<T>(DependencyObject padre, string nombreHijo) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(padre); i++)
            {
                var hijo = VisualTreeHelper.GetChild(padre, i);
                if (hijo is T hijoTipado && ((FrameworkElement)hijo).Name == nombreHijo)
                {
                    return hijoTipado;
                }

                var hijoDeHijo = EncontrarHijoVisual<T>(hijo, nombreHijo);
                if (hijoDeHijo != null)
                {
                    return hijoDeHijo;
                }
            }
            return null;
        }
    }
}
