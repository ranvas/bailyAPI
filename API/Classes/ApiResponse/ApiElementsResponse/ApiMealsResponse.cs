using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    class ApiMealsResponse : ApiElementsResponse
    {
        private object _meals;


        public object Meals
        {
            set
            {
                _meals = value;
            }
            get
            {
                return _meals;
            }
        }
    }
}
