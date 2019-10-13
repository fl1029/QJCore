using Newtonsoft.Json.Linq;
using System.Data;
using System.Reflection;

namespace QJY.API
{
    public class YCGLManage 
    {
      
        #region 车辆管理

      

        #region 获取所有车辆
        /// <summary>
        /// 获取所有车辆
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETALLCLLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("SELECT car.* from SZHL_YCGL_CAR car Where  car.Status='可用' ", UserInfo.User.ComId);
            msg.Result = new JH_Auth_UserB().GetDTByCommand(strSql);
        }
        #endregion



        #region 用车管理日历视图
        /// <summary>
        /// 用车管理日历视图
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETYCGLVIEW(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("SELECT  ycgl.ID,ycgl.intProcessStanceid,car.CarBrand,car.CarType,car.CarNum, ycgl.StartTime start, ycgl.EndTime end1  from SZHL_YCGL ycgl left outer join SZHL_YCGL_CAR car on ycgl.CarID = car.ID   where 1=1 ");
            if (P1 != "0")
            {
                strSql += string.Format(" and ycgl.CarID={0} ", P1);
            }
            DataTable dt = new JH_Auth_UserB().GetDTByCommand(strSql);
            dt.Columns.Add("end");
            dt.Columns.Add("title");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["end"] = dt.Rows[i]["end1"];
                dt.Rows[i]["title"] = dt.Rows[i]["CarBrand"].ToString() +"-"+ dt.Rows[i]["CarType"].ToString() + dt.Rows[i]["CarNum"].ToString();
            }
            msg.Result = dt;
        }
        #endregion


      
      

     
        #endregion
    }
}