using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    class ApiOperatorsResponse : ApiElementsResponse
    {
        private object _operators;


        public object Operators
        {
            set
            {
                _operators = value;
            }
            get
            {
                return _operators;
            }
        }
    }
}
