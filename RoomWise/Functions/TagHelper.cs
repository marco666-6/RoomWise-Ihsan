using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RoomWise.Function
{
    public static class TagHelper
    {
        public static string IsActive(this IHtmlHelper helper, string controller, string action)
        {
            ViewContext context = helper.ViewContext;
            RouteValueDictionary values = context.RouteData.Values;
            string? _controller = values["controller"]?.ToString();
            string? _action = values["action"]?.ToString();
            
            if ((_action == action) && (_controller == controller))
            {
                return "active";
            }
            else
            {
                return "";
            }
        }
        
        public static string IsMenuOpen(this IHtmlHelper helper, string controller, string action)
        {
            ViewContext context = helper.ViewContext;
            RouteValueDictionary values = context.RouteData.Values;
            string? _controller = values["controller"]?.ToString();
            string? _action = values["action"]?.ToString();
            
            if ((_action == action) && (_controller == controller))
            {
                return "menu-open";
            }
            else
            {
                return "";
            }
        }
    }
}