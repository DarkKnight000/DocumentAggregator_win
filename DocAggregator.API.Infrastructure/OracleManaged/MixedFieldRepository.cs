using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Реализует интерфейс <see cref="IClaimFieldRepository"/> используя базу данных Oracle.
    /// </summary>
    public class MixedFieldRepository : IClaimFieldRepository
    {
        readonly ILogger _logger;
        readonly SqlConnectionResource _sqlResource;
        readonly Dictionary<int, System.Tuple<string, string>> _fieldNamesCache;

        /// <summary>
        /// Инициализирует объект <see cref="MixedFieldRepository"/>.
        /// </summary>
        public MixedFieldRepository(SqlConnectionResource sqlResource, ILoggerFactory logger)
        {
            _logger = logger.GetLoggerFor<IClaimFieldRepository>();
            _sqlResource = sqlResource;
            _fieldNamesCache = new Dictionary<int, System.Tuple<string, string>>();
        }

        public IEnumerable<ClaimField> GetFiledListByClaimId(Claim claim, bool loadNames)
        {
            QueryExecuterWorkspace executerWork = new QueryExecuterWorkspace()
            {
                Claim = claim,
                Logger = _logger,
                SqlReqource = _sqlResource,
            };
            var result = new List<ClaimField>();
            if (loadNames)
            {
                if (_fieldNamesCache.Count == 0)
                {
                    executerWork.Query = string.Format(_sqlResource.GetStringByName("Q_HRDClaimFieldNameList_ByRequestType"), claim.TypeID);
                    using (QueryExecuter executer = new QueryExecuter(executerWork))
                        while (executer.Reader.Read())
                        {
                            string categoryName = executer.Reader.GetString(0);
                            string attributeName = executer.Reader.GetString(1);
                            int attributeId = executer.Reader.GetInt32(2);
                            _fieldNamesCache.Add(attributeId, System.Tuple.Create(categoryName, attributeName));
                        }
                }
            }
            executerWork.Query = string.Format(_sqlResource.GetStringByName("Q_HRDAttributeIdsValues_ByRequest"), claim.ID);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
                while (executer.Reader.Read())
                {
                    result.Add(new ClaimField()
                    {
                        VerbousID = executer.Reader.GetString(0),
                        Category = loadNames ? _fieldNamesCache[executer.Reader.GetInt32(0)].Item1 : string.Empty,
                        Attribute = loadNames ? _fieldNamesCache[executer.Reader.GetInt32(0)].Item2 : string.Empty,
                        Value = executer.Reader.IsDBNull(1) ? string.Empty : executer.Reader.GetString(1),
                    });
                }
            executerWork.Query = string.Format(_sqlResource.GetStringByName("Q_HRDAddressAction_ByRequest"), claim.ID);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
                while (executer.Reader.Read())
                {
                    for (int i = 0; i < executer.Reader.FieldCount; i++)
                    {
                        result.Add(new ClaimField()
                        {
                            VerbousID = executer.Reader.GetName(i),
                            Category = string.Empty,
                            Attribute = loadNames ? executer.Reader.GetName(i) : string.Empty,
                            Value = executer.Reader.IsDBNull(i) ? string.Empty : executer.Reader.GetString(i),
                        });
                    }
                }
            return result;
        }

        public IEnumerable<InformationResource> GetInformationalResourcesByClaim(Claim claim)
        {
            var result = new Dictionary<int, InformationResource>();
            QueryExecuterWorkspace accessListRetrieve = new QueryExecuterWorkspace()
            {
                Query = string.Format(_sqlResource.GetStringByName("Q_HRDClaimInformationalResourcesList_ByRequest"), claim.ID),
                Claim = claim,
                Logger = _logger,
                SqlReqource = _sqlResource,
            };
            using (QueryExecuter executer = new QueryExecuter(accessListRetrieve))
                while (executer.Reader.Read())
                {
                    AccessRightStatus stat = AccessRightStatus.NotMentioned;
                    if (!executer.Reader.IsDBNull(4))
                    {
                        switch (executer.Reader.GetString(4))
                        {
                            case "0":
                                stat = AccessRightStatus.Denied;
                                break;
                            case "1":
                                stat = AccessRightStatus.Allowed;
                                break;
                            default:
                                break;
                        }
                    }
                    AccessRightField accessField = new AccessRightField()
                    {
                        NumeralID = executer.Reader.GetInt32(3),
                        Name = executer.Reader.GetString(2),
                        Status = stat,
                    };
                    int infoResourceId = executer.Reader.GetInt32(1);
                    if (result.TryGetValue(infoResourceId, out InformationResource infoResource))
                    {
                        infoResource.AccessRightFields = System.Linq.Enumerable.Append(infoResource.AccessRightFields, accessField);
                    }
                    else
                    {
                        result.Add(infoResourceId, new InformationResource()
                            {
                                ID = infoResourceId,
                                Name = executer.Reader.GetString(0),
                                AccessRightFields = new AccessRightField[] { accessField },
                            });
                    }
                }
            return result.Values;
        }

        public IEnumerable<AccessRightField> GetFilledAccessListByClaimId(Claim claim)
        {
            var result = new List<AccessRightField>();
            QueryExecuterWorkspace accessListRetrieve = new()
            {
                Query = string.Format(_sqlResource.GetStringByName("Q_HRDClaimAccessList_ByRequest"), claim.ID),
                Claim = claim,
                Logger = _logger,
                SqlReqource = _sqlResource,
            };
            using (QueryExecuter executer = new QueryExecuter(accessListRetrieve))
                while (executer.Reader.Read())
                {
                    AccessRightStatus stat = AccessRightStatus.NotMentioned;
                    if (!executer.Reader.IsDBNull(2))
                    {
                        switch (executer.Reader.GetString(2))
                        {
                            case "0":
                                stat = AccessRightStatus.Denied;
                                break;
                            case "1":
                                stat = AccessRightStatus.Allowed;
                                break;
                            default:
                                break;
                        }
                    }
                    result.Add(new AccessRightField()
                    {
                        NumeralID = executer.Reader.GetInt32(1),
                        Name = executer.Reader.GetString(0),
                        Status = stat,
                    });
                }
            return result;
        }
    }
}
