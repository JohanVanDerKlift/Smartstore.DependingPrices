using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Plugin.Widgets.DependingPrices
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.DependingPrices",
                 "Plugins/DependingPrices/{action}",
                 new { controller = "DependingPrices", action = "Configure" },
                 new[] { "SmartStore.DependingPrices.Controllers" }
            )
			.DataTokens["area"] = "SmartStore.DependingPrices";
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
