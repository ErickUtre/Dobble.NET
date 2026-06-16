namespace Dominio
{
    /// <summary>
    /// DTO local con la información de un amigo que se muestra en la interfaz
    /// (nombre de usuario, puntaje y foto de perfil).
    /// </summary>
    public class CuentaUsuarioAmigo
    {
        public string Usuario { get; set; }
        public int Puntaje { get; set; }
        public byte[] Foto { get; set; }
    }
}
