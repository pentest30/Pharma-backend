namespace GHPCommerce.IS4Admin.Models.ClientModels
{
    using System.Collections.Generic;
    using System.Linq;
    using IdentityServer4.EntityFramework.Entities;

    public class PropertiesModel : PropertyModel
    {
        public List<PropertyModel> Properties { get; set; }

        public static PropertiesModel FromEntity(Client client)
        {
            var clientModel = ClientModel.FromEntity(client);
            return new PropertiesModel
            {
                Client = clientModel,
                Properties = client.Properties?.Select(x => new PropertyModel
                {
                    Id = x.Id,
                    Key = x.Key,
                    Value = x.Value,
                    Client = clientModel,
                })?.ToList(),
            };
        }
    }
}
