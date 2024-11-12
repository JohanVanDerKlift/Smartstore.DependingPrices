using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;
using SmartStore.DependingPrices.Data;
using SmartStore.DependingPrices.Domain;
using SmartStore.DependingPrices.Services;
using SmartStore.Services.Catalog;
using SmartStore.Web.Controllers;

namespace SmartStore.DependingPrices
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            if (isActiveModule)
            {
                builder.RegisterType<DependingPricesService>().As<IDependingPricesService>().InstancePerRequest();

                builder.RegisterType<MyPriceCalculationService>().As<IPriceCalculationService>().InstancePerRequest();

				//register named context
				builder.Register<IDbContext>(c => new DependingPricesObjectContext(DataSettings.Current.DataConnectionString))
                    .Named<IDbContext>(DependingPricesObjectContext.ALIASKEY)
                    .InstancePerRequest();

                builder.Register(c => new DependingPricesObjectContext(DataSettings.Current.DataConnectionString))
                    .InstancePerRequest();

                //override required repository with our custom context
                builder.RegisterType<EfRepository<DependingPricesRecord>>()
                    .As<IRepository<DependingPricesRecord>>()
                    .WithParameter(ResolvedParameter.ForNamed<IDbContext>(DependingPricesObjectContext.ALIASKEY))
                    .InstancePerRequest();
            }
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
