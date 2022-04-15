using DocAggregator.API.Core;
using System.Text;

namespace DocAggregator.API.Infrastructure.OpenXMLProcessing
{
    internal class UnoConverterLogger
    {
        const string UNO_INFO = "INFO:unoserver:";

        int _state;
        ILogger _logger;
        StringBuilder _buffer;

        public UnoConverterLogger(ILogger logger)
        {
            _state = 0;
            _logger = logger;
            _buffer = new StringBuilder();
        }

        public void Log(string inputLine)
        {
            if (inputLine == null)
            {
                if (_buffer.Length != 0)
                {
                    _logger.Error(_buffer.ToString());
                    _buffer.Clear();
                }
                return;
            }
            switch (_state)
            {
                case 0:
                    if (inputLine.StartsWith(UNO_INFO))
                    {
                        _logger.Information(inputLine.Substring(UNO_INFO.Length));
                    }
                    else
                    {
                        _buffer.Append(inputLine);
                        _state = 1;
                    }
                    break;
                case 1:
                    if (inputLine.StartsWith(UNO_INFO))
                    {
                        _logger.Error(_buffer.ToString());
                        _buffer.Clear();
                        _state = 0;
                        goto case 0;
                    }
                    else
                    {
                        _buffer.Append('\n');
                        _buffer.Append(inputLine);
                    }
                    break;
                default:
                    _logger.Error("Internal state is unexpected.");
                    break;
            }
        }
    }
}
