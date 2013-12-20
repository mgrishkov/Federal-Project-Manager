using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FederalProjectManager.Model
{
    public class UserSettings
    {
        public static string ConnectionString
        {
            get
            {
                return FederalProjectManager.Properties.Settings.Default.FPMConnectionString;
            }
            set
            {
                FederalProjectManager.Properties.Settings.Default.FPMConnectionString = value;
            }
        }
        public static string StoragePath
        {
            get
            {
                return FederalProjectManager.Properties.Settings.Default.StoragePath;
            }
            set
            {
                FederalProjectManager.Properties.Settings.Default.StoragePath = value;
            }
        }
        public static ERole ProfileRole
        {
            get { return (ERole)FederalProjectManager.Properties.Settings.Default.ProfileRole; }
            set { FederalProjectManager.Properties.Settings.Default.ProfileRole = (int)value; }
        }
        public static string AdminPassword
        {
            get
            {
                string password = String.Empty;
                using (var dc = new ORM.FPMDataContext())
                {
                     password = dc.GlobalSetting.Single(x => x.Parameter == "AdminPassword").StringValue;
                };
                return password;
            }
        }

        public static string ApplicationVersion
        {
            get 
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version.ToString();
            }
        }
        public static DateTime ApplicationBuildDate
        {
            get 
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var info = new System.IO.FileInfo(assembly.Location);
                return info.LastWriteTime;
            }
        }
        

        public static bool CheckConnection()
        {
            var msg = String.Empty;
            return CheckConnection(out msg);
        }
        public static bool CheckConnection(out string Message)
        {
            return CheckConnection(ConnectionString, out Message);
        }
        public static bool CheckConnection(string connectionString, out string Message)
        {
            bool result = false;
            try
            {
                using (var dc = new ORM.FPMDataContext(connectionString))
                {
                    if (dc.Parameter.Any())
                    {
                        result = true;
                    };
                };
                Message = String.Empty;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
            return result;
        }
    }
}
