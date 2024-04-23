namespace bco.atlantida.estadocuenta.webapp.Models.ViewModel
{
    public class MovimientosViewModel
    {
        public int IdMovimiento { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public int TipoMovimiento { get; set; }
        public int IdTarjeta { get; set; }
        public decimal? SaldoTarjeta { get; set; }
        public string FechaTexto { get; set; }
    }
}
