using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FederalProjectManager.ORM;

namespace FederalProjectManager.Model
{
    public static class ProjectInfoViewExtension
    {
        public static Project ToProject(this ProjectInfoView sender)
        {
            var newProject = new Project()
            {
                Caption = sender.Caption,
                ContactID = sender.ContactID,
                CrtDate = sender.CrtDate,
                ID = sender.ID,
                IsInWork = sender.IsInWork,
                Note = sender.Note,
                Priority = sender.Priority,
                Progress = sender.Progress,
                ResponsiblePerson = sender.ResponsiblePerson,
                RowState = sender.RowState,
                Type = sender.Type
            };
            using (var dc = new ORM.FPMDataContext())
            {
                if (dc.Contact.Any(x => x.ID == sender.ContactID))
                {
                    newProject.Contact = dc.Contact.Single(x => x.ID == sender.ContactID);
                };
            };
            return newProject;
        }
    }
}
