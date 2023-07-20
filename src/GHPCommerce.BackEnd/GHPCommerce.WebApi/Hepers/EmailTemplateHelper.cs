using Scriban;

namespace GHPCommerce.WebApi.Hepers
{
    public static class EmailTemplateHelper
    {
        public static string GetEmailConfirmationTemplate(AccountModel model)
        {
            var template = Template.Parse(@"<table class='main'> 
                <table border = '0' >
                <tbody><tr>
                <td>
                <p>Bonjour {{user.name}},</p>
                <p>Merci pour votre inscription sur la plateforme GHP E-commerce,</p>    
                <p>avant de commancer à utiliser GHP E-commerce, Veuillez confirmer votre adresse e-mail.</p>
                <a href = '{{user.url}}' target='_blank'>Confirmer votre email</a>
                <p>Email sent by Groupe hydrapharm.</p>
                </td>
                </tr>
                </tbody></table>
                </td>
                </tr>
                </tbody></table>");
            var result = template.Render(new { user = model });
            return result;
        } 
    }

    public class AccountModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
