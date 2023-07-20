using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.Brands.DTOs;
using GHPCommerce.Application.Catalog.Dosages.DTOs;
using GHPCommerce.Application.Catalog.Forms.DTOs;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;
using GHPCommerce.Application.Catalog.INNs.DTOs;
using GHPCommerce.Application.Catalog.Lists.DTOs;
using GHPCommerce.Application.Catalog.Manufacturers.DTOs;
using GHPCommerce.Application.Catalog.Packaging.DTOs;
using GHPCommerce.Application.Catalog.PharmacologicalClasses.DTOs;
using GHPCommerce.Application.Catalog.PickingZones.DTOs;
using GHPCommerce.Application.Catalog.ProductClasses.DTOs;
using GHPCommerce.Application.Catalog.Products.Commands;
using GHPCommerce.Application.Catalog.Products.DTOs;
using GHPCommerce.Application.Catalog.RequestLogs;
using GHPCommerce.Application.Catalog.TaxGroups.DTOs;
using GHPCommerce.Application.Catalog.TherapeuticClasses.DTOs;
using GHPCommerce.Application.Catalog.TransactionTypes.Commands;
using GHPCommerce.Application.Catalog.TransactionTypes.DTOs;
using GHPCommerce.Application.Membership.Users.DTOs;
using GHPCommerce.Application.Tiers.BankAccounts.DTOs;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Application.Tiers.Suppliers.Commands;
using GHPCommerce.Application.Tiers.Suppliers.DTOs;
using GHPCommerce.Application.Tiers.Suppliers.Events;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Core.Shared.Contracts.PickingZone.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Shared;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.WebApi.Mappings
{
    public class ApiMappings : Profile
    {
    }
    public class ProductClassDtoMappingConfiguration : Profile
    {
        public ProductClassDtoMappingConfiguration()
        {
            CreateMap<ProductClassDto, ProductClass>().ReverseMap();
            CreateMap<ProductClass, AllowedProductClass>()
                .ForMember(x=>x.ProductClassId, o=>o.MapFrom(x=>x.Id)).ReverseMap();
        }
    }
    public class OrganizationDtoV2MappingConfiguration : Profile
    {
        public OrganizationDtoV2MappingConfiguration()
        {
            CreateMap<OrganizationDtoV2, Organization>().ReverseMap();
            CreateMap<Organization, OrganizationDtoV2>()
                .ForMember(x => x.OrganizationActivity, m => m.MapFrom(o => SetActivity(o.Activity)))
                .ReverseMap();
        }
        private string SetActivity(OrganizationActivity oActivity)
        {
            switch (oActivity)
            {
                case OrganizationActivity.Doctor: return "Médecin";
                case OrganizationActivity.Hospital: return "Hôpital";
                case OrganizationActivity.Pharmacist: return "Officine";
                case OrganizationActivity.Wholesaler: return "Répartiteur";

            }
            return String.Empty;

        }
    }
    public class BrandDtoConfigurationMapping : Profile
    {
        public BrandDtoConfigurationMapping()
        {
            CreateMap<BrandDto, Brand>().ReverseMap();
        }
    }
    public class DosageDtoConfigurationMapping : Profile
    {
        public DosageDtoConfigurationMapping()
        {
            CreateMap<DosageDto, Dosage>().ReverseMap();
        }
    }
    public class FormDtoConfigurationMapping : Profile
    {
        public FormDtoConfigurationMapping()
        {
            CreateMap<FormDto, Form>().ReverseMap();
        }
    }
    public class InnDtoConfigurationMapping : Profile
    {
        public InnDtoConfigurationMapping()
        {
            CreateMap<InnDto, INN>().ReverseMap();
        }
    }
    public class ListDtoConfigurationMapping : Profile
    {
        public ListDtoConfigurationMapping()
        {
            CreateMap<ListDto, List>().ReverseMap();
        }
    }
    public class ManufacturerDtoMappingConfiguration : Profile
    {
        public ManufacturerDtoMappingConfiguration()
        {
            CreateMap<ManufacturerDto, Manufacturer>().ReverseMap();
        }
    }
    public class AddressDtoMappingConfiguration : Profile
    {
        public AddressDtoMappingConfiguration()
        {
            CreateMap<AddressDto, Address>().ReverseMap();
        }
    }
    public class PhoneNumberDtoMappingConfiguration : Profile
    {
        public PhoneNumberDtoMappingConfiguration()
        {
            CreateMap<PhoneNumberDto, PhoneNumber>().ReverseMap();
        }
    }
    public class EmailDtoMappingConfiguration : Profile
    {
        public EmailDtoMappingConfiguration()
        {
            CreateMap<EmailDto, EmailModel>().ReverseMap();
        }
    }
    public class PackagingDtoConfigurationMapping : Profile
    {
        public PackagingDtoConfigurationMapping()
        {
            CreateMap<PackagingDto, Packaging>().ReverseMap();
        }
    }
    public class PharmacologicalClassDtoConfigurationMapping : Profile
    {
        public PharmacologicalClassDtoConfigurationMapping()
        {
            CreateMap<PharmacologicalClassDto, PharmacologicalClass>().ReverseMap();
        }
    }
    public class PickingZoneDtoConfigurationMapping : Profile
    {
        public PickingZoneDtoConfigurationMapping()
        {
            CreateMap<PickingZone,PickingZoneDto>()
                .ForMember(x=>x.GroupName , o=> o.MapFrom(z=> z.ZoneGroup !=null? z.ZoneGroup.Name : string.Empty)).ReverseMap();
            CreateMap<PickingZone, PickingZoneDtoV1>().ReverseMap();

        }
    }
    public class ProductDtoConfigurationMapping : Profile
    {
        public ProductDtoConfigurationMapping()
        {
            CreateMap<Product, ProductDtoV2>()
                .ForMember(s => s.SHP, o => o.MapFrom(d => d.List.SHP))
                .ForMember(s => s.HasQuota, o => o.MapFrom(d => d.Quota))

                .ReverseMap();
        }
    }
    public class TaxGroupDtoMappingConfiguration : Profile
    {
        public TaxGroupDtoMappingConfiguration()
        {
            CreateMap<TaxGroupDto, TaxGroup>().ReverseMap();
        }
    }
    public class TherapeuticClassDtoConfigurationMapping : Profile
    {
        public TherapeuticClassDtoConfigurationMapping()
        {
            CreateMap<TherapeuticClassDto, TherapeuticClass>().ReverseMap();
        }
    }
    public class LogRequestConfigurationMapping : Profile
    {
        public LogRequestConfigurationMapping()
        {
            CreateMap<LogRequest, LogApiModel>().ReverseMap();
        }
    }
    public class UserDtoConfigurationMapping : Profile
    {
        public UserDtoConfigurationMapping()
        {
            CreateMap<UserDto, User>().ReverseMap();
        }
    }

    public class CreateDraftProductCommandValidatorConfigurationMapping : Profile
    {
        public CreateDraftProductCommandValidatorConfigurationMapping()
        {
            CreateMap<CreateDraftProductCommand, Product>().ReverseMap();
        }
    }

   
    public class CreateBankingAccountCommandConfigMapping : Profile
    {
        public CreateBankingAccountCommandConfigMapping()
        {
            CreateMap<BankAccountDto, BankAccount>().ReverseMap();
        }
    }
    public class OrganizationDtoV1MappingConfiguration : Profile
    {
        public OrganizationDtoV1MappingConfiguration()
        {
            CreateMap<Organization, OrganizationDtoV1>()
                .ForMember(x => x.OrganizationActivity, m => m.MapFrom(o => SetActivity(o.Activity)))
                .ForMember(x => x.OrganizationState, m => m.MapFrom(o => SetStatus(o.OrganizationStatus))
                
                ).ForMember(x => x.EstablishmentDateShort, m => m.MapFrom(o => o.EstablishmentDate.HasValue? o.EstablishmentDate.Value.Date.ToShortDateString() : ""))
                ;
        }

        private string SetStatus(OrganizationStatus oOrganizationStatus)
        {
            switch (oOrganizationStatus)
            {
                case OrganizationStatus.Blocked: return "Bloquée";
                case OrganizationStatus.Active: return "Active";


            }
            return String.Empty;
        }

        private string SetActivity(OrganizationActivity oActivity)
        {
            switch (oActivity)
            {
                case OrganizationActivity.Doctor: return "Médecin";
                case OrganizationActivity.Hospital: return "Hôpital";
                case OrganizationActivity.Pharmacist: return "Officine";
                case OrganizationActivity.Wholesaler: return "Répartiteur";

            }
            return String.Empty;

        }
    }

    public class SupplierCustomerDtoConfigurationMapping : Profile
    {
        public SupplierCustomerDtoConfigurationMapping()
        {
            CreateMap<SupplierCustomer, SupplierCustomerDto>()
                .ForMember(x => x.OrganizationName, o =>
                    o.MapFrom(org => org.Customer.Organization != null ? org.Customer.Organization.Name : string.Empty))
                .ForMember(o => o.AllowedProductClasses, x => x.MapFrom(p => p.PermittedProductClasses))
                .ForMember(o => o.TaxGroupId, x => x.MapFrom(p => p.TaxGroup.Id))
                .ForMember(x => x.OrganizationId, o => o.MapFrom(org => org.Customer.OrganizationId))
                .ForMember(x => x.CreatedBy, o => o.MapFrom(org => org.Customer.CreatedBy))
                .ForMember(x => x.UpdatedBy, o => o.MapFrom(org => org.Customer.UpdatedBy))
                .ForMember(x => x.DeliveryTypeDescription, o => o.MapFrom(org => SetDeliveryType(org.DeliveryType)))
                .ForMember(x => x.OrganizationStatusDescription, o => o.MapFrom(org => SetStatus(org.OrganizationStatus)))
                .ForMember(x=> x.Debt, o=> o.MapFrom(org => org.Dept))
                .ForMember(x=> x.Sector, o => o.MapFrom(org => org.Sector.Name))
                .ForMember(x => x.MonthlyObjective, o=> o.MapFrom(org => org.MonthlyObjective))
                .ForMember(x => x.DefaultSalesGroupName, o => o.MapFrom(org => org.SalesPersonName))
                .ForMember(x => x.OrganizationGroupCode, o => o.MapFrom(org => org.Customer.Organization.OrganizationGroupCode))
                .ForMember(x => x.LimitCredit, o=> o.MapFrom(org => org.LimitCredit))
                .ForMember(x => x.PaymentDeadline, o => o.MapFrom(org => org.PaymentDeadline))
                .ForMember(x => x.PaymentMode, o => o.MapFrom(org => org.PaymentMode))
                .ReverseMap();
        }

        private string SetDeliveryType(DeliveryType deliveryType)
        {
            switch (deliveryType)
            {
                case DeliveryType.Delivery: return "Livré au client";
                case DeliveryType.PickUp: return "Récupéré par le client";

            }
            return String.Empty;
        }

        private string SetStatus(OrganizationStatus status)
        {
            switch (status)
            {
                case OrganizationStatus.Active: return "Actif";
                case OrganizationStatus.Blocked: return "Bloqué";

            }
            return String.Empty;
        }
    }
    public class SupplierConfigMapping : Profile
    {
        public SupplierConfigMapping()
        {
            CreateMap<CreateSupplierCommand, SupplierCreatedEvent>().ReverseMap();
            CreateMap<SupplierCreatedEvent, SupplierCustomer>().ReverseMap();
            CreateMap<Supplier, SupplierDto>().ReverseMap();

        }
    }

    public class InnCodeDosageConfigMapping : Profile
    {
        public InnCodeDosageConfigMapping()
        {
            CreateMap<INNCodeDosage, InnCodeDosageDto>().ReverseMap();
        }
    }

    public class TransactionTypeConfigurationMapping : Profile
    {
        public TransactionTypeConfigurationMapping()
        {
            CreateMap<CreateTransactionTypeCommand, TransactionType>().ReverseMap();
            CreateMap<UpdateTransactionTypeCommand, ZoneGroup>().ReverseMap();

            CreateMap<TransactionType, TransactionTypeDto>().ReverseMap();
            CreateMap<TransactionTypeDtoV1, TransactionType>().ReverseMap();


        }
    }
}
