using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Repositories;

namespace GHPCommerce.Modules.Sales.ShoppingCarts.Commands
{
    public class ShoppingCartCommandsHandler : ICommandHandler<CreateShoppingCartItemCommand, ValidationResult>
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IMapper _mapper;

        public ShoppingCartCommandsHandler(IShoppingCartRepository shoppingCartRepository, IMapper mapper)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _mapper = mapper;
        }
        public async Task<ValidationResult> Handle(CreateShoppingCartItemCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<ShoppingCartItem>(request);
            _shoppingCartRepository.Add(entity);
            await _shoppingCartRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }
    }
}
