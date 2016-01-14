using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    public static class CargoEntityFrameworkExtensions
    {
        public static void MapCargoContent(this DbModelBuilder model, string prefix = null, string schema = null)
        {
            var contentItem = ToTableSmart(model.Entity<ContentItem>(), prefix + "ContentItems", schema);
            contentItem.Property(x => x.Key).IsUnicode().IsRequired().HasMaxLength(200);
            contentItem.Property(x => x.Location).IsUnicode().IsRequired().HasMaxLength(200);
            contentItem.Property(x => x.Content).IsUnicode().IsRequired();

            //making this the primary key has it's drawbacks, but these
            //should be offset by caching.
            contentItem.HasKey(x => new { x.Location, x.Key });

            //we ignore "id" because it's derived from key and location.
            contentItem.Ignore(x => x.Id);
        }
        
        private static EntityTypeConfiguration<TEntityType> ToTableSmart<TEntityType>(EntityTypeConfiguration<TEntityType> entity, string tableName, string schemaName)
            where TEntityType : class
        {
            if (string.IsNullOrEmpty(schemaName)) return entity.ToTable(tableName);
            else return entity.ToTable(tableName, schemaName);
        }
    }
}
