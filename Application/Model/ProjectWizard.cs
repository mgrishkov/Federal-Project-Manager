sing System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FederalProjectManager.ORM;

namespace FederalProjectManager.Model
{
    public class ProjectWizard
    {
        private static FPMDataContext _datacontext = new FPMDataContext();

        private Project _project { get; set; }

        public string FullProjectCaption
        {
            get { return _project.Caption; }
            set { _project.Caption = value; }
        }
        public string ShortProjectCaption
        {
            get { return (_project.Caption.Length < 20) ? _project.Caption : String.Format("{0}...", _project.Caption.Substring(0, 17)); }
        }
        
        public ProjectWizard(int projectID)
        {
            if (projectID > 0)
            {
                _project = _datacontext.Project.Single(x => x.ID == projectID);
            }
            else
            {
                _project = new Project()
                {
                    ID = -1,
                    Caption = "Новый проект",
                    ProjectStage = new System.Data.Linq.EntitySet<ProjectStage>(),
                    RowState = 1
                };
            }
        }/*
        public ProjectStage GetStage(int stageID)
        {
            return _project.ProjectStage.Where(x => x.ID == stageID);
        }
        public List<ProjectStageParameter> GetStgeParameters(int stageID)
        {
            var result = new List<ProjectStageParameter>();
            if(stageID == -1)
            {
                var tmp = from t in _datacontext.StageParameterView
                         where t.StageID == stageID
                         select t;
            }
            return result;
        }*/
    }
}
