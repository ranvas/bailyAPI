using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    class ApiErrorResponse : IApiResponse
    {
        private object _errorMessage;
        private int _responseCode;

        public ApiErrorResponse(string errorMessage, int code = 500)
        {
            ResponseCode = code;
            Body = errorMessage;
        }

        public int ResponseCode
        {
            get 
            {
                if (_responseCode == 0)
                {
                    _responseCode = 500;
                }
                return _responseCode; 
            }
            set 
            { 
                _responseCode = value; 
            }
        }

        public object Body
        {
            set
            {
                _errorMessage = value;
            }
            get
            {
                return _errorMessage;
            }
        }
    }
}
