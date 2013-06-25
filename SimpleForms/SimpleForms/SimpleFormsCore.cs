using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleForms
{
    public class SimpleFormsCore
    {
        public static void Authorize()
        {
            if (umbraco.BusinessLogic.User.GetCurrent() == null)
            {
                HttpContext.Current.Response.StatusCode = 403;
                HttpContext.Current.Response.End();
            }
        }
    }
}