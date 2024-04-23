using bco.atlantida.estadocuenta.webapp.Core.Interface;
using bco.atlantida.estadocuenta.webapp.Models.ViewModel;
using System.Globalization;

namespace bco.atlantida.estadocuenta.webapp.Repositorio.Infraestructura
{
    public class Helpsers : IHelpers
    {
        public async Task<List<SelectHelpers>> GetListaMeses()
        {
            string[] cultura = DateTimeFormatInfo.CurrentInfo.MonthNames;
            var lista = new List<SelectHelpers>();
            int count = 0;
            foreach (var mes in cultura)
            {
                count++;
                lista.Add(new SelectHelpers
                {
                    Descripcion = mes,
                    Id = count
                });
            }
            return lista;
        }
        public async Task<List<SelectHelpers>> GetListaAnios()
        {
            var anioActual = DateTime.Now.Year+1;
            var lista = Enumerable.Range(anioActual, 10).ToList();
            var l = new List<SelectHelpers>();
            int count = 0;
            foreach (var item in lista)
            {
                l.Add(new SelectHelpers
                {
                    Descripcion = item.ToString(),
                    Id = item
                });
            }
            return l;
        }
    }
}
