using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Предстваляет модель запроса для использования в конфигурации.
    /// </summary>
    [Serializable]
    public class SqlQuery
    {
        /// <summary>
        /// Название запроса.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// Тело запроса.
        /// </summary>
        [XmlText]
        public string Query { get; set; }
        /// <summary>
        /// Определяет, является ли запрос устаревшим.
        /// </summary>
        [XmlIgnore]
        public bool IsObsolete => false; // TODO: Проверить использование. Удалить, если не нужно.
    }

    /// <summary>
    /// Статический класс, предоставляющий ресурс именнованных запросов.
    /// </summary>
    public static class SqlResource
    {
        private static Dictionary<string, SqlQuery> dictionary;

        static SqlResource()
        {
            // TODO: Убрать в конфигурацию проекта
            string file = @"D:\Users\akkostin\source\repos\DocumentGeneration\WordprocessingWebAPI\App_Data\SqlResource.xml";
            List<SqlQuery> list;

            // deserialize the xml file
            using (StreamReader streamReader = new StreamReader(file))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(List<SqlQuery>));
                list = (List<SqlQuery>)deserializer.Deserialize(streamReader);
            }
            dictionary = new Dictionary<string, SqlQuery>();
            foreach (var item in list)
            {
                dictionary.Add(item.Name, item);
            }
        }

        public static string GetStringByName(string name) => GetQueryByName(name).Query;

        /// <summary>
        /// Получает запрос по его имени.
        /// </summary>
        /// <param name="name">Имя запроса.</param>
        /// <returns>Объект запроса.</returns>
        public static SqlQuery GetQueryByName(string name)
        {
            SqlQuery query = dictionary[name];

            if (query == null)
                throw new ArgumentException("The query '" + name + "' is not valid.");

            if (query.IsObsolete)
            {
                // TODO: log this.
            }
            return query;
        }
    }
}
