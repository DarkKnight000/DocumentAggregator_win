using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Xml.Linq;

namespace DocAggregator.API.Core.Models
{
    public class Inventory : IDisposable
    {
        public string Template { get; set; }

        public XElement Root { get; set; }

        public string GetField(string xPath)
        {
            return Root.Element(xPath.ToUpper())?.Value ?? "";
        }

        public bool TestBool(string xPath)
        {
            return bool.TryParse(Root.Element(xPath.ToUpper())?.Value, out bool result) & result;
        }

        /// <summary>
        /// Подключение к базе данных.
        /// </summary>
        public DbConnection DbConnection
        {
            get
            {
                if (_dbConnection == null)
                {
                    return null;
                }
                if (!_dbConnection.State.HasFlag(ConnectionState.Open))
                {
                    _dbConnection.Open();
                }
                return _dbConnection;
            }
            init => _dbConnection = value;
        }
        private DbConnection _dbConnection;

        public void Dispose()
        {
            if (DbConnection.State.HasFlag(ConnectionState.Open))
            {
                DbConnection.Close();
            }
            ((IDisposable)DbConnection).Dispose();
        }
    }
}
