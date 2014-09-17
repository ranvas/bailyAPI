using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    class ApiCategoriesResponse : ApiElementsResponse
    {
        
        private object _categories;



        public object Categories
        {
            set
            {
                _categories = value;
            }
            get
            {
                return _categories;
            }
        }
    }
}
