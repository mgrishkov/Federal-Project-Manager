using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FederalProjectManager.Model;

namespace FederalProjectManager.ORM
{
    public partial class ProjectStage
    {
        public string GetStagePath()
        {
            return System.IO.Path.Combine(this.Project.GetProjectPath(), String.Format("Stage{0}", this.ID));
        }
        public bool IsEditable
        {
            get
            {
                var role = Model.UserSettings.ProfileRole;
                return ((this.Project != null && this.Project.RowState == Convert.ToInt16((int)ERowState.Active))
                        && ((this.ResponsibleRole.HasValue && (Model.ERole)this.ResponsibleRole.Value == role) || role == Model.ERole.Administrator));
            }
        }
    }
}
