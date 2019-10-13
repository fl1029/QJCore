using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QJY.Common;
using QJY.Data;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QJY.API
{
    public class DATABIManage
    {
        #region DataSet

        //获取数据集
        public void GETBIDBSETLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = " 1=1 ";

            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request("p") ?? "1", out page);
            int.TryParse(context.Request("pagecount") ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new BI_DB_SetB().GetDataPager(" BI_DB_Set T left join BI_DB_Source S on T.SID=S.ID ", " T.*,S.Name AS SJY,S.DBType ", pagecount, page, " CRDate desc ", strWhere, ref total);


            msg.Result = dt;
            msg.Result1 = total;
            msg.Result2 = new BI_DB_SourceB().GetEntities(d => d.DBType == "SQLSERVER");

        }

        //添加修改数据集
        public void ADDBIDBSET(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tt = JsonConvert.DeserializeObject<BI_DB_Set>(P1);
            if (tt.ID == 0)
            {
                tt.CRUser = UserInfo.User.UserName;
                tt.CRDate = DateTime.Now;
                new BI_DB_SetB().Insert(tt);
            }
            else
            {
                tt.UPDate = DateTime.Now;
                new BI_DB_SetB().Update(tt);
            }


            msg.Result = tt;

        }

        //删除数据集
        public void DELBIDBSET(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int ID = int.Parse(P1);
                new BI_DB_SetB().Delete(d => d.ID == ID);
                new BI_DB_DimB().Delete(d => d.STID == ID);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }


        public void GETBIDBSET(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int ID = int.Parse(P1);
                msg.Result = new BI_DB_SetB().GetEntity(d => d.ID == ID);
                msg.Result1 = new BI_DB_DimB().GetEntities(d => d.STID == ID && d.Dimension == "1");
                msg.Result2 = new BI_DB_DimB().GetEntities(d => d.STID == ID && d.Dimension == "2");

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }

        public void UPBIDSET(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string WD = context.Request("WD") ?? "";
                string DL = context.Request("DL") ?? "";
                var tt = JsonConvert.DeserializeObject<BI_DB_Set>(P1);
                tt.UPDate = DateTime.Now;
                new BI_DB_SetB().Update(tt);

                List<BI_DB_Dim> ListWD = JsonConvert.DeserializeObject<List<BI_DB_Dim>>(WD);
                List<BI_DB_Dim> ListDL = JsonConvert.DeserializeObject<List<BI_DB_Dim>>(DL);
                new BI_DB_DimB().Delete(D => D.STID == tt.ID);

                ListWD.ForEach(D => D.CRDate = DateTime.Now);
                ListWD.ForEach(D => D.CRUser = UserInfo.User.UserName);

                ListDL.ForEach(D => D.CRDate = DateTime.Now);
                ListDL.ForEach(D => D.CRUser = UserInfo.User.UserName);

                new BI_DB_DimB().Insert(ListWD);
                new BI_DB_DimB().Insert(ListDL);


            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }


        /// <summary>
        /// 仪表盘页面使用数据集数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETYBDATASET(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                DataTable dt = new BI_DB_SetB().GetDTByCommand("select *  from BI_DB_Set  ORDER BY  ID DESC");
                dt.Columns.Add("wd", Type.GetType("System.Object"));
                dt.Columns.Add("dl", Type.GetType("System.Object"));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["wd"] = new BI_DB_SetB().GetDTByCommand("select * from BI_DB_Dim WHERE STID='" + dt.Rows[i]["ID"].ToString() + "' AND Dimension='1' ORDER BY  ColumnName");
                    dt.Rows[i]["dl"] = new BI_DB_SetB().GetDTByCommand("select * from BI_DB_Dim WHERE STID='" + dt.Rows[i]["ID"].ToString() + "' AND Dimension='2' ORDER BY  ColumnName");
                }
                msg.Result = dt;
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }



        public void JXSQL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int ID = int.Parse(P1);
                BI_DB_Set DS = new BI_DB_SetB().GetEntity(d => d.ID == ID);
                DBFactory db = new BI_DB_SourceB().GetDB(DS.SID.Value);
                DataTable dt = new DataTable();
                dt = db.GetSQL(CommonHelp.Filter(P2));
                List<BI_DB_Dim> ListDIM = new BI_DB_SetB().getCType(dt);
                ListDIM.ForEach(D => D.STID = ID);
                msg.Result = ListDIM.Where(D => D.Dimension == "1");
                msg.Result1 = ListDIM.Where(D => D.Dimension == "2");

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }




        #endregion

        #region DataSource
        //测试数据源连接
        public void TESTBIDBSOURCE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tt = JsonConvert.DeserializeObject<BI_DB_Source>(P1);
            var db = new DBFactory(tt.DBType, tt.DBIP, tt.Port, tt.DBName, tt.Schema, tt.DBUser, tt.DBPwd);
            if (db.TestConn())
            {
                msg.Result = "1"; //1：代表连接成功
            }
            else
            {
                msg.ErrorMsg = "连接失败";
            }

        }

        //获取数据源
        public void GETBIDBSOURCELIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = "1=1";
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request("p") ?? "1", out page);
            int.TryParse(context.Request("pagecount") ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            var dt = new BI_DB_SourceB().Db.Queryable<BI_DB_Source>().Where(strWhere).OrderBy(it => it.CRDate, OrderByType.Desc).ToPageList(page, pagecount, ref total);
            msg.Result = dt;
            msg.Result1 = total;
        }

        //添加修改数据源
        public void ADDBIDBSOURCE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tt = JsonConvert.DeserializeObject<BI_DB_Source>(P1);
            if (tt.ID == 0)
            {
                tt.CRUser = UserInfo.User.UserName;
                tt.CRDate = DateTime.Now;
                new BI_DB_SourceB().Insert(tt);
            }
            else
            {
                new BI_DB_SourceB().Update(tt);
            }
            msg.Result = tt;

        }

        //删除数据源
        public void DELBIDBSOURCE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int ID = int.Parse(P1);
                new BI_DB_SourceB().Delete(d => d.ID == ID);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }


        //获取数据集表名和视图名
        public void GETBIDBSOURCEVIEWLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            DBFactory db = new BI_DB_SourceB().GetDB(ID);

            msg.Result = db.GetTables();

        }


        /// <summary>
        /// 生成数据集
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>

        public void ADDBISETLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            DBFactory db = new BI_DB_SourceB().GetDB(ID);
            string strTableName = P2;
            string strDataSetName = context.Request("DsetName") ?? "1";





            BI_DB_Set DS = new BI_DB_Set();
            DS.Name = strDataSetName;
            DS.SID = ID;
            DS.SName = strTableName;
            DS.CRDate = DateTime.Now;
            DS.CRUser = UserInfo.User.UserName;
            DS.Type = "SQL";
            DS.DSQL = "SELECT  * FROM " + strTableName;
            new BI_DB_SetB().Insert(DS);




            DataTable dt = db.GetDBClient().SqlQueryable<Object>(CommonHelp.Filter("SELECT  * FROM " + strTableName)).ToDataTablePage(1,1);

            List<BI_DB_Dim> ListDIM = new BI_DB_SetB().getCType(dt);
            ListDIM.ForEach(D => D.STID = DS.ID);
            ListDIM.ForEach(D => D.CRDate = DateTime.Now);
            ListDIM.ForEach(D => D.CRUser = UserInfo.User.UserName);

            new BI_DB_DimB().Insert(ListDIM);




        }
        #endregion



        #region YBP
        public void GETYBLISTDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new BI_DB_YBPB().GetALLEntities();

        }


        public void SAVEDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            BI_DB_YBP model = new BI_DB_YBP();
            model.Name = P1;
            model.YBType = P2;
            model.CRUser = UserInfo.User.UserName;
            model.CRDate = DateTime.Now;
            new BI_DB_YBPB().Insert(model);
            msg.Result = model;
        }

        public void UPYBDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            string strFormName = context.Request("FormName") ?? "";
            string strFB = context.Request("ISFB") ?? "N";



            int ID = Int32.Parse(P1);
            BI_DB_YBP model = new BI_DB_YBPB().GetEntities(d => d.ID == ID).FirstOrDefault();
            model.YBContent = P2;
            if (strFormName != "")
            {
                model.Name = strFormName;
            }
            if (strFB == "Y")
            {
                model.YBOption = P2;
            }
            new BI_DB_YBPB().Update(model);
            msg.Result = model;
        }

        public void GETYBBYID(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            BI_DB_YBP model = new BI_DB_YBPB().GetEntities(d => d.ID == ID).FirstOrDefault();
            msg.Result = model;
        }

        public void DELYBDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            new BI_DB_YBPB().Delete(D => D.ID == ID);
            new BI_DB_DimB().Delete(D => D.STID == ID);
        }


        /// <summary>
        /// 获取仪表盘数据接口
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETYBDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                JObject wigdata = JObject.Parse(P1);



                string datatype = (string)wigdata["datatype"];//数据来源类型0:SQL,1:API
                if (datatype == "0")//SQL取数据
                {
                    string strWigdetType = (string)wigdata["component"];
                    string strDateSetName = (string)wigdata["datasetname"];
                    string filtervalsql = (string)wigdata["filtervalsql"] ?? "";
                    string ordersql = (string)wigdata["order"] ?? "";


                    string strPageCount = context.Request("pagecount") ?? "10";
                    string strquerydata = context.Request("querydata") ?? "";//查询条件数据
                    string isglquery = (string)wigdata["isglquery"] == "True" ? "Y" : "N";//关联查询条件数据
                    string strWhere = "";
                    BI_DB_Set DS = new BI_DB_SetB().GetEntities(d => d.Name == strDateSetName).FirstOrDefault();
                    DBFactory db = new BI_DB_SourceB().GetDB(DS.SID.Value);



                    //默认过滤
                    if (filtervalsql != "")
                    {
                        strWhere = " AND " + filtervalsql;
                    }

                    ///有查询字段数据并且关联查询组件时生成查询条件
                    if (strquerydata != "" && isglquery == "Y")
                    {
                        JArray categories = JArray.Parse(strquerydata);
                        foreach (JObject item in categories)
                        {
                            string FiledName = (string)item["glfiled"];
                            string ColumnType = (string)item["ColumnType"];
                            string eltype = (string)item["component"];
                            if (eltype == "qjInput")
                            {
                                string strValue = (string)item["value"];
                                if (!string.IsNullOrEmpty(strValue))
                                {
                                    string strSQL = string.Format(" AND {0} LIKE ('%{1}%')", FiledName.Replace(',', '+'), strValue);
                                    strWhere = strWhere + strSQL;
                                }
                            }
                            if (eltype == "qjSeluser" || eltype == "qjSelbranch")
                            {
                                string strValue = (string)item["value"];
                                if (!string.IsNullOrEmpty(strValue))
                                {
                                    string strSQL = string.Format(" AND {0} IN ('{1}')", FiledName.Replace(',', '+'), strValue.ToFormatLike());
                                    strWhere = strWhere + strSQL;
                                }
                            }
                            if (eltype == "qjMonth" || eltype == "qjDate")
                            {
                                if (item["value"] != null && item["value"].ToString() != "")
                                {
                                    string strval = item["value"].ToString();
                                    string sDate = strval.Split(',')[0].ToString();
                                    string eDate = strval.Split(',')[1].ToString();
                                    string strSQL = string.Format(" AND {0} BETWEEN '{1} 00:00' AND '{2} 23:59' ", FiledName, sDate, eDate);
                                    strWhere = strWhere + strSQL;
                                }

                            }

                        }
                    }
                    //if (strWigdetType == "qjTable")
                    //{
                    //    string strTablefiled = "";
                    //    JArray categoriestab = (JArray)wigdata["tabfiledlist"];//查询字段
                    //    foreach (JObject item in categoriestab)
                    //    {
                    //        string FiledName = (string)item["colid"];
                    //        string FiledJSType = (string)item["caltype"] ?? "";

                    //        strTablefiled = strTablefiled + FiledName + ",";

                    //    }

                    //    msg.Result = db.GetYBData(DS, strWD, strDL, strTablefiled, strPageCount, strWhere);

                    //}
                    if (strWigdetType == "qjChartPie" || strWigdetType == "qjKBan" || strWigdetType == "qjTable" || strWigdetType == "qjChartBar")
                    {
                        JArray wdlist = (JArray)wigdata["wdlist"];
                        JArray dllist = (JArray)wigdata["dllist"];
                        string strWD = "";
                        foreach (JObject item in wdlist)
                        {
                            strWD = strWD + (string)item["colid"] + ",";
                        }
                        strWD = strWD.TrimEnd(',');

                        string strDL = "";
                        foreach (JObject item in dllist)
                        {
                            strDL = strDL + " " + (string)item["caltype"] + "(" + (string)item["colid"] + ") AS " + (string)item["colid"] + ",";
                        }
                        strDL = strDL.TrimEnd(',');




                        msg.Result = db.GetYBData(DS, strWD, strDL, strPageCount, strWhere, ordersql);

                    }
                }
                else//API取数据
                {
                    string strAPIUrl = (string)wigdata["apiurl"] + "&szhlcode=" + UserInfo.User.pccode;
                    string str = CommonHelp.GetAPIData(strAPIUrl);
                    msg.Result = str;
                }

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }




        /// <summary>
        /// 验证API数据接口
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void YZAPIDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strAPIUrl = P1 + "&szhlcode=" + UserInfo.User.pccode;
                msg.Result = CommonHelp.GetAPIData(strAPIUrl);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }

        public void GETSQLDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string SQL = CommonHelp.Filter(P1);
            DBFactory db = new BI_DB_SourceB().GetDB(0);
            //var dt = new Dictionary<string, object>();
            //dt.Add("ID", "6988");
            //dt.Add("Remark1", "123");
            //dt.Add("Remark2", "asdasd");
            //db.UpdateData(dt, "JH_Auth_ZiDian");
            DataTable dt = db.GetSQL(SQL);
            msg.Result = dt;
        }

        #endregion

    }
}