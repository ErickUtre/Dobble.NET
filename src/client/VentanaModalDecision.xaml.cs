using System.Windows;

namespace DobbleGame
{
    public partial class VentanaModalDecision : Window
    {
        public VentanaModalDecision(string mensaje)
        {
            InitializeComponent();
            tbMensaje.Text = mensaje;
            btnAceptar.Content = Properties.Resources.global_Aceptar;
            btnCancelar.Content = Properties.Resources.global_Cancelar;
        }

        public VentanaModalDecision(string mensaje, string nombreBotonAceptar, string nombreBotonCancelar)
        {
            InitializeComponent();
            tbMensaje.Text = mensaje;
            btnAceptar.Content = nombreBotonAceptar;
            btnCancelar.Content = nombreBotonCancelar;
        }

        private void BtnAceptar(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancelar(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
