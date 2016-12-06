using Common.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientService : ChannelFactory<IClientContract>, IClientContract, IDisposable
    {
        private IClientContract factory;

        public ClientService(NetTcpBinding binding, EndpointAddress address)
			: base(binding, address)
		{
            
        }
    }
}
