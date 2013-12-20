using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FederalProjectManager.ORM
{
    public partial class StageParameter : IDataErrorInfo
    {
        private bool _isValid = false;

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    SendPropertyChanged("IsValid");
                }
            }
        }
        public string Validate()
        {
            var sb = new StringBuilder();
            sb.Append(this["ParameterID"]);
            sb.Append(this["NumberValue"]);
            sb.Append(this["StringValue"]);
            sb.Append(this["DateTimeValue"]);
            IsValid = sb.Length == 0;
            return sb.ToString();
        }
        public string Error
        {
            get { return String.Empty; }
        }
        public string this[string propertyName]
        {
            get
            {
                String errorMessage = String.Empty;
                if (propertyName == "ParameterID")
                {
                    if (this.ParameterID <= 0)
                    {
                        errorMessage = "Не задан параметр";
                    };
                }
                else if (propertyName == "NumberValue" || propertyName == "StringValue" || propertyName == "DateTimeValue")
                {
                    if(!this.NumberValue.HasValue && String.IsNullOrWhiteSpace(this.StringValue) && !this.DateTimeValue.HasValue)
                    {
                        errorMessage = "Не задано значение параметра";
                    }
                };
                return errorMessage;
            }
        }


    }
}
