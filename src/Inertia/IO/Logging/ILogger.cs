﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Logging
{
    public interface ILogger 
    {
        public void Debug(object content);
        public void Warn(object content);
        public void Error(object content);
    }
}