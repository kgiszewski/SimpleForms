using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Script.Serialization;

using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.template;
using umbraco.BusinessLogic;
using umbraco.DataLayer;

using Umbraco.Core.Logging;

using SimpleForms;

namespace SimpleForms
{
    public partial class Editform : umbraco.BasePages.BasePage
    {
        private Document formDocument;
        private DropDownList templateDdl, dateChoices;
        private DocumentType documentType;
        internal HtmlGenericControl h1, h2, div, label, input, button, table, thead, tbody, tr, th, td, urlTD, statusTD;
        internal HtmlTextArea saveBox;
        internal TextBox formIDBox, formName, actionBox, maxResults, keywords;
        private FormSchema formSchema;
        private Template template;
        private User currentUser;
        private JavaScriptSerializer jsonSerializer=new JavaScriptSerializer();
        private HtmlGenericControl saveMessages = new HtmlGenericControl("ul");

        private bool userCanPublish = false;

        private enum actions {Unpublish, Save, SavePublish}

        protected void Page_Init(object sender, EventArgs e)
        {
            formDocument = new Document(Convert.ToInt32(HttpContext.Current.Request.QueryString["id"]));
            template = new Template(formDocument.Template);

            h1 = new HtmlGenericControl("h1");
            h1.InnerHtml = "Simple Forms";
            settings.Controls.Add(h1);

            saveBox = new HtmlTextArea();
            saveBox.Attributes["class"] = "saveBox";
            settings.Controls.Add(saveBox);

            formIDBox = new TextBox();
            formIDBox.Attributes["class"] = "formIDbox";
            settings.Controls.Add(formIDBox);

            actionBox = new TextBox();
            actionBox.Attributes["class"] = "actionBox";
            settings.Controls.Add(actionBox);

            table = new HtmlGenericControl("table");
            table.Attributes["class"] = "settingsTable";
            settings.Controls.Add(table);

            //template ddl
            templateDdl = new DropDownList();
            templateDdl.CssClass = "template";

            tr = new HtmlGenericControl("tr");
            table.Controls.Add(tr);

            td = new HtmlGenericControl("td");
            td.InnerText = t("Template");
            tr.Controls.Add(td);

            td = new HtmlGenericControl("td");
            td.Controls.Add(templateDdl);
            tr.Controls.Add(td);

            documentType = new DocumentType(formDocument.ContentType.Id);

            bool optionSelected = false;
            foreach (Template thisTemplate in documentType.allowedTemplates)
            {
                ListItem option = new ListItem(thisTemplate.Text, thisTemplate.Id.ToString());
                if(formDocument.Template==thisTemplate.Id&&optionSelected==false){
                    option.Selected=true;
                    optionSelected = true;
                }
                templateDdl.Items.Add(option);
            }

            //form name
            formName = new TextBox();
            formName.Attributes["class"] = "formName";
            settings.Controls.Add(formName);

            tr = new HtmlGenericControl("tr");
            table.Controls.Add(tr);

            td = new HtmlGenericControl("td");
            td.InnerText = t("Name");
            tr.Controls.Add(td);

            td = new HtmlGenericControl("td");
            td.Controls.Add(formName);
            tr.Controls.Add(td);

            //url
            tr = new HtmlGenericControl("tr");
            table.Controls.Add(tr);

            td = new HtmlGenericControl("td");
            td.InnerText = t("Url");
            tr.Controls.Add(td);

            urlTD = new HtmlGenericControl("td");
            tr.Controls.Add(urlTD);

            //statusTD
            tr = new HtmlGenericControl("tr");
            table.Controls.Add(tr);

            td = new HtmlGenericControl("td");
            td.InnerText = t("Status");
            tr.Controls.Add(td);

            statusTD = new HtmlGenericControl("td");
            statusTD.InnerHtml = (formDocument.Published) ? "Published" : "Unpublished";
            tr.Controls.Add(statusTD);

            saveMessages.Attributes["class"] = "saveMessages";
        }

        protected void Page_Load(object sender, EventArgs e)
        {          
            //set the user and check access
            SimpleFormsCore.Authorize();
            currentUser = umbraco.BusinessLogic.User.GetCurrent();
            userCanPublish = currentUser.GetPermissions(formDocument.Path).Contains('R');
            
            //handle any postbacks
            if (Page.IsPostBack)
            {
                HandlePostback();
            }

            //get the form schema
            formSchema = new FormSchema(template.Alias);

            h2 = new HtmlGenericControl("h2");
            h2.InnerHtml = "<a class='formFields' href='#'>" + t("Form Fields") + "</a>";

            if (currentUser.IsAdmin())
            {
                settings.Controls.Add(h2);
                BuildFieldTable();
            }            

            formIDBox.Text = formSchema.ID.ToString();
            formName.Text = formSchema.Name;

            BuildDocumentButtons();

            //update the url
            string niceurl = umbraco.library.NiceUrl(formDocument.Id);
            string url = "<a target='_blank' href='" + niceurl + "'>" + niceurl + "</a>";
            urlTD.InnerHtml = url;

            BuildSubmissionsUI();

            BuildSubmissionsTable();
        }

        private void BuildFieldTable()
        {
            table = new HtmlGenericControl("table");
            table.Attributes["class"] = "fieldsTable";
            table.Attributes["formAlias"] = formSchema.Alias;
            settings.Controls.Add(table);

            thead = new HtmlGenericControl("thead");
            table.Controls.Add(thead);

            tr = new HtmlGenericControl("tr");
            thead.Controls.Add(tr);

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "";

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "Name";

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "Alias";

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "";

            tbody = new HtmlGenericControl("tbody");
            table.Controls.Add(tbody);

            //list each form field
            var sortedFields = formSchema.FormFields.OrderBy(o => o.sortOrder);         

            foreach (FormField thisField in sortedFields)
            {
                AddField(thisField);
            }

            if (sortedFields.Count() == 0)
            {
                AddField(new FormField());
            }
        }

        private void SaveFormFields(List<FormField> formField)
        {
            IParameter[] parameters;
            
            foreach (FormField thisFormField in formField)
            {
                //Response.Write(thisFormField.ID+" "+thisFormField.alias + "<br/>");
                if(thisFormField.remove==true){
                    //Response.Write("Trying to remove=>"+thisFormField.alias+"<br/>");                    

                    parameters = new IParameter[1];
                    parameters[0] = FormSchema.SqlHelper.CreateParameter("@fieldID", thisFormField.ID);
                    
                    //could throw FK constraint
                    try
                    {
                        int deletedRowsCount = FormSchema.SqlHelper.ExecuteNonQuery(@"
                            DELETE
                            FROM SimpleFormsFields
                            WHERE fieldID=@fieldID
                        ", parameters);
                    }
                    catch (Exception e)
                    {
                       AddSaveMessage("Error saving '" + thisFormField.alias + "': Cannot delete a field if there is data saved in the DB." + e.Message);
                    }
                } else {

                    if (thisFormField.alias != "")
                    {
                        if (thisFormField.ID != 0)
                        {
                            //update
                            //Response.Write("Updating=>" + thisFormField.alias + "<br/>");
                            //could throw unique constraint

                            parameters = new IParameter[4];
                            parameters[0] = FormSchema.SqlHelper.CreateParameter("@alias", thisFormField.alias);
                            parameters[1] = FormSchema.SqlHelper.CreateParameter("@name", thisFormField.name);
                            parameters[2] = FormSchema.SqlHelper.CreateParameter("@sortOrder", thisFormField.sortOrder);
                            parameters[3] = FormSchema.SqlHelper.CreateParameter("@fieldID", thisFormField.ID);
                            try
                            {
                                int rowsUpdated = FormSchema.SqlHelper.ExecuteNonQuery(@"
                                    UPDATE SimpleFormsFields 
                                    SET
                                        alias=@alias, 
                                        name=@name, 
                                        sortOrder=@sortOrder
                                    WHERE fieldID=@fieldID 
                            
                                ", parameters);
                            }
                            catch (Exception e)
                            {
                                AddSaveMessage("Error saving '" + thisFormField.alias + "': Alias' must be unique. " + e.Message);
                            }
                        }
                        else
                        {
                            //insert
                            //Response.Write("Inserting=>" + thisFormField.alias + "<br/>");

                            parameters = new IParameter[4];
                            parameters[0] = FormSchema.SqlHelper.CreateParameter("@formID", Convert.ToInt32(formIDBox.Text));
                            parameters[1] = FormSchema.SqlHelper.CreateParameter("@alias", thisFormField.alias);
                            parameters[2] = FormSchema.SqlHelper.CreateParameter("@name", thisFormField.name);
                            parameters[3] = FormSchema.SqlHelper.CreateParameter("@sortOrder", thisFormField.sortOrder);

                            //could throw unique constraint
                            try
                            {
                                int fieldID = FormSchema.SqlHelper.ExecuteScalar<int>(@"
                                    INSERT 
                                    INTO SimpleFormsFields 
                                    (formID, alias, name, sortOrder) 
                                    VALUES 
                                    (@formID, @alias, @name, @sortOrder);
                                    SELECT SCOPE_IDENTITY() AS fieldID;
                                ", parameters);
                            }
                            catch (Exception e)
                            {
                                AddSaveMessage("Error saving '" + thisFormField.alias + "': Alias' must be unique. " + e.Message);
                            }
                        }
                    }
                    else
                    {
                        AddSaveMessage("Alias' cannot be blank!");
                    }
                }
            }
        }

        private void AddSaveMessage(string message)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            saveMessages.Controls.Add(li);
            li.InnerText = t(message);
        }

        private void AddField(FormField field)
        {
            tr = new HtmlGenericControl("tr");
            tr.Attributes["fieldID"] = field.ID.ToString();
            tbody.Controls.Add(tr);;

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            td.Attributes["class"] = "fieldControls";
            td.InnerHtml = "<img class='sortFieldsHandle' src='/umbraco/plugins/SimpleForms/images/sort.png'/><img class='add' src='/umbraco/plugins/SimpleForms/images/plus.png'/>";

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            td.InnerHtml = "<input type='text' value='" + HttpUtility.UrlDecode(field.name) + "'/>";

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            td.InnerHtml = "<input type='text' value='" + HttpUtility.UrlDecode(field.alias) + "'/>";

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            td.Attributes["class"] = "fieldControls2";
            td.InnerHtml = "<img class='remove' src='/umbraco/plugins/SimpleForms/images/minus.png'/>";
        }

        private void SaveFormSettings()
        {
            try
            {
                IParameter[] parameters = new IParameter[2];
                parameters[0] = FormSchema.SqlHelper.CreateParameter("@name", formName.Text);
                parameters[1] = FormSchema.SqlHelper.CreateParameter("@formID", formIDBox.Text);

                int rowsUpdated = FormSchema.SqlHelper.ExecuteNonQuery(@"
                                    UPDATE SimpleForms
                                    SET                                     
                                        name=@name
                                    WHERE formID=@formID
                                ", parameters);
            }
            catch (Exception e)
            {
                AddSaveMessage("Error saving form settings: " + e.Message);
            }
        }

        private void BuildDocumentButtons()
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes["class"] = "adminButtons";
            settings.Controls.Add(div);

            if (userCanPublish)
            {

                button = new HtmlGenericControl("input");
                button.Attributes["type"] = "submit";
                button.Attributes["class"] = "unpublish";
                button.Attributes["value"] = t("Unpublish");
                div.Controls.Add(button);
            }

            button = new HtmlGenericControl("input");
            button.Attributes["type"] = "button";
            button.Attributes["class"] = "preview";
            button.Attributes["value"] = t("Preview");
            button.Attributes["docID"]=formDocument.Id.ToString();
            div.Controls.Add(button);

            button = new HtmlGenericControl("input");
            button.Attributes["type"] = "submit";
            button.Attributes["class"] = "save";
            button.Attributes["value"] = t("Save");
            div.Controls.Add(button);

            if (userCanPublish)
            {
                button = new HtmlGenericControl("input");
                button.Attributes["type"] = "submit";
                button.Attributes["class"] = "savePublish";
                button.Attributes["value"] = t("Save & Publish");
                div.Controls.Add(button);
            }
        }

        private void HandlePostback()
        {
            actions action = (actions)Convert.ToInt32(actionBox.Text);
            if (formName.Text != "")
            {
                formDocument.Text = formName.Text;
            }

            switch (action)
            {

                case actions.Save:
                case actions.SavePublish:
                    formDocument.Template = Convert.ToInt32(templateDdl.SelectedValue);

                    if (action == actions.SavePublish && userCanPublish)
                    {
                        //Log.Add(LogTypes.Custom, 0, currentUser.GetPermissions(formDocument.Path));

                        //Log.Add(LogTypes.Custom, 0, "Publishing");

                        //Response.Write("Save and Publish");
                        formDocument.Save();
                        formDocument.Publish(currentUser);
                        umbraco.library.UpdateDocumentCache(formDocument.Id);
                        statusTD.InnerHtml = t("Published");
                    }
                    else
                    {
                        //Response.Write("Save Only");
                        //Log.Add(LogTypes.Custom, 0, "Saving");
                        formDocument.Save();
                    }

                    //have to redefine template in the case the user has switched it
                    template = new Template(formDocument.Template);

                    //save the form settings
                    SaveFormSettings();

                    //save out the fields
                    if (formIDBox.Text != "")
                    {
                        List<FormField> formFields = jsonSerializer.Deserialize<List<FormField>>(saveBox.Value);
                        if (currentUser.IsAdmin())
                        {
                            SaveFormFields(formFields);
                        }
                    }

                    settings.Controls.Add(saveMessages);
                    break;

                case actions.Unpublish:
                    //Response.Write("Unpublish");
                    if (formDocument.Published)
                    {
                        formDocument.UnPublish();
                        umbraco.library.UpdateDocumentCache(formDocument.Id);
                        statusTD.InnerHtml = t("Unpublished");
                    }
                    break;
            }
        }
        
        private void BuildSubmissionsUI()
        {
            //title
            h2 = new HtmlGenericControl("h2");
            submissions.Controls.Add(h2);
            h2.InnerHtml = t("Submissions");

            //UI
            div = new HtmlGenericControl("div");
            submissions.Controls.Add(div);
            div.Attributes["class"] = "resultsUI";

            label = new HtmlGenericControl("label");
            div.Controls.Add(label);
            label.InnerHtml = t("Keywords");

            keywords = new TextBox();
            keywords.Attributes["class"] = "keywords";
            div.Controls.Add(keywords);

            label = new HtmlGenericControl("label");
            div.Controls.Add(label);
            label.InnerHtml = t("Max Results");

            maxResults = new TextBox();
            maxResults.Attributes["class"] = "maxResults";
            maxResults.Text = "25";
            div.Controls.Add(maxResults);

            label = new HtmlGenericControl("label");
            div.Controls.Add(label);
            label.InnerHtml = t("Occurring");

            dateChoices = new DropDownList();
            div.Controls.Add(dateChoices);
            dateChoices.Attributes["class"] = "occurring";
            if (dateChoices.Items.Count == 0)
            {
                dateChoices.Items.Add(new ListItem(t("Ever"), ""));
                dateChoices.Items.Add(new ListItem(t("Today"), "0"));
                dateChoices.Items.Add(new ListItem(t("Yesterday and Today"), "-1"));
                dateChoices.Items.Add(new ListItem(t("In the Last 7 Days"), "-7"));
                dateChoices.Items.Add(new ListItem(t("In the Last 30 Days"), "-30"));
                dateChoices.Items.Add(new ListItem(t("In the Last 60 Days"), "-60"));
                dateChoices.Items.Add(new ListItem(t("In the Last 90 Days"), "-90"));
                dateChoices.Items.Add(new ListItem(t("In the Last 180 Days"), "-180"));
                dateChoices.Items.Add(new ListItem(t("In the Last 365 Days"), "-365"));
            }

            input = new HtmlGenericControl("input");
            div.Controls.Add(input);
            input.Attributes["type"] = "button";
            input.Attributes["class"] = "download";
            input.Attributes["value"] = t("Download");

            input = new HtmlGenericControl("input");
            div.Controls.Add(input);
            input.Attributes["type"] = "button";
            input.Attributes["class"] = "search";
            input.Attributes["value"] = t("Search");
        }

        private void BuildSubmissionsTable()
        {
            //results
            div = new HtmlGenericControl("div");
            submissions.Controls.Add(div);
            div.Attributes["class"] = "resultsDiv";

            //table
            table = new HtmlGenericControl("table");
            div.Controls.Add(table);

            thead = new HtmlGenericControl("thead");
            table.Controls.Add(thead);

            //build the column headers
            tr = new HtmlGenericControl("tr");
            thead.Controls.Add(tr);

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "<a href='#'>Date/Time</a>";

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "<a href='#'>IP</a>";

            foreach (FormField thisFormField in formSchema.FormFields.OrderBy(o => o.sortOrder))
            {
                th = new HtmlGenericControl("th");
                tr.Controls.Add(th);
                th.InnerHtml = "<a href='#'>" + HttpUtility.UrlDecode(thisFormField.name) + "</a>";
            }

            tbody = new HtmlGenericControl("tbody");
            table.Controls.Add(tbody);

            tr = new HtmlGenericControl("tr");
            tbody.Controls.Add(tr);

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            td.Attributes["colspan"] = (formSchema.FormFields.Count() + 2).ToString();
            td.InnerHtml = t("Click search for results.");
        }

        private string t(string input){
            string dictionaryItem = umbraco.library.GetDictionaryItem(input);

            if (dictionaryItem != "")
            {
                return dictionaryItem;
            }
            return input;
        }
    }
}