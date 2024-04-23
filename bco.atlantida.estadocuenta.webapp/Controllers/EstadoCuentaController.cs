using bco.atlantida.estadocuenta.webapp.Core.Interface;
using bco.atlantida.estadocuenta.webapp.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace bco.atlantida.estadocuenta.webapp.Controllers
{
    public class EstadoCuentaController : Controller
    {
        private readonly IRequest _request;
        private readonly IConfiguration _configuration;
        private readonly string _url = "api/EstadoCuenta";
        private readonly string urlConf = "api/Configuraciones";
        private readonly string _urlMov = "api/Movimientos";
        public EstadoCuentaController(IRequest request, IConfiguration configuration)
        {
            _request = request;
            _configuration = configuration;
        }
        public async Task<IActionResult> GetEstadoCuentaTarjeta(int IdTarjeta)
        {
            EstadoCuentaViewModel data = new EstadoCuentaViewModel();
            TarjetaViewModel tarjeta = new TarjetaViewModel();
            try
            {
                var x = await _request.GetData($"{_configuration["APIurl"]}{_url}/GetEstadoCuentaByIdTarjeta/{IdTarjeta}");
                if (x != null)
                {
                    var r = JsonConvert.DeserializeObject<List<EstadoCuentaViewModel>>(x);
                    data = r.SingleOrDefault();

                    var m = await _request.GetData($"{_configuration["APIurl"]}{_urlMov}/GetComprasByIdTarjeta/{IdTarjeta}");
                    var p = await _request.GetData($"{_configuration["APIurl"]}{_urlMov}/GetPagosByIdTarjeta/{IdTarjeta}");
                    if (m != null)
                    {
                        tarjeta.Compras = JsonConvert.DeserializeObject<List<MovimientosViewModel>>(m);
                        List<MovimientosViewModel> pagos = JsonConvert.DeserializeObject<List<MovimientosViewModel>>(p);
                        decimal totalPagos = pagos.Sum(pg => pg.Monto);
                        tarjeta.MontoTotalCompras = tarjeta.Compras.Sum(y => y.Monto);
                        tarjeta.MontoTotalCompras = tarjeta.MontoTotalCompras - totalPagos;
                        if (tarjeta.MontoTotalCompras < 0)
                        {
                            tarjeta.EstadoCuenta.SaldoDisponible = tarjeta.Compras.Sum(y => y.Monto) + tarjeta.MontoTotalCompras;
                            tarjeta.EstadoCuenta.SaldoActual = Math.Abs(tarjeta.MontoTotalCompras);
                            tarjeta.MontoTotalCompras = 0;
                        }
                        //buscar la configuracion para los calculos de procentajes
                        var conf = await _request.GetData($"{_configuration["APIurl"]}{urlConf}/GetByIdTarjeta/{IdTarjeta}");
                        if (conf != null)
                        {
                            var _cf = JsonConvert.DeserializeObject<List<ConfiguracionViewModel>>(conf);
                            if (_cf.Count > 0)
                            {
                                tarjeta.Configuracion = _cf.SingleOrDefault();
                                tarjeta.PorcentajeInteres = tarjeta.Configuracion.PorcentajeInteres;
                                tarjeta.PorcentajeSaldoMin = tarjeta.Configuracion.PorcentajeSaldoMin;
                                double pInteres = (double)tarjeta.PorcentajeInteres / 100;
                                double psalMin = (double)tarjeta.PorcentajeSaldoMin / 100;
                                tarjeta.InteresBonificable = tarjeta.MontoTotalCompras * (decimal)pInteres;
                                tarjeta.CuotaMinimaPago = tarjeta.MontoTotalCompras * (decimal)psalMin;
                                tarjeta.ContadoConIntereses = tarjeta.MontoTotalCompras + tarjeta.InteresBonificable;
                            }
                            else
                            {
                                tarjeta.Configuracion = new ConfiguracionViewModel();
                            }
                        }
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }

            return Ok(new { EstadoCuenta = data, Totales = tarjeta });
        }
    }
}
