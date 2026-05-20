using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DobbleGame
{
    public partial class ControlDeUsuarioNotificacion : UserControl
    {
        public ControlDeUsuarioNotificacion()
        {
            InitializeComponent();
        }

        public void MostrarNotificacion(string mensaje)
        {
            tbMensaje.Text = mensaje;
            Notificacion.IsOpen = true;

            Task.Delay(5000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() => Notificacion.IsOpen = false);
            });
        }
    }
}
