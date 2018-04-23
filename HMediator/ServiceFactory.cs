using System;

namespace HMediator
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly Func<Type, object> _getService;

        public ServiceFactory(Func<Type, object> getService)
        {
            _getService = getService;
        }

        public object Get(Type serviceType)
        {
            return _getService(serviceType);
        }
    }
}