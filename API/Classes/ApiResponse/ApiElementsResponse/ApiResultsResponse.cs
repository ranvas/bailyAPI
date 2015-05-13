using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    class ApiResultsResponse : ApiElementsResponse
    {
        private object _results;
        public object Results
        {
            set
            {
                _results = value;
            }
            get
            {
                return _results;
            }
        }
    }
}
