using bco.atlantida.estadocuenta.webapp.Core.Interface;
using bco.atlantida.estadocuenta.webapp.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace bco.atlantida.estadocuenta.webapp.Controllers
{
    public class MovimientosController : Controller
    {
        private readonly IRequest _request;
        private readonly IConfiguration _configuration;
        private readonly string _url = "api/Movimientos";
        private readonly string _urlEc = "api/EstadoCuenta";
        private readonly string urlConf = "api/Configuraciones";
        public MovimientosController(IRequest request, IConfiguration config)
        {
            _configuration = config;
            _request = request;
        }
        public async Task<IActionResult> HistorialPagos(int IdTarjeta)
        {
            List<MovimientosViewModel> list = new List<MovimientosViewModel>();
            try
            {
                var x = await _request.GetData($"{_configuration["APIurl"]}{_url}/GetPagosByIdTarjeta/{IdTarjeta}");
                if (x != null)
                {
                    list = JsonConvert.DeserializeObject<List<MovimientosViewModel>>(x);
                }
            }
            catch (Exception)
            {
                //Retornar mensaje de error
            }
            return PartialView(list);
        }
        public async Task<IActionResult> HistorialCompras(int IdTarjeta)
        {
            List<MovimientosViewModel> list = new List<MovimientosViewModel>();
            try
            {
                var x = await _request.GetData($"{_configuration["APIurl"]}{_url}/GetComprasByIdTarjeta/{IdTarjeta}");
                if (x != null)
                {
                    list = JsonConvert.DeserializeObject<List<MovimientosViewModel>>(x);
                }
            }
            catch (Exception)
            {
                //Retornar mensaje de error
            }
            return PartialView(list);
        }
        public async Task<IActionResult> DetallesMovimiento(int IdMovimiento)
        {
            MovimientosViewModel data = new MovimientosViewModel();
            try
            {
                if (IdMovimiento > 0)
                {
                    var r = await _request.GetData($"{_configuration["APIurl"]}{_url}/DetalleMovimiento/{IdMovimiento}");
                    if (r != null)
                    {
                        data = JsonConvert.DeserializeObject<MovimientosViewModel>(r);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return PartialView(data);
        }
        public async Task<IActionResult> FormCompras(int IdMovimiento, int? IdTarjeta, decimal? saldoTarjeta)
        {
            MovimientosViewModel data = new MovimientosViewModel();
            data.IdTarjeta = IdTarjeta != null ? (int)IdTarjeta : 0;
            data.SaldoTarjeta = saldoTarjeta != null ? (decimal)saldoTarjeta : 0;
            try
            {
                if (IdMovimiento > 0)
                {
                    var r = await _request.GetData($"{_configuration["APIurl"]}{_url}/{IdMovimiento}");
                    if (r != null)
                    {
                        data = JsonConvert.DeserializeObject<MovimientosViewModel>(r);
                    }
                }
            }
            catch (Exception)
            {
            }
            return PartialView(data);
        }
        public async Task<IActionResult> FormPagos(int IdTarjeta, decimal SaldoPagar, decimal? saldoTarjeta)
        {
            MovimientosViewModel data = new MovimientosViewModel();
            data.IdTarjeta = IdTarjeta != null ? (int)IdTarjeta : 0;
            data.Monto = SaldoPagar != null ? (decimal)SaldoPagar : 0;
            data.SaldoTarjeta = saldoTarjeta != null ? (decimal)saldoTarjeta : 0;
            data.Descripcion = "Pago a la tarjeta";
            return PartialView(data);
        }
        [HttpPost]
        public async Task<IActionResult> AccionesPagos(MovimientosViewModel? movimiento)
        {
            MensajesViewModel<List<MovimientosViewModel>> data = new MensajesViewModel<List<MovimientosViewModel>>();
            bool realizarRegistro = false;
            try
            {
                if (movimiento != null)
                {
                    var ec = await _request.GetData($"{_configuration["APIurl"]}{_urlEc}/GetEstadoCuentaByIdTarjeta/{movimiento.IdTarjeta}");
                    if (ec != null)
                    {
                        var resultEc = JsonConvert.DeserializeObject<List<EstadoCuentaViewModel>>(ec);
                        var ecdata = new EstadoCuentaViewModel();
                        if (resultEc.Count > 0)
                        {
                            ecdata = resultEc.SingleOrDefault();
                            if (movimiento.Monto <= ecdata.TotalMasIntereses && movimiento.Monto > 0)
                            {
                                //actualizar el estado cuenta
                                ecdata.SaldoDisponible = ecdata.SaldoDisponible + movimiento.Monto;
                                ecdata.TotalMasIntereses = ecdata.TotalMasIntereses - movimiento.Monto;
                                if (ecdata.TotalMasIntereses == 0)
                                {
                                    ecdata.SaldoActual = 0;
                                    ecdata.SaldoDisponible = (decimal)movimiento.SaldoTarjeta;
                                }
                                else
                                {
                                    ecdata.SaldoActual = ecdata.SaldoActual - movimiento.Monto;
                                }
                                await _request.PostData(ecdata, $"{_configuration["APIurl"]}{_urlEc}", HttpMethod.Put);
                                realizarRegistro = true;
                            }

                            if (realizarRegistro)
                            {
                                //crear el registro del pago realizado
                                movimiento.FechaMovimiento = DateTime.Now;
                                movimiento.TipoMovimiento = 2;
                                var r = await _request.PostData(movimiento, $"{_configuration["APIurl"]}{_url}", HttpMethod.Post);
                                if (r != null)
                                {
                                    var dm = JsonConvert.DeserializeObject<MovimientosViewModel>(r);
                                    //devolver el objeto recien agregado
                                    data.data = new List<MovimientosViewModel>();
                                    dm.FechaTexto = dm.FechaMovimiento.ToString("dd/MM/yyyy");
                                    data.data.Add(dm);
                                    data.Mensaje = "Pago realizado correctamente";
                                }
                            }
                            else
                            {
                                //retornar mensaje de error
                                data.Mensaje = "El monto ingresado no es valido.";
                            }
                        }
                        else
                        {
                            data.Mensaje = "No se puede realizar un pago ya que el saldo actual es $0.00";
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            return Ok(data);
        }
        [HttpPost]
        public async Task<IActionResult> AccionesMovimientos(MovimientosViewModel? movimiento)
        {
            MensajesViewModel<List<MovimientosViewModel>> data = new MensajesViewModel<List<MovimientosViewModel>>();
            bool realizarRegistro = false;
            try
            {
                if (movimiento != null)
                {
                    var tipoAccion = HttpMethod.Post;
                    if (movimiento.IdMovimiento > 0)
                    {
                        tipoAccion = HttpMethod.Put;
                    }
                    //verificar si la tarjeta contiene el saldo disponible para hacer la transaccion
                    var ec = await _request.GetData($"{_configuration["APIurl"]}{_urlEc}/GetEstadoCuentaByIdTarjeta/{movimiento.IdTarjeta}");
                    if (ec != null)
                    {
                        var resultEc = JsonConvert.DeserializeObject<List<EstadoCuentaViewModel>>(ec);
                        var ecdata = new EstadoCuentaViewModel();
                        TarjetaViewModel trjdata = new TarjetaViewModel();
                        //buscar la configuracion para los calculos de procentajes
                        var conf = await _request.GetData($"{_configuration["APIurl"]}{urlConf}/GetByIdTarjeta/{movimiento.IdTarjeta}");
                        if (resultEc.Count > 0)
                        {
                            ecdata = resultEc.SingleOrDefault();
                            if (ecdata.SaldoDisponible >= movimiento.Monto)
                            {
                                if (conf != null)
                                {
                                    ecdata.SaldoDisponible = ecdata.SaldoDisponible - movimiento.Monto;
                                    ecdata.SaldoActual = ecdata.SaldoActual + movimiento.Monto;
                                    var _cf = JsonConvert.DeserializeObject<List<ConfiguracionViewModel>>(conf);
                                    if (_cf.Count > 0)
                                    {

                                        trjdata.Configuracion = _cf.SingleOrDefault();
                                        trjdata.PorcentajeInteres = trjdata.Configuracion.PorcentajeInteres;
                                        trjdata.PorcentajeSaldoMin = trjdata.Configuracion.PorcentajeSaldoMin;
                                        double pInteres = (double)trjdata.PorcentajeInteres / 100;
                                        double psalMin = (double)trjdata.PorcentajeSaldoMin / 100;
                                        trjdata.InteresBonificable = ecdata.SaldoActual * (decimal)pInteres;
                                        trjdata.CuotaMinimaPago = ecdata.SaldoActual * (decimal)psalMin;
                                        trjdata.ContadoConIntereses = ecdata.SaldoActual + trjdata.InteresBonificable;
                                        ecdata.TotalMasIntereses = trjdata.ContadoConIntereses;
                                    }
                                    else
                                    {
                                        ecdata.TotalMasIntereses = 0;
                                    }
                                }
                                //actualizar el estado cuenta
                                await _request.PostData(ecdata, $"{_configuration["APIurl"]}{_urlEc}", HttpMethod.Put);
                                realizarRegistro = true;
                            }
                        }
                        else
                        {
                            //agregar un nuevo registro
                            ecdata = new EstadoCuentaViewModel();
                            ecdata.IdTarjeta = movimiento.IdTarjeta;
                            ecdata.SaldoDisponible = (decimal)movimiento.SaldoTarjeta - movimiento.Monto;
                            ecdata.SaldoActual = movimiento.Monto;
                            if (conf != null)
                            {
                                var _cf = JsonConvert.DeserializeObject<List<ConfiguracionViewModel>>(conf);
                                if (_cf.Count > 0)
                                {

                                    trjdata.Configuracion = _cf.SingleOrDefault();
                                    trjdata.PorcentajeInteres = trjdata.Configuracion.PorcentajeInteres;
                                    trjdata.PorcentajeSaldoMin = trjdata.Configuracion.PorcentajeSaldoMin;
                                    double pInteres = (double)trjdata.PorcentajeInteres / 100;
                                    double psalMin = (double)trjdata.PorcentajeSaldoMin / 100;
                                    trjdata.InteresBonificable = ecdata.SaldoActual * (decimal)pInteres;
                                    trjdata.CuotaMinimaPago = ecdata.SaldoActual * (decimal)psalMin;
                                    trjdata.ContadoConIntereses = ecdata.SaldoActual + trjdata.InteresBonificable;
                                    ecdata.TotalMasIntereses = trjdata.ContadoConIntereses;
                                }
                                else
                                {
                                    ecdata.TotalMasIntereses = 0;
                                }
                            }

                            await _request.PostData(ecdata, $"{_configuration["APIurl"]}{_urlEc}", HttpMethod.Post);
                            realizarRegistro = true;
                        }
                    }
                    if (realizarRegistro)
                    {
                        //crear el registro de la compra realizada
                        movimiento.FechaMovimiento = DateTime.Now;
                        movimiento.TipoMovimiento = 1;
                        var r = await _request.PostData(movimiento, $"{_configuration["APIurl"]}{_url}", tipoAccion);
                        if (r != null)
                        {
                            var dm = JsonConvert.DeserializeObject<MovimientosViewModel>(r);
                            //devolver el objeto recien agregado
                            data.data = new List<MovimientosViewModel>();
                            dm.FechaTexto = dm.FechaMovimiento.ToString("dd/MM/yyyy");
                            data.data.Add(dm);
                            data.Mensaje = "Compra registrada correctamente";
                        }
                    }
                    else
                    {
                        //retornar mensaje de error
                        data.Mensaje = "El saldo disponible es insuficiente para realizar la compra ingresada.";
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Ok(data);
        }
    }
}
