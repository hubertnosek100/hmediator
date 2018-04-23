using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HMediator
{
    public class Mediator : IMediator
    {
        private readonly IServiceFactory _serviceFactory;

        public Mediator(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public void Send(ICommand request)
        {
            SendCommand(request);
        }

        public TResult Send<TResult>(IQuery<TResult> request)
        {
            return SendQuery(request);
        }

        private TResult SendQuery<TResult>(IQuery<TResult> query)
        {
            Type queryType = GetQueryType(query);
            string method = GetMethodForQueryHandler();
            return (TResult) SendRequest(query, queryType, method);
        }

        private void SendCommand(ICommand cmd)
        {
            Type cmdType = GetCommandType(cmd);
            string method = GetMethodForCommandHandler();
            SendRequest(cmd, cmdType, method);
        }

        private string GetMethodForQueryHandler()
        {
            return nameof(IQueryHandler<object,object>.Handle);
        }
        
        private string GetMethodForCommandHandler()
        {
            return nameof(ICommandHandler<object>.Handle);
        }

        private Type GetQueryType<TResult>(IQuery<TResult> query)
        {
            return typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        }

        private Type GetCommandType(ICommand cmd)
        {
            return typeof(ICommandHandler<>).MakeGenericType(cmd.GetType());
        }

        private object SendRequest(object request, Type cmdType, string method)
        {
            Type handlerType = GetHandlerType(request, cmdType);
            object[] objectServicesParameters = GetServices(handlerType);
            object instance = Activator.CreateInstance(handlerType, objectServicesParameters);
            MethodInfo handle = handlerType.GetTypeInfo().GetDeclaredMethod(method);
            return handle.Invoke(instance, new List<object> {request}.ToArray());
        }

        private Type GetHandlerType(object request, Type ihandler)
        {
            List<TypeInfo> assemblies = FindHandlersTypeInfo(request, ihandler);
            TypeInfo typeInfo = GetUniqueTypeInfo(ihandler, assemblies);
            return typeInfo.AsType();
        }


        private List<TypeInfo> FindHandlersTypeInfo(object request, Type ihandler)
        {
            Assembly currentAssembly = request.GetType().GetTypeInfo().Assembly;
            return currentAssembly.DefinedTypes
                .Where(type => type
                    .ImplementedInterfaces
                    .Any(inter => inter == ihandler))
                .ToList();
        }

        private static TypeInfo GetUniqueTypeInfo(Type ihandler, List<TypeInfo> assemblies)
        {
            if (assemblies.Count > 1)
                throw new MediatorException(
                    $"There is more assembilies which can handle commmand of type {ihandler}");

            TypeInfo typeInfo = assemblies.FirstOrDefault();

            if (typeInfo == null)
                throw new MediatorException(
                    $"There is no assembilies which can handle commmand of type {ihandler}");
            return typeInfo;
        }

        private object[] GetServices(Type type)
        {
            List<object> objectServices = new List<object>();
            foreach (ParameterInfo parameter in GetServiceTypes(type))
            {
                object service = GetService(parameter);
                objectServices.Add(service);
            }

            return objectServices.ToArray();
        }

        private ParameterInfo[] GetServiceTypes(Type type)
        {
            ConstructorInfo ctor = GetConstructorInfo(type);
            return ctor.GetParameters();
        }

        private object GetService(ParameterInfo parameter)
        {
            object service = _serviceFactory.Get(parameter.ParameterType);
            if (service == null)
                throw new MediatorException(
                    $"There is no service of type {parameter.ParameterType} injected to mediator.");
            return service;
        }

        private ConstructorInfo GetConstructorInfo(Type type)
        {
            ConstructorInfo ctor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault();
            if (ctor == null)
                throw new MediatorException($"Type {type} has not constructor");
            return ctor;
        }
    }
}