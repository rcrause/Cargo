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

        public ContentCollection(IEnumerable<ContentItem> content, ContentContext contentContext)
        {
            ContentContext = contentContext;
            Content = content.ToLookup(x => x.Key);
        }
        
        public ContentContext ContentContext { get; private set; }

        public ILookup<string, ContentItem> Content { get; private set; }

        public string GetLocalizedStringToken(string key, string originalContent)
        {
            var content = GetLocalizedString(key, originalContent);
            return GetToken(key, content);
        }

        public string GetLocalizedStringToken(string originalContent)
        {
            return GetLocalizedStringToken(ComputeHash(originalContent), originalContent);
        }

        public string GetLocalizedString(string key, string originalContent)
        {
            var contentItem = GetContentItem(key);
            if (contentItem != null) return contentItem.Content;
            else return originalContent;
        }

        public string GetLocalizedString(string originalContent)
        {
            return GetLocalizedString(ComputeHash(originalContent), originalContent);
        }

        private ContentItem GetContentItem(string key)
        {
            if (Content.Contains(key))
            {
                IEnumerable<ContentItem> contentItems = Content[key];
                //TODO: evaluate conditions
                ContentItem mostRelevant = contentItems.First();
                return mostRelevant;
            }
            else
            {
                return null;
            }
        }

        private string GetToken(string key, string content)
        {
            if (content.Contains('\\')) content = content.Replace("\\", "\\\\");
            if (content.Contains('^')) content = content.Replace("^", "\\^");
            return string.Format("~{0}#{1}^", key, content);
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
