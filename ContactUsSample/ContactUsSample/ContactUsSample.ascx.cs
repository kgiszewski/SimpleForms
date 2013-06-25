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
    public partial class ContactUsSample : System.Web.UI.UserControl
    {
        //HtmlForm form;
        //HtmlControl.List<HtmlControl> formControls = new List<HtmlControl>();
        SimpleForm myForm;

        public string Message
        {
            get;
            set;
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            myForm = new SimpleForm(this);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                //many ways to access controls
                //HtmlInputText name = (HtmlInputText)myForm.Controls["name"];
                HtmlInputText email = (HtmlInputText)myForm.Controls["email"];
                //HtmlInputText phone = (HtmlInputText)myForm.Controls["phone"];
                //HtmlInputText company = (HtmlInputText)myForm.Controls["company"];
                //HtmlInputText country = (HtmlInputText)myForm.Controls["country"];
                //HtmlSelect dropdown = (HtmlSelect)myForm.Controls["dropdown"];
                //HtmlTextArea comments = (HtmlTextArea)myForm.Controls["comments"];

                List<HtmlControl> allControls = myForm.GetAllControls();
                //have to then cast each one if need be

                //access the form element
                //myForm.Form;             

                //clear any previous errorClasses
                myForm.RemoveAllErrorClass(allControls);

                //Do some sort of validation
                HtmlControl missingRequiredControl=myForm.CheckMissingRequiredControl();
                if (missingRequiredControl == null)
                {

                    if (someValidationMethod(email.Value))
                    {
                        myForm.Save();

                        myForm.Form.InnerHtml = "";

                        //access the values of the controls as key/values
                        
                        //this can be useful if needing to send a formatted list of values as an email.
                        //foreach (KeyValuePair<string,string> keyValue in myForm.GetAllValues())
                        //{
                        //    myForm.Form.InnerHtml += keyValue.Key + "=>" + keyValue.Value+"<br/>";
                        //}

                        //send an email
                        //myForm.SendMail("to@someone.com", "from@someone.com", "Some Subject", "Some Message", true);

                        myForm.Form.InnerHtml += Message;
                    }
                    else
                    {
                        myForm.AddErrorClass(email);//helper method
                    }
                }
                else
                {
                    myForm.AddErrorClass(missingRequiredControl);//helper method
                }
            }
        }

        private bool someValidationMethod(string email)
        {
            return SimpleForm.CheckEmail(email);
        }
    }
}
