using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FederalProjectManager.Model;

namespace FederalProjectManager.ORM
{
    public partial class Project
    {
        public string GetProjectPath()
        {
            return System.IO.Path.Combine(Model.UserSettings.StoragePath, String.Format("Project{0}", this.ID));
        }
        public static bool IsEditable
        {
            get { return  (UserSettings.ProfileRole == ERole.Administrator); }
        }
    }
}
