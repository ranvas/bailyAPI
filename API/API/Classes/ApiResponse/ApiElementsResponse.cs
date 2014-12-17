using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    abstract class ApiElementsResponse : IApiResponse
    {
        private int _responseCode;
        private int _countOfElements;

        public int ResponseCode
        {
            get 
            {
                if (_responseCode == 0) 
                {
                    _responseCode = 200;
                }
                return _responseCode; 
            }
            set 
            { 
                _responseCode = value; 
            }
        }

        public int CountOfElements
        {
            set
            {
                _countOfElements = value;
            }
            get
            {
                return _countOfElements;
            }
        }


    }
}
