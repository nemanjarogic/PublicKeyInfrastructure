﻿using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class VAProxy : ChannelFactory<IValidationAuthorityContract>, IValidationAuthorityContract,IDisposable
    {
        private IValidationAuthorityContract proxy;

    }
}