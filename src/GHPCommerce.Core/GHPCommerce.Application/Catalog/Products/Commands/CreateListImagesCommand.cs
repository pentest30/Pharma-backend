using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Products.Commands
{
    public class CreateListImagesCommand :ICommand
    {
        public CreateListImagesCommand()
        {
            ImageCommands = new List<CreateImageCommand>();
        }
       
        public List<CreateImageCommand> ImageCommands { get; set; }
        public Guid Id { get; set; }
    }
}
