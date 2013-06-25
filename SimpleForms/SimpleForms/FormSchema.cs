using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Web.UI.HtmlControls;

namespace SimpleForms
{
    public class FormSchema
    {
        private int DBformID=0;
        private List<FormField> formFields = new List<FormField>();
        private Dictionary<int,string> saveValues= new Dictionary<int, string>();
        private Dictionary<string, string> formValues = new Dictionary<string, string>();
        private string formName="";
        private string alias = "";

        public const int TEXTBOX = 0;
        public const int TEXTAREA = 1;
        public const int CHECKBOX = 2;
        public const int RADIO = 3;
        public const int GROUP = 4;
        public const int SELECT = 5;

        private Dictionary<Type, int> formTypes = new Dictionary<Type, int>()
        {
            {typeof(HtmlInputText),TEXTBOX},
            {typeof(HtmlTextArea), TEXTAREA},
            {typeof(HtmlInputCheckBox), CHECKBOX},
            {typeof(HtmlInputRadioButton), RADIO},
            {typeof(HtmlGenericControl), GROUP},
            {typeof(HtmlSelect), SELECT}
        };

        public Dictionary<int,string> SaveValues{
            get { return saveValues; }
        }

        public Dictionary<string, string> FormValues
        {
            get { return formValues; }
        }

        public int ID
        {
            get { return DBformID; }
        }

        public string Name
        {
            get { return formName; }
        }

        public string Alias
        {
            get { return alias; }
        }

        public FormSchema(string alias, bool createNewIfNotFound=true)
        {
            SetFormByAlias(alias, createNewIfNotFound);
            SetFormFieldsByFormId(DBformID);
        }

        public List<FormField> FormFields
        {
            get { return formFields; }
        }

        private void SetFormByAlias(string alias, bool createNewIfNotFound)
        {
            this.alias = alias;
            IRecordsReader reader = SqlHelper.ExecuteReader("SELECT formID, name FROM SimpleForms WHERE alias = @alias", SqlHelper.CreateParameter("@alias", alias));

            if (reader.HasRecords)
            {
                reader.Read();
                DBformID = reader.Get<int>("formID");
                formName = reader.Get<string>("name");
            }

            //creates new form in DB if not found
            if (DBformID == 0 && createNewIfNotFound)
            {
                //throw new Exception("Form not found in the SimpleForms table.");
                IParameter[] parameters = new IParameter[1];
                parameters[0] = FormSchema.SqlHelper.CreateParameter("@alias", alias);

                //could throw unique constraint
                try
                {
                    DBformID = FormSchema.SqlHelper.ExecuteScalar<int>(@"
                        INSERT 
                        INTO SimpleForms 
                        (alias) 
                        VALUES 
                        (@alias);
                        SELECT SCOPE_IDENTITY() AS formID;
                    ", parameters);
                }
                catch (Exception e)
                {
                    
                }
            }
        }

        private void SetFormFieldsByFormId(int formID)
        {
            IRecordsReader reader = SqlHelper.ExecuteReader("SELECT * FROM SimpleFormsFields WHERE formID = @formID", SqlHelper.CreateParameter("@formID", formID));

            if (!reader.HasRecords)
            {
                //throw new Exception("Fields not found for the given form ID.");
            }

            while (reader.Read())
            {
                formFields.Add(new FormField() { ID = reader.Get<int>("fieldID"), name = reader.Get<string>("name"), alias = reader.Get<string>("alias"), sortOrder = reader.Get<int>("sortOrder") });
            }
        }

        public void LoadValues(Dictionary<string, HtmlControl> formWebControls)
        {
            //create a list of all the form fields, then set their value from the incoming submission
            foreach (FormField thisField in formFields)
            {
                int count = 0;
                //HttpContext.Current.Response.Write("schema field=>" + thisField.alias + thisField.ID + "<br/>");
                try
                {
                    switch (formTypes[formWebControls[thisField.alias].GetType()])
                    {
                        case TEXTBOX:
                            saveValues[thisField.ID] = ((HtmlInputText)formWebControls[thisField.alias]).Value;
                            formValues[thisField.alias] = saveValues[thisField.ID];
                            break;

                        case TEXTAREA:
                            saveValues[thisField.ID] = ((HtmlTextArea)formWebControls[thisField.alias]).Value;
                            formValues[thisField.alias] = saveValues[thisField.ID];
                            break;

                        case SELECT:
                            HtmlSelect thisSelect=((HtmlSelect)formWebControls[thisField.alias]);
                            count = 0;
                            foreach (System.Web.UI.WebControls.ListItem thisOption in thisSelect.Items)
                            {
                                //HttpContext.Current.Response.Write("option->"+thisOption.Value+thisOption.Selected.ToString()+"<br/>");
                                if (thisOption.Selected)
                                {
                                    if (count == 0)
                                    {
                                        saveValues[thisField.ID] = thisOption.Value;
                                        formValues[thisField.alias] = saveValues[thisField.ID];
                                    }
                                    else
                                    {
                                        saveValues[thisField.ID] += "," + thisOption.Value;
                                    }

                                    count++;
                                }                              
                                    
                            }
                            formValues[thisField.alias] = saveValues[thisField.ID];
                            break;

                        case GROUP:
                            //this could be full of radios or checkboxes
                            //find all radios/checkboxes

                            //HttpContext.Current.Response.Write("found group container <br/>");
                            HtmlGenericControl group = ((HtmlGenericControl)formWebControls[thisField.alias]);

                            count = 0;

                            //HttpContext.Current.Response.Write("looking for controls <br/>");
                            foreach (var thisGroupControl in group.Controls)
                            {
                                //HttpContext.Current.Response.Write("found a " + thisGroupControl.GetType().ToString()+ " <br/>");
                                try
                                {
                                    switch (formTypes[thisGroupControl.GetType()])
                                    {
                                        case CHECKBOX:
                                            HtmlInputCheckBox thisChecbox = ((HtmlInputCheckBox)thisGroupControl);

                                            //could have multiple selected, so concat
                                            if(thisChecbox.Checked){
                                                if (count == 0)
                                                {
                                                    saveValues[thisField.ID] = thisChecbox.Value;
                                                }
                                                else
                                                {
                                                    saveValues[thisField.ID]+= "," + thisChecbox.Value;
                                                }

                                                //HttpContext.Current.Response.Write("cb->"+saveValues[thisField.ID]+"<br/>");
                                                count++;
                                            }
                                            break;

                                        case RADIO:
                                            //HttpContext.Current.Response.Write("found a radio<br/>");

                                            HtmlInputRadioButton thisRadio = ((HtmlInputRadioButton)thisGroupControl);
                                            if(thisRadio.Checked){
                                                saveValues[thisField.ID] = thisRadio.Value;
                                                formValues[thisField.alias] = saveValues[thisField.ID];
                                            }
                                            break;
                                    }
                                }
                                catch (Exception e3)
                                {
                                    //HttpContext.Current.Response.Write("e3=>"+e3.Message + "<br/>");
                                }
                            }
                            formValues[thisField.alias] = saveValues[thisField.ID];
                            break;
                    }
                    //HttpContext.Current.Response.Write("form value "+thisField.alias+"=>" + saveValues[thisField.ID] + "<br/>");

                }
                catch (Exception e)
                {
                    //HttpContext.Current.Response.Write(e.Message + "<br/>");
                }
            }
        }

        public int CreateSubmission()
        {
            IParameter[] parameters = new IParameter[2];
            parameters[0] = SqlHelper.CreateParameter("@DBformID", DBformID);
            parameters[1] = SqlHelper.CreateParameter("@ipAddress", GetUserIP());

            //create submission
            int submissionID = SqlHelper.ExecuteScalar<int>("INSERT INTO SimpleFormsSubmissions (formID, IP, datetime) VALUES (@DBformID, @ipAddress, GETDATE());SELECT SCOPE_IDENTITY() AS submissionID;", parameters);
            //HttpContext.Current.Response.Write("Submission ID=>" + submissionID + "<br/>");

            if (submissionID == 0)
            {
                throw new Exception("Could not create a record in table SimpleFormsSubmissions.");
            }

            parameters = new IParameter[3];
            parameters[0] = SqlHelper.CreateParameter("@submissionID", submissionID);

            foreach (int key in saveValues.Keys)
            {
                try
                {
                    parameters[1] = SqlHelper.CreateParameter("@fieldID", key);
                    parameters[2] = SqlHelper.CreateParameter("@value", HttpUtility.HtmlEncode(saveValues[key]));
                    int entryID = SqlHelper.ExecuteScalar<int>("INSERT INTO SimpleFormsEntries (submissionID, fieldID, value) VALUES (@submissionID, @fieldID, @value);SELECT SCOPE_IDENTITY() AS entryID;", parameters);
                    //HttpContext.Current.Response.Write("Saving FieldID=>" + key + "=>" + saveValues[key] + "<br/>");

                    //HttpContext.Current.Response.Write("EntryID=>" + entryID + "<br/>");
                }
                catch (Exception e2)
                {
                    //HttpContext.Current.Response.Write(e2.Message);
                }
            }

            return submissionID;
        }

        public static ISqlHelper SqlHelper
        {
            get
            {
                return Application.SqlHelper;
            }
        }

        private string GetUserIP()
        {
            string ipList = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipList))
            {
                return ipList.Split(',')[0];
            }

            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }
    }

    public class FormField
    {
        public int ID;
        public string name = "";
        public string alias = "";
        public int sortOrder;
        public bool remove = false;
    }
}