using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{

    class ApiCurrenciesResponse : ApiElementsResponse
    {
        private object _currencies;


        public object Currencies
        {
            set
            {
                _currencies = value;
            }
            get
            {
                return _currencies;
            }
        }
    }
}
