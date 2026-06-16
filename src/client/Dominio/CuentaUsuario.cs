namespace Dominio
{
    /// <summary>
    /// Modelo local de la cuenta de un usuario. Refleja el tipo expuesto por el
    /// servidor (DobbleGame.Servidor.CuentaUsuario) y, de forma estática, mantiene
    /// la cuenta del usuario conectado actualmente (CuentaUsuarioActual).
    /// </summary>
    public class CuentaUsuario
    {
        /// <summary>Cuenta del usuario con sesión activa en esta instancia del cliente.</summary>
        public static CuentaUsuario CuentaUsuarioActual { get; set; }

        public int IdCuentaUsuario { get; set; }
        public string Usuario { get; set; }
        public string Correo { get; set; }
        public string Contraseña { get; set; }
        public byte[] Foto { get; set; }
        public int Puntaje { get; set; }
        public bool Estado { get; set; }
        public bool EsInvitado { get; set; }
    }
}
