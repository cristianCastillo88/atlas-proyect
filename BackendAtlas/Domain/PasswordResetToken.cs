namespace BackendAtlas.Domain
{
    /// <summary>
    /// Token de recuperación de contraseña con seguridad reforzada.
    /// Mejoras de seguridad:
    /// - Token es GUID cryptographically secure (no predecible)
    /// - Expiración de 1 hora (ventana pequeña de ataque)
    /// - Un solo uso (flag Usado previene replay attacks)
    /// - Audit trail completo (fecha creación, uso, etc.)
    /// </summary>
    public class PasswordResetToken
    {
        public int Id { get; set; }
        
        public int UsuarioId { get; set; }
        
        /// <summary>
        /// Token único generado con Guid.NewGuid()
        /// 128 bits de entropía - imposible de adivinar
        /// </summary>
        public required string Token { get; set; }
        
        /// <summary>
        /// Fecha de creación en UTC para evitar problemas de zona horaria
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Fecha de expiración: FechaCreacion + 1 hora
        /// Después de esta fecha, el token es inválido
        /// </summary>
        public DateTime FechaExpiracion { get; set; }
        
        /// <summary>
        /// Flag de seguridad: previene reutilización del token
        /// Una vez usado, permanece true para siempre
        /// </summary>
        public bool Usado { get; set; } = false;
        
        /// <summary>
        /// Timestamp del momento en que se usó el token
        /// Null si nunca se usó
        /// </summary>
        public DateTime? FechaUso { get; set; }
        
        // Navegación
        public Usuario Usuario { get; set; } = null!;
        
        /// <summary>
        /// Valida si el token está todavía válido
        /// Seguro: checks tanto expiración como uso
        /// </summary>
        public bool EsValido()
        {
            if (Usado)
            {
                return false; // Ya fue usado
            }
            
            if (DateTime.UtcNow > FechaExpiracion)
            {
                return false; // Expirado
            }
            
            return true;
        }
    }
}
