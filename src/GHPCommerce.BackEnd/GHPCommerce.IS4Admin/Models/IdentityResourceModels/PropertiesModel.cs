namespace GHPCommerce.IS4Admin.Models.IdentityResourceModels
{
    using System.Collections.Generic;
    using System.Linq;
    using IdentityServer4.EntityFramework.Entities;

    public class PropertiesModel
    {
        public int IdentityResourceId { get; set; }
        public string IdentityResourceName { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public List<IdentityResourcePropertyModel> Properties { get; set; }

        public static PropertiesModel FromEntity(IdentityResource identityResource)
        {
            return new PropertiesModel
            {
                IdentityResourceId = identityResource.Id,
                IdentityResourceName = identityResource.Name,
                Properties = identityResource.Properties?.Select(x => new IdentityResourcePropertyModel
                {
                    Id = x.Id,
                    Key = x.Key,
                    Value = x.Value,
                })?.ToList(),
            };
        }
    }
}
