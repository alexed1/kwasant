﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KwasantCore.Exceptions
{
    public class EntityNotFoundException : ApplicationException
    {
        public EntityNotFoundException()
        {
            
        }

        public EntityNotFoundException(string message)
            : base(message)
        {
            
        }
    }

    public class EntityNotFoundException<T> : EntityNotFoundException
    {
        public EntityNotFoundException()
            : base(string.Format("{0} not found.", typeof(T).Name))
        {
            
        }

        public EntityNotFoundException(string message)
            : base(message)
        {
            
        }
    }
}