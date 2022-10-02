using DocAggregator.API.Core;
using System;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public static class RepositoryExceptionHelper
    {
        public static void ThrowConfigurationQueriesFileFailure(Exception ex)
        {
            var message = "не удалось загрузить файл именованных запросов по заданному пути";
            var cure = $"перейдите по пути в поле {RepositoryConfigOptions.RepositoryConfig}:{nameof(RepositoryConfigOptions.QueriesFile)} " +
                $"конфигурации и убедитесь в наличии доступа к заданному файлу";
            throw new SolvableException(message, ex, cure);
        }

        public static void ThrowConfigurationModelFolderFailure(Exception ex)
        {
            var message = "не удалось загрузить модели документа по заданому пути";
            var cure = $"перейдите по пути в поле {RepositoryConfigOptions.RepositoryConfig}:{nameof(RepositoryConfigOptions.TemplateBindings)} " +
                $"конфигурации и убедитесь в наличии доступа к заданной папке";
            throw new SolvableException(message, ex, cure);
        }

        public static void ThrowConfigurationTemplateFolderFailure(Exception ex)
        {
            var message = "не удалось загрузить карту шаблонов документа по заданому пути";
            var cure = $"перейдите по пути в поле {RepositoryConfigOptions.RepositoryConfig}:{nameof(RepositoryConfigOptions.TemplateMaps)} " +
                $"конфигурации и убедитесь в наличии доступа к заданной папке";
            throw new SolvableException(message, ex, cure);
        }

        public static void ThrowConfigurationTemplateFileFailure(string document, Exception ex)
        {
            var message = "не удалось загрузить карту шаблонов документа по заданому пути";
            var cure = $"перейдите по пути в поле {RepositoryConfigOptions.RepositoryConfig}:{nameof(RepositoryConfigOptions.TemplateMaps)} " +
                $"конфигурации и убедитесь в наличии доступа к файлу {document}"; // document is a fullpath
            throw new SolvableException(message, ex, cure);
        }

        public static void ThrowTemplateNotFoundFailure(string document)
        {
            var message = $"документ типа \"{document}\" не распознан";
            var cure = $"перейдите по пути в поле {RepositoryConfigOptions.RepositoryConfig}:{nameof(RepositoryConfigOptions.TemplateMaps)} " +
                $"конфигурации и убедитесь в наличии доступа к файлу {document}.xml";
            throw new SolvableException(message, cure);
        }

        public static void ThrowTemplateNotMatchedFailure(string document, string element, string attributes)
        {
            //var message = $"Template has not found for a {document} with ID = {element}. Affected attributes: {{{attributes}}}.";
            var message = $"шаблон не найден для типа {document} с ID = {element}; затронутые атрибуты : {{{attributes}}}.";
            var cure = $"сверьте комбинацию затронутых атрибутов с записями шаблонов типа {document}";
            throw new SolvableException(message, cure);
        }

        public static void ThrowModelNotFoundFailure(string document)
        {
            var message = $"модель типа {document} не найдена";
            var cure = $"проверьте наличие файла {document}.xml в папке моделей, " +
                $"параметр конфигурации {RepositoryConfigOptions.RepositoryConfig}:{nameof(RepositoryConfigOptions.TemplateBindings)}";
            throw new SolvableException(message, cure);
        }

        public static void ThrowIndexOfForElementFailure(Exception ex)
        {
            var message = $"атрибут itemIndexColumn элемента For не может быть установлен";
            var cure = $"проверьте наличие атрибута itemIndexColumn и соответствие его значения одному из столбцов запроса";
            throw new SolvableException(message, ex, cure);
        }
    }
}
