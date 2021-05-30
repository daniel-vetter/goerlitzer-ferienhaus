using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.RegularExpressions;

namespace WebApp.Infrastructure
{
    public class KebaberizeUrls : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            foreach (var modelSelector in model.Selectors)
                modelSelector.AttributeRouteModel.Template = Kebaberize(modelSelector.AttributeRouteModel.Template);
        }

        private string Kebaberize(string input)
        {
            return Regex.Replace(Regex.Replace(Regex.Replace(input, @"([\p{Lu}]+)([\p{Lu}][\p{Ll}])", "$1-$2"), @"([\p{Ll}\d])([\p{Lu}])", "$1-$2"), @"[-\s]", "-").ToLower();
        }
    }
}