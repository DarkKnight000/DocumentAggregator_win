﻿using System.Xml.Linq;

namespace DocAggregator.API.Infrastructure.OpenXMLProcessing
{
    internal static class W14
    {
        public static readonly XNamespace w14 = "http://schemas.microsoft.com/office/word/2010/wordml";

        public static readonly XName checkbox = w14 + "checkbox";
        public static readonly XName check = w14 + "checked";
        public static readonly XName val = w14 + "val";
    }

    internal static class W
    {
        public static readonly XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        public static readonly XName sdt = w + "sdt";
        public static readonly XName sdtPr = w + "sdtPr";
        public static readonly XName tag = w + "tag";
        public static readonly XName alias = w + "alias";
        public static readonly XName val = w + "val";
        public static readonly XName sdtContent = w + "sdtContent";
        public static readonly XName p = w + "p";
        public static readonly XName r = w + "r";
        public static readonly XName t = w + "t";
    }
}
