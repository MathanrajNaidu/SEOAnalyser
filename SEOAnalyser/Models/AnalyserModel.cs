using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SEOAnalyser.Models
{
    public class AnalyserModel
    {
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public TextType SelectedTextType { get; set; } = TextType.Url;

        public enum TextType {
            Url,
            Text
        }

        public string Error { get; set; }

        public Dictionary<string, int> Words { get; set; }

        public Dictionary<string, int> MetaTagWords { get; set; }

        public Dictionary<string, int> ExternalLinks { get; set; }

        public bool GetWordCounts { get; set; } = true;

        public bool GetWordCountsOfMetaTags { get; set; } = true;

        public bool GetLinksCount { get; set; } = true;
    }
}