using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    [Serializable]
    public class SqlQuery
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlText]
        public string Query { get; set; }
        [XmlIgnore]
        public bool IsObsolete => false;
    }

    public static class SqlResource
    {
        private static Dictionary<string, SqlQuery> dictionary;

        static SqlResource()
        {
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
