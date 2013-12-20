using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FederalProjectManager.ORM
{
    public partial class ProjectInfoView : IDataErrorInfo
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
            sb.Append(this["Caption"]);
            sb.Append(this["CustomerFullName"]);
            sb.Append(this["Type"]);
            sb.Append(this["Priority"]);
            sb.Append(this["ResponsiblePerson"]);
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
                switch (propertyName)
                {
                    case "Caption":
                        if (String.IsNullOrWhiteSpace(this.Caption))
                        {
                            errorMessage = "Проект должен иметь название";
                        }
                        else if (this.Caption.Length > 255)
                        {
                            errorMessage = "Название не должно быть длинее 255 символов";
                        };
                        break;
                    case "CustomerFullName":
                        if (this.ContactID <= 0)
                        {
                            errorMessage = "Необходимо указать заказчика";
                        }
                        break;
                    case "Type":
                        if(this.Type <= 0)
                        {
                            errorMessage = "Проект должен иметь тип";
                        };
                        break;
                    case "Priority":
                        if (this.Priority <= 0)
                        {
                            errorMessage = "Не задан приоритет проекта";
                        };
                        break;
                    case "ResponsiblePerson":
                        if (String.IsNullOrWhiteSpace(this.ResponsiblePerson))
                        {
                            errorMessage = "Не задан ответственный за проект";
                        };
                        break;
                };
                IsValid = String.IsNullOrEmpty(errorMessage);
                return errorMessage;
            }
        }


    }
}
