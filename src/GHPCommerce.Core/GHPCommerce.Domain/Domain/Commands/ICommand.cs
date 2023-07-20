using System;
using MediatR;

namespace GHPCommerce.Domain.Domain.Commands
{
    public interface  ICommand :IRequest  
    {
         Guid Id { get; set; }
    }
   
    public interface ICommand< out TResponse> : IRequest<TResponse> 
    {
       
    }
}
