using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    abstract class ApiElementsResponse : IApiResponse
    {
        private int _responseCode;
        private int _numberOfElements;


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

        public int NumberOfElements
        {
            set
            {
                _numberOfElements = value;
            }
            get
            {
                return _numberOfElements;
            }
        }


    }
}
