using DobbleGame.Servidor;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DobbleGame
{
    public class SelectorPlantillaJugadorPartida : DataTemplateSelector
    {
        public DataTemplate Plantilla1 { get; set; }
        public DataTemplate Plantilla2 { get; set; }
        public DataTemplate Plantilla3 { get; set; }
        public DataTemplate Plantilla4 { get; set; }

        private readonly Dictionary<string, bool> _plantillasDisponibles = new Dictionary<string, bool>
        {
            {"Plantilla1", true},
            {"Plantilla2", true},
            {"Plantilla3", true},
            {"Plantilla4", true},
        };

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var jugador = item as Jugador;
            if (jugador == null)
                return base.SelectTemplate(item, container);

            if (jugador.Usuario == Dominio.CuentaUsuario.CuentaUsuarioActual.Usuario
                && _plantillasDisponibles["Plantilla1"])
            {
                _plantillasDisponibles["Plantilla1"] = false;
                return Plantilla1;
            }
                
            if (_plantillasDisponibles["Plantilla2"])
            {
                _plantillasDisponibles["Plantilla2"] = false;
                return Plantilla2;
            }
            if (_plantillasDisponibles["Plantilla3"])
            {
                _plantillasDisponibles["Plantilla3"] = false;
                return Plantilla3;
            }
            if (_plantillasDisponibles["Plantilla4"])
            {
                _plantillasDisponibles["Plantilla4"] = false;
                return Plantilla4;
            }

            return base.SelectTemplate(item, container);
        }

        public void ReiniciarPlantillasPartida()
        {
            foreach (var llave in _plantillasDisponibles.Keys.ToList())
            {
                _plantillasDisponibles[llave] = true;
            }
        }
    }
}
