using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo
{
    /// <summary>
    /// Provides extensions for Cargo to do with Entity Framework.
    /// </summary>
    public static class CargoEntityFrameworkExtensions
    {
        /// <summary>
        /// Map the entities needed for this <see cref="DbContext"/> to be used by an
        /// <see cref="EntityFrameworkCargoDataSource"/>. 
        /// </summary>
        /// <param name="model">The <see cref="DbModelBuilder"/> passed to <see cref="DbContext.OnModelCreating(DbModelBuilder)"/>.</param>
        /// <param name="prefix">The prefix to add before each created table.</param>
        /// <param name="schema">The schema for each created table.</param>
        public static void MapCargoContent(this DbModelBuilder model, string prefix = null, string schema = null)
        {
            var contentItem = model.Entity<ContentItem>();
            ToTableSmart(contentItem, prefix + "ContentItems", schema);
            
            contentItem.Property(x => x.Key).IsUnicode().IsRequired().HasMaxLength(200);
            contentItem.Property(x => x.Location).IsUnicode().IsRequired().HasMaxLength(200);
            contentItem.Property(x => x.Content).IsUnicode().IsRequired();
            contentItem.Property(x => x.OriginalContent).IsUnicode();

            //making this the primary key has it's drawbacks, but these
            //should be offset by caching.
            contentItem.HasKey(x => new { x.Location, x.Key });

            //we ignore "id" because it's derived from key and location.
            contentItem.Ignore(x => x.Id);
        }
        
        private static void ToTableSmart<TEntityType>(EntityTypeConfiguration<TEntityType> entity, string tableName, string schemaName)
            where TEntityType : class
        {
            if (string.IsNullOrEmpty(schemaName)) entity.ToTable(tableName);
            else entity.ToTable(tableName, schemaName);
        }
    }
}
