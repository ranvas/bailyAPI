﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API
{
    /// <summary>
    /// Базовый класс при формировании ответа API
    /// </summary>
    public interface IApiResponse
    {
        int ResponseCode
        {
            get;
            set;
        }
    }
}
