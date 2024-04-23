using bco.atlantida.estadocuenta.webapp.Models.ViewModel;

namespace bco.atlantida.estadocuenta.webapp.Core.Interface
{
    public interface IHelpers
    {
        Task<List<SelectHelpers>> GetListaMeses();
        Task<List<SelectHelpers>> GetListaAnios();
    }
}
