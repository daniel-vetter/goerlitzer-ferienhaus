using Humanizer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WebApp.Infrastructure
{
    public class KebaberizeUrls : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            foreach (var modelSelector in model.Selectors)
                modelSelector.AttributeRouteModel.Template = modelSelector.AttributeRouteModel.Template.Kebaberize();
        }
    }
}