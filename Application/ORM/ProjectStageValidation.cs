using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FederalProjectManager.ORM
{
    public partial class ProjectStage : IDataErrorInfo
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
            sb.Append(this["Name"]);
            sb.Append(this["ResponsibleRole"]);
            sb.Append(this["StageState"]);
            sb.Append(this["StageState"]);
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
                var errorMessage = String.Empty;
                switch (propertyName)
                {
                    case "Name":
                        if (String.IsNullOrWhiteSpace(this.Name))
                        {
                            errorMessage = "Стадия должна иметь имя";
                        };
                        break;
                    case "ResponsibleRole":
                        if (!this.ResponsibleRole.HasValue || this.ResponsibleRole.Value <= 0)
                        {
                            errorMessage = "Необходимо указать роль, ответсвенного за данную стадию";
                        }
                        break;
                    case "StageState":
                        if(this.StageState <= 0)
                        {
                            errorMessage = "Необходимо указать текущее состояние стадии проекта";
                        };
                        break;
                };
                return errorMessage;
            }
        }


    }
}
