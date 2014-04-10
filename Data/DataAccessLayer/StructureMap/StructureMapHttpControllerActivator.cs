using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using StructureMap;

namespace Data.DataAccessLayer.StructureMap
{
    public class StructureMapHttpControllerActivator : IHttpControllerActivator
    {
        #region Members

        private readonly IContainer _container;

        #endregion

        #region Constructors

        public StructureMapHttpControllerActivator(IContainer container)
        {
            this._container = container;
        }

        #endregion

        #region Method

        public IHttpController Create(
            HttpRequestMessage request,
            HttpControllerDescriptor controllerDescriptor,
            Type controllerType)
        {
            var nestedContainer = _container.GetNestedContainer();
            request.RegisterForDispose(nestedContainer);
            return (IHttpController) nestedContainer.GetInstance(controllerType);
        }

        #endregion

    }
}