using System;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Представляет базовый абстрактный класс для обработчика.
    /// </summary>
    /// <typeparam name="TResponse">Тип объекта запроса.</typeparam>
    /// <typeparam name="TRequest">Тип объекта ответа.</typeparam>
    public abstract class InteractorBase<TResponse, TRequest> where TResponse : InteractorResponseBase, new()
    {
        /// <summary>
        /// Объект запроса.
        /// </summary>
        protected TRequest Request { get; private set; }

        /// <summary>
        /// Объект ответа.
        /// </summary>
        protected TResponse Response { get; private set; }

        /// <summary>
        /// Метод для внешнего вызова. Производит обработку запроса с отловом исключений.
        /// </summary>
        /// <param name="request">Объект запроса.</param>
        /// <returns>Объект ответа.</returns>
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

        /// <summary>
        /// Метод для внутреннего вызова. Производит обработку без отлова исключений,
        /// используя локальные поля запроса и ответа.
        /// </summary>
        protected abstract void Handle();
    }
}
