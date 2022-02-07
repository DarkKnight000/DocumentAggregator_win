using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    internal static class StaticExtensions
    {
       internal const string CONNECTION_STRING = "Data Source=(DESCRIPTION =(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST = 10.50.12.6)(PORT = 1521)))(CONNECT_DATA =(SERVICE_NAME = WDB)));User ID=HRD_NEW_DOC;Password=123;";
    }
}
