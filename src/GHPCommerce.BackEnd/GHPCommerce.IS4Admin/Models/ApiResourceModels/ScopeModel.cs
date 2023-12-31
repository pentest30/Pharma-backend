﻿namespace GHPCommerce.IS4Admin.Models.ApiResourceModels
{
    using System.Collections.Generic;
    using IdentityServer4.EntityFramework.Entities;

    public class ScopeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public bool Emphasize { get; set; }
        public bool ShowInDiscoveryDocument { get; set; }
        public List<ApiScopeClaim> UserClaims { get; set; }
        public string UserClaimsItems { get; set; }
        public int ApiResourceId { get; set; }
        public string ApiResourceName { get; set; }
        public ApiResource ApiResource { get; set; }

        public static ScopeModel FromEntity(ApiScope apiScope)
        {
            return new ScopeModel
            {
                Id = apiScope.Id,
                Name = apiScope.Name,
                DisplayName = apiScope.DisplayName,
                Description = apiScope.Description,
                Required = apiScope.Required,
                Emphasize = apiScope.Emphasize,
                ShowInDiscoveryDocument = apiScope.ShowInDiscoveryDocument,
                UserClaims = apiScope.UserClaims,
                
            };
        }

        public void UpdateEntity(ApiScope entity)
        {
            entity.Name = Name;
            entity.DisplayName = DisplayName;
            entity.Description = Description;
            entity.Required = Required;
            entity.Emphasize = Emphasize;
            entity.ShowInDiscoveryDocument = ShowInDiscoveryDocument;
        }
    }
}
