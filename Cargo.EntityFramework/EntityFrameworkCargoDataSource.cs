using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo.EntityFramework
{
    public class EntityFrameworkCargoDataSource : CargoDataSourceBase
    {
        private DbContext _dataContext;
        private DbSet<ContentItem> ContentItems { get { return _dataContext.Set<ContentItem>(); } }

        public EntityFrameworkCargoDataSource(DbContext dataContext)
        {
            _dataContext = dataContext;
        }

        public override ContentItem Get(string location, string key)
        {
            throw new NotImplementedException();
        }

        public override ICollection<ContentItem> GetAllContent()
        {
            throw new NotImplementedException();
        }

        public override ICollection<ContentItem> GetAllContentForLocation(string location)
        {
            throw new NotImplementedException();
        }

        public override ICollection<string> GetAllLocations()
        {
            throw new NotImplementedException();
        }

        public override ContentItem GetById(string id)
        {
            throw new NotImplementedException();
        }

        public override void Remove(IEnumerable<string> contentItemIds)
        {
            throw new NotImplementedException();
        }

        public override void SetByIdInternal(IEnumerable<KeyValuePair<string, string>> idContentPairs)
        {
            throw new NotImplementedException();
        }

        public override void SetInternal(IEnumerable<ContentItem> contentItems)
        {
            throw new NotImplementedException();
        }

        protected override ContentItem CreateInternal(string location, string key, string content)
        {
            throw new NotImplementedException();
        }
    }
}
