using System;

namespace HMediator
{
    public interface IServiceFactory
    {
        object Get(Type serviceType);
    }
}