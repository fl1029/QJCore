﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using QJY.Common;
using QJY.Data;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;

namespace QJY.API
{
    public class QYWDManage 
    {
    


        /// <summary>
        /// 添加文件夹
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDFLODER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            FT_Folder Folder = JsonConvert.DeserializeObject<FT_Folder>(P1);
            if (Folder.Name == "")
            {
                Folder.Name = "新建文件夹";
            }
            if (Folder.ID == 0)
            {
                Folder.CRUser = UserInfo.User.UserName;
                Folder.CRDate = DateTime.Now;
                Folder.ComId = UserInfo.User.ComId;
                new FT_FolderB().Insert(Folder);


                //更新文件夹路径Code
                Folder.Remark = Folder.Remark + "-" + Folder.ID;
                new FT_FolderB().Update(Folder);
            }
            else
            {

            }
            msg.Result = Folder;

        }
        public void UPDATENAME(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strName = context.Request("Name") ?? "";
            if (strName != "")
            {
                if (P1 == "file")
                {
                    new FT_FileB().ExsSql("UPDATE FT_File SET NAME ='" + strName + "'WHERE ID = " + P2);
                }
                else
                {
                    new FT_FolderB().ExsSql("UPDATE FT_Folder SET NAME ='" + strName + "'WHERE ID = " + P2);
                }
            }

        }


        /// <summary>
        /// 移动剪切-粘贴
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void PASTEITEM(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int PID = int.Parse(P1);
            JArray PASTEITEMS = (JArray)JsonConvert.DeserializeObject(P2);
            string strPASTEtype = context.Request("PASTETYPE");
            if (strPASTEtype == "copy")
            {
                foreach (var item in PASTEITEMS)
                {
                    int itemid = int.Parse(item["ID"].ToString());
                    if (item["type"].ToString() == "file")
                    {
                        FT_File Model = new FT_FileB().GetEntity(d => d.ID == itemid);
                        Model.FolderID = PID;
                        new FT_FileB().Insert(Model);
                    }
                    else
                    {
                        new FT_FolderB().CopyFloderTree(itemid, PID, UserInfo.User.ComId.Value);

                    }
                }
            }
            else
            {
                foreach (var item in PASTEITEMS)
                {
                    int itemid = int.Parse(item["ID"].ToString());

                    if (item["type"].ToString() == "file")
                    {
                        FT_File Model = new FT_FileB().GetEntity(d => d.ID == itemid);
                        Model.FolderID = PID;
                        new FT_FileB().Update(Model);
                    }
                    else
                    {
                        FT_Folder PModel = new FT_FolderB().GetEntity(d => d.ID == PID);
                        FT_Folder Model = new FT_FolderB().GetEntity(d => d.ID == itemid);
                        Model.PFolderID = PID;
                        Model.Remark = PModel.Remark + "-" + Model.ID;
                        new FT_FolderB().Update(Model);

                        //子文件夹路径修改
                        new FT_FolderB().ExsSql("  UPDATE  FT_Folder set Remark= '" + PModel.Remark + "'+SUBSTRING(Remark, CHARINDEX('-" + Model.ID + "-',Remark), 2000) WHERE ComId=" + UserInfo.User.ComId + " AND  Remark LIKE '" + Model.Remark + "-%' ");
                    }
                }
            }



        }
        public void DELFLODER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JArray DELITEM = (JArray)JsonConvert.DeserializeObject(P1);
            foreach (var item in DELITEM)
            {
                if (item["type"].ToString() == "file")
                {//删除文件
                    new FT_FileB().Delete(d => d.ID.ToString() == item["ID"].ToString() && d.ComId == UserInfo.User.ComId);
                    new FT_File_UserAuthB().Delete(d => d.RefID.ToString() == item["ID"].ToString() && d.ComId == UserInfo.User.ComId && d.RefType == "1");
                }
                else
                {//删除目录
                    new FT_FolderB().DelWDTree(int.Parse(item["ID"].ToString()), UserInfo.User.ComId.Value);
                    new FT_File_UserAuthB().Delete(d => d.RefID.ToString() == item["ID"].ToString() && d.ComId == UserInfo.User.ComId && d.RefType == "0");

                }
            }
        }
        public void ADDFILE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            new FT_FileB().ADDFILE(msg, P1, P2, UserInfo);
        }



       

        /// <summary>
        /// 获取文件数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">文件夹类型</param>
        /// <param name="P2">上级文件夹ID</param>
        /// <param name="UserInfo"></param>
        public void GETLISTDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //默认找出企业文件夹查看属性为空或者包含当前用户的数据

            string itemtype = context.Request("itemtype") ?? "";//文件或文件夹类型（1:企业文件夹,2:个人文件夹）
            if (itemtype == "1")//企业文件夹
            {
                int FolderID = int.Parse(P1);//
                //默认找出企业文件夹查看属性为空或者包含当前用户的数据
                if (P2 == "Y")//如果是后台查看企业文件夹
                {
                    //msg.Result = new FT_FolderB().GetEntities(" ComId=" + UserInfo.User.ComId + " AND  PFolderID=" + FolderID);
                    //msg.Result1 = new FT_FileB().GetEntities("ComId=" + UserInfo.User.ComId + " AND  FolderID=" + FolderID);

                    string strSQL = string.Format("SELECT FT_Folder.*,ISNULL(FT_File_UserAuth.AuthUser, '') as AuthUser from FT_Folder LEFT JOIN  FT_File_UserAuth  on FT_Folder.ID= FT_File_UserAuth.RefID  and FT_File_UserAuth.RefType='0' where FT_Folder.ComId='{0}' and FT_Folder.PFolderID='{1}' ", UserInfo.User.ComId, FolderID.ToString());
                    msg.Result = new FT_FolderB().GetDTByCommand(strSQL);

                    string strSQLFile = string.Format("SELECT FT_File.*,ISNULL(FT_File_UserAuth.AuthUser, '') as AuthUser from FT_File LEFT JOIN  FT_File_UserAuth  on FT_File.ID= FT_File_UserAuth.RefID  and FT_File_UserAuth.RefType='1' where FT_File.ComId='{0}' and FT_File.FolderID='{1}' ", UserInfo.User.ComId, FolderID.ToString());
                    msg.Result1 = new FT_FileB().GetDTByCommand(strSQLFile);
                    return;
                }
                else
                {
                    string strSQL = string.Format("SELECT FT_Folder.*,ISNULL(FT_File_UserAuth.AuthUser, '') as AuthUser from FT_Folder LEFT JOIN  FT_File_UserAuth  on FT_Folder.ID= FT_File_UserAuth.RefID  and FT_File_UserAuth.RefType='0' where FT_Folder.ComId='{0}' and FT_Folder.PFolderID='{1}'   and (  AuthUser is NULL OR  ',' + AuthUser  + ',' like '%,{2},%' )", UserInfo.User.ComId, FolderID.ToString(), UserInfo.User.UserName);
                    msg.Result = new FT_FolderB().GetDTByCommand(strSQL);

                    string strSQLFile = string.Format("SELECT FT_File.*,ISNULL(FT_File_UserAuth.AuthUser, '') as AuthUser from FT_File LEFT JOIN  FT_File_UserAuth  on FT_File.ID= FT_File_UserAuth.RefID  and FT_File_UserAuth.RefType='1' where FT_File.ComId='{0}' and FT_File.FolderID='{1}' ", UserInfo.User.ComId, FolderID.ToString());
                    msg.Result1 = new FT_FileB().GetDTByCommand(strSQLFile);
                    return;
                }
            }
            if (itemtype == "2")//个人文件夹
            {
                int FolderID = int.Parse(P1);//
                string strSQL = string.Format("SELECT FT_Folder.*,ISNULL(FT_File_UserAuth.AuthUser, '') as AuthUser from FT_Folder LEFT JOIN  FT_File_UserAuth  on FT_Folder.ID= FT_File_UserAuth.RefID  and FT_File_UserAuth.RefType='0' where FT_Folder.ComId='{0}' and FT_Folder.PFolderID='{1}' and  FT_Folder.CRUser='{2}'", UserInfo.User.ComId, FolderID.ToString(), UserInfo.User.UserName);
                msg.Result = new FT_FolderB().GetDTByCommand(strSQL);

                string strSQLFile = string.Format("SELECT FT_File.*,ISNULL(FT_File_UserAuth.AuthUser, '') as AuthUser from FT_File LEFT JOIN  FT_File_UserAuth  on FT_File.ID= FT_File_UserAuth.RefID  and FT_File_UserAuth.RefType='1' where FT_File.ComId='{0}' and FT_File.FolderID='{1}' and  FT_File.CRUser='{2}' ", UserInfo.User.ComId, FolderID.ToString(), UserInfo.User.UserName);
                msg.Result1 = new FT_FileB().GetDTByCommand(strSQLFile);
                return;
            }
            if (itemtype == "4")//共享文档
            {
                int FolderID = int.Parse(P1);//
                msg.Result = new FT_FolderB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.PFolderID == FolderID);
                msg.Result1 = new FT_FileB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.FolderID == FolderID);
                return;
            }
            if (itemtype == "3")//我的收藏
            {

                string strWhere = string.Format(SqlHelp.concat("','+CollectUser+','") + "  like '%," + UserInfo.User.UserName + ",%'");
                string strWhere1 = strWhere;
                if (!string.IsNullOrEmpty(P1))
                {
                    strWhere1 = string.Format("  PFolderID=" + P1);
                }
                string strSQLFLODER = string.Format(@"select * from FT_Folder  where ComId=" + UserInfo.User.ComId + " and  {0}   order by CRDate desc  ", strWhere1);
                DataTable dtFLODER = new FT_FolderB().GetDTByCommand(strSQLFLODER);

                string strWhere2 = strWhere;
                if (!string.IsNullOrEmpty(P1))
                {
                    strWhere2 = string.Format("  FolderID=" + P1);
                }
                string strSQLFILE = string.Format(@"select  *  from FT_File  where  ComId=" + UserInfo.User.ComId + "   and  {0}   order by CRDate desc  ", strWhere2);
                DataTable dtFILE = new FT_FileB().GetDTByCommand(strSQLFILE);
                msg.Result = dtFLODER;
                msg.Result1 = dtFILE;

                return;
            }
        }
        public void GETFOLDERLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int FolderID = int.Parse(P1);//
            msg.Result = new FT_FolderB().GetEntities(" ComId=" + UserInfo.User.ComId + " AND  PFolderID=" + FolderID);
        }
        /// <summary>
        /// 搜索文件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">文件夹类型</param>
        /// <param name="P2">搜索关键字</param>
        /// <param name="UserInfo"></param>
        public void QUERYFILE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            msg.Result = new FT_FileB().GetDTByCommand("select FT_File.*,[FT_Folder].FolderType from FT_File left join [FT_Folder] on FT_File.FolderID=[FT_Folder].ID where FT_File.ComId=" + UserInfo.User.ComId + " AND foldertype='" + P1 + "' " + (P1 == "2" ? "And FT_File.CRUser='" + UserInfo.User.UserName + "'" : "") + " and( FT_File.Name like '%" + P2 + "%' or FT_File.FileExtendName like '%" + P2 + "%')");

        }



        public void GETFOLDERTREE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            List<FT_FolderB.FoldFileItem> ListID = new List<FT_FolderB.FoldFileItem>();
            if (P1 == "1")
            {
                msg.Result = new FT_FolderB().GetWDTREE(int.Parse(P1), ref ListID, UserInfo.User.ComId.Value);
            }
            if (P1 == "2")
            {
                msg.Result = new FT_FolderB().GetWDTREE(int.Parse(P1), ref ListID, UserInfo.User.ComId.Value, UserInfo.User.UserName);
            }
            msg.Result1 = ListID;
        }

        /// <summary>
        /// 添加外部分享链接
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDSHARECODE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string sharepad = DateTime.Now.Minute.ToString() + DateTime.Now.Millisecond.ToString();
            int ID = int.Parse(P1);
            string strType = P2;
            List<FT_File_Share> shareList = new FT_File_ShareB().GetEntities(d => d.RefID == ID && d.ComId == UserInfo.User.ComId && d.CRUser == UserInfo.User.UserName).ToList();
            FT_File_Share Model = new FT_File_Share();
            if (shareList.Count() > 0)
            {
                if (Model.IsDel != "Y")
                {
                    msg.Result1 = 1;
                }
                Model = shareList.First();
                Model.ShareURL = context.Request("url") + "?ID=" + Model.ID;
                Model.IsDel = "N";
                if (Model.ShareDueDate < DateTime.Now)
                {
                    Model.ShareDueDate = DateTime.Now.AddDays(1);
                }
                new FT_File_ShareB().Update(Model);
                msg.Result = shareList.First();
                return;
            }
            Model.ComId = UserInfo.QYinfo.ComId;
            Model.CRDate = DateTime.Now;
            Model.CRUser = UserInfo.User.UserName;
            Model.CRUserName = UserInfo.User.UserRealName;
            Model.RefID = ID;
            Model.RefType = strType;
            Model.ShareDueDate = DateTime.Now.AddDays(1);//默认当天就过期
            Model.SharePasd = "";
            Model.ShareType = "0";
            Model.ShareURL = "";
            Model.IsDel = "N";
            Model.AuthType = "0";
            new FT_File_ShareB().Insert(Model);
            Model.ShareURL = context.Request("url") + "?ID=" + Model.ID;
            new FT_File_ShareB().Update(Model);
            msg.Result = Model;
        }
        public void GETSHAREINFO(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int id = 0;
            int.TryParse(P1, out id);
            msg.Result = new FT_File_ShareB().GetEntity(d => d.ID == id && d.ComId == UserInfo.User.ComId);
        }
        /// <summary>
        /// 添加外部分享链接
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void MODIFYJZDATE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            DateTime newDate = DateTime.Now;
            if (DateTime.TryParse(P2, out newDate))
            {
                FT_File_Share share = new FT_File_ShareB().GetEntity(d => d.ID == ID);
                share.ShareDueDate = newDate;
                new FT_File_ShareB().Update(share);
                msg.Result = share;
            }
            else
            {
                msg.ErrorMsg = "请检查要更新的截止日期格式";
            }

        }
        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void MODIFYPASSWORD(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            FT_File_Share share = new FT_File_ShareB().GetEntity(d => d.ID == ID);
            share.ShareType = P2;
            string sharecode = GenerateCheckCode(6);
            if (P2 == "0")
            {
                sharecode = "";
            }
            share.SharePasd = sharecode;
            new FT_File_ShareB().Update(share);
            msg.Result = share;


        }
        /// <summary>
        /// 取消分享
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">类型（file:文件,folder:目录）</param>
        /// <param name="P2">ID</param>
        /// <param name="UserInfo"></param>
        public void DELSHARECODE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            FT_File_Share Model = new FT_File_ShareB().GetEntity(d => d.ID == ID);
            Model.IsDel = "Y";
            new FT_File_ShareB().Update(Model);
        }



        /// <summary>
        /// 收藏目录或文档
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void COLLECTITEM(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int DataID = int.Parse(P2);
            if (P1 == "file")//
            {
                FT_File Model = new FT_FileB().GetEntity(d => d.ID == DataID);
                if (string.IsNullOrEmpty(Model.CollectUser) || !Model.CollectUser.SplitTOList(',').Contains(UserInfo.User.UserName))
                {
                    Model.CollectUser = (string.IsNullOrEmpty(Model.CollectUser) ? "" : Model.CollectUser.TrimEnd(',') + ",") + UserInfo.User.UserName;
                    new FT_FileB().Update(Model);
                }
            }
            else
            {
                FT_Folder Model = new FT_FolderB().GetEntity(d => d.ID == DataID);
                if (string.IsNullOrEmpty(Model.CollectUser) || !Model.CollectUser.SplitTOList(',').Contains(UserInfo.User.UserName))
                {
                    Model.CollectUser = (string.IsNullOrEmpty(Model.CollectUser) ? "" : Model.CollectUser.TrimEnd(',') + ",") + UserInfo.User.UserName;
                    new FT_FolderB().Update(Model);
                }
            }
        }

        /// <summary>
        /// 取消收藏
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void CANCOLLECTITEM(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int DataID = int.Parse(P2);

            if (P1 == "file")//
            {
                FT_File Model = new FT_FileB().GetEntity(d => d.ID == DataID);
                if (Model.CollectUser.SplitTOList(',').Contains(UserInfo.User.UserName))
                {
                    Model.CollectUser = Model.CollectUser.Replace(UserInfo.User.UserName, ",").Replace(",,", ",").TrimEnd(',');
                    new FT_FileB().Update(Model);
                }
            }
            else
            {
                FT_Folder Model = new FT_FolderB().GetEntity(d => d.ID == DataID);
                if (Model.CollectUser.SplitTOList(',').Contains(UserInfo.User.UserName))
                {
                    Model.CollectUser = Model.CollectUser.Replace(UserInfo.User.UserName, ",").Replace(",,", ",").TrimEnd(',');
                    new FT_FolderB().Update(Model);
                }
            }
        }

        /// <summary>
        /// 获取文档ITEM
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWDITEM(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P2);
            if (P1 == "file")//
            {
                FT_File Model = new FT_FileB().GetEntity(d => d.ID == ID);
                List<FT_File_Vesion> ListVer = new FT_File_VesionB().GetEntities(D => D.ComId == UserInfo.User.ComId && D.RFileID == Model.ID).ToList();
                msg.Result = Model;
                msg.Result1 = ListVer;

            }
            else
            {
                FT_Folder Model = new FT_FolderB().GetEntity(d => d.ID == ID);
                msg.Result = Model;
            }
        }

        /// <summary>
        /// 设置权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SETAUTH(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strAuthUsers = P1;
            int DataID = int.Parse(P2);
            string RefType = context.Request("REFTYPE") == "file" ? "1" : "0";//默认文件夹
            new FT_File_UserAuthB().Delete(d => d.RefID == DataID && d.RefType == RefType && d.CRUser == UserInfo.User.UserName);

            FT_File_UserAuth Model = new FT_File_UserAuth();
            Model.ComId = UserInfo.User.ComId;
            Model.CRDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Model.CRUser = UserInfo.User.UserName;
            Model.AuthType = "0";//查看权限
            Model.AuthUser = strAuthUsers;
            Model.RefID = DataID;
            Model.RefType = RefType;//文件夹
            new FT_File_UserAuthB().Insert(Model);
        }
        public void CANCELAUTH(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string RefType = context.Request("REFTYPE") == "file" ? "1" : "0";//默认文件夹
            int DataID = int.Parse(P2);
            new FT_File_UserAuthB().Delete(d => d.RefID == DataID && d.RefType == RefType && d.CRUser == UserInfo.User.UserName);

        }


        /// <summary>
        ///更具ID获取相应具有内部授权的用户
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETNBSQUSERS(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strUserS = "";
            int DataID = int.Parse(P1);
            FT_File_UserAuth MODEL = new FT_File_UserAuthB().GetEntities(d => d.RefID == DataID && d.CRUser == UserInfo.User.UserName).FirstOrDefault();
            if (MODEL != null)
            {
                strUserS = MODEL.AuthUser;
            }
            msg.Result = strUserS;
        }



        /// <summary>
        /// 获取内部共享来源
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETNBGXLY(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSQL = " SELECT DISTINCT  FT_File_UserAuth.CRUser,JH_Auth_User.UserRealName FROM FT_File_UserAuth LEFT JOIN  JH_Auth_User ON FT_File_UserAuth.CRUser=JH_Auth_User.UserName  WHERE    " + SqlHelp.concat("','+AuthUser+','") + "   like '%," + UserInfo.User.UserName + ",%'";
            DataTable dtUserS = new FT_FolderB().GetDTByCommand(strSQL);
            msg.Result = dtUserS;
        }


        /// <summary>
        /// 获取能够查看的内部人员共享目录
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETNBSHARELIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strUser = P1;
            string strSQL = "SELECT FT_Folder.* FROM FT_File_UserAuth LEFT JOIN FT_Folder on  FT_File_UserAuth.RefID=FT_Folder.ID WHERE  " + SqlHelp.concat("','+AuthUser+','") + "  like '%," + UserInfo.User.UserName + ",%' and FT_File_UserAuth.RefType='0'";
            if (strUser != "")
            {
                strSQL = strSQL + "  AND FT_File_UserAuth.CRUser='" + strUser + "'  ";
            }

            string strSQLFile = "SELECT FT_File.* FROM FT_File_UserAuth LEFT JOIN FT_File on  FT_File_UserAuth.RefID=FT_File.ID  WHERE  " + SqlHelp.concat("','+AuthUser+','") + "   like '%," + UserInfo.User.UserName + ",%' and FT_File_UserAuth.RefType='1'";
            if (strUser != "")
            {
                strSQLFile = strSQLFile + "  AND FT_File_UserAuth.CRUser='" + strUser + "'  ";
            }
            DataTable dtFLODER = new FT_FolderB().GetDTByCommand(strSQL);
            DataTable dtFile = new FT_FileB().GetDTByCommand(strSQLFile);

            msg.Result = dtFLODER;
            msg.Result1 = dtFile;

        }



        /// <summary>
        /// 向服务器发送压缩目录命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">目录ID</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void COMPRESSFOLDER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strCode = P1;
            FT_FolderB.FoldFile Mode = new FT_FolderB.FoldFile();
            Mode.FolderID = -1;
            Mode.Name = "压缩文件";
            Mode.SubFileS = new List<FT_File>();
            Mode.SubFolder = new List<FT_FolderB.FoldFile>();
            foreach (string item in P1.SplitTOList(','))
            {
                int FileID = int.Parse(item.Split('|')[0].ToString());
                string strType = item.Split('|')[1].ToString();
                if (item.Split('|')[1].ToString() == "file")
                {
                    FT_File file = new FT_FileB().GetEntity(d => d.ID == FileID);
                    file.YLUrl = "";
                    Mode.SubFileS.Add(file);
                }
                else
                {
                    List<FT_FolderB.FoldFileItem> ListID = new List<FT_FolderB.FoldFileItem>();
                    FT_FolderB.FoldFile obj = new FT_FolderB().GetWDTREE(FileID, ref ListID, UserInfo.User.ComId.Value);
                    Mode.SubFolder.Add(obj);
                }
            }
            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string Result = JsonConvert.SerializeObject(Mode, Formatting.Indented, timeConverter).Replace("null", "\"\"");
            string strData = new FileHelp().CompressZip(Result, UserInfo.QYinfo);
            msg.Result = strData;
        }

        public void ADDVERSION(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int FolderID = int.Parse(P2);
            JArray Files = (JArray)JsonConvert.DeserializeObject(P1);
            var date = DateTime.Now;
            List<FT_File> ListData = new List<FT_File>();
            foreach (var item in Files)
            {
                int index = item["filename"].ToString().LastIndexOf('.');

                FT_File newfile = new FT_File();
                newfile.Name = item["filename"].ToString().Substring(0, index);
                newfile.FileMD5 = item["md5"].ToString();
                newfile.FileSize = item["filesize"].ToString();
                newfile.FileVersin = 0;
                newfile.CRDate = date;
                newfile.ComId = UserInfo.User.ComId;
                newfile.CRUser = UserInfo.User.UserName;
                newfile.FolderID = FolderID;
                newfile.FileExtendName = item["filename"].ToString().Substring(index + 1);
                if (new List<string>() { "pdf", "txt", "html", "doc", "docx", "ppt", "pptx", "mp4", "flv", "ogg" }.Contains(newfile.FileExtendName))
                {
                    newfile.ISYL = "Y";
                }
                ListData.Add(newfile);
            }
        }

        #region 获取应用附件列表
        /// <summary>
        /// 获取附件列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">附件Id，多个以逗号隔开</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETFILESLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            int[] fileIds = P1.SplitTOInt(',');
            msg.Result = new FT_FileB().GetEntities(d => fileIds.Contains(d.ID));
        }


        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETFILEINFO(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int FileID = int.Parse(P1);//

            FT_File MODEL = new FT_FileB().GetEntity(d => d.ID == FileID);
            msg.Result = MODEL;
            //msg.Result2 = new FT_FileB().GetYLURL(MODEL);
        }
        #endregion

        private int rep = 0;
        private string GenerateCheckCode(int codeCount)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + this.rep;
            this.rep++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> this.rep)));
            for (int i = 0; i < codeCount; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str = str + ch.ToString();
            }
            return str;
        }


        #region 转化Office
        public void LCSP_CHECK(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_TXSX TX = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);



        }
        #endregion

        #region 预览页面接口
        /// <summary>
        /// 获取文档资源
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWDZY(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWDYM = CommonHelp.GetConfig("WDYM");

            FT_File ff = new FT_FileB().GetEntities(p => p.YLCode == P1 && p.ComId == UserInfo.QYinfo.ComId).FirstOrDefault();
            if (ff != null)
            {
                msg.Result = strWDYM + ff.YLPath;
                msg.Result1 = ff.YLCount;
                msg.Result2 = ff.Name + "." + ff.FileExtendName;

            }
            else
            {
                msg.ErrorMsg = "此文件不存在或您没有权限！";
            }
        }

        /// <summary>
        /// 获取页面html(excel)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETHTML(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWDYM = CommonHelp.GetConfig("WDYM");

            FT_File ff = new FT_FileB().GetEntities(p => p.YLCode == P1 && p.ComId == UserInfo.QYinfo.ComId).FirstOrDefault();
            if (ff != null)
            {
                //定义局部变量
                HttpWebRequest httpWebRequest = null;
                HttpWebResponse httpWebRespones = null;
                Stream stream = null;
                string htmlString = string.Empty;
                string url = strWDYM + ff.YLPath;

                //请求页面
                try
                {
                    httpWebRequest = WebRequest.Create(url + ".html") as HttpWebRequest;
                }
                //处理异常
                catch
                {
                    msg.ErrorMsg = "建立页面请求时发生错误！";
                }
                httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; Maxthon 2.0)";
                //获取服务器的返回信息
                try
                {
                    httpWebRespones = (HttpWebResponse)httpWebRequest.GetResponse();
                    stream = httpWebRespones.GetResponseStream();
                }
                //处理异常
                catch
                {
                    msg.ErrorMsg = "接受服务器返回页面时发生错误！";
                }

                StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                //读取返回页面
                try
                {
                    htmlString = streamReader.ReadToEnd();
                }
                //处理异常
                catch
                {
                    msg.ErrorMsg = "读取页面数据时发生错误！";
                }
                //释放资源返回结果
                streamReader.Close();
                stream.Close();

                msg.Result = htmlString;
                msg.Result1 = url;

            }
            else
            {
                msg.ErrorMsg = "此文件不存在或您没有权限！";
            }
        }

        #endregion

        #region 判断文件是否转换成功
        public void ISCOV(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            List<FT_File> ListData = JsonConvert.DeserializeObject<List<FT_File>>(P1);
            if (ListData.Count > 0)
            {
                int[] fileId = ListData.Select(d => d.ID).ToList().ListTOString(',').SplitTOInt(',');
                int count = new FT_FileB().GetEntities(d => fileId.Contains(d.ID) && d.FileExtendName == "pdf" && d.YLPath == null).Count();
                if (count == 0)
                {
                    msg.Result = new FT_FileB().GetEntities(d => fileId.Contains(d.ID));
                }
                else
                {
                    msg.ErrorMsg = "转换未完成";
                }
            }
        }
        #endregion



        #region 我的收藏

        #region 获取我的收藏
        /// <summary>
        ///获取我的收藏
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void COLLECTLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = "";
            strWhere = string.Format(" and MsgType ='{0}' order by CRDate desc", P1);
            if (P1 == "shortvideo")
            {
                strWhere = string.Format(" and MsgType in ('shortvideo','video') order by CRDate desc");
            }
            string strSql = string.Format("SELECT ID,MsgType,FromUserName,MediaId,ThumbMediaId,MsgId,AgentID,MsgContent,PicUrl,URL,Format,CRDate,Description,FileId,Title,Tags from  JH_Auth_WXMSG  where FromUserName='{0}' AND ComId={1} AND ModeCode='QYWD' {2}",  UserInfo.User.UserName, UserInfo.User.ComId, strWhere);
            msg.Result = new JH_Auth_WXMSGB().GetDTByCommand(strSql);
        }
        #endregion

        #region 我的收藏（修改获取笔记网页）
        /// <summary>
        /// 我的收藏添加修改
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDTEXT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_WXMSG wxmsg = JsonConvert.DeserializeObject<JH_Auth_WXMSG>(P1);
            if (wxmsg.ID == 0)
            {
                wxmsg.ComId = UserInfo.User.ComId;
                wxmsg.FromUserName = UserInfo.User.UserName;
                wxmsg.CRDate = DateTime.Now;
                wxmsg.CRUser = UserInfo.User.UserName;
                wxmsg.MsgType = P2;
                wxmsg.ModeCode = "QYWD";
                new JH_Auth_WXMSGB().Insert(wxmsg);
            }
            else
            {
                new JH_Auth_WXMSGB().Update(wxmsg);
            }
            msg.Result = wxmsg;

        }


        /// <summary>
        /// 获取我的收藏byID
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">ID</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETTEXT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int DataID = 0;
            int.TryParse(P1, out DataID);
            JH_Auth_WXMSG wxmsg = new JH_Auth_WXMSGB().GetEntity(d => d.ID == DataID);
            msg.Result = wxmsg;
        }
        #endregion

        #region 删除我的收藏byID
        public void DELSC(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int[] scIDs = P1.SplitTOInt(',');
            new JH_Auth_WXMSGB().Delete(d => scIDs.Contains(d.ID));
        }

        #endregion

        /// <summary>
        /// 获取用户定义的标签
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETSCTAGS(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            List<string> Tags = new JH_Auth_WXMSGB().GetEntities(d => d.MsgType == P1 && d.CRUser == UserInfo.User.UserName && d.Tags != null && d.Tags != "" && d.ComId == UserInfo.User.ComId).Select(d => d.Tags).Distinct().ToList();
            msg.Result = Tags;
        }

        #endregion


        #region 文档知识库



        public void GETZYFLJSON(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            var query = new FT_FolderB().GetEntities(d => d.ComId == UserInfo.User.ComId.Value);
            JArray arr = new JArray();
            foreach (var item in query.Where(d => d.PFolderID == 1).OrderBy(d => d.CRDate))
            {
                JObject jsonitem =
                new JObject(
                    new JProperty("name", item.Name),
                    new JProperty("sort", "1"),
                    new JProperty("id", item.ID),
                    new JProperty("pid", item.PFolderID),
                    new JProperty("kcflitem",
                        new JArray(
                            from p in query.Where(d => d.PFolderID == item.ID)
                            orderby p.CRDate
                            select new JObject(
                                new JProperty("name", p.Name),
                                new JProperty("id", p.ID),
                                 new JProperty("pid", p.PFolderID),
                                new JProperty("sort", "1")))));

                arr.Add(jsonitem);
            }

            msg.Result = arr;
        }
        public void ADDZYFL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strName = P1;
                int PID = int.Parse(P2);
                int ID = int.Parse(context.Request("selid").ToString());

                FT_Folder MODEL = new FT_Folder();
                if (strName.Trim() != "")
                {
                    MODEL.ID = ID;
                    MODEL.PFolderID = PID;
                    MODEL.Name = strName;

                    MODEL.CRUser = UserInfo.User.UserName;
                    MODEL.CRDate = DateTime.Now;
                    MODEL.ComId = UserInfo.User.ComId.Value;
                    if (MODEL.ID == 0)
                    {
                        new FT_FolderB().Insert(MODEL);
                        //更新文件夹路径Code
                        MODEL.Remark = MODEL.Remark + "-" + MODEL.ID;
                        new FT_FolderB().Update(MODEL);
                    }
                    else
                    {
                        new FT_FolderB().Update(MODEL);
                    }
                }
                else
                {
                    msg.ErrorMsg = "请输入内容";
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }


        public void DELZYFL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            new FT_FolderB().Delete(d => d.ID == ID && d.ComId == UserInfo.User.ComId.Value);

        }
        public void GETZYGLLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = " ComId=" + UserInfo.User.ComId;
            if (P1 != "")
            {
                strWhere += string.Format(" And  title like '%{0}%'", P1);
            }
            if (context.Request("type") != "" && context.Request("type") != null)
            {
                strWhere += string.Format(" And  FolderID = '{0}'", context.Request("type") ?? "");
            }
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request("p") ?? "1", out page);
            int.TryParse(context.Request("pagecount") ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            var dt = new FT_FileB().Db.Queryable<FT_File>().Where(strWhere).OrderBy(it => it.CRDate, OrderByType.Desc).ToPageList(page, pagecount, ref total);
            msg.Result = dt;
            msg.Result1 = total;

        }

        public void DELFILE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            new FT_FileB().Delete(d => d.ID == ID && d.ComId == UserInfo.User.ComId.Value);

        }


        public void ZYADD(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int FOLERID = int.Parse(P2);

            List<JObject> ss = JsonConvert.DeserializeObject<List<JObject>>(P1);
            foreach (var s in ss)
            {
                string Title = s["filename"].ToString();
                FT_File file = new FT_File();
                file.Name = Title.Substring(0, Title.LastIndexOf('.'));
                file.zyid = s["zyid"].ToString();
                file.FileExtendName = Title.Substring(Title.LastIndexOf('.')).Trim('.');
                file.FolderID = FOLERID;
                file.CRDate = DateTime.Now;
                file.FileSize= s["filesize"].ToString();
                file.ComId = UserInfo.User.ComId.Value;
                file.CRUser = UserInfo.User.UserName;
                new FT_FileB().Insert(file);


            }
        }

        public void UPDZY(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            String zyid = context.Request("ZYID") ?? "0";


            FT_File model = new FT_FileB().GetEntity(d => d.zyid == zyid && d.ComId == UserInfo.User.ComId.Value);
            model.Name = P1;
            new FT_FileB().Update(model);
            msg.Result = model;

        }

        #endregion
    }
}