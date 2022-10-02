using DocAggregator.API.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DocAggregator.API.Presentation
{
    /// <summary>
    /// Содержит методы для представления ответа обработчика документа.
    /// </summary>
    public static class ClaimResponsePresenter
    {
        /// <summary>
        /// Получает ответ обработчика документа
        /// и упаковывает в ответное действие типа файлового потока.
        /// </summary>
        /// <param name="response">Ответ обработчика документа.</param>
        /// <returns>Результат действия в виде файлового потока.</returns>
        public static FileStreamResult ToFileStreamResult(DocumentResponse response)
        {
            return new FileStreamResult(response.ResultStream, "application/pdf");
        }

        /// <summary>
        /// Получает ответ обработчика, собирает ошибки
        /// и возвращает отформатированный ответ с 500 кодом.
        /// </summary>
        /// <param name="response">Ответ обработчика.</param>
        /// <returns>Текстовое содержание ошибок, сообщений и стэка вызовов с кодом 500.</returns>
        public static ObjectResult ToErrorReport(InteractorResponseBase response)
        {
            StringBuilder result = new StringBuilder();
            foreach (var err in response.Errors)
            {
                if (result.Length != 0)
                {
                    result.AppendLine(new string('-', 14));
                }
                result.Append(err.GetType().ToString());
                result.Append(": ");
                result.AppendLine(err.Message);
                if (err is SolvableException sol)
                {
                    for (int i = 0; i < sol.CureOptions.Length; i++)
                    {
                        result.Append("Решение #");
                        result.Append(i + 1);
                        result.Append(": ");
                        result.AppendLine(sol.CureOptions[i]);
                    }
                }
                result.AppendLine(err.StackTrace);
            }
            return new ObjectResult(result.ToString())
            {
                StatusCode = 500,
            };
        }
    }
}
