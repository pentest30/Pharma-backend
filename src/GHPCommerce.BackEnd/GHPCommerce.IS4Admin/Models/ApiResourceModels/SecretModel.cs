﻿namespace GHPCommerce.IS4Admin.Models.ApiResourceModels
{
    using System;
    using IdentityServer4.EntityFramework.Entities;

    public class SecretModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public DateTime? Expiration { get; set; }
        public string Type { get; set; }
        public string HashType { get; set; }
        public DateTime Created { get; set; }
        public int ApiResourceId { get; set; }
        public string ApiResourceName { get; set; }
        public ApiResourceModel ApiResource { get; set; }

        public static SecretModel FromEntity(Secret secret)
        {
            return new SecretModel
            {
                Id = secret.Id,
                Description = secret.Description,
                Value = secret.Value,
                Expiration = secret.Expiration,
                Type = secret.Type,
                Created = secret.Created,
               
            };
        }
    }
}
