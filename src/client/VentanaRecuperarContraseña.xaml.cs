using System.Windows;

namespace DobbleGame
{
    public partial class VentanaRecuperarContraseña : Window
    {
        public VentanaRecuperarContraseña()
        {
            InitializeComponent();
            MainWindow.NavigationService.Navigate(new PaginaRecuperarContraseña(this));
        }
    }
}
