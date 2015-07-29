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
        private ContentContext _contentContext;
        private ContentCollection _cargoContent;
        private CargoEngine _cargoEngine;

        public CargoEngine CargoEngine { get { return _cargoEngine; } }

        public ContentContext CargoContext { get { return _contentContext; } }

        public ContentCollection Cargo { get { return _cargoContent; } }

        public override void ExecutePageHierarchy()
        {
            _cargoEngine = DependencyResolver.Current.GetService<CargoEngine>();
            _contentContext = new ContentContext
            {
                Locale = this.Culture,
                Locality = this.Request.RawUrl,
                Properties = new Dictionary<string, object>(),
                EditingEnabled = true //Request.IsAuthenticated
            };

            _cargoContent = _cargoEngine.GetContent(_contentContext);

            base.ExecutePageHierarchy();
        }

        public IHtmlString Content(string key, string originalContent)
        {
            if (CargoContext.EditingEnabled)
            {
                return new HtmlString(Cargo.GetLocalizedStringToken(key, originalContent));
            }
            else
            {
                return new HtmlString(Cargo.GetLocalizedString(key, originalContent));
            }
        }

        public IHtmlString Content(string originalContent)
        {
            if (CargoContext.EditingEnabled)
            {
                return new HtmlString(Cargo.GetLocalizedStringToken(originalContent));
            }
            else
            {
                return new HtmlString(Cargo.GetLocalizedString(originalContent));
            }
        }
    }
}