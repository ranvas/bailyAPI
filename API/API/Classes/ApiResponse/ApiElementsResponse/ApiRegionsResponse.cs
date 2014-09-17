using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    class ApiRegionsResponse : ApiElementsResponse
    {
        private object _regions;

        public object Regions
        {
            set
            {
                _regions = value;
            }
            get
            {
                return _regions;
            }
        }
    }
}
