using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FederalProjectManager.ORM;

namespace FederalProjectManager.Model
{
    public class ParameterRepository
    {
        private static List<ORM.Parameter> _parameters;
        private static List<ORM.ParameterValue> _values;

        private static void loadParameters()
        {
            if (_parameters == null)
            {
                using (var dc = new ORM.FPMDataContext())
                {
                    _parameters = dc.Parameter.ToList();
                };
            };
        }
        private static void loadValues()
        {
            if (_values == null)
            {
                using (var dc = new ORM.FPMDataContext())
                {
                    _values = dc.ParameterValue.ToList();
                };
            };
        }

        public static List<ORM.Parameter> Parameters
        {
            get
            {
                loadParameters();
                return _parameters;
            }
        }

        public static EParameterValueType GetParameterValueType(int parameterID)
        {
            var result = EParameterValueType.Undefined;
            loadParameters();
            if(_parameters.Any(x => x.ID == parameterID))
            {
                result = (EParameterValueType)Convert.ToInt32(_parameters.Single(x => x.ID == parameterID).ValueType);
            };
            return result;
        }
        public static string GetParameterName(int parameterID)
        {
            var result = String.Empty;
            loadParameters();
            if(_parameters.Any(x => x.ID == parameterID))
            {
                result = _parameters.Single(x => x.ID == parameterID).Name;
            };
            return result;
        }
        public static IEnumerable<ORM.ParameterValue> GetParameterValues(int parameterID)
        {
            loadValues();
            return _values.Where(x => x.ParameterID == parameterID);
        }
        public static string GetParameterValue(int parameterID, int valueID)
        {
            loadValues();
            return _values.Single(x => x.ParameterID == parameterID && x.ID == valueID).Value;
        }

        public static void Reload()
        {
            _parameters = null;
            _values = null;
        }
    }
}
