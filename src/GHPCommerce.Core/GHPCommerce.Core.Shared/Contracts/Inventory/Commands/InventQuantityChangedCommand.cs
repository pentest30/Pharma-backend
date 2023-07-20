using System;
using GHPCommerce.Domain.Domain.Commands;

public class InventQuantityChangedCommand : ICommand<object>
{
    public Guid ProductId { get; set; }
    public int CurrentQuantity { get; set; }
}
