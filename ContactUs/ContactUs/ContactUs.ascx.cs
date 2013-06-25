using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using SimpleForms;

using umbraco.BusinessLogic;

namespace ContactUsSample
{
    public partial class ContactUs : System.Web.UI.UserControl
    {
        SimpleForm simpleForm;

        public string To
        {
            get;
            set;
        }

        public string From
        {
            get;
            set;
        }

        public string Subject
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
        
        public string FinalMessage
        {
            get;
            set;
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            simpleForm = new SimpleForm(this);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                HtmlInputText email=(HtmlInputText)simpleForm.Controls["email"];

                //Do some sort of validation
                if(someValidationMethod(email.Value))
                {
                    //send an email
                    simpleForm.SendMail(To, From, Subject, Message, true);
                    simpleForm.Save();
                    

                    foreach (KeyValuePair<string,string> keyValue in simpleForm.GetAllValues())
                    {
                        simpleForm.Form.InnerHtml += keyValue.Key + "=>" + keyValue.Value+"<br/>"+Environment.NewLine;
                    }

                    simpleForm.Form.InnerHtml = FinalMessage +"<br/>"+Environment.NewLine+ simpleForm.Form.InnerHtml;
                }
                else
                {
                    simpleForm.AddErrorClass(email);//helper method
                }
            }
        }

        private bool someValidationMethod(string email)
        {
            return SimpleForm.CheckEmail(email);
        }
    }
}
