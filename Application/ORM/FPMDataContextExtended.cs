using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FederalProjectManager.ORM
{
    public partial class FPMDataContext
    {
        public FPMDataContext() : base(Model.UserSettings.ConnectionString)
        {
        }
    }
}
