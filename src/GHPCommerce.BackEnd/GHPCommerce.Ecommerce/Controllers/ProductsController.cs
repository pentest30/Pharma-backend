using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Ecommerce.Helpers.UI;
using GHPCommerce.Ecommerce.Models;

namespace GHPCommerce.Ecommerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductsController(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index(Guid catalogId, int? page , int pageSize = 10)
        {
            int?  currentPage = page ?? 1;
            var result = await _productService.GetListOfProductsAsync(catalogId, currentPage.Value  , pageSize);
            var pager = new Pager(result.Total, page);

            var viewModel = new PagerViewModel<ProductModel>
            {
                Id = catalogId,
                Items = _mapper.Map<IEnumerable<ProductModel>>(result.Data),
                Pager = pager
            };

            return View(viewModel);
        }


        public IActionResult Details(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
