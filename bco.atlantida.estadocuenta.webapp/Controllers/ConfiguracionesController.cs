using bco.atlantida.estadocuenta.webapp.Core.Interface;
using bco.atlantida.estadocuenta.webapp.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Policy;

namespace bco.atlantida.estadocuenta.webapp.Controllers
{
    public class ConfiguracionesController : Controller
    {
        private readonly IRequest _request;
        private readonly IConfiguration _configuration;
        private readonly string url = "api/Configuraciones";
        public ConfiguracionesController(IRequest request, IConfiguration configuration)
        {
            _request = request;
            _configuration = configuration;
        }
        public async Task<IActionResult> GetConfiguracionByIdTarjeta(int IdTarjeta)
        {
            ConfiguracionViewModel data = new ConfiguracionViewModel();
            try
            {
                var r = await _request.GetData($"{_configuration["APIurl"]}{url}/GetByIdTarjeta/{IdTarjeta}");
                if (r != null)
                {
                    var x = JsonConvert.DeserializeObject<List<ConfiguracionViewModel>>(r);
                    if (x.Count > 0)
                    {
                        data = x.SingleOrDefault();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return PartialView(data);
        }
        public async Task<IActionResult> FormConfiguraciones(int IdConfiguracion, int? IdTarjeta)
        {
            ConfiguracionViewModel data = new ConfiguracionViewModel();
            data.IdTarjeta = IdTarjeta != null ? (int)IdTarjeta : 0;

            try
            {
                if (IdConfiguracion > 0)
                {
                    var r = await _request.GetData($"{_configuration["APIurl"]}{url}/{IdConfiguracion}");
                    if (r != null)
                    {
                        data = JsonConvert.DeserializeObject<ConfiguracionViewModel>(r);
                    }
                }
            }
            catch (Exception)
            {
            }
            return PartialView(data);
        }
        [HttpPost]
        public async Task<IActionResult> AccionesConfiguraciones(ConfiguracionViewModel? tarjeta)
        {
            MensajesViewModel<ConfiguracionViewModel> data = new MensajesViewModel<ConfiguracionViewModel>();
            data.Mensaje = "Configuracion registrada exitosamente";
            try
            {
                if (tarjeta != null)
                {
                    var tipoAccion = HttpMethod.Post;
                    if (tarjeta.IdConfiguracion > 0)
                    {
                        tipoAccion = HttpMethod.Put;
                        data.Mensaje = "Configuracion modificada exitosamente";
                    }
                    var r = await _request.PostData(tarjeta, $"{_configuration["APIurl"]}{url}", tipoAccion);
                    if (r != null)
                    {
                        data.data = JsonConvert.DeserializeObject<ConfiguracionViewModel>(r);
                    }
                }
            }
            catch (Exception ex)
            {
                data.Mensaje = "Ocurrio un error";
            }
            return Redirect(tarjeta.urlBusqueda);
        }
    }
}
