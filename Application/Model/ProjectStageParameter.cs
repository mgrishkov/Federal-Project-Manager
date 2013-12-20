using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FederalProjectManager.Model
{
    public class ProjectStageParameter
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }

        public EParameterValueType ValueType { get; set; }
    }
}
