using System;
using System.Collections.Generic;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Представляет базовый абстрактный класс для объектов ответа обработчиков.
    /// </summary>
    public abstract class InteractorResponseBase
    {
        /// <summary>
        /// Отловленные внутренние ошибки.
        /// </summary>
        public ICollection<Exception> Errors { get; private set; }

        /// <summary>
        /// Возвращает истину при отсутствии ошибок.
        /// </summary>
        public bool Success => Errors.Count == 0;

        /// <summary>
        /// Базовый конструктор, инициализирующий поля.
        /// </summary>
        public InteractorResponseBase()
        {
            Errors = new List<Exception>();
        }

        /// <summary>
        /// Вспомогательный метод, добавляющий список ошибок к текущему.
        /// </summary>
        /// <param name="exceptions">Объединяемый список ошибок.</param>
        internal void AddErrors(params Exception[] exceptions)
        {
            foreach (var ex in exceptions)
            {
                Errors.Add(ex);
            }
        }
    }
}
