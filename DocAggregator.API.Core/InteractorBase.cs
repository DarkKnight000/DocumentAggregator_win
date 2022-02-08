using System;

namespace DocAggregator.API.Core
{
    public abstract class InteractorBase<TResponse, TRequest> where TResponse : InteractorResponseBase, new()
    {
        protected TRequest Request { get; private set; }
        protected TResponse Response { get; private set; }

        public TResponse Handle(TRequest request)
        {
            Request = request;
            Response = new TResponse();
            try
            {
                Handle();
            }
            catch (Exception ex)
            {
                Response.Errors.Add(ex);
            }
            return Response;
        }

        protected abstract void Handle();
    }
}
