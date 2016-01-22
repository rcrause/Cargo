using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// Represents a collection of content for a specific <see cref="IContentContext"/>.
    /// </summary>
    public class ContentView : IDisposable
    {
        private SHA1Managed _sha = new SHA1Managed();
        private Encoding _utf8 = Encoding.UTF8;

        /// <summary>
        /// Create a new <see cref="ContentView"/> with given content, a content context, and a data source.
        /// This <see cref="ContentView"/> owns the data source and will dispose it when disposed.
        /// </summary>
        /// <param name="content">The initial content for the view.</param>
        /// <param name="contentContext">The content context this view represents.</param>
        /// <param name="dataSource">The data source to use.</param>
        public ContentView(IEnumerable<ContentItem> content, IContentContext contentContext, ICargoDataSource dataSource)
        {
            ContentContext = contentContext;
            Content = content.ToDictionary(x => x.Key);
            DataSource = dataSource;
        }
        
        /// <summary>
        /// The content context for this view.
        /// </summary>
        public IContentContext ContentContext { get; private set; }

        /// <summary>
        /// The content for this content view.
        /// </summary>
        public IDictionary<string, ContentItem> Content { get; private set; }

        /// <summary>
        /// The data source for content.
        /// </summary>
        public ICargoDataSource DataSource { get; private set; }

        /// <summary>
        /// Returns a tokenized content item for use when the Cargo edit javascript is present to transform it.
        /// </summary>
        /// <param name="key">The key for the content item.</param>
        /// <param name="defaultContent">The default content to use if the content item does not exist yet.</param>
        public string GetTokenizedContent(string key, string defaultContent)
        {
            var contentItem = GetContentItem(key, defaultContent);
            return Tokenize(contentItem.Id, contentItem.Content);
        }

        /// <summary>
        /// Returns a tokenized content item using a derived key, for use when the Cargo edit javascript is present to transform it.
        /// </summary>
        /// <param name="defaultContent">The default content to use if the content item does not exist yet.</param>
        public string GetTokenizedContent(string defaultContent)
        {
            return GetTokenizedContent(ComputeHash(defaultContent), defaultContent);
        }

        /// <summary>
        /// Returns the content for the content item with the given key.
        /// </summary>
        /// <param name="key">The key for the content item.</param>
        /// <param name="defaultContent">The default content to use if the content item does not exist yet.</param>
        public string GetContent(string key, string defaultContent)
        {
            return GetContentItem(key, defaultContent)?.Content;
        }

        /// <summary>
        /// Returns the content for the content item using a key derived from the default content.
        /// </summary>
        /// <param name="defaultContent">The default content to use if the content item does not exist yet.</param>
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
            content = WebUtility.HtmlEncode(content);
            if (content.Contains('`')) content = content.Replace("`", "``");
            if (content.Contains('~')) content = content.Replace("~", "`t");
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
        
        /// <summary>
        /// Disposes this instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> when called from <see cref="Dispose()"/> method. <c>false</c> when called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if(DataSource!= null)
                {
                    DataSource.Dispose();
                    DataSource = null;
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
