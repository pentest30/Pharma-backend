using GHPCommerce.Modules.Sales.DTOs;
using Scriban;

namespace GHPCommerce.Modules.Sales.Helpers
{
    public static class OrderEmailTemplateHelper
    {
        public static string GetOrderGuestTemplate(OrderEmailDto order)
        {
            var template = Template.Parse(@"
        <table>
            <tr class='email-intro'>
                <td>
                    <p class='greeting'>{{order.guest}}</p>
                    <p>
                        <strong>Merci pour votre commande.</strong>
                      </p>
                    <p>
                        nous avons reçu votre commande et nous vous contacterons dans les plus brefs délais lors de l'expédition de votre colis.
                        Si vous avez des questions sur votre commande, vous pouvez nous envoyer un e-mail à <a href='mailto:'{{order.email}}>{{order.email}}</a>  ou appelez-nous au <a href='tel:' {{order.phone}}>{{order.phone}}</a>.
               
                    </p>
                </td>
            </tr>
            <tr class='email-summary'>
                <td>
                    <h1>Commande N°: <span class='no-link'>{{order.number}}</span> </h1>
                    <p>Créée le <span class='no-link'>{{order.date }}</span></p>
                </td>
            </tr>
            <tr class='email-information'>
                <td>
           
                    <table class='order-details'>
                        <tr>
                            <td class='address-details'>
                                <h3>Informations de facturation</h3>
                                  <p>{{order.address}}</p>
                                <p>{{order.city}}</p>
                                <p>{{order.state}}, {{order.zip}}</p>
                                 <p>{{ order.vendor }}</p>
                            </td>
                   
                            <td class='address-details'>
                                <h3> Informations de livraison</h3>
                                <p>{{order.address}}</p>
                                <p>{{order.city}}</p>
                                <p>{{order.state}}, {{order.zip}}</p>
                                <p>{{ order.vendor }}</p>

                            </td>
                  
                        </tr>
                        </table>
            
                </td>
            </tr>
        </table>");
            var result = template.Render(new {Order = order});
            return result;
        }
    }
}
