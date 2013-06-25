using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic;

namespace SimpleForms
{
    //this class if for v6
    //public class SimpleFormEvents : IApplicationEventHandler
    //{
    //    private static object _lockObj = new object();
    //    private static bool _ran = false ;

    //    public void OnApplicationStarted(UmbracoApplicationBase httpApplication, Umbraco.Core.ApplicationContext applicationContext)
    //    {
    //        // lock
    //        if (!_ran)
    //        {
    //            lock (_lockObj)
    //            {
    //                if (!_ran)
    //                { 
    //                    BaseTree.BeforeNodeRender += new BaseTree.BeforeNodeRenderEventHandler(this.BaseTree_BeforeNodeRender);
    //                    _ran = true;
    //                }
    //            }
    //        }
    //    }
            
    //    public void OnApplicationStarting(UmbracoApplicationBase httpApplication, Umbraco.Core.ApplicationContext applicationContext)
    //    {
    //    }
 
    //    public void OnApplicationInitialized(UmbracoApplicationBase httpApplication, Umbraco.Core.ApplicationContext applicationContext)
    //    {
    //    }        

    //    private void BaseTree_BeforeNodeRender(ref XmlTree sender, ref XmlTreeNode node, EventArgs e)
    //    {
    //        if (node.Icon == "simpleform.gif")
    //        {
    //            //here we are using the context menu as the only means to differentiate whether we are on the permissions page or the usual content page.  Both permissions and the content sections report the tree type as 'content'.
    //            var contextMenu=HttpContext.Current.Request.QueryString["contextMenu"];
    //            if (contextMenu != null&&Convert.ToBoolean(contextMenu))
    //            {
    //                //Log.Add(LogTypes.Custom, 0, "page=>" + HttpContext.Current.Request.QueryString["contextMenu"]);
    //                node.Action = "javascript:parent.right.document.location.href='/umbraco/plugins/SimpleForms/editform.aspx?id=" + node.NodeID + "'";
    //            }
    //        }
    //    }
    //}


    //this class is for v4
    public class SimpleFormEvents4 : ApplicationStartupHandler
    {
        public SimpleFormEvents4()
        {
            BaseTree.BeforeNodeRender += new BaseTree.BeforeNodeRenderEventHandler(this.BaseTree_BeforeNodeRender);
        }

        private void BaseTree_BeforeNodeRender(ref XmlTree sender, ref XmlTreeNode node, EventArgs e)
        {
            if (node.TreeType.ToLower() == "content")
            {
                Document document = new Document(Convert.ToInt32(node.NodeID));
                if (document.ContentType.Alias == "SimpleForm" || document.ContentType.Alias == "SimpleFormTranslation")
                {
                    //here we are using the context menu as the only means to differentiate whether we are on the permissions page or the usual content page.  Both permissions and the content sections report the tree type as 'content'.
                    var contextMenu = HttpContext.Current.Request.QueryString["contextMenu"];
                    if (contextMenu != null && Convert.ToBoolean(contextMenu))
                    {
                        //Log.Add(LogTypes.Custom, 0, "page=>" + HttpContext.Current.Request.QueryString["contextMenu"]);
                        node.Action = "javascript:parent.right.document.location.href='/umbraco/plugins/SimpleForms/editform.aspx?id=" + node.NodeID + "'";
                    }
                }
            }
        }
    }
}
 