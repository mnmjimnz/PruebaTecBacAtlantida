using bco.atlantida.estadocuenta.webapp.Core.Interface;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using System.Text;

namespace bco.atlantida.estadocuenta.webapp.Repositorio.Infraestructura
{
    class RequestRepositorio : IRequest
    {
        private readonly HttpClient httpClient;
        private readonly AsyncRetryPolicy<HttpResponseMessage> politicaReintentos;
        private readonly IConfiguration _config;
        public RequestRepositorio(HttpClient _httpClient, IConfiguration config)
        {
            httpClient = _httpClient;
            _config = config;
            int nReintentos = Convert.ToInt32(_config["NReintentos"]);
            politicaReintentos = HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(retryCount:nReintentos,
                //.WaitAndRetryAsync(retryCount: 5,
                intentos => TimeSpan.FromSeconds(5),
                onRetry: (excepcion, tiempo, contexto) =>
                {
                    //Console.WriteLine("Reintento de conexion");
                    //Console.WriteLine($"Hora error: {DateTime.Now}");
                });
        }
        public async Task<string> GetData(string url)
        {
            try
            {
                var respuestaHttp = await politicaReintentos.ExecuteAsync(async () =>
                {
                    return await httpClient.GetAsync(url.ToString());
                });
                var x = respuestaHttp.Content.ReadAsStringAsync();
                return await respuestaHttp.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<string> PostData<T>(T item, string url, HttpMethod tipo) where T : class
        {
            try
            {
                var respuestaHttp = await politicaReintentos.ExecuteAsync(async () =>
                {
                    using (var request = new HttpRequestMessage(tipo, url))
                    {

                        var contenido = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
                        request.Content = contenido;
                        return await httpClient.SendAsync(request);
                    }
                });
                return await respuestaHttp.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
