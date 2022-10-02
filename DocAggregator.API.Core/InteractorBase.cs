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
        /// Служба ведения журнала.
        /// </summary>
        protected ILogger Logger { get; set; }

        public InteractorBase(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Метод для внешнего вызова. Производит обработку запроса с отловом исключений.
        /// </summary>
        /// <param name="request">Объект запроса.</param>
        /// <returns>Объект ответа.</returns>
        public TResponse Handle(TRequest request)
        {
            TResponse response = new TResponse();
            try
            {
                Handle(response, request);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "An error occured in {0} while handling a request.", GetType().Name);
                response.Errors.Add(ex);
            }
            return response;
        }

        /// <summary>
        /// Метод для внутреннего вызова. Производит обработку без отлова исключений,
        /// используя локальные поля запроса и ответа.
        /// </summary>
        /// <param name="response">Объект ответа.</param>
        /// <param name="request">Объект запроса.</param>
        /// <exception cref="Exception"/>
        protected abstract void Handle(TResponse response, TRequest request);
    }
}
