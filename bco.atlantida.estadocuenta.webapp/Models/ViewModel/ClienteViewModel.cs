using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace bco.atlantida.estadocuenta.webapp.Models.ViewModel
{
    public class ClienteViewModel
    {
        public int IdCliente { get; set; }

        [Display(Name = "Nombres")]
        [Required(ErrorMessage = "Debe ingresar al menos un nombre")]
        [RegularExpression(@"^.{5,}$", ErrorMessage = "Minimo 3 caracteres son requeridos.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre es muy corto")]
        public string? NombresCliente { get; set; }

        [Display(Name = "Apellidos")]
        [Required(ErrorMessage = "Debe ingresar al menos un apellido")]
        [RegularExpression(@"^.{5,}$", ErrorMessage = "Minimo 3 caracteres son requeridos.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El apellido es muy corto")]
        public string? ApellidosCliente { get; set; }
    }
}
