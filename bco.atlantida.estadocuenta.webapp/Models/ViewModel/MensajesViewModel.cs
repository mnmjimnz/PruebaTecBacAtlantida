namespace bco.atlantida.estadocuenta.webapp.Models.ViewModel
{
    public class MensajesViewModel<T> where T : class
    {
        public T? data { get; set; }
        public string Mensaje { get; set; }
        public string Tipo { get; set; }
    }
}
