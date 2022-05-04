using System.Collections.Generic;
using System.ComponentModel;

namespace Jasily.ViewModel.Internal
{
    internal struct PropertiesChangedData
    {
        private readonly RefreshPropertiesMapper _mapper;

        internal PropertiesChangedData(
            RefreshPropertiesMapper mapper, 
            IReadOnlyCollection<PropertyChangedEventArgs> properties)
        {
            this._mapper = mapper;
            this.Properties = properties;
        }

        public IReadOnlyCollection<PropertyChangedEventArgs> Properties { get; }
    }
}
