using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// Represents a collection of content for a specific <see cref="IContentContext"/>.
    /// </summary>
    public class ContentView
    {
        private SHA1Managed _sha = new SHA1Managed();
        private Encoding _utf8 = Encoding.UTF8;

        public ContentView(IEnumerable<ContentItem> content, IContentContext contentContext, ICargoDataSource dataSource)
        {
            ContentContext = contentContext;
            Content = content.ToDictionary(x => x.Key);
            DataSource = dataSource;
        }
        
        public IContentContext ContentContext { get; private set; }
        public IDictionary<string, ContentItem> Content { get; private set; }
        public ICargoDataSource DataSource { get; private set; }

        public string GetTokenizedContent(string key, string defaultContent)
        {
            var contentItem = GetContentItem(key, defaultContent);
            return Tokenize(contentItem.Id, contentItem.Content);
        }

        public string GetTokenizedContent(string defaultContent)
        {
            return GetTokenizedContent(ComputeHash(defaultContent), defaultContent);
        }

        public string GetContent(string key, string defaultContent)
        {
            return GetContentItem(key, defaultContent)?.Content;
        }

        public string GetContent(string defaultContent)
        {
            return GetContent(ComputeHash(defaultContent), defaultContent);
        }

        private ContentItem GetContentItem(string key, string defaultContent)
        {
            ContentItem item;
            if(!Content.TryGetValue(key, out item))
            {
                item = DataSource.GetOrCreate(ContentContext.Location, key, defaultContent);
            }
            return item;
        }

        private string Tokenize(string key, string content)
        {
            //TODO:figure out how to not have to do this:
            if (content.Contains('~')) content = content.Replace("~", "");
            return string.Format("~{0}#{1}~", key, content);
        }

        private string ComputeHash(string originalContent)
        {
            byte[] textBytes = _utf8.GetBytes(originalContent);
            byte[] keyBytes = _sha.ComputeHash(textBytes);
            string key = UrlTokenEncode(keyBytes);
            return key;
        }

        private string UrlTokenEncode(byte[] keyBytes)
        {
            return Convert.ToBase64String(keyBytes)
                .Replace("=", "")
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
