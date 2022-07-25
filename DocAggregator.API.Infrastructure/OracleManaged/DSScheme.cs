using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Document Structure Scheme class
    /// </summary>
    class DSS
    {
        public static readonly XNamespace dss = "http://ntc.rosneft.ru/DBD/DocumentAggegator/Structure";

        public static readonly XName DataSource = dss + "DataSource";
        public static readonly XName Query = dss + "Query";
        public static readonly XName Collection = dss + "Collection";
        public static readonly XName Table = dss + "Table";
        public static readonly XName For = dss + "For";
        public static readonly XName Get = dss + "Get";
    }
}
