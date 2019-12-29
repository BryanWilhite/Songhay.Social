using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Models;
using Songhay.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Songhay.Social.Activities
{
    public class DeliciousActivity : IActivity
    {
        public static ICollection<DeliciousLink> LoadDeliciousLinks(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new NullReferenceException("The expected path is not here.");
            if (!File.Exists(path)) throw new FileLoadException("The expected file is not here.");
            var lines = File.ReadAllLines(path);
            var deliciousLinks = new List<DeliciousLink>();
            lines.Select((o, i) => new { Line = o, LineNumber = i }).ForEachInEnumerable(anonym =>
            {
                if (anonym.Line.StartsWith("<DT>"))
                {
                    var line = anonym.Line.Remove(0, 4)
                        .Replace("&amp;", "&")
                        .Replace("&", "&amp;");
                    var innerXml = HtmlUtility.GetInnerXml(line, "A");
                    line = line.Replace(innerXml, XmlUtility.ExpandSpecialChars(innerXml));
                    var A = XElement.Parse(line);
                    var link = new DeliciousLink
                    {
                        AddDate = FrameworkTypeUtility.ConvertDateTimeFromUnixTime(Convert.ToDouble(A.Attribute("ADD_DATE").Value)),
                        Href = new Uri(A.Attribute("HREF").Value, UriKind.Absolute),
                        IsPrivate = FrameworkTypeUtility.ParseBoolean(A.Attribute("PRIVATE").Value).GetValueOrDefault(),
                        Tags = A.Attribute("TAGS").Value,
                        Title = A.Value
                    };
                    deliciousLinks.Add(link);
                }
                else if (anonym.Line.StartsWith("<DD>"))
                {
                    var previousLink = deliciousLinks.Last();
                    previousLink.DD = anonym.Line.Remove(0, 4);
                }
            });

            return deliciousLinks.Take(100).ToList();
        }

        public static ICollection<DeliciousLink> LoadDeliciousLinks(string path, string tags, string searchText)
        {
            var links = LoadDeliciousLinks(path)
                .Where(i =>
                {
                    if (string.IsNullOrEmpty(tags) && string.IsNullOrEmpty(i.Tags)) return true;
                    else if (string.IsNullOrEmpty(tags)) return true;
                    else if (string.IsNullOrEmpty(i.Tags)) return false;
                    else
                    {
                        var test = false;
                        tags.Split(',').ForEachInEnumerable(j => test = i.Tags.Contains(j));
                        return test;
                    }
                })
                .Where(i =>
                {
                    if (string.IsNullOrEmpty(searchText)) return true;
                    else
                    {
                        var test = !string.IsNullOrEmpty(i.Title) && i.Title.Contains(searchText) ||
                            (i.Href != null) && i.Href.AbsoluteUri.Contains(searchText) ||
                            !string.IsNullOrEmpty(i.DD) && (i.DD.Contains(searchText));
                        return test;
                    }
                });
            return links.ToList();
        }

        public string DisplayHelp(ProgramArgs args)
        {
            throw new NotImplementedException();
        }

        public void Start(ProgramArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
