using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FederalProjectManager.Model;

namespace FederalProjectManager.ORM
{
    public partial class ActualProjectView
    {
        public EStatisticsTile GetProjectStatisticsTile()
        {
            var result = EStatisticsTile.Current;
            if (this.IsHighPriority)
            {
                result = EStatisticsTile.HighPriority;
            }
            else if (this.IsCurrent)
            {
                result = EStatisticsTile.Current;
            }
            else if (this.IsArchive)
            {
                result = EStatisticsTile.Archive;
            }
            else if (this.IsOverstay)
            {
                result = EStatisticsTile.Overstay;
            }
            else if (this.IsPrepare)
            {
                result = EStatisticsTile.Prepare;
            }
            else if (this.IsProductionCompleted)
            {
                result = EStatisticsTile.ProductionCompleted;
            }
            else if (this.IsToday)
            {
                result = EStatisticsTile.Today;
            };
            return result;
        }
    }
}
