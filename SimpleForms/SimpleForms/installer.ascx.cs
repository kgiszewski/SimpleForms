using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using umbraco.BusinessLogic;

namespace SimpleForms
{
    public partial class Installer : System.Web.UI.UserControl
    {
        private HtmlGenericControl messageList, li;

        protected void Page_Load(object sender, EventArgs e)
        {
            Log.Add(LogTypes.Custom, 0, "Running Simple Forms Installer...");

            messageList = new HtmlGenericControl("ul");
            wrapper.Controls.Add(messageList);

            AddTable("SimpleForms", @"
                CREATE TABLE [dbo].[SimpleForms](
	                [formID] [int] IDENTITY(1,1) NOT NULL,
	                [name] [nvarchar](50) NULL,
	                [alias] [nvarchar](50) NOT NULL,
                 CONSTRAINT [PK_SimpleForms] PRIMARY KEY CLUSTERED 
                (
	                [formID] ASC
                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY];

                INSERT
                INTO [SimpleForms]
                  (name, alias)
                  VALUES
                  ('Contact Us Sample', 'ContactUsSample')
            ");

            AddTable("SimpleFormsFields", @"
                CREATE TABLE [dbo].[SimpleFormsFields](
	                [fieldID] [int] IDENTITY(1,1) NOT NULL,
	                [formID] [int] NOT NULL,
	                [name] [nvarchar](50) NOT NULL,
	                [alias] [nvarchar](50) NOT NULL,
	                [sortOrder] [int] NOT NULL,
                 CONSTRAINT [PK_SimpleFormsFields] PRIMARY KEY CLUSTERED 
                (
	                [fieldID] ASC
                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY];
                

                ALTER TABLE [dbo].[SimpleFormsFields]  WITH CHECK ADD  CONSTRAINT [FK_SimpleFormsFields_SimpleForms] FOREIGN KEY([formID])
                REFERENCES [dbo].[SimpleForms] ([formID]);

                ALTER TABLE [dbo].[SimpleFormsFields] CHECK CONSTRAINT [FK_SimpleFormsFields_SimpleForms];

                INSERT
                INTO [SimpleFormsFields]
                  (formID, name, alias, sortOrder)
                  VALUES
                  (1, 'Email', 'email', 0),
                  (1, 'Name', 'name', 1),
                  (1, 'Phone Number', 'phone', 2),
                  (1, 'Company', 'company', 3),
                  (1, 'Country', 'country', 4),
                  (1, 'DDL', 'dropdown', 5),
                  (1, 'CBL', 'checkgroup', 6),
                  (1, 'Radio', 'radiogroup', 7),
                  (1, 'Question/Comments', 'comments', 8)
                
            ");

            AddTable("SimpleFormsSubmissions", @"  
                CREATE TABLE [dbo].[SimpleFormsSubmissions](
	                [submissionID] [int] IDENTITY(1,1) NOT NULL,
	                [formID] [int] NOT NULL,
	                [IP] [nvarchar](50) NOT NULL,
	                [datetime] [datetime] NOT NULL,
                    CONSTRAINT [PK_SimpleFormSubmissions] PRIMARY KEY CLUSTERED 
                (
	                [submissionID] ASC
                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY];                

                ALTER TABLE [dbo].[SimpleFormsSubmissions]  WITH CHECK ADD  CONSTRAINT [FK_SimpleFormSubmissions_SimpleForms] FOREIGN KEY([formID])
                REFERENCES [dbo].[SimpleForms] ([formID]);                

                ALTER TABLE [dbo].[SimpleFormsSubmissions] CHECK CONSTRAINT [FK_SimpleFormSubmissions_SimpleForms];                
            ");

            AddTable("SimpleFormsEntries", @" 
                CREATE TABLE [dbo].[SimpleFormsEntries](
	                [entryID] [int] IDENTITY(1,1) NOT NULL,
	                [submissionID] [int] NOT NULL,
	                [fieldID] [int] NOT NULL,
	                [value] [nvarchar](max) NOT NULL,
                    CONSTRAINT [PK_SimpleFormEntries] PRIMARY KEY CLUSTERED 
                (
	                [entryID] ASC
                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY];

                ALTER TABLE [dbo].[SimpleFormsEntries]  WITH CHECK ADD  CONSTRAINT [FK_SimpleFormEntries_SimpleFormsFields] FOREIGN KEY([fieldID])
                REFERENCES [dbo].[SimpleFormsFields] ([fieldID]);
                
                ALTER TABLE [dbo].[SimpleFormsEntries] CHECK CONSTRAINT [FK_SimpleFormEntries_SimpleFormsFields];                
            ");
        }


        private void AddTable(string name, string SQL)
        {
            li = new HtmlGenericControl("li");
            messageList.Controls.Add(li);
            li.InnerHtml = "Adding Table '" + name + "'...";
            try
            {
                FormSchema.SqlHelper.ExecuteNonQuery(SQL);
            }
            catch (Exception e)
            {
                li = new HtmlGenericControl("li");
                messageList.Controls.Add(li);
                li.InnerHtml = "ERROR: Adding Table '" + name + "' " + e.Message;
                Log.Add(LogTypes.Custom, 0, "ERROR: Adding Table '" + name + "' " + e.Message);
            }
            li = new HtmlGenericControl("li");
            messageList.Controls.Add(li);
            li.InnerHtml = "'" + name + "' added successfully.";
        }       
    }
}