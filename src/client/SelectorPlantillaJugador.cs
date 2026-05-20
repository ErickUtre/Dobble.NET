using DobbleGame.Servidor;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DobbleGame
{
    public class SelectorPlantillaJugador : DataTemplateSelector
    {
        public DataTemplate JugadorRojoPlantilla { get; set; }
        public DataTemplate JugadorAzulPlantilla { get; set; }
        public DataTemplate JugadorVerdePlantilla { get; set; }
        public DataTemplate JugadorAmarilloPlantilla { get; set; }

        private int _contadorPlantillas = 0;
        private Dictionary<DataTemplate, bool> _plantillasDisponibles;

        public SelectorPlantillaJugador() { }

        public void IniciarlizarPlantillas()
        {
            _plantillasDisponibles = new Dictionary<DataTemplate, bool>();

            if (JugadorAzulPlantilla != null)
                _plantillasDisponibles.Add(JugadorAzulPlantilla, true);
            if (JugadorVerdePlantilla != null)
                _plantillasDisponibles.Add(JugadorVerdePlantilla, true);
            if (JugadorAmarilloPlantilla != null)
                _plantillasDisponibles.Add(JugadorAmarilloPlantilla, true);
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var usuario = item as Jugador;

            if (usuario == null) return null;

            usuario.NumeroJugador = _contadorPlantillas + 1;

            if (usuario.EsAnfitrion)
            {
                _contadorPlantillas += 1;
                
                if (JugadorRojoPlantilla != null)
                {
                    return JugadorRojoPlantilla;
                }
            }

            var plantillaDisponible = _plantillasDisponibles.FirstOrDefault(p => p.Value).Key;
            
            if (plantillaDisponible != null)
            {
                _contadorPlantillas += 1;
                _plantillasDisponibles[plantillaDisponible] = false;
                return plantillaDisponible;
            }

            return JugadorAmarilloPlantilla;
        }

        public void ReiniciarPlantillas()
        {
            _contadorPlantillas = 0;

            foreach (var llave in _plantillasDisponibles.Keys.ToList())
            {
                _plantillasDisponibles[llave] = true;
            }
        }
    }
}
