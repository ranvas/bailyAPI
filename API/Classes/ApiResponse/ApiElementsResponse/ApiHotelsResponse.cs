using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    class ApiHotelsResponse : ApiElementsResponse
    {
        private object _hotels;

        public object Hotels
        {
            set
            {
                _hotels = value;
            }
            get
            {
                return _hotels;
            }
        }
    }
}
