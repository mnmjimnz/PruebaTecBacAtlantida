using System.ComponentModel.DataAnnotations;

namespace bco.atlantida.estadocuenta.webapp.Models.ViewModel
{
    public class TarjetaViewModel
    {
        public int IdTarjeta { get; set; }
        [Display(Name = "N° Tarjeta")]
        [Required(ErrorMessage = "Debe ingresar un numero de tarjeta")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Debe ingresar un numero de tarjeta valido")]
        public string NumeroTarjeta { get; set; }
        public DateTime FechaExpiracion { get; set; }
        [Display(Name = "CCV")]
        [Required(ErrorMessage = "Debe ingresar codigo de seguridad")]
        //[MaxLength(3, ErrorMessage = "Debe ingresar un codigo de seguridad  valido"), MinLength(3, ErrorMessage = "Debe ingresar un codigo de seguridad valido")]
        public int CodigoSeguridad { get; set; }
        public decimal Limite { get; set; }
        public int IdCliente { get; set; }

        [Display(Name = "Año Expiración")]
        [Required(ErrorMessage = "Debe ingresar el año de expiracion")]
        //[MaxLength(4, ErrorMessage = "Debe ingresar un año valido"), MinLength(4, ErrorMessage = "Debe ingresar un año valido")]
        public int Anio { get; set; }
        [Display(Name = "Mes Expiración")]
        [Required(ErrorMessage = "Este campo es requerido")]
        //[MaxLength(2, ErrorMessage = "Debe ingresar un mes valido"),MinLength(1, ErrorMessage = "Debe ingresar un mes valido")]
        public int Mes { get; set; }
        public string NombreCliente { get; set; }
        public List<MovimientosViewModel> Compras { get; set; }
        public EstadoCuentaViewModel EstadoCuenta { get; set; }
        public ConfiguracionViewModel Configuracion { get; set; }
        public List<SelectHelpers> ListaMeses { get; set; }
        public List<SelectHelpers> ListaAnios { get; set; }
        public decimal MontoTotalCompras { get; set; }
        public decimal TotalMesActual { get; set; }
        public decimal TotalMesAnterior { get; set; }
        public int PorcentajeInteres { get; set; }
        public decimal PorcentajeSaldoMin { get; set; }
        public decimal InteresBonificable { get; set; }
        public decimal CuotaMinimaPago { get; set; }
        public decimal ContadoConIntereses { get; set; }
    }
}
