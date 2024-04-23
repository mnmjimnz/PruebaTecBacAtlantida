using bco.atlantida.estadocuenta.webapp.Core.Interface;
using bco.atlantida.estadocuenta.webapp.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Policy;

namespace bco.atlantida.estadocuenta.webapp.Controllers
{
    public class TarjetaController : Controller
    {
        private readonly IRequest _request;
        private readonly IConfiguration _configuration;
        private readonly IHelpers _helper;
        private readonly string _url = "api/Tarjeta";
        private readonly string _urlMov = "api/Movimientos";
        private readonly string _urlEc = "api/EstadoCuenta";
        private readonly string urlConf = "api/Configuraciones";
        private readonly string urlClient = "api/Cliente";
        public TarjetaController(IRequest request, IConfiguration configuration, IHelpers helpers)
        {
            _request = request;
            _configuration = configuration;
            _helper = helpers;
        }
        public async Task<IActionResult> ListadoTarjetas(int IdCliente)
        {
            List<TarjetaViewModel> list = new List<TarjetaViewModel>();
            try
            {
                if (IdCliente > 0)
                {
                    var r = await _request.GetData($"{_configuration["APIurl"]}{_url}/{IdCliente}");
                    if (r != null)
                    {
                        list = JsonConvert.DeserializeObject<List<TarjetaViewModel>>(r);
                        ViewBag.IdCliente = IdCliente;
                        foreach (var item in list)
                        {
                            //buscar la configuracion
                            var conf = await _request.GetData($"{_configuration["APIurl"]}{urlConf}/GetByIdTarjeta/{item.IdTarjeta}");
                            if (conf != null)
                            {
                                var _cf = JsonConvert.DeserializeObject<List<ConfiguracionViewModel>>(conf);
                                if (_cf.Count > 0)
                                {
                                    item.Configuracion = _cf.SingleOrDefault();
                                }
                                else
                                {
                                    item.Configuracion = null;
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return View(list);
        }
        public async Task<IActionResult> DetalleTarjeta(int IdTarjeta)
        {
            TarjetaViewModel data = new TarjetaViewModel();
            try
            {
                if (IdTarjeta > 0)
                {
                    var r = await _request.GetData($"{_configuration["APIurl"]}{_url}/DetalleTarjeta/{IdTarjeta}");
                    if (r != null)
                    {
                        data = JsonConvert.DeserializeObject<TarjetaViewModel>(r);
                        data.Anio = data.FechaExpiracion.Year;
                        data.Mes = data.FechaExpiracion.Month;
                        //buscar compras realizadas con esta tarjeta
                        var m = await _request.GetData($"{_configuration["APIurl"]}{_urlMov}/GetComprasByIdTarjeta/{IdTarjeta}");
                        //obtener pagos para restar y que los calculos se vean bien
                        var p = await _request.GetData($"{_configuration["APIurl"]}{_urlMov}/GetPagosByIdTarjeta/{IdTarjeta}");
                        if (m != null && p != null)
                        {
                            data.Compras = JsonConvert.DeserializeObject<List<MovimientosViewModel>>(m);
                            List<MovimientosViewModel> pagos = JsonConvert.DeserializeObject<List<MovimientosViewModel>>(p);
                            decimal totalPagos = pagos.Sum(pg => pg.Monto);
                            data.Compras.ForEach(x => x.FechaTexto = x.FechaMovimiento.ToString("dd/MM/yyyy"));
                            data.MontoTotalCompras = data.Compras.Sum(y => y.Monto);
                            //restar los pagos realizados al total
                            data.MontoTotalCompras = data.MontoTotalCompras - totalPagos;

                            //buscar el estado de cuenta actual
                            var ec = await _request.GetData($"{_configuration["APIurl"]}{_urlEc}/GetEstadoCuentaByIdTarjeta/{IdTarjeta}");
                            if (ec != null)
                            {
                                var lec = JsonConvert.DeserializeObject<List<EstadoCuentaViewModel>>(ec);
                                if (lec.Count > 0)
                                {
                                    data.EstadoCuenta = lec.SingleOrDefault();
                                }
                                else
                                {
                                    data.EstadoCuenta = new EstadoCuentaViewModel();
                                }
                            }
                            if (data.MontoTotalCompras < 0)
                            {
                                data.EstadoCuenta.SaldoDisponible = data.Compras.Sum(y => y.Monto) + data.MontoTotalCompras;
                                data.EstadoCuenta.SaldoActual = Math.Abs(data.MontoTotalCompras);
                                data.MontoTotalCompras = 0;
                            }
                            DateTime FechaMesActual_1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 00, 00, 00);
                            DateTime FechaMesActual_2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month), 11, 59, 59);
                            DateTime FechaMesAnterior_1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1, 00, 00, 00);
                            DateTime FechaMesAnterior_2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month - 1), 11, 59, 59);
                            //total por mes
                            var sumMesActual = (from cp in data.Compras
                                                    //let dataYearMonth = new DateTime(cp.FechaMovimiento.Year, cp.FechaMovimiento.Month, 0)
                                                where cp.FechaMovimiento >= FechaMesActual_1 && cp.FechaMovimiento <= FechaMesActual_2
                                                select cp).ToList().Sum(o => o.Monto);
                            data.TotalMesActual = sumMesActual;
                            //total mes anterior
                            var sumMesAnterior = (from cp in data.Compras
                                                      //let dataYearMonth = new DateTime(cp.FechaMovimiento.Year, cp.FechaMovimiento.Month, 0)
                                                  where cp.FechaMovimiento >= FechaMesAnterior_1 && cp.FechaMovimiento <= FechaMesAnterior_2
                                                  select cp).ToList().Sum(o => o.Monto);
                            data.TotalMesAnterior = sumMesAnterior;
                        }

                        //buscar la configuracion para los calculos de procentajes
                        var conf = await _request.GetData($"{_configuration["APIurl"]}{urlConf}/GetByIdTarjeta/{IdTarjeta}");
                        if (conf != null)
                        {
                            var _cf = JsonConvert.DeserializeObject<List<ConfiguracionViewModel>>(conf);
                            if (_cf.Count > 0)
                            {
                                data.Configuracion = _cf.SingleOrDefault();
                                data.PorcentajeInteres = data.Configuracion.PorcentajeInteres;
                                data.PorcentajeSaldoMin = data.Configuracion.PorcentajeSaldoMin;
                                double pInteres = (double)data.PorcentajeInteres / 100;
                                double psalMin = (double)data.PorcentajeSaldoMin / 100;
                                data.InteresBonificable = data.MontoTotalCompras * (decimal)pInteres;
                                data.CuotaMinimaPago = data.MontoTotalCompras * (decimal)psalMin;
                                data.ContadoConIntereses = data.MontoTotalCompras + data.InteresBonificable;
                            }
                            else
                            {
                                data.Configuracion = new ConfiguracionViewModel();
                            }
                        }
                        //buscar la informacion del cliente
                        var client = await _request.GetData($"{_configuration["APIurl"]}{urlClient}/{data.IdCliente}");
                        if (client != null)
                        {
                            var _cl = JsonConvert.DeserializeObject<ClienteViewModel>(client);
                            data.NombreCliente = $"{_cl.NombresCliente} {_cl.ApellidosCliente}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return PartialView(data);
        }
        public async Task<IActionResult> FormTarjeta(int? IdTarjeta, int? IdCliente)
        {
            TarjetaViewModel data = new TarjetaViewModel();
            data.IdCliente = IdCliente != null ? (int)IdCliente : 0;
            string client = string.Empty;
            try
            {
                if (IdTarjeta > 0)
                {
                    var r = await _request.GetData($"{_configuration["APIurl"]}{_url}/DetalleTarjeta/{IdTarjeta}");
                    if (r != null)
                    {
                        data = JsonConvert.DeserializeObject<TarjetaViewModel>(r);
                        data.Anio = data.FechaExpiracion.Year;
                        data.Mes = data.FechaExpiracion.Month;
                        client = await _request.GetData($"{_configuration["APIurl"]}{urlClient}/{data.IdCliente}");
                    }
                }
                else
                {
                    client = await _request.GetData($"{_configuration["APIurl"]}{urlClient}/{IdCliente}");
                }
                //buscar la informacion del cliente
                if (client != null)
                {
                    var _cl = JsonConvert.DeserializeObject<ClienteViewModel>(client);
                    data.NombreCliente = $"{_cl.NombresCliente} {_cl.ApellidosCliente}";
                }
            }
            catch (Exception)
            {
            }
            data.ListaMeses = await _helper.GetListaMeses();
            data.ListaAnios = await _helper.GetListaAnios();
            return PartialView(data);
        }
        [HttpPost]
        public async Task<IActionResult> AccionesTarjeta(TarjetaViewModel? tarjeta)
        {
            TarjetaViewModel data = new TarjetaViewModel();
            try
            {
                if (tarjeta != null)
                {
                    var tipoAccion = HttpMethod.Post;
                    if (tarjeta.IdTarjeta > 0)
                    {
                        tipoAccion = HttpMethod.Put;
                    }
                    tarjeta.FechaExpiracion = new DateTime(tarjeta.Anio, tarjeta.Mes, 01);
                    var r = await _request.PostData(tarjeta, $"{_configuration["APIurl"]}{_url}", tipoAccion);
                    if (r != null)
                    {
                        data = JsonConvert.DeserializeObject<TarjetaViewModel>(r);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("ListadoTarjetas", "Tarjeta", new { IdCliente = tarjeta?.IdCliente });
        }
    }
}
