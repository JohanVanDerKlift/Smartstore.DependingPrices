using SmartStore.Services.DataExchange.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.DependingPrices.Domain
{
    public class SampleDataColumn : IDataColumn
    {
        public SampleDataColumn(string name, Type type)
        {
            Guard.NotEmpty(name, nameof(name));
            Guard.NotNull(type, nameof(type));

            this.Name = name;
            this.Type = type;
        }

        public string Name
        {
            get;
            private set;
        }

        public Type Type
        {
            get;
            private set;
        }
    }
}