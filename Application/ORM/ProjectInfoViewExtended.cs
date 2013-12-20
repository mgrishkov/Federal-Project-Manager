using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FederalProjectManager.Model;

namespace FederalProjectManager.ORM
{
    public partial class ProjectInfoView
    {
        public bool IsEditable
        {
            get { return (this.RowState == Convert.ToInt16((int)ERowState.Active) && Project.IsEditable); }
        }
    }
}
