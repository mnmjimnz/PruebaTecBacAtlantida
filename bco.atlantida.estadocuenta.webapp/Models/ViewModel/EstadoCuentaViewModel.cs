namespace bco.atlantida.estadocuenta.webapp.Models.ViewModel
{
    public class EstadoCuentaViewModel
    {
        public int IdEstadoCuenta { get; set; }
        public decimal SaldoActual { get; set; }
        public decimal SaldoDisponible { get; set; }
        public decimal TotalMasIntereses { get; set; }
        public int IdTarjeta { get; set; }
    }
}
