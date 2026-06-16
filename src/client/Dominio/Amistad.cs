namespace Dominio
{
    /// <summary>
    /// DTO local que representa una relación de amistad (o solicitud) entre dos usuarios.
    /// Refleja el tipo expuesto por el servidor (DobbleGame.Servidor.Amistad).
    /// </summary>
    public class Amistad
    {
        public int IdAmistad { get; set; }
        public bool EstadoSolicitud { get; set; }
        public int UsuarioPrincipalId { get; set; }
        public int UsuarioAmigoId { get; set; }
    }
}
