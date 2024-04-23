using bco.atlantida.estadocuenta.webapp.Core.Interface;
using bco.atlantida.estadocuenta.webapp.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Security.Policy;

namespace bco.atlantida.estadocuenta.webapp.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IRequest _request;
        private readonly IConfiguration _configuration;
        private readonly string _url = "api/Cliente";
        public ClienteController(IRequest request, IConfiguration configuration)
        {
            _request = request;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            List<ClienteViewModel> list = new List<ClienteViewModel>();
            try
            {
                var x = await _request.GetData($"{_configuration["APIurl"]}{_url}");
                if (x != null)
                {
                    list = JsonConvert.DeserializeObject<List<ClienteViewModel>>(x);
                }
            }
            catch (Exception)
            {
                //Retornar mensaje de error
            }
            return View(list);
        }
        public async Task<IActionResult> FormCliente(int IdCliente)
        {
            ClienteViewModel data = new ClienteViewModel();
            try
            {
                if (IdCliente > 0)
                {
                    var r = await _request.GetData($"{_configuration["APIurl"]}{_url}/{IdCliente}");
                    if (r != null)
                    {
                        data = JsonConvert.DeserializeObject<ClienteViewModel>(r);
                    }
                }
            }
            catch (Exception)
            {
            }
            return PartialView(data);
        }
        [HttpPost]
        public async Task<IActionResult> AccionesCliente(ClienteViewModel? _cliente)
        {
            MensajesViewModel<ClienteViewModel> data = new MensajesViewModel<ClienteViewModel>();
            data.Mensaje = "Cliente registrado exitosamente";
            try
            {
                if (_cliente != null)
                {
                    var tipoAccion = HttpMethod.Post;
                    if (_cliente.IdCliente > 0)
                    {
                        tipoAccion = HttpMethod.Put;
                        data.Mensaje = "Cliente modificado exitosamente";
                    }
                    var r = await _request.PostData(_cliente, $"{_configuration["APIurl"]}{_url}", tipoAccion);
                    if (r != null)
                    {
                        data.data = JsonConvert.DeserializeObject<ClienteViewModel>(r);
                    }
                }
            }
            catch (Exception ex)
            {
                data.Mensaje = "Ocurrio un error";
            }
            return RedirectToAction("Index", "Cliente");
        }
    }
}
