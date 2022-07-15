using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace DocAggregator.API.Core.Models
{
    [Obsolete("Remove this.")]
    public class OS
    {
        public string Name { get; init; }
        public string SerialNumber { get; init; }
        public string InventoryNumber { get; init; }
    }

    public class Inventory : IDisposable
    {
        public string Template { get; set; }

        public IEnumerable<ClaimField> InventoryFields { get; set; }

        public IEnumerable<OS> OSs { get; set; }

        public string GetField(string xPath)
        {
            return InventoryFields.SingleOrDefault((field) => field.VerbousID.Equals(xPath, StringComparison.OrdinalIgnoreCase))?.Value ?? "";
        }

        public bool TestBool(string path)
        {
            return InventoryFields.SingleOrDefault((field) => field.VerbousID.Equals(path, StringComparison.OrdinalIgnoreCase))?.ToBoolean() ?? false;
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
