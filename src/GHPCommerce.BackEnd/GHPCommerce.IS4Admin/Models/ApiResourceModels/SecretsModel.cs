﻿namespace GHPCommerce.IS4Admin.Models.ApiResourceModels
{
    using System.Collections.Generic;
    using System.Linq;
    using IdentityServer4.Models;

    public class SecretsModel : SecretModel
    {
        public List<SecretModel> Secrets { get; set; }
        public List<string> TypeList { get; } = new List<string>
        {
            "SharedSecret",
            "X509Thumbprint",
            "X509Name",
            "X509CertificateBase64",
        };
        public List<string> HashTypes { get; } = new List<string>
        {
            "Sha256",
            "Sha512",
        };

        public void HashSecret()
        {
            if (Type != "SharedSecret")
            {
                return;
            }

            if (HashType == "Sha256")
            {
                Value = Value.Sha256();
            }
            else if (HashType == "Sha512")
            {
                Value = Value.Sha512();
            }
        }

        public void UpdateEntity(IdentityServer4.EntityFramework.Entities.Secret entity)
        {
            entity.Description = Description;
            entity.Value = Value;
            entity.Expiration = Expiration;
            entity.Type = Type;
        }

        public static SecretsModel FromEntity(IdentityServer4.EntityFramework.Entities.ApiResource apiResource)
        {
            return new SecretsModel
            {
                ApiResourceId = apiResource.Id,
                ApiResourceName = apiResource.Name,
                Secrets = apiResource.Secrets?.Select(x => FromEntity(x))?.ToList(),
            };
        }
    }
}
