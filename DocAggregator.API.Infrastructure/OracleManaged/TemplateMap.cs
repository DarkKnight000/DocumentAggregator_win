using DocAggregator.API.Core;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Timers;
using Oracle.ManagedDataAccess.Client;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Представляет модель связки типа заявки с его шаблоном.
    /// </summary>
    [Serializable]
    public class TemplateBind
    {
        
        /// <summary>
        /// Ограничивающие выбор признаки.
        /// </summary>
        [XmlAnyAttribute]
        public XmlAttribute[] Filter { get; set; }
        /// <summary>
        /// Имя файла.
        /// </summary>
        [XmlText]
        public string FileName { get; set; }
    }

    /// <summary>
    /// Класс-ресурс шаблонов заявки.
    /// </summary>
    public class TemplateMap
    {
        private static Timer timer;
        //Test
        RepositoryConfigOptions db2;

        //
        public bool change;
        public bool isf;
        public string MEGA;
        private ILogger _logger;
        private IDictionary<string, IEnumerable<TemplateBind>> _bindsMap;

        public IDictionary<string, IEnumerable<TemplateBind>> BindsMap => _bindsMap;

        public TemplateMap(IOptionsFactory optionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<TemplateMap>();
            _bindsMap = new Dictionary<string, IEnumerable<TemplateBind>>();
            var db = optionsFactory.GetOptionsOf<RepositoryConfigOptions>();
            db2 = db;
            List<TemplateBind> config = null;
            string[] files = null;
            DoWork();
            void DoWork()
            {
                try
                {
                    // TODO: FILEWATCHER
                    files = Directory.GetFiles(Path.GetFullPath(db.TemplateMaps), "*.xml");
                }
                catch (Exception ex)
                {
                    RepositoryExceptionHelper.ThrowConfigurationTemplateFolderFailure(ex);
                }
                foreach (var type in files)
                {
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(type))
                        {
                            XmlSerializer deserializer = new XmlSerializer(typeof(List<TemplateBind>));
                            config = (List<TemplateBind>)deserializer.Deserialize(streamReader);
                        }
                    }
                    catch (Exception ex)
                    {
                        RepositoryExceptionHelper.ThrowConfigurationTemplateFileFailure(type, ex);
                        MEGA = RepositoryExceptionHelper.ThrowConfigurationTemplateFileFailure(type, ex);
                        using (StreamWriter streamWriter = new StreamWriter("TextFile1.txt", true, System.Text.Encoding.Default))
                        {
                            streamWriter.WriteLineAsync("\n\n" + MEGA + "\n\n");
                            change = true;
                        }
                    }
                    finally
                    {
                        if (change)
                        {
                            //SendMail();
                            change = false;
                        }
                        _bindsMap.Add(Path.GetFileNameWithoutExtension(type).ToLower(), config);
                        //Task.Delay(10000);
                    }
                    //_bindsMap.Add(Path.GetFileNameWithoutExtension(type).ToLower(), config);
                    //DoWork();
                }
                
            }
        }

        
        public void SendMail()
        {
            string connectionString = "Data Source=(DESCRIPTION =(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST = localhost)(PORT = 1521)))(CONNECT_DATA =(SERVICE_NAME = WDB)));" +
                    "User Id=HRD_NEW_DOC;Password=123";

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                DateTime datatemp = DateTime.Now;
                string insertDataQuery = "INSERT INTO TempEr (temp,data) VALUES (:temp,:data)";
                OracleCommand insertDataCommand = new OracleCommand(insertDataQuery, connection);
                insertDataCommand.Parameters.Add(":temp", OracleDbType.Varchar2).Value = MEGA;
                insertDataCommand.Parameters.Add(":data", OracleDbType.Date).Value = datatemp;
                insertDataCommand.ExecuteNonQuery();

                connection.Close();
            }
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                OracleCommand mailsend = new OracleCommand();
                mailsend.CommandText = "DECLARE\r\ntmpVar NUMBER;\r\nmsg varchar2(4000);\r\n\r\nBEGIN\r\n    tmpVar := 0;\r\n    msg :=0;\r\n     \r\n\r\n   SELECT max(id) INTO tmpVar FROM temper;\r\n   select temp into msg from temper where id=tmpVar;\r\n   \r\n   hrd.msg_pack.sent_email('Обработчик XML-шаблонов',msg,null,1,'dd_skuba@ntc.rosneft.ru','AV_Grishin6@ntc.rosneft.ru');\r\nend;";
                mailsend.Connection = connection;
                mailsend.ExecuteNonQuery();
            }
            
        }
        public void Check()
        {
            List<TemplateBind> config = null;
            string[] files = null;
                try
                {
                    // TODO: FILEWATCHER
                    files = Directory.GetFiles(Path.GetFullPath(db2.TemplateMaps), "*.xml");
                }
                catch (Exception ex)
                {
                    RepositoryExceptionHelper.ThrowConfigurationTemplateFolderFailure(ex);
                }
                foreach (var type in files)
                {
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(type))
                        {
                            XmlSerializer deserializer = new XmlSerializer(typeof(List<TemplateBind>));
                            config = (List<TemplateBind>)deserializer.Deserialize(streamReader);
                        }
                    }
                    catch (Exception ex)
                    {
                        RepositoryExceptionHelper.ThrowConfigurationTemplateFileFailure(type, ex);
                        MEGA = RepositoryExceptionHelper.ThrowConfigurationTemplateFileFailure(type, ex);
                        using (StreamWriter streamWriter = new StreamWriter("TextFile1.txt", true, System.Text.Encoding.Default))
                        {
                            streamWriter.WriteLineAsync("\n\n" + MEGA + "\n\n");
                            change = true;
                        }
                    }
                    finally
                    {
                        if (change)
                        {
                            //SendMail();
                            change = false;
                        }
                        //_bindsMap.Add(Path.GetFileNameWithoutExtension(type).ToLower(), config);
                        //Task.Delay(10000);
                    }
                    //_bindsMap.Add(Path.GetFileNameWithoutExtension(type).ToLower(), config);
                    //DoWork();
                }

            


        }

       
    
        /// <summary>
        /// Получает путь к шаблону по типу документа и данных его модели.
        /// </summary>
        /// <param name="documentType">Тип документа.</param>
        /// <param name="model">Модель документа.</param>
        /// <returns>Путь к документу шаблона.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="SolvableException"/>
        public string GetTemplate(string documentType, XElement model)
        {
            if (isf)
            {
                Check();
            }
            isf = true;
            if (string.IsNullOrEmpty(documentType) || !_bindsMap.ContainsKey(documentType))
            {
                RepositoryExceptionHelper.ThrowTemplateNotFoundFailure(documentType);
            }
            var binds = _bindsMap[documentType];
            HashSet<string> affectedAttributes = new HashSet<string>();
            foreach (var bind in binds)
            {
                if (bind.Filter?.All( // If we miss a property, ignore this returning 'true'.
                        attr =>
                        {
                            affectedAttributes.Add(attr.Name.ToLower());
                            return model.Element(attr.Name.ToLower())?.Value?.Equals(attr.Value) ?? true;
                        }
                    ) ?? true)
                {
                    _logger.Trace("Have got a template for a {0} with proprties: {{{1}}}",
                        documentType,
                        string.Join(", ", bind.Filter?.Select(
                            attr => $"\"{attr.Name}\":\"{attr.Value}\""
                        ) ?? Enumerable.Empty<string>()));
                    return bind.FileName;
                }
            }
            RepositoryExceptionHelper.ThrowTemplateNotMatchedFailure(documentType,
                model?.Element("id")?.Value ?? "unknown",
                string.Join(",", affectedAttributes.Select(
                    attr => $"\"{attr}\":\"{model.Element(attr).Value}\""
                ) ?? Enumerable.Empty<string>()));
            return null;
        }
    }
}
