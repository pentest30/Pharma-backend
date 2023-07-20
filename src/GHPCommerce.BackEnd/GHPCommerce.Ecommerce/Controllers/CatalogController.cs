using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Ecommerce.Controllers
{
    public class CatalogController : ViewComponent
    {
        private readonly ICatalogService _catalogService;
        private readonly IMapper _mapper;

        public CatalogController(ICatalogService catalogService , IMapper mapper)
        {
            _catalogService = catalogService;
            _mapper = mapper;
        }
       
        [HttpGet]
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var catalog =await _catalogService.GetCatalogsAsync();
            var rst = _mapper.Map<IEnumerable<CatalogModel>>(catalog);
            return View("_NavBar", rst);

        }
    }
}
