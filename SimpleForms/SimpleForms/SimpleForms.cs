using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Net.Mail;
using umbraco.DataLayer;
using umbraco.BusinessLogic;

using System.Text.RegularExpressions;


namespace SimpleForms
{
    public class SimpleForm
    {
        private UserControl userControl;
        private HtmlForm form;
        private Dictionary<string, HtmlControl> formWebControls = new Dictionary<string, HtmlControl>();
        private List<HtmlControl> allWebControls = new List<HtmlControl>();

        private FormSchema formSchema;

        public const string ERROR_CLASS = "simpleFormError";
        public const string REQUIRED_CLASS = "required";

        public SimpleForm(UserControl userControl){
            this.userControl = userControl;
            this.form = FindForm(userControl);           

            if (form == null)
            {
                throw new Exception("Form could not be found.");
            }

            if (form.ID == null)
            {
                throw new Exception("Form ID not set.  The form ID is needed as this is the name of the table which the data will be stored.");
            }

            //grab the form controls
            LoadControls();
            
            //create a schema
            formSchema = new FormSchema(form.ID);
        }

        public Dictionary<string, HtmlControl> Controls{
            get { return formWebControls; }
        }

        public FormSchema FormSchema
        {
            get { return formSchema; }
        }

        public List<HtmlControl> GetAllControls()
        {
            formSchema.LoadValues(this.formWebControls);
            return allWebControls;
        }

        public HtmlForm Form{
            get{return form;}
        }

        public void AddErrorClass(HtmlControl control)
        {
            control.Attributes["class"]+=" "+ERROR_CLASS;
        }

        public void RemoveAllErrorClass(List<HtmlControl> controls)
        {
            foreach (HtmlControl control in controls)
            {
                RemoveErrorClass(control);
            }
        }

        public void RemoveErrorClass(HtmlControl control)
        {
            string classAttribute=control.Attributes["class"];
            if (classAttribute != null)
            {
                List<string> classes = new List<string>();
                classes.AddRange(classAttribute.Split(' '));

                classes.Remove(ERROR_CLASS);

                control.Attributes["class"] = String.Join(" ", classes);
            }
        }

        private HtmlForm FindForm(UserControl userControl)
        {
            //find the form control
            foreach (var thisControl in userControl.Parent.Parent.Controls)
            {
                if (thisControl.GetType() == typeof(HtmlForm))
                {
                    return (HtmlForm)thisControl;
                }
            }
            return null;
        }

        private void LoadControls()
        {
            foreach (var thisControl in form.Controls)
            {
                if (thisControl.GetType().IsSubclassOf(typeof(HtmlControl)))
                {
                    HtmlControl htmlControl = (HtmlControl)thisControl;
                    formWebControls.Add(htmlControl.ID, htmlControl);

                    allWebControls.Add(htmlControl);
                    //HttpContext.Current.Response.Write("adding control=>"+ htmlControl.ID +" "+ htmlControl.GetType().ToString() +"<br/>");
                }
            }
        }

        public void SendMail(string to, string from, string subject, string body, bool html)
        {
            MailMessage message = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(from),
                Subject = subject,
                IsBodyHtml = html
            };

            message.To.Add(to);

            message.Body = body;
            SmtpClient smtp = new System.Net.Mail.SmtpClient();
            smtp.Send(message);
        }

        public void Save()
        {
            formSchema.LoadValues(formWebControls);
            
            formSchema.CreateSubmission();
        }

        public Dictionary<string, string> GetAllValues()
        {
            return formSchema.FormValues;
        }

        public static bool CheckEmail(string email)
        {
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            return match.Success;
        }

        public HtmlControl CheckMissingRequiredControl()
        {
            foreach(KeyValuePair<string, string> keyValue in GetAllValues()){

                string classAttribute=formWebControls[keyValue.Key].Attributes["class"];
                if (classAttribute != null)
                {
                    List<string> classes= new List<string>();
                    classes.AddRange(classAttribute.Split(' '));

                    if (classes.Contains(REQUIRED_CLASS) && String.IsNullOrEmpty(keyValue.Value))
                    {
                        //Log.Add(LogTypes.Custom, 0, "Field=>" + keyValue.Key + " Value=>" + keyValue.Value);
                        return formWebControls[keyValue.Key];
                    }
                }
            }
            return null;
        }
    }
}