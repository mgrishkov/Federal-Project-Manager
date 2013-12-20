using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FederalProjectManager.ORM;
using FederalProjectManager.Model;
using DevExpress.Xpf.LayoutControl;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Globalization;
using System.ComponentModel;

namespace FederalProjectManager.Model
{
    public class ProjectStageTile : ProjectStage, INotifyPropertyChanged
    {
        private bool _isFocused;


        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                if (_isFocused != value)
                {
                    _isFocused = value;
                    SendPropertyChanged("IsFocused");
                };
            }
        }
        public ProjectStageTile()
            : base()
        {
            IsFocused = false;
            ID = -1;
            SortIndex = 1;
        }
        public ProjectStageTile(ORM.Project project, int sortIndex)
            : this()
        {
            ID = 0;
            CrtDate = DateTime.Now;
            IsFocused = true;
            IsPublic = true;
            Name = "Новая стадия";
            Note = String.Empty;
            Project = project;
            ProjectID = project.ID;
            ResponsibleRole = Convert.ToInt16(ERole.Administrator);
            RowState = Convert.ToInt16(ERowState.Active);
            SortIndex = sortIndex;
            StageState = (int)EStageState.Configurating;
        }
        public ProjectStageTile(ProjectStage stage)
            : this()
        {
            ID = stage.ID;
            CrtDate = stage.CrtDate;
            IsFocused = false;
            IsPublic = stage.IsPublic;
            Name = stage.Name;
            Note = stage.Note;
            Project = stage.Project;
            ProjectID = stage.ProjectID;
            ResponsibleRole = stage.ResponsibleRole;
            RowState = stage.RowState;
            SortIndex = stage.SortIndex;
            StageParameter = stage.StageParameter;
            StageState = stage.StageState;
        }

        public ProjectStageTile(EStageTemplate template, int sortIndex)
        {
            IsFocused = false;
            this.RowState = Convert.ToInt16(ERowState.Active);
            this.StageState = Convert.ToInt16(EStageState.Configurating);
            this.CrtDate = DateTime.Now;
            switch (template)
            {
                case EStageTemplate.Contract:
                    contractTemplate(sortIndex);
                    break;
                case EStageTemplate.Sketch:
                    sketchTemplate(sortIndex);
                    break;
                case EStageTemplate.Payment:
                    paymentTemplate(sortIndex);
                    break;
                case EStageTemplate.Shipment:
                    shipmentTemplate(sortIndex);
                    break;
            }
        }

        private void contractTemplate(int sortIndex)
        {
            this.SortIndex = sortIndex;
            this.Name = "Договор";
            this.ResponsibleRole = Convert.ToInt16(ERole.Accountant);
            this.IsPublic = false;

            this.StageParameter.Add(new StageParameter()
            {
                CrtDate = DateTime.Now,
                ParameterID = 13
            });
            this.StageParameter.Add(new StageParameter()
            {
                CrtDate = DateTime.Now,
                ParameterID = 11
            });
            this.StageParameter.Add(new StageParameter()
            {
                CrtDate = DateTime.Now,
                ParameterID = 21
            });
        }
        private void sketchTemplate(int sortIndex)
        {
            this.SortIndex = sortIndex;
            this.Name = "Эскиз";
            this.ResponsibleRole = Convert.ToInt16(ERole.Designer);
            this.IsPublic = true;
        }
        private void paymentTemplate(int sortIndex)
        {
            this.SortIndex = sortIndex;
            this.Name = "Оплата";
            this.ResponsibleRole = Convert.ToInt16(ERole.Accountant);
            this.IsPublic = false;

            this.StageParameter.Add(new StageParameter()
            {
                CrtDate = DateTime.Now,
                ParameterID = 16
            });
            this.StageParameter.Add(new StageParameter()
            {
                CrtDate = DateTime.Now,
                ParameterID = 12
            });
            this.StageParameter.Add(new StageParameter()
            {
                CrtDate = DateTime.Now,
                ParameterID = 17
            });
            this.StageParameter.Add(new StageParameter()
            {
                CrtDate = DateTime.Now,
                ParameterID = 19
            });
        }
        private void shipmentTemplate(int sortIndex)
        {
            this.SortIndex = sortIndex;
            this.Name = "Отгрузка";
            this.ResponsibleRole = Convert.ToInt16(ERole.Administrator);
            this.IsPublic = true;
        }
    }
}
