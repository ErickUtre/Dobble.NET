using DobbleGame.Servidor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DobbleGame
{
    public partial class VentanaMarcadorPartida : Window
    {
        public VentanaMarcadorPartida(List<Jugador> resultadoJugadores)
        {
            InitializeComponent();
            MarcadorFinal.ItemsSource = resultadoJugadores;
            IniciarDatos(resultadoJugadores);
        }

        private void IniciarDatos(List<Jugador> resultadoJugadores)
        {
            if (resultadoJugadores != null)
            {
                var primerJugador = resultadoJugadores.FirstOrDefault();
                var segundoJugador = resultadoJugadores.Skip(1).FirstOrDefault();
                
                if (primerJugador != null)
                {
                    if (segundoJugador != null && primerJugador.PuntosEnPartida ==  segundoJugador.PuntosEnPartida)
                    {
                        lbResultadoPartida.Content = Properties.Resources.lb_Empate;
                    }
                    else
                    {
                        lbResultadoPartida.Content = Properties.Resources.lb_Victoria;
                        lbGanador.Content = primerJugador.Usuario;
                    }
                }
            }
        }

        private void BtnIrSala(object sender, RoutedEventArgs e)
        {
            Utilidades.Utilidades.PingConexion(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario, Application.Current.MainWindow);

            var proxyGestionAmigos = new GestionAmigosClient();
            try
            {
                if (!Dominio.CuentaUsuario.CuentaUsuarioActual.EsInvitado)
                {
                    proxyGestionAmigos.NotificarCambios(Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario);
                }
                
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                Utilidades.Utilidades.ManejarExcepciones(proxyGestionAmigos, ex, this);
            }
        }
    }
}
