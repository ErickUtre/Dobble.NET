using System.Windows;

namespace DobbleGame
{
    public partial class VentanaRegistroExitoso : Window
    {
        public VentanaRegistroExitoso()
        {
            InitializeComponent();
        }

        private void BtnAceptar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
