using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public class ContentCollection
    {
        private SHA1Managed _sha = new SHA1Managed();
        private Encoding _utf8 = Encoding.UTF8;

        public ContentCollection(IEnumerable<ContentItem> content, IContentContext contentContext)
        {
            ContentContext = contentContext;
            Content = content.ToLookup(x => x.Key);
        }
        
        public IContentContext ContentContext { get; private set; }

        public ILookup<string, ContentItem> Content { get; private set; }

        public string GetTokenizedContent(string key, string defaultContent)
        {
            var content = GetContent(key, defaultContent);
            return Tokenize(key, content);
        }

        public string GetTokenizedContent(string defaultContent)
        {
            return GetTokenizedContent(ComputeHash(defaultContent), defaultContent);
        }

        public string GetContent(string key, string defaultContent)
        {
            var contentItem = GetContentItem(key);
            if (contentItem != null) return contentItem.Content;
            else return defaultContent;
        }

        public string GetContent(string defaultContent)
        {
            return GetContent(ComputeHash(defaultContent), defaultContent);
        }

        private ContentItem GetContentItem(string key)
        {
            IEnumerable<ContentItem> itemsWithKey = Content[key];
            return itemsWithKey.FirstOrDefault();
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
