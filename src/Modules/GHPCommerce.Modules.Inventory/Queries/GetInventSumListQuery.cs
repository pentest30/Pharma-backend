using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Inventory.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Inventory.Queries
{
     [BindProperties]
    public class GetInventSumListQuery : CommonListQuery, ICommand<PagingResult<InventSumDto>>
    {
        
        private Guid? _productId;
        private string _productFullName;
        private string _productCode;
        private string _vendorBatchNumber;
        private string _internalBatchNumber;
        private DateTime? _productionDate;
        private DateTime? _expiryDate;
        private DateTime? _bestBeforeDate;
        private string _color;
        private string _size;
        private bool? _isPublic;
        private Guid? _siteId;
        private string _siteName;
        private Guid? _warehouseId;
        private string _warehouseName;
        private double? _minThresholdAlert;


        public GetInventSumListQuery(string term, string sort, int page, int pageSize, Guid? productId, string productFullName, string productCode, string vendorBatchNumber, string internalBatchNumber, DateTime? productionDate, DateTime? expiryDate, DateTime? bestBeforeDate, string color, string size, bool? isPublic, Guid? siteId, string siteName, Guid? warehouseId, string warehouseName, double? minThresholdAlert) : base(term, sort, page, pageSize)
        {
            _productId = productId;
            _productFullName = productFullName;
            _productCode = productCode;
            _vendorBatchNumber = vendorBatchNumber;
            _internalBatchNumber = internalBatchNumber;
            _productionDate = productionDate;
            _expiryDate = expiryDate;
            _bestBeforeDate = bestBeforeDate;
            _color = color;
            _size = size;
            _isPublic = isPublic;
            _siteId = siteId;
            _siteName = siteName;
            _warehouseId = warehouseId;
            _warehouseName = warehouseName;
            _minThresholdAlert = minThresholdAlert;
        }

        public GetInventSumListQuery()
        {
            
        }
        public Guid? ProductId
        {
            get => _productId;
            set => _productId = value;
        }

        public string ProductFullName
        {
            get => _productFullName;
            set => _productFullName = value;
        }

        public string ProductCode
        {
            get => _productCode;
            set => _productCode = value;
        }

        public string VendorBatchNumber
        {
            get => _vendorBatchNumber;
            set => _vendorBatchNumber = value;
        }

        public string InternalBatchNumber
        {
            get => _internalBatchNumber;
            set => _internalBatchNumber = value;
        }

        public DateTime? ProductionDate
        {
            get => _productionDate;
            set => _productionDate = value;
        }

        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set => _expiryDate = value;
        }

        public DateTime? BestBeforeDate
        {
            get => _bestBeforeDate;
            set => _bestBeforeDate = value;
        }

        public string Color
        {
            get => _color;
            set => _color = value;
        }

        public string Size
        {
            get => _size;
            set => _size = value;
        }

        public bool? IsPublic
        {
            get => _isPublic;
            set => _isPublic = value;
        }

        public Guid? SiteId
        {
            get => _siteId;
            set => _siteId = value;
        }

        public string SiteName
        {
            get => _siteName;
            set => _siteName = value;
        }

        public Guid? WarehouseId
        {
            get => _warehouseId;
            set => _warehouseId = value;
        }

        public string WarehouseName
        {
            get => _warehouseName;
            set => _warehouseName = value;
        }

        public double? MinThresholdAlert
        {
            get => _minThresholdAlert;
            set => _minThresholdAlert = value;
        }
        
    }
}
