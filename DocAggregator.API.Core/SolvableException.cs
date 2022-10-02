using System;
using System.Linq;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Базовый класс для исключений, предоставляющих расширенную информацию о предотвращении его появления.
    /// </summary>
    public class SolvableException : Exception
    {
        /// <summary>
        /// Варианты предотвращения возникшего исключения.
        /// </summary>
        public string[] CureOptions { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SolvableException"/>.
        /// </summary>
        public SolvableException() : base()
        {
            CureOptions = Array.Empty<string>();
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SolvableException"/>
        /// с указанным сообщением и вариантами предотвращения исключения.
        /// </summary>
        /// <param name="message">Сообщение, обписывающее проблему.</param>
        /// <param name="cureOptions">Варианты предотвращения проблемы.</param>
        public SolvableException(string message, params string[] cureOptions) : base(message)
        {
            CureOptions = cureOptions;
            CopyCureToData();
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SolvableException"/>
        /// с указанным сообщением, вариантами предотвращения исключения и внутреннего исключения, ставшего причиной.
        /// </summary>
        /// <param name="message">Сообщение, обписывающее проблему.</param>
        /// <param name="innerException">Исключение, послужившее причиной этой ошибки.</param>
        /// <param name="cureOptions">Варианты предотвращения проблемы.</param>
        public SolvableException(string message, Exception innerException, params string[] cureOptions) : base(message, innerException)
        {
            CureOptions = cureOptions;
            CopyCureToData();
        }

        private void CopyCureToData()
        {
            foreach (var option in CureOptions.Select((c, i) => (c, i)))
            {
                Data.Add("Cure " + option.i, option.c);
            }
        }
    }
}
