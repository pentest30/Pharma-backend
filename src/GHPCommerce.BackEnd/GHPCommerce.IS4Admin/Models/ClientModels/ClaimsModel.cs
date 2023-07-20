namespace GHPCommerce.IS4Admin.Models.ClientModels
{
    using System.Collections.Generic;
    using System.Linq;
    using IdentityServer4.EntityFramework.Entities;

    public class ClaimsModel : ClaimModel
    {
        public List<ClaimModel> Claims { get; set; }

        public static ClaimsModel FromEntity(Client client)
        {
            var clientModel = ClientModel.FromEntity(client);

            return new ClaimsModel
            {
                Client = clientModel,
                Claims = client.Claims?.Select(x => FromEntity(x))?.ToList(),
            };
        }
    }
}
