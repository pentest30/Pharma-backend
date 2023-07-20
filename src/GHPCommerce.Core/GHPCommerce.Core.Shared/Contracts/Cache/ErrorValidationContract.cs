using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Cache
{
    public class ErrorValidationContract
    {
        public ErrorValidationContract()
        {
            Errors = new Dictionary<string, string>();
            
        }

        public Guid OrderId { get; set; }
        public Dictionary<string,string> Errors { get; set; }
        public uint Action { get; set; }
    }

    public enum ActionType : uint
    {
        Save = 0,
        Cancel = 1
    }
}