using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    class ApiCountriesResponse : ApiElementsResponse
    {
        private object _countries;

        public object Countries
        {
            set
            {
                _countries = value;
            }
            get
            {
                return _countries;
            }
        }
    }
}
