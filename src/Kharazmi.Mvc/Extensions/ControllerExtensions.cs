#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Kharazmi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

#endregion

namespace Kharazmi.Mvc.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>([NotNull]this Controller controller, string viewName,
            TModel model, bool isMainPage = false)
        {
            if (viewName.IsEmpty()) viewName = controller.ControllerContext.ActionDescriptor.ActionName;

            controller.ViewData.Model = model;

            await using var writer = new StringWriter();
            IViewEngine? viewEngine =
                controller.HttpContext?.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

            var viewResult = viewName.EndsWith(".cshtml")
                ? viewEngine?.GetView(viewName, viewName, isMainPage)
                : viewEngine?.FindView(controller.ControllerContext, viewName, isMainPage);


            if (viewEngine is null || viewResult is null || !viewResult.Success) return $"A view with the name {viewName} could not be found";

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);

            return writer.GetStringBuilder().ToString();
        }

        public static bool IsMvcController(this Type controllerType)
        {
            var controllerBase = typeof(ControllerBase);
            var controller = typeof(Controller);

            return controllerType != null &&
                   controllerType.IsSubclassOf(controllerBase) &&
                   controllerType.IsSubclassOf(controller);
        }

        public static bool IsApiController(this Type controllerType)
        {
            var controllerBase = typeof(ControllerBase);
            var controller = typeof(Controller);

            return controllerType != null &&
                   controllerType.IsSubclassOf(controllerBase) &&
                   !controllerType.IsSubclassOf(controller);
        }
    }
}