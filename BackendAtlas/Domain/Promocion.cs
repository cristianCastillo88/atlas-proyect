namespace BackendAtlas.Domain
{
    public class Promocion
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public decimal DescuentoPorcentaje { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activa { get; set; }

        // Navegación inversa (si se relaciona con pedidos o productos, pero por ahora no especificado)
    }
}