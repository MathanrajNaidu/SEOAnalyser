using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using SEOAnalyser.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace SEOAnalyser.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(Models.AnalyserModel input)
        {
            var model = new Models.AnalyserModel() { 
                SelectedTextType = input.SelectedTextType
            };
            var metaTagsBuilder = new StringBuilder();
            string allText = string.Empty;
            var url = string.Empty;           

            if(input.SelectedTextType == Models.AnalyserModel.TextType.Url && !string.IsNullOrEmpty(input.Text))
            {
                url = JsonConvert.SerializeObject(input.Text.Trim());

                if (!Uri.IsWellFormedUriString(JsonConvert.DeserializeObject<string>(url), UriKind.RelativeOrAbsolute))
                {
                    ModelState.AddModelError("Text", "Invalid Url");
                }
                else 
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            var html = new HtmlDocument();
                            html.Load(client.OpenRead(new UriBuilder(JsonConvert.DeserializeObject<string>(url)).Uri), Encoding.UTF8);

                            input.Text = html.DocumentNode.InnerText;
                            allText = html.Text;
                            var metaTags = html.DocumentNode.SelectNodes("//meta");
                            foreach (var node in metaTags)
                            {
                                metaTagsBuilder.Append(node.GetAttributeValue("content", ""));
                            }
                        }
                    } catch (Exception ex)
                    {
                        model.Error = ex.Message;
                        ModelState.AddModelError("Text", ex.Message);
                    }
                }
            }

            var words = new Dictionary<string, int>();
            if (!string.IsNullOrEmpty(input.Text) && ModelState.IsValid)
            {
                words = StopwordHelper.RemoveStopwordsAndGetCount(input.Text);

                if (input.GetWordCounts) model.Words = words.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, y => y.Value);

                if (input.GetWordCountsOfMetaTags)
                {
                    var metaTagsDict = StopwordHelper.RemoveStopwordsAndGetCount(metaTagsBuilder.ToString());
                    model.MetaTagWords = words.Where(x => metaTagsDict.ContainsKey(x.Key))
                                        .OrderByDescending(x => x.Value).ToDictionary(x => x.Key, y => y.Value);
                }

                if (input.GetLinksCount)
                {
                    model.ExternalLinks = new Dictionary<string, int>();
                    foreach (Match item in Regex.Matches(allText, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                    {
                        if(input.SelectedTextType == Models.AnalyserModel.TextType.Url)
                        {
                            var host = new UriBuilder(JsonConvert.DeserializeObject<string>(url)).Uri.Host;
                            if (new UriBuilder(item.Value).Uri.Host == host) continue;
                        }
                       
                        if (model.ExternalLinks.ContainsKey(item.Value))
                        {
                            model.ExternalLinks[item.Value] = ++model.ExternalLinks[item.Value];
                            continue;
                        }
                        model.ExternalLinks.Add(item.Value, 1);

                    }
                }
            }
            return View(model);
        }
    }
}