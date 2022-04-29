using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DocAggregator.API.Core.Models
{
    /// <summary>
    /// Представляет объект заявки.
    /// </summary>
    /// <remarks>
    /// Реализует паттерн единицы работы, отвечая за жизненный цикл соединения с бд.
    /// </remarks>
    public class Claim : IDisposable
    {
        /// <summary>
        /// Идентификатор заявки, согласно базе данных.
        /// </summary>
        public int ID { get; init; }

        /// <summary>
        /// Идентификатор типа заявки.
        /// </summary>
        public int TypeID { get; init; }

        /// <summary>
        /// Идентификатор системы, к которой зарашивается доступ.
        /// </summary>
        public int SystemID { get; init; }

        /// <summary>
        /// Шаблон соответствующий типу заявки.
        /// </summary>
        public string Template { get; init; }

        /// <summary>
        /// Поля заявки.
        /// </summary>
        public IEnumerable<ClaimField> ClaimFields { get; set; }

        /// <summary>
        /// Поля прав доступа заявки.
        /// </summary>
        public IEnumerable<AccessRightField> AccessRightFields { get; set; }

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
