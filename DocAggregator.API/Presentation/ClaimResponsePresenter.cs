using DocAggregator.API.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DocAggregator.API.Presentation
{
    /// <summary>
    /// Содержит методы для представления ответа обработчика заявки.
    /// </summary>
    public static class ClaimResponsePresenter
    {
        /// <summary>
        /// Получает ответ обработчика заявки
        /// и упаковывает в ответное действие типа файлового потока.
        /// </summary>
        /// <param name="response">Ответ обработчика заявки.</param>
        /// <returns>Результат действия в виде файлового потока.</returns>
        public static FileStreamResult ToFileStreamResult(ClaimResponse response)
        {
            return new FileStreamResult(response.ResultStream, "application/pdf");
        }

        /// <summary>
        /// Получает ответ обработчика заявки, собирает ошибки
        /// и возвращает отформатированный ответ с 500 кодом.
        /// </summary>
        /// <param name="response">Ответ обработчика заявки.</param>
        /// <returns>Текстовое содержание ошибок, сообщений и стэка вызовов с кодом 500.</returns>
        public static ObjectResult ToErrorReport(ClaimResponse response)
        {
            StringBuilder result = new StringBuilder();
            foreach (var err in response.Errors)
            {
                if (result.Length != 0)
                {
                    result.AppendLine(new string('-', 14));
                }
                result.AppendLine(err.GetType().ToString());
                result.AppendLine(err.Message);
                result.AppendLine(err.StackTrace);
            }
            return new ObjectResult(result.ToString())
            {
                StatusCode = 500,
            };
        }
    }
}
