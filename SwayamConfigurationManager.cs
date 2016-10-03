using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Configuration;
using System.Web.Caching;


namespace MHRD.Swayam.Common
{
    public static class SwayamConfigurationManager
    {

        public static List<SettingDetails> AppSettings()
        {
            string strConn = ConfigurationManager.AppSettings["WebSettingsConnString"];
            string siteIdentifier = ConfigurationManager.AppSettings["WorkLoadName"] + "";

            CacheService SwayamConfigCache = new CacheService();

            var ConfigList = new List<SettingDetails>();

            string config = null;

            if (SwayamConfigCache.IsAlive())
                config = SwayamConfigCache.Get(siteIdentifier);

            string SQLstr = "Select WorkLoadName,KeyName, KeyValue,KeyDescr FROM tbl_swayamsettings where WorkLoadName= '" + siteIdentifier + "'";
            if (config == null)
            {
                using (SqlConnection con = new SqlConnection(strConn))
                {
                    using (SqlCommand cmd = new SqlCommand(SQLstr, con))
                    {
                        con.Open();
                        var result = cmd.ExecuteReader();
                        while (result.Read())
                        {
                            var SettingDetail = new SettingDetails()
                            {
                                WorkLoadName = result["WorkLoadName"] == DBNull.Value
                                ? string.Empty : Convert.ToString(result["WorkLoadName"]),

                                KeyName = result["KeyName"] == DBNull.Value
                                ? string.Empty : Convert.ToString(result["KeyName"]),

                                KeyValue = result["KeyValue"] == DBNull.Value
                                ? string.Empty : Convert.ToString(result["KeyValue"]),

                                KeyDescr = result["KeyDescr"] == DBNull.Value
                                ? string.Empty : Convert.ToString(result["KeyDescr"])

                            };
                            ConfigList.Add(SettingDetail);
                        }
                        con.Close();
                    }

                    if (SwayamConfigCache.IsAlive())
                    {
                        SwayamConfigCache.Save(siteIdentifier, ConfigList, TimeSpan.FromMilliseconds(300000));
                    }
                }
            }
            else
            {

                ConfigList = JsonConvert.DeserializeObject<List<SettingDetails>>(config);

            }

            return ConfigList;
        }

        public static string AppSettings(string KeyName)
        {
            //string KeyValue = "";
            // var ConfigList = new List<SettingDetails>();

            var ConfigList  = AppSettings();

            // ConfigList = ConfigList.Where(c => !KeyName.Contains(KeyName)).ToList();

            //return ConfigList[0].KeyValue.ToString();
            // return KeyValue;

            string KeyValue = ConfigList.First(item => item.KeyName == KeyName).KeyValue;
            return KeyValue;
            
        }

        public static void clearSetting()
        {

            string siteIdentifier = ConfigurationManager.AppSettings["WorkLoadName"] + "";
            CacheService SwayamConfigCache = new CacheService();

           // var ConfigList = new List<SettingDetails>();

            string config = null;
            try
            { 
                    if (SwayamConfigCache.IsAlive())
                    {
                        config = SwayamConfigCache.Get(siteIdentifier);
                        if (config != null)
                            SwayamConfigCache.Remove(siteIdentifier);
                    }
            }
            catch
            {

            }

            finally
            {

            }

        }

    }
}
