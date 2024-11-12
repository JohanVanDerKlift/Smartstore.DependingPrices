using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using SmartStore.Core;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.DependingPrices.Data.Migrations;

namespace SmartStore.DependingPrices.Data
{
	/// <summary>
	/// Object context
	/// </summary>
	public class DependingPricesObjectContext : ObjectContextBase
	{
        public const string ALIASKEY = "sm_object_context_depending_prices";

		static DependingPricesObjectContext()
		{
            var initializer = new MigrateDatabaseInitializer<DependingPricesObjectContext, Configuration>
            {
                TablesToCheck = new[] { "DependingPrices" }
            };
            Database.SetInitializer(initializer);
		}

		/// <summary>
		/// For tooling support, e.g. EF Migrations
		/// </summary>
		public DependingPricesObjectContext()
			: base()
		{
		}

        public DependingPricesObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY) 
		{
		}


		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
            modelBuilder.Configurations.Add(new DependingPricesRecordMap());

			//disable EdmMetadata generation
			//modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
			base.OnModelCreating(modelBuilder);
		}

	}
}