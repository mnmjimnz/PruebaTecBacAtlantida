using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace bco.atlantida.estadocuenta.webapp.Core.Interface
{
    public interface IRequest
    {
        public Task<string> PostData<T>(T item, string url, HttpMethod tipo) where T : class;
        public Task<string> GetData(string url);
    }
}
