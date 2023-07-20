using System;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;

namespace GHPCommerce.Core.Shared.Contracts.Sagas
{
    public class QueueNames
    {
        public const string SagaInvent = "invent-queue";
        private const string rabbitUri = "queue:";
        public static Uri GetMessageUri(string key)
        {
            return new Uri(rabbitUri + key.PascalToKebabCaseMessage());
        }
        public static Uri GetActivityUri(string key)
        {
            var kebabCase =  key.PascalToKebabCaseActivity();
            if (kebabCase.EndsWith('-'))
            {
                kebabCase = kebabCase.Remove(kebabCase.Length - 1);
            }
            return new Uri(rabbitUri + kebabCase + '_'  + "execute");
        }
    }
}
