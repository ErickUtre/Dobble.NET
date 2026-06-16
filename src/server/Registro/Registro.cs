using System;
using System.Diagnostics;
using System.Text;

namespace Registro
{
    // Implementación simple de registro para evitar la dependencia de log4net
    public static class Registro
    {
        static Registro()
        {
            // Configuración mínima: asegurarse de que las trazas se muestran
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;
        }

        public static void Informacion(string mensaje)
        {
            Trace.TraceInformation(FormatMessage("INFO", mensaje));
        }

        public static void Depuracion(string mensaje)
        {
            Trace.TraceInformation(FormatMessage("DEBUG", mensaje));
        }

        public static void Advertencia(string mensaje)
        {
            Trace.TraceWarning(FormatMessage("WARN", mensaje));
        }

        public static void Error(string mensaje, Exception ex = null)
        {
            var full = new StringBuilder();
            full.AppendLine(FormatMessage("ERROR", mensaje));
            if (ex != null)
            {
                full.AppendLine(ex.ToString());
            }
            Trace.TraceError(full.ToString());
        }

        public static void Fatal(string mensaje, Exception ex = null)
        {
            var full = new StringBuilder();
            full.AppendLine(FormatMessage("FATAL", mensaje));
            if (ex != null)
            {
                full.AppendLine(ex.ToString());
            }
            Trace.TraceError(full.ToString());
        }

        private static string FormatMessage(string nivel, string mensaje)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{nivel}] {mensaje}";
        }
    }
}
