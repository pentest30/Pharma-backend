using System;
using GHPCommerce.Domain.Domain.Catalog;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GHPCommerce.Application.Catalog.Products.DTOs
{
    public class ProductDto
    {
        public ProductDto(Product product)
        {
            FullName = product.FullName;
            Code = product.Code;
            State = product.ProductState;
            Description = product.Description;
            ManufacturerName = product.Manufacturer?.Name;
            ProductState = ConvertProductState(product.ProductState);
            ProductClassName = product.ProductClass?.Name;
            Quota = product.Quota;
            if (product.ProductClassId != null)
                ProductClassId = product.ProductClassId.Value;
            Id = product.Id;
        }

        private string ConvertProductState(ProductState state)
        {
            if (state ==Domain.Domain.Catalog.ProductState.Valid) return "Validé";
            if (state == Domain.Domain.Catalog.ProductState.Deactivated) return "Désactivé";
            return state == Domain.Domain.Catalog.ProductState.Draft ? "Brouillon" : "";
        }

        public Guid Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public string RegistrationNumber { get; set; }
        public Guid ProductClassId { get; set; }
        public string ManufacturerName { get; set; }
        public string ProductClassName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public string ProductState { get; set; }
        public bool Quota { get; set; }
        public ProductState State { get; set; }
    }
}
