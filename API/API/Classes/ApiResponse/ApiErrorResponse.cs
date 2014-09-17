using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    class ApiErrorResponse : IApiResponse
    {
        private string _errorMessage;
        private int _responseCode;

        public int ResponseCode
        {
            get 
            {
                if (_responseCode == 0)
                {
                    _responseCode = 400;
                }
                return _responseCode; 
            }
            set 
            { 
                _responseCode = value; 
            }
        }

        public string ErrorMessage
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
