using Cargo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace CargoExample
{
    public abstract class ViewBase<T> : WebViewPage<T>
    {
        private IContentContext _contentContext;
        private ContentView _cargoContent;
        private CargoEngine _cargoEngine;

        public CargoEngine CargoEngine { get { return _cargoEngine; } }

        public IContentContext CargoContext { get { return _contentContext; } }

        public ContentView Cargo { get { return _cargoContent; } }

        public override void ExecutePageHierarchy()
        {
            _cargoEngine = Startup.CargoEngine;
            _contentContext = new ContentContext
            {
                Locale = this.Culture,
                Location = this.VirtualPath.Replace("~", ""),
                EditingEnabled = true //Request.IsAuthenticated
            };

            _cargoContent = _cargoEngine.GetContent(_contentContext);

            base.ExecutePageHierarchy();
        }

        public IHtmlString Content(string key, string originalContent)
        {
            if (CargoContext.EditingEnabled)
            {
                return new HtmlString(Cargo.GetTokenizedContent(key, originalContent));
            }
            else
            {
                return new HtmlString(Cargo.GetContent(key, originalContent));
            }
        }

        public IHtmlString Content(string originalContent)
        {
            if (CargoContext.EditingEnabled)
            {
                return new HtmlString(Cargo.GetTokenizedContent(originalContent));
            }
            else
            {
                return new HtmlString(Cargo.GetContent(originalContent));
            }
        }
    }
}