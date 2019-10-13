using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QJY.Common;
using QJY.Data;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.Entities;
using Senparc.Weixin.Work.AdvancedAPIs.MailList;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace QJY.API
{
    public class AuthManage
    {


        public SqlSugarClient Db;//用来处理事务多表查询和复杂的操作



        #region 登录及密码
        public void MODIFYPWD(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            if (P1 == P2)
            {
                P1 = CommonHelp.GetMD5(P1);
                string userName = UserInfo.User.UserName;
                string uName = context.Request("username") ?? "";
                if (!string.IsNullOrEmpty(uName))
                {
                    userName = uName;
                }
                new JH_Auth_UserB().UpdatePassWord(UserInfo.User.ComId.Value, userName, P1);
            }
            else
            {
                msg.ErrorMsg = "确认密码不一致";
            }
        }
        #endregion


        #region 部门管理


        /// <summary>
        /// 添加部门
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">部门名称</param>
        /// <param name="P2">部门描述</param>
        /// <param name="strUserName"></param>
        public void ADDBRANCH(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Branch branch = new JH_Auth_Branch();
            branch = JsonConvert.DeserializeObject<JH_Auth_Branch>(P1);
            new JH_Auth_BranchB().AddBranch(UserInfo, branch, msg);
        }
        /// <summary>
        /// 获取部门信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETBRANCHBYCODE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int code = int.Parse(P1);
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, code);
            msg.Result = branch;
            msg.Result1 = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, branch.DeptRoot);
        }
        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">用户名</param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void DELBRANCH(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {


            int deptCode = int.Parse(P1);
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetEntity(d => d.DeptCode == deptCode);
            if (branch != null)
            {

                if (new JH_Auth_UserB().GetEntities(d => d.BranchCode == deptCode && d.ComId == UserInfo.User.ComId).ToList().Count > 0)
                {
                    msg.ErrorMsg = "本部门中存在用户，请先删除用户";
                    return;
                }
                if (new JH_Auth_BranchB().GetEntities(d => d.DeptRoot == branch.DeptCode).Count() > 0)
                {
                    msg.ErrorMsg = "本部门中存在子部门，请先删除子部门";
                    return;
                }
                if (UserInfo.QYinfo.IsUseWX == "Y")
                {
                    WXHelp bm = new WXHelp(UserInfo.QYinfo);
                    bm.WX_DelBranch(branch.WXBMCode.ToString());
                }
                if (!new JH_Auth_BranchB().Delete(d => d.DeptCode == deptCode))
                {
                    msg.ErrorMsg = "删除部门失败";
                    return;
                }
            }


        }
        /// <summary>
        /// 获取部门列表，分配角色组织机构以及选择组织机构用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETALLBMUSERLISTNEW(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //判断是否强制加载所有部门数据
            string isSupAdmin = UserInfo.UserRoleCode.Contains("1218") ? "Y" : "N";//是不是超级管理员
            DataTable dtBMS = new DataTable();
            int deptRoot = 1;//默认从根目录加载
            string branchId = "";

            if (isSupAdmin == "N")
            {
                deptRoot = int.Parse(UserInfo.User.remark);
                branchId = UserInfo.UserBMQXCode;
                //不是超级管理员,从单位开始加载,权限部门设为空
            }
            //获取有权限的部门Id
            string strUserTree = "[" + new JH_Auth_BranchB().GetBranchTree(deptRoot, UserInfo.User.ComId.Value, P1, branchId).TrimEnd(',') + "]";
            msg.Result = strUserTree;
            msg.Result1 = UserInfo.User.BranchCode;
        }

        //新分级显示获取部门数据
        public void GETALLBMNEW(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dtBMS = new DataTable();
            //获取有权限的部门Id
            string branchId = new JH_Auth_BranchB().GetBranchQX(UserInfo);
            int rootBrancode = 1;


            if (!UserInfo.UserRoleCode.Contains("1218"))
            {
                rootBrancode = int.Parse(UserInfo.User.remark);
            }
            DataTable dt = new JH_Auth_BranchB().GetDTByCommand("SELECT * from JH_Auth_Branch  where DeptCode=" + rootBrancode + " and ComId=" + UserInfo.User.ComId.Value + " order by DeptShort DESC");
            dt.Columns.Add("ChildBranch", Type.GetType("System.Object"));

            dtBMS = new JH_Auth_BranchB().GetBranchList(rootBrancode, UserInfo.User.ComId.Value, branchId);
            dt.Rows[0]["ChildBranch"] = dtBMS;
            msg.Result = dt;
        }
        //机构管理添加人员加载的部门列表
        public void GETDLLBRANCHLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //获取有权限的部门Id
            string branchId = new JH_Auth_BranchB().GetBranchQX(UserInfo);
            List<JH_Auth_Branch> branchList = new List<JH_Auth_Branch>();
            if (!string.IsNullOrEmpty(branchId))
            {
                int deptCode = int.Parse(branchId);
                JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, deptCode);
                string strSql = string.Format(" SELECT * from JH_Auth_Branch  where ComId={0} and (Remark1 like '%{1}%' OR DeptCode={2})", UserInfo.User.ComId, branch.DeptRoot + "-" + branch.DeptCode, branch.DeptCode); ;
                msg.Result = new JH_Auth_BranchB().GetDTByCommand(strSql);
            }
            else
            {
                branchList = new JH_Auth_BranchB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.DeptRoot != -1).ToList();

                msg.Result = branchList;
            }

        }
        #endregion

        #region 企业信息
        /// <summary>
        /// 添加或编辑企业信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void EDITCOMPANY(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_QY company = new JH_Auth_QY();
            company = JsonConvert.DeserializeObject<JH_Auth_QY>(P1);
            if (company.ComId != 0)
            {
                if (!new JH_Auth_QYB().Update(company))
                {
                    msg.ErrorMsg = "修改企业信息失败";
                }
            }
            else
            {
                JH_Auth_QY company1 = new JH_Auth_QYB().GetEntity(d => d.QYName == company.QYName);
                if (company1 != null)
                {
                    msg.ErrorMsg = "企业名称已存在";
                    return;
                }
                company.CRUser = UserInfo.User.UserName;
                company.CRDate = DateTime.Now;
                if (!new JH_Auth_QYB().Insert(company))
                {
                    msg.ErrorMsg = "添加企业信息失败";
                }
            }
        }

        /// <summary>
        /// 获取企业信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETCOMPANYINFO(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = UserInfo.QYinfo;
        }
        public void SAVECOMPANYQZ(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            UserInfo.QYinfo.DXQZ = P1;
            new JH_Auth_QYB().Update(UserInfo.QYinfo);
        }
        //更新下次不再提示
        public void UPDATECOMPANYNOALERT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_QY qymodel = UserInfo.QYinfo;
            qymodel.SystemGGId = "Y";
            new JH_Auth_QYB().Update(qymodel);
        }
        #endregion

        #region 组织部门、人员

        /// <summary>
        /// 添加人员
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void ADDUSER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            JH_Auth_User user = new JH_Auth_User();
            user = JsonConvert.DeserializeObject<JH_Auth_User>(P1);
            if (string.IsNullOrEmpty(user.UserName))
            {
                msg.ErrorMsg = "用户名必填";
                return;
            }
            if (string.IsNullOrEmpty(user.mobphone))
            {
                msg.ErrorMsg = "手机号必填";
                return;
            }
            //Regex regexPhone = new Regex("^0?1[3|4|5|8|7][0-9]\\d{8}$");
            //if (!regexPhone.IsMatch(user.mobphone))
            //{
            //    msg.ErrorMsg = "手机号填写不正确";
            //    return;
            //}
            Regex regexOrder = new Regex("^[0-9]*$");
            if (user.UserOrder != null && !regexOrder.IsMatch(user.UserOrder.ToString()))
            {
                msg.ErrorMsg = "序号必须是数字";
                return;
            }
            if (user.ID != 0)
            {
                if (UserInfo.QYinfo.IsUseWX == "Y")
                {
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.WX_UpdateUser(user);
                }
                if (!new JH_Auth_UserB().Update(user))
                {
                    msg.ErrorMsg = "修改用户失败";
                }
            }
            else
            {
                JH_Auth_User user1 = new JH_Auth_UserB().GetUserByUserName(UserInfo.QYinfo.ComId, user.UserName);
                if (user1 != null)
                {
                    msg.ErrorMsg = "用户已存在";
                    return;
                }
                List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => d.mobphone == user.mobphone && d.ComId == UserInfo.User.ComId).ToList();
                if (userList.Count > 0)
                {
                    msg.ErrorMsg = "此手机号的用户已存在";
                    return;
                }
                user.UserPass = CommonHelp.GetMD5(user.UserPass);
                user.ComId = UserInfo.User.ComId;
                if (UserInfo.QYinfo.IsUseWX == "Y")
                {
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.WX_CreateUser(user);
                }
                user.CRDate = DateTime.Now;
                user.CRUser = UserInfo.User.UserName;
                user.logindate = DateTime.Now;
                JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(user.ComId.Value, user.BranchCode);

                user.remark = branch.Remark1.Split('-')[0];
                if (!new JH_Auth_UserB().Insert(user))
                {
                    msg.ErrorMsg = "添加用户失败";
                }

            }

            if (P2 != "")
            {
                new JH_Auth_UserRoleB().Delete(d => d.UserName == user.UserName);
                foreach (string code in P2.Split(','))
                {
                    //添加默认员工角色
                    JH_Auth_UserRole Model = new JH_Auth_UserRole();
                    Model.UserName = user.UserName;
                    Model.RoleCode = int.Parse(code);
                    Model.ComId = user.ComId;
                    new JH_Auth_UserRoleB().Insert(Model);
                }
            }

            msg.Result = user;


        }
        //批量设置部门
        public void PLSETBRANCH(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string[] userNames = P1.Split(',');
            int branchCode = 0;
            int.TryParse(P2, out branchCode);
            List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => d.ComId == UserInfo.User.ComId && userNames.Contains(d.UserName)).ToList();
            foreach (JH_Auth_User user in userList)
            {
                JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(user.ComId.Value, branchCode);

                user.BranchCode = branchCode;
                user.remark = branch.Remark1.Split('-')[0];
                if (UserInfo.QYinfo.IsUseWX == "Y")
                {
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.WX_UpdateUser(user);
                }
                new JH_Auth_UserB().Update(user);
            }
        }
        /// <summary>
        /// 根据用户删除用户
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">用户名</param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void DELUSER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            if (UserInfo.QYinfo.IsUseWX == "Y")
            {
                WXHelp bm = new WXHelp(UserInfo.QYinfo);
                bm.WX_DelUser(P1);
            }
            if (!new JH_Auth_UserB().Delete(d => d.ComId == UserInfo.QYinfo.ComId && d.UserName == P1))
            {
                msg.ErrorMsg = "删除失败";
            }


        }
        /// <summary>
        /// 更改用户是否禁用的状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">用户名</param>
        /// <param name="P2">状态</param>
        /// <param name="strUserName"></param>
        public void UPDATEUSERISUSE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            JH_Auth_User UPUser = new JH_Auth_UserB().GetUserByUserName(UserInfo.QYinfo.ComId, P1);
            UPUser.IsUse = P2;

            if (UserInfo.QYinfo.IsUseWX == "Y")
            {
                WXHelp bm = new WXHelp(UserInfo.QYinfo);
                bm.WX_UpdateUser(UPUser);//为了更新微信用户状态
            }
            if (!new JH_Auth_UserB().Update(UPUser))
            {
                msg.ErrorMsg = "更新失败";
            }

        }

        public void SETISSHOWYD(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_User user = UserInfo.User;
            user.IsShowYD = 1;
            new JH_Auth_UserB().Update(user);
        }

        /// <summary>
        /// 根据部门编号获取部门人员
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETUSERBYCODENEW(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int deptCode = 0;
            if (!int.TryParse(P1, out deptCode))
            {
                deptCode = 1;
            }
            DataTable dt = new JH_Auth_UserB().GetUserListbyBranch(deptCode, P2, UserInfo.QYinfo.ComId);
            dt.Columns.Add("ROLENAME");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["ROLENAME"] = new JH_Auth_UserRoleB().GetRoleNameByUserName(dt.Rows[i]["UserName"].ToString(), UserInfo.User.ComId.Value);
            }
            msg.Result = dt;
        }
        /// <summary>
        /// 根据部门编号获取部门人员
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETUSERBYCODENEWPAGE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int deptCode = 0;
            int.TryParse(P1, out deptCode);
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, deptCode);
            if (branch == null) { msg.ErrorMsg = "数据异常"; }
            string strQXWhere = "";
            if (deptCode != 1)
            {
                strQXWhere = string.Format("And  b.Remark1 like '%{0}%'", branch.DeptCode);

            }
            string strWhere = string.Format("u.ComId={0}   {1}", UserInfo.User.ComId, strQXWhere);
            if (P2 != "")
            {
                strWhere += string.Format(" And (u.UserName like '%{0}%'  or u.UserRealName like '%{0}%'  or b.DeptName like '%{0}%' or u.mobphone like '%{0}%' ) ", P2);
            }
            int page = 0;
            int pagecount = 8;
            int.TryParse(context["p"] == null ? "0" : context["p"].ToString(), out page);
            int.TryParse(context["pagecount"] == null ? "8" : context["pagecount"].ToString(), out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new JH_Auth_UserB().Db.SqlQueryable<Object>("select u.*,b.DeptName,b.DeptCode from JH_Auth_User u  inner join JH_Auth_Branch b on u.branchCode=b.DeptCode where " + strWhere).OrderBy("UserOrder asc").ToDataTablePage(page, pagecount, ref total);
            dt.Columns.Add("ROLENAME");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["ROLENAME"] = new JH_Auth_UserRoleB().GetRoleNameByUserName(dt.Rows[i]["UserName"].ToString(), UserInfo.User.ComId.Value);
            }
            msg.Result = dt;
            msg.Result1 = Math.Ceiling(total * 1.0 / 8);
            msg.Result2 = total;
        }
        //导出员工


        /// <summary>
        /// 根据部门编号获取可用人员
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETYUSERBYCODE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int deptCode = 0;
            if (!int.TryParse(P1, out deptCode))
            {
                deptCode = 1;
            }
            DataTable dtUser = new JH_Auth_UserB().GetUserListbyBranchUse(deptCode, P2, UserInfo);
            msg.Result = dtUser;
        }
        //根据角色获取用户
        public void GETUSERBYROLECODE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int roleCode = int.Parse(P1);
            DataTable dt = new JH_Auth_UserRoleB().GetUserDTByRoleCode(roleCode, UserInfo.User.ComId.Value);

            if (!UserInfo.UserRoleCode.Contains("1218"))
            {
                msg.Result = dt.FilterTable("remark=" + UserInfo.User.remark);//根据权限找同一个单位得
            }
            else
            {
                msg.Result = dt;
            }

        }
        /// <summary>
        /// 获取前端需要的人员选择列表
        /// </summary>
        public void GETUSERJS(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dtUsers = new JH_Auth_UserB().GetDTByCommand(" SELECT UserName,UserRealName,DeptCode,DeptName FROM JH_Auth_User INNER JOIN JH_Auth_Branch ON JH_Auth_User.ComId=JH_Auth_Branch.ComId AND JH_Auth_User.BranchCode=JH_Auth_Branch.DeptCode WHERE  JH_Auth_Branch.ComId='" + UserInfo.User.ComId + "'");
            //获取选择用户需要的HTML和转化用户名需要的json数据
            msg.Result = dtUsers;
        }

        public void SETBRANCHLEADER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("UPDATE JH_Auth_Branch set BranchLeader='{0}' where  DeptCode={1}", P1, P2);
            new JH_Auth_BranchB().ExsSql(strSql);
        }
        public void SETUSERLEADER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("UPDATE JH_Auth_User set UserLeader='{0}' where  username='{1}' and ComID={2}", P1, P2, UserInfo.User.ComId);
            new JH_Auth_BranchB().ExsSql(strSql);
        }
        #endregion




        #region 角色管理
        public void EDITROLE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Role role = new JH_Auth_Role();
            role = JsonConvert.DeserializeObject<JH_Auth_Role>(P1);

            if (role.RoleCode != 0)
            {
                if (role.RoleCode == 0 && P2 == "")
                {
                    msg.ErrorMsg = "管理员至少有一人！！！";
                    return;
                }
                if (!new JH_Auth_RoleB().Update(role))
                {
                    msg.ErrorMsg = "修改职务失败";
                }

            }
            else
            {
                if (string.IsNullOrEmpty(role.ComId.ToString()))
                {
                    JH_Auth_Role user1 = new JH_Auth_RoleB().GetEntity(d => d.RoleName == role.RoleName && d.ComId == UserInfo.User.ComId && d.IsUse == "Y");
                    if (user1 != null)
                    {
                        msg.ErrorMsg = "职务已存在";
                        return;
                    }
                    role.isSysRole = "N";
                    role.ComId = UserInfo.User.ComId;
                    if (!new JH_Auth_RoleB().Insert(role))
                    {
                        msg.ErrorMsg = "添加职务失败";
                    }
                }

            }
            if (msg.ErrorMsg == "")
            {
                new JH_Auth_UserRoleB().Delete(d => d.RoleCode == role.RoleCode);
            }
            if (P2 != "")
            {
                try
                {
                    var insertObjs = new List<JH_Auth_UserRole>();

                    foreach (string name in P2.Split(','))
                    {
                        insertObjs.Add(new JH_Auth_UserRole() { UserName= name, ComId= UserInfo.User.ComId , RoleCode= role.RoleCode });
                    }
                    var s9 = new JH_Auth_UserRoleB().Db.Insertable(insertObjs.ToArray()).ExecuteCommand();
                }
                catch (Exception ex)
                {
                    msg.ErrorMsg = ex.Message;
                }
            }
            msg.Result = role;
        }

        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETROLE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = " where r.PRoleCode<>-1 and r.RoleCode!=0 and ( r.ComId=0 or  r.ComId=" + UserInfo.User.ComId + ")";
            if (P2 == "Y")
            {
                //去掉超级管理员
                strWhere = strWhere + " AND  r.RoleCode !='1218'";

            }
            if (P1 != "")
            {
                int Id = int.Parse(P1);
                msg.Result1 = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, Id);
            }
            DataTable dt = new JH_Auth_RoleB().GetDTByCommand(@" select r.RoleCode,r.RoleName,r.isSysRole,r.RoleQX,r.IsHasQX,COUNT(distinct u.UserName) userCount from JH_Auth_Role r left join JH_Auth_UserRole ur on r.RoleCode=ur.RoleCode and ur.ComId=" + UserInfo.User.ComId + @"     
                                                               left join JH_Auth_User u on ur.UserName=u.UserName  and u.ComId=" + UserInfo.User.ComId + strWhere + " group by r.RoleCode,r.RoleName,r.isSysRole,r.RoleQX,r.IsHasQX");

            msg.Result = dt;
            msg.Result2 = UserInfo.QYinfo.ComId.ToString();

        }

        public void DELROLE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            JH_Auth_Role role = new JH_Auth_RoleB().GetEntity(d => d.RoleCode == ID);
            if (role != null && role.ComId != 0 && role.RoleCode != 2)
            {
                new JH_Auth_RoleB().delRole(ID, UserInfo.User.ComId.Value);
            }
            else
            {
                msg.ErrorMsg = "此职务不能删除";
            }
        }
        public void GETROLEBYCODE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            int ComId = UserInfo.User.ComId.Value;
            if (Id == 0)
            {
                ComId = 0;
            }
            msg.Result = new JH_Auth_RoleB().GetEntity(d => d.RoleCode == Id && d.ComId == ComId);
            int roleCode = int.Parse(P1);
            msg.Result1 = new JH_Auth_UserRoleB().GetDTByCommand("SELECT DISTINCT u.UserName,userrole.RoleCode from JH_Auth_User u inner join   JH_Auth_UserRole  userrole on u.username=userrole.username where userrole.RoleCode=" + roleCode);
        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETROLEFUN(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            if (!string.IsNullOrEmpty(P1) && UserInfo.UserRoleCode != "" && UserInfo.User.isSupAdmin != "Y")
            {
                msg.Result = new JH_Auth_RoleB().GetModelFun(UserInfo.User.ComId.Value, UserInfo.UserRoleCode, P1);
            }
            else
            {

                msg.Result = new JH_Auth_RoleB().GetModelFun(UserInfo.User.ComId.Value, "0", P1);
            }
        }
        //删除角色人员
        public void DELROLEUSER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int roleCode = 0;
            int.TryParse(P1, out roleCode);
            new JH_Auth_UserRoleB().Delete(d => d.RoleCode == roleCode && d.UserName == P2 && d.ComId == UserInfo.User.ComId);
        }

        #endregion




        #region 消息中心
        public void GETXXZXIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format(" ComId={0} and UserTO='{1}' and isRead=0", UserInfo.User.ComId, UserInfo.User.UserName);
            string strSQL = string.Format(@"SELECT  MsgType,MsgContent,CRDate,UserFrom,isRead,ID,MsgLink from JH_Auth_User_Center Where  {0}  order by CRDate desc", strWhere);
            DataTable dt = new JH_Auth_User_CenterB().GetDTByCommand(strSQL);
            msg.Result = dt;
            msg.Result1 = new JH_Auth_User_CenterB().ExsSclarSql("SELECT count(0) from  JH_Auth_User_Center Where  " + strWhere);
        }
        //抄送给我的消息
        public void GETXXZXABOUTLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format("  isCS='Y'  And  ComId={0} and UserTO='{1}' and isRead=0 ", UserInfo.User.ComId, UserInfo.User.UserName);
            if (P1 != "")
            {
                strWhere += string.Format(" And MsgType='{0}'", P1);
            }
            string strSQL = string.Format(@"SELECT * from JH_Auth_User_Center Where {0}", strWhere);
            var dt = new JH_Auth_User_CenterB().Db.SqlQueryable<JH_Auth_User_Center>(strSQL).OrderBy(d => d.CRDate, OrderByType.Desc).Take(8);
            msg.Result = dt;
            msg.Result1 = new JH_Auth_User_CenterB().ExsSclarSql("SELECT count(0) from  JH_Auth_User_Center Where  " + strWhere);
        }
        //抄送给我的消息
        public void GETXXZXABOUTTYPE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format("  isCS='Y'  And  ComId={0} and UserTO='{1}' and isRead=0 ", UserInfo.User.ComId, UserInfo.User.UserName);
            string strSQL = string.Format(@"SELECT DISTINCT MsgType from JH_Auth_User_Center Where {0} ", strWhere);
            DataTable dt = new JH_Auth_User_CenterB().GetDTByCommand(strSQL);
            msg.Result = dt;

        }
        /// <summary>
        /// 获取用户消息列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">消息类型</param>
        /// <param name="P2">消息内容模糊查询</param>
        /// <param name="strUserName"></param>
        public void GETXXZXISTPAGE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format(" ComId={0}  And UserTO='{1}'", UserInfo.User.ComId, UserInfo.User.UserName);
            if (P1 != "")
            {
                strWhere += string.Format(" and isRead in ({0}) ", P1);
            }
            if (P2 != "")
            {
                strWhere += string.Format(" and MsgContent like  '%{0}%'", P2);

            }
            string msgType = context["msgType"] != null ? context["msgType"].ToString() : "";
            if (msgType != "")
            {
                strWhere += string.Format(" and MsgType ='{0}'", msgType);
            }
            string msgTypes = context["msgTypes"] != null ? context["msgTypes"].ToString() : "";
            if (msgTypes != "")
            {
                msgTypes = System.Web.HttpUtility.UrlDecode(msgTypes);
                strWhere += string.Format(" and MsgModeID in ('{0}')", msgTypes.Replace(",", "','"));
            }

            int page = 0;
            int pagecount = 8;
            int.TryParse(context["pagecount"] != null ? context["pagecount"].ToString() : "8", out pagecount);
            int.TryParse(context["p"] != null ? context["p"].ToString() : "0", out page);
            page = page == 0 ? 1 : page;
            int total = 0;
            var dt = new JH_Auth_User_CenterB().Db.Queryable<JH_Auth_User_Center>().Where(strWhere).Select("MsgType,MsgModeID,MsgContent,CRDate,UserFrom,isRead,ID,wxLink,MsgLink").OrderBy(it => it.CRDate).ToPageList(page, pagecount, ref total);

            msg.Result = dt;
            msg.Result1 = total;

        }

        //是否有未读消息
        public void HASREADMSG(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string SQL = string.Format("select count(1) from JH_Auth_User_Center where ComId={0} and UserTO='{1}' and isRead=0 ", UserInfo.User.ComId, UserInfo.User.UserName);
            object ss2 = new JH_Auth_User_CenterB().ExsSclarSql(SQL);
            if (ss2 != null)
            {
                msg.Result = "1";  //1:标记有未读消息
            }
        }
        /// <summary>
        /// 更新读取状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void UPDTEREADSTATES(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strSql = "";
                string status = context["s"] != null ? context["s"].ToString() : "1";

                strSql = string.Format("UPDATE JH_Auth_User_Center set isRead='{0}' where ID in ({1}) ", status, P1);

                new JH_Auth_User_CenterB().ExsSql(strSql);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }

        /// <summary>
        /// 根据消息类别设置消息状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void UPMSGSTATE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strSql = "";
                strSql = string.Format("UPDATE JH_Auth_User_Center set isRead='1' where MsgModelID = '{0}' AND UserTO='{1}' ", P1, UserInfo.User.UserName);
                new JH_Auth_User_CenterB().ExsSql(strSql);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }
        /// <summary>
        /// 获取消息中心类型
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETUSERCENTERTYPE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Empty;
            if (P1 != "")
            {
                string msgTypes = System.Web.HttpUtility.UrlDecode(P1);
                strWhere += string.Format(" and MsgModeID in ('{0}')", msgTypes.Replace(",", "','"));
            }
            string strSql = string.Format("SELECT  DISTINCT MsgType from JH_Auth_User_Center where ComId={0} and  userTo='{1}' " + strWhere, UserInfo.User.ComId, UserInfo.User.UserName);
            msg.Result = new JH_Auth_User_CenterB().GetDTByCommand(strSql);
        }
        //获取详细详情
        public void GETXXZXMODEL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            JH_Auth_User_Center userCenter = new JH_Auth_User_CenterB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            msg.Result = userCenter;
        }

        //删除消息中心消息
        public void DELXXZX(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                if (P1 != "")
                {
                    string strSql = string.Format("Delete JH_Auth_User_Center where ID in ({0}) ", P1);
                    new JH_Auth_User_CenterB().ExsSql(strSql);
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }
        public void GETXXZXMODELINFO(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format(@"SELECT DISTINCT MsgType,MsgModeID,SUM( case when isRead=0 then 1 else 0 END) num ,MAX(CRDate) CRDate 
                                      from JH_Auth_User_Center where ComId={0}  and UserTO='{1}' group by MsgType,MsgModeID order by CRDate DESC", UserInfo.User.ComId, UserInfo.User.UserName);
            DataTable dt = new JH_Auth_User_CenterB().GetDTByCommand(strSql);
            dt.Columns.Add("NewXX", Type.GetType("System.Object"));
            foreach (DataRow row in dt.Rows)
            {
                string strSql2 = "SELECT  * from JH_Auth_User_Center where ComId=" + UserInfo.User.ComId + "  and   UserTO='" + UserInfo.User.UserName + "' and MsgType='" + row["MsgType"] + "' ";
                if (row["num"].ToString() != "0")
                {
                    row["NewXX"] = new JH_Auth_User_CenterB().GetDTByCommand(strSql2 + " and isRead=0 order by CRDate DESC").SplitDataTable(1, 1);
                }
                else
                {
                    row["NewXX"] = new JH_Auth_User_CenterB().GetDTByCommand(strSql2 + " and isRead=1 order by CRDate DESC").SplitDataTable(1, 1);
                }
            }
            msg.Result = dt.OrderBy(" num desc");
        }
        #endregion

        public void GETUSERBYUSERNAME(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //如果获取当前用户信息，直接返回，否则按用户名查找
            if (P1 == UserInfo.User.UserName)
            {
                msg.Result = UserInfo;
                if (!string.IsNullOrEmpty(UserInfo.User.Files))
                {
                    int[] FilesIds = UserInfo.User.Files.SplitTOInt(',');
                    msg.Result1 = new FT_FileB().GetEntities(d => FilesIds.Contains(d.ID));
                    msg.Result2 = UserInfo.QYinfo.CRDate.Value.AddYears(1);
                }
            }
            else
            {
                UserInfo = new JH_Auth_UserB().GetUserInfo(UserInfo.User.ComId.Value, P1);
                msg.Result = UserInfo;
                if (!string.IsNullOrEmpty(UserInfo.User.Files))
                {
                    int[] FilesIds = UserInfo.User.Files.SplitTOInt(',');
                    msg.Result1 = new FT_FileB().GetEntities(d => FilesIds.Contains(d.ID));
                    msg.Result2 = UserInfo.QYinfo.CRDate.Value.AddYears(1);
                }
            }
        }









        #region 字典管理
        public void GETZIDIANLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var zdlist = new JH_Auth_ZiDianB().GetEntities(d => d.Class.ToString() == P1 && d.ComId == UserInfo.User.ComId && d.Remark == "0");
            if (P2 != "")//内容查询
            {
                zdlist = zdlist.Where(d => d.TypeName.Contains(P2));
            }
            msg.Result = zdlist;
        }
        public void GETZIDIANSLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dt = new DataTable();
            if (P1 != "")
            {
                string[] strs = P1.Split(',');
                if (strs.Length > 1)
                {
                    dt.Columns.Add("Class", Type.GetType("System.String"));
                    dt.Columns.Add("Item", Type.GetType("System.Object"));

                    foreach (string str in strs)
                    {
                        DataRow dr = dt.NewRow();
                        dr["Class"] = str;
                        dr["Item"] = new JH_Auth_ZiDianB().GetEntities(d => d.Class.ToString() == str && d.ComId == UserInfo.User.ComId && d.Remark == "0");
                        dt.Rows.Add(dr);
                    }
                }
                else
                {
                    dt = new JH_Auth_ZiDianB().GetEntities(d => d.Class.ToString() == P1 && d.ComId == UserInfo.User.ComId && d.Remark == "0").ToDataTable();
                }
            }
            else if (P2 != "")
            {
                dt = new JH_Auth_ZiDianB().GetEntities(d => d.Class.ToString() == P2 && d.ComId == UserInfo.User.ComId && d.Remark == "0").ToDataTable();
            }
            msg.Result = dt;
        }
        //获取CRM分类设置列表
        public void GETCRMZIDIANLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int[] classid = new int[] { 10, 11, 12, 13, 14, 15, 16, 17 };
            List<JH_Auth_ZiDian> ZiDianList = new JH_Auth_ZiDianB().GetEntities(d => classid.Contains(d.Class.Value) && (d.ComId == 0 || d.ComId == UserInfo.User.ComId) && d.Remark == "0").ToList();
            DataTable dt = new DataTable();
            dt.Columns.Add("Class");
            dt.Columns.Add("ZiDianDataList", Type.GetType("System.Object"));
            for (int i = 10; i < 18; i++)
            {
                DataRow row = dt.NewRow();
                row["Class"] = i;
                row["ZiDianDataList"] = ZiDianList.Where(d => d.Class == i).ToList();
                dt.Rows.Add(row);
            }
            msg.Result = dt;
        }
        public void DELTYPEBYID(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            JH_Auth_ZiDian zidian = new JH_Auth_ZiDianB().GetEntity(d => d.ID == Id);
            if (zidian != null)
            {

                if (zidian.ComId == 0)
                {
                    msg.ErrorMsg = "此类型不能删除";
                }
                else if (zidian.Remark == "0")
                {
                    zidian.Remark = "1";
                    new JH_Auth_ZiDianB().Update(zidian);
                }
            }
        }
        public void SAVETYPEMODEL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_ZiDian zidian = JsonConvert.DeserializeObject<JH_Auth_ZiDian>(P1);
            if (zidian.TypeName.Length > 18)
            {
                msg.ErrorMsg = "分类名称建议不超过10个字!";
                return;
            }
            List<JH_Auth_ZiDian> zidiannew = new JH_Auth_ZiDianB().GetEntities(d => d.TypeName == zidian.TypeName && d.Class == zidian.Class && d.ComId == UserInfo.QYinfo.ComId && d.Remark != "1" && d.ID != zidian.ID).ToList();

            if (zidiannew.Count > 0)
            {
                msg.ErrorMsg = "此分类已存在";
                return;
            }
            if (zidian.ID == 0)
            {
                zidian.ComId = UserInfo.User.ComId;
                zidian.Remark = "0";
                zidian.CRDate = DateTime.Now;
                zidian.CRUser = UserInfo.User.UserName;
                new JH_Auth_ZiDianB().Insert(zidian);

            }
            else
            {
                new JH_Auth_ZiDianB().Update(zidian);
            }
            msg.Result = zidian;
        }
        #endregion

        #region 微信使用
        /// <summary>
        /// 获取微信选人列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWXUSER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            List<WXUserBR> list = new List<WXUserBR>();
            //获取所有部门
            var listALL = new JH_Auth_BranchB().GetEntities(p => p.ComId == UserInfo.User.ComId).ToList();

            //第一级显示的部门
            var list0 = new List<JH_Auth_Branch>();

            //获取顶级部门信息，加载第二级部门信息列表
            var dcbm = new JH_Auth_BranchB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.DeptRoot == -1);
            list0 = listALL.Where(p => p.DeptRoot == dcbm.DeptCode).OrderBy(p => p.DeptShort).ToList();
            //获取用户所在部门没有权限看到的部门Ids
            string branchId = new JH_Auth_BranchB().GetBranchQX(UserInfo);
            //如果有不能看到的部门Ids,排除用户不能看到的部门
            if (!string.IsNullOrEmpty(branchId))
            {
                int[] noqxBranch = branchId.SplitTOInt(',');
                list0 = list0.Where(d => !noqxBranch.Contains(d.DeptCode)).ToList();
            }

            //var list0 = listALL.Where(p => p.DeptCode == dept).OrderBy(p => p.DeptShort).ToList();
            //循环要显示的第一级部门信息加载部门以下的部门及部门人员信息
            foreach (var v in list0)
            {
                WXUserBR wx = new WXUserBR();
                wx.DeptCode = v.DeptCode;
                wx.DeptName = v.DeptName;
                var users = new JH_Auth_UserB().GetEntities(d => d.BranchCode == v.DeptCode && d.IsUse == "Y").ToList().Select(d => new { d.ID, d.UserName, d.UserRealName, d.telphone, d.mobphone, d.zhiwu });
                wx.DeptUser = users;
                wx.DeptUserNum = users.Count();
                GetNextWxUser(wx, listALL);
                list.Add(wx);
            }

            msg.Result = list;

            DataTable dtUser = new JH_Auth_UserB().GetUserListbyBranchUse(dcbm.DeptCode, "", UserInfo);

            msg.Result1 = dtUser.DelTableCol("ID,UserName,UserRealName,mobphone,telphone,zhiwu");
            msg.Result2 = UserInfo.QYinfo.QYCode;
            msg.Result3 = new JH_Auth_UserB().GetEntities(d => d.BranchCode == dcbm.DeptCode && d.IsUse == "Y").ToList().Select(d => new { d.ID, d.UserName, d.UserRealName, d.telphone, d.mobphone, d.zhiwu });
        }
        /// <summary>
        /// 获取部门下的列表
        /// </summary>
        /// <param name="wx"></param>
        /// <param name="listALL"></param>
        public void GetNextWxUser(WXUserBR wx, List<JH_Auth_Branch> listALL)
        {
            var list = (from p in listALL
                        where p.DeptRoot == wx.DeptCode
                        orderby p.DeptShort
                        select new WXUserBR { DeptCode = p.DeptCode, DeptName = p.DeptName }).ToList();

            wx.SubDept = list;
            foreach (var v in list)
            {
                var users = new JH_Auth_UserB().GetEntities(d => d.BranchCode == v.DeptCode && d.IsUse == "Y").ToList().Select(d => new { d.ID, d.UserName, d.UserRealName, d.telphone, d.mobphone, d.UserGW });
                v.DeptUser = users;
                v.DeptUserNum = users.Count();
                wx.DeptUserNum = wx.DeptUserNum + users.Count();
                GetNextWxUser(v, listALL);
            }
        }

        /// <summary>
        /// 初始化移动端
        /// </summary>
        public void WXINIT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dtUsers = new JH_Auth_UserB().GetDTByCommand(" SELECT UserName,UserRealName,mobphone FROM JH_Auth_User where ComId='" + UserInfo.User.ComId + "'");
            //获取选择用户需要的HTML和转化用户名需要的json数据
            msg.Result = dtUsers;
            JH_Auth_Common url = new JH_Auth_CommonB().GetEntity(p => p.ModelCode == P1 && p.MenuCode == P2);
            if (url != null)
            {
                msg.Result1 = url.Url1;
            }
            msg.Result2 = UserInfo.User.UserName + "," + UserInfo.User.UserRealName + "," + UserInfo.User.BranchCode + "," + UserInfo.BranchInfo.DeptName;
            msg.Result3 = UserInfo.QYinfo.FileServerUrl;
            msg.Result4 = UserInfo.QYinfo.QYCode;

        }


        public void GETUSERINFO(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = UserInfo;
        }

        /// <summary>
        /// 搜索关键字对应的用户和部门
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETSEARCHINFO(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new JH_Auth_BranchB().GetEntities(D => D.DeptName.Contains(P1));
            msg.Result1 = new JH_Auth_BranchB().GetEntities(D => D.DeptName.Contains(P1));

        }





        #endregion





        #region 菜单应用

        //获取应用菜单及接口
        public void GETFUNCTION(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("select JH_Auth_Model.* from  JH_Auth_Model WHERE JH_Auth_Model.ComId='0' and JH_Auth_Model.PModelCode<>'' ORDER by ModelType");
            DataTable dt = new JH_Auth_ModelB().GetDTByCommand(strSql);
            dt.Columns.Add("FunData", Type.GetType("System.Object"));
            DataTable dtRoleFun = new JH_Auth_RoleFunB().GetDTByCommand(@"SELECT DISTINCT fun.*,rolefun.ActionCode RoleFun,rolefun.FunCode   from JH_Auth_Function fun left join JH_Auth_RoleFun rolefun on fun.ID=rolefun.FunCode and rolefun.ComId=" + UserInfo.User.ComId + " and rolefun.RoleCode= " + P1 + " Where  fun.ComId=0 or fun.ComId=" + UserInfo.User.ComId + " ORDER BY fun.FunOrder ");
            int roleId = 0;
            int.TryParse(P1, out roleId);
            JH_Auth_Role roleModel = new JH_Auth_RoleB().GetEntity(d => d.RoleCode == roleId && d.ComId == UserInfo.User.ComId);
            string isinit = "N";//是否需要默认加载权限
            if (roleModel.isSysRole == "Y")
            {
                DataRow[] roleFun = dtRoleFun.Select(" RoleFun is not null");
                isinit = roleFun.Count() > 0 ? "N" : "Y";//>0已分配过权限，==0未分配权限
            }

            foreach (DataRow row in dt.Rows)
            {
                int modelId = int.Parse(row["ID"].ToString());
                row["FunData"] = dtRoleFun.FilterTable(" ModelID =" + modelId);
            }
            msg.Result = dt;
            msg.Result1 = isinit;
        }
        //添加角色接口权限
        public void ADDROLEFUN(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int roleCode = int.Parse(P1);
            //删除之前设置的接口权限
            new JH_Auth_RoleFunB().Delete(d => d.ComId == UserInfo.User.ComId && d.RoleCode == roleCode);
            //添加要设置的接口权限
            List<JH_Auth_RoleFun> roleFunList = JsonConvert.DeserializeObject<List<JH_Auth_RoleFun>>(P2);
            foreach (JH_Auth_RoleFun fun in roleFunList)
            {
                fun.ComId = UserInfo.User.ComId;
                new JH_Auth_RoleFunB().Insert(fun);
            }
        }
        //获取应用二级菜单
        public void GETFUNCTIONDATE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int modelId = int.Parse(P1);
            msg.Result = new JH_Auth_FunctionB().GetEntities(d => d.ModelID == modelId);
        }
        public void DELFUNDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            new JH_Auth_FunctionB().Delete(d => d.ID == Id);
        }

        //获取二级菜单详细
        public void GETFUNCTIONMODEL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            msg.Result = new JH_Auth_FunctionB().GetEntity(d => d.ID == Id);
        }  //初始化系统菜单数据

        #endregion

        #region 用户自定义分组
        //获取自定义列表
        public void GETUSERGROUP(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new JH_Auth_UserCustomDataB().GetEntities(d => d.UserName == UserInfo.User.UserName && d.ComId == UserInfo.User.ComId && d.DataType == P1);
            if (P1 == "DXGL")
            {
                string sql = string.Format("SELECT Distinct DataContent1 FROM JH_Auth_UserCustomData WHERE dataType='DXGL' AND UserName = '{0}' AND ComId={1} GROUP BY DataContent1 ORDER BY DataContent1 DESC", UserInfo.User.UserName, UserInfo.User.ComId);
                msg.Result1 = new JH_Auth_UserCustomDataB().GetDTByCommand(sql);
            }
        }
        //添加自定义组
        public void ADDUSERGROUP(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_UserCustomData customData = new JH_Auth_UserCustomData();
            customData.ComId = UserInfo.User.ComId;
            customData.CRDate = DateTime.Now;
            customData.CRUser = UserInfo.User.UserName;
            customData.DataContent = P1;
            customData.DataContent1 = P2.Trim();
            customData.DataType = context["DataType"] != null ? context["DataType"].ToString() : "";
            customData.UserName = UserInfo.User.UserName;
            new JH_Auth_UserCustomDataB().Insert(customData);
            msg.Result = customData;





        }
        //根据组id获取用户列表
        public void GETUSERLISTBYGROUP(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            JH_Auth_UserCustomData customData = new JH_Auth_UserCustomDataB().GetEntity(d => d.ID == Id);
            string[] usernames = customData.DataContent1.Split(',');
            msg.Result = new JH_Auth_UserB().GetEntities(d => usernames.Contains(d.UserName) && d.ComId == UserInfo.User.ComId);

        }
        //删除用户自定义分组 ，短信模板
        public void DELUSERGROUP(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            new JH_Auth_UserCustomDataB().Delete(d => d.ID == Id);
        }
        //删除通讯录分类分组
        public void DELUSERGROUPTXL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            DataTable dt = new JH_Auth_UserCustomDataB().GetDTByCommand("SELECT * from SZHL_TXL where TagName=" + Id + " and ComId=" + UserInfo.User.ComId);
            if (dt.Rows.Count == 0)
            {
                new JH_Auth_UserCustomDataB().Delete(d => d.ID == Id);
            }
            else
            {
                msg.ErrorMsg = "请先删除此分类下的人员信息";
            }
        }


        #endregion

        #region 系统日志
        public void GETXTRZ(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = "1=1";
            string strContent = context["Content"] != null ? context["Content"].ToString() : "";
            strContent = strContent.TrimEnd();
            if (strContent != "")
            {
                strWhere += string.Format(" and (LogContent like  '%{0}%' or Remark1 like  '%{0}%' or IP like  '%{0}%')", strContent);
            }
            if (P1 != "")
            {
                //strWhere += string.Format(" and LogType IN ('{0}')", P2.Replace(",", "','"));  //多个类型逗号隔开传过来
                switch (P1)
                {
                    case "1": strWhere += " and ComId !='0'"; break;
                    case "2": strWhere += " and ComId ='0'"; break;
                }
            }
            int page = 0;
            int pagecount = 8;
            int.TryParse(context["p"] == null ? "0" : context["p"].ToString(), out page);
            int.TryParse(context["pagecount"] == null ? "8" : context["pagecount"].ToString(), out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            var dt = new JH_Auth_LogB().Db.Queryable<JH_Auth_Log>().Where(strWhere).OrderBy(it => it.CRDate).ToPageList(page, pagecount, ref total);
            msg.Result = dt;
            msg.Result1 = total;

        }
        public void DELXTRZ(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format("( ComId={0} or ComId=0 )", UserInfo.User.ComId);
            if (P1 != "" && P2 != "")
            {
                switch (P1)
                {
                    case "1": strWhere += " and ID ='" + P2 + "'"; break;
                    case "2": strWhere += " and ID in(" + P2 + ")"; break;
                }
                try
                {
                    new JH_Auth_LogB().ExsSql(" delete from JH_Auth_Log where " + strWhere);
                }
                catch (Exception ex)
                {
                    msg.ErrorMsg = ex.Message;
                }
            }
            else
            {
                msg.ErrorMsg = "删除失败";
            }
        }
        #endregion





        #region  获取评论
        public void GETCOMENT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new JH_Auth_TLB().GetTL(P1, P2);
        }
        #endregion

        #region  删除评论
        public void DELCOMENT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tl = new JH_Auth_TLB().GetEntities(" ID ='" + P1 + "'").FirstOrDefault();
            if (tl != null)
            {
                new JH_Auth_TLB().Delete(tl);
            }
        }
        #endregion

        #region 添加评论
        /// <summary>
        /// 添加评论
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="strParamData"></param>
        /// <param name="strUserName"></param>
        public void ADDCOMENT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strMsgType = context.Request("MsgType") ?? "";
            string strMsgLYID = context.Request("MsgLYID") ?? "";
            string strPoints =  context.Request("Points") ?? "0";
            string strfjID =    context.Request("fjID") ?? "";
            string strTLID =    context.Request("TLID") ?? "";


        
            JH_Auth_TL Model = new JH_Auth_TL();
            Model.CRDate = DateTime.Now;
            Model.CRUser = UserInfo.User.UserName;
            Model.CRUserName = UserInfo.User.UserRealName;
            Model.MSGContent = P1;
            Model.ComId = UserInfo.User.ComId;
            Model.MSGTLYID = strMsgLYID;
            Model.MSGType = strMsgType;
            Model.MSGisHasFiles = strfjID;
            Model.Remark1 = P1;

            if (strTLID != "")
            {
                int TLID = Int32.Parse(strTLID);
                var tl = new JH_Auth_TLB().GetEntity(p => p.ID == TLID);
                if (tl != null)
                {
                    Model.TLID = TLID;
                    Model.ReUser = tl.CRUserName;
                }

            }


            int record = 0;
            int.TryParse(strPoints, out record);
            Model.Points = record;
            new JH_Auth_TLB().Insert(Model);
            msg.Result = Model;
            if (Model.MSGisHasFiles == "")
                Model.MSGisHasFiles = "0";
            msg.Result1 = new FT_FileB().GetEntities(" ID in (" + Model.MSGisHasFiles + ")");
        }
        public void SENDPLMSG(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_TXSX TX = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);
            Article ar0 = new Article();
            ar0.Title = TX.TXContent;
            ar0.Description = "";
            ar0.Url = TX.MsgID;
            List<Article> al = new List<Article>();
            al.Add(ar0);
            if (!string.IsNullOrEmpty(TX.TXUser))
            {
                try
                {
                    //发送PC消息
                    UserInfo = new JH_Auth_UserB().GetUserInfo(TX.ComId.Value, TX.CRUser);
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.SendTH(al, TX.TXMode, "A", TX.TXUser);
                    new JH_Auth_User_CenterB().SendMsg(UserInfo, TX.TXMode, TX.TXContent, TX.MsgID, TX.TXUser);
                }
                catch (Exception)
                {
                }
                //发送微信消息

            }
        }


        /// <summary>
        /// 获取评论列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETPLLIST(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = " JH_Auth_TL.ComId=" + UserInfo.User.ComId;
            if (P1 != "")
            {
                strWhere += string.Format(" And  JH_Auth_TL.MSGContent like '%{0}%'", P1);
            }
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request("p") ?? "1", out page);
            int.TryParse(context.Request("pagecount") ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new JH_Auth_TLB().GetDataPager(" JH_Auth_TL LEFT JOIN  SZHL_TSSQ ON  JH_Auth_TL.MSGTLYID=SZHL_TSSQ.ID ", " JH_Auth_TL.*,SZHL_TSSQ.HTNR ", pagecount, page, " JH_Auth_TL.CRDate desc", strWhere, ref total);
            msg.Result = dt;
            msg.Result1 = total;
        }

        #endregion


        #region 设置常用菜单显示
        //设置手机APP，PC首页菜单显示应用
        public void SETAPPINDEX(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string type = context["type"] != null ? context["type"].ToString() : "APPINDEX";//默认为APP首页显示菜单，传值为PC首页的快捷方式按钮

            foreach (string str in P1.Split(','))
            {
                string[] content = str.Split(':');
                string modelCode = content[0];
                //判断是否存在菜单的数据，存在只更新状态，不存在添加
                JH_Auth_UserCustomData customData = new JH_Auth_UserCustomDataB().GetEntity(d => d.UserName == UserInfo.User.UserName && d.DataType == type && d.ComId == UserInfo.User.ComId && d.DataContent == modelCode);
                string status = content[1];
                if (customData != null)
                {
                    customData.DataContent1 = status;
                    new JH_Auth_UserCustomDataB().Update(customData);
                }
                else
                {
                    customData = new JH_Auth_UserCustomData();
                    customData.ComId = UserInfo.User.ComId;
                    customData.UserName = UserInfo.User.UserName;
                    customData.CRDate = DateTime.Now;
                    customData.CRUser = UserInfo.User.UserName;
                    customData.DataContent = modelCode;
                    customData.DataContent1 = status;
                    customData.DataType = type;
                    new JH_Auth_UserCustomDataB().Insert(customData);
                }
                if (type == "APPINDEX")
                {
                    msg.Result = customData;
                }
            }


        }
        #endregion

        #region 设置部门人员的查看权限
        public void SETBRANCHQX(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            int deptCode = int.Parse(P2);
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetEntity(d => d.ComId == UserInfo.User.ComId && d.DeptCode == deptCode);
            branch.TXLQX = P1;
            branch.IsHasQX = context["qx"] != null ? context["qx"].ToString() : "N";
            new JH_Auth_BranchB().Update(branch);

        }
        #endregion
        #region 设置部门人员的查看角色
        public void SETROLEQX(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            int roleCode = int.Parse(P2);
            JH_Auth_Role role = new JH_Auth_RoleB().GetEntity(d => d.RoleCode == roleCode);
            role.RoleQX = P1;
            role.IsHasQX = context["qx"] != null ? context["qx"].ToString() : "N";
            new JH_Auth_RoleB().Update(role);
            msg.Result = role;
        }
        #endregion



        #region 获取已发送短信数及容量使用情况
        public void GETDXANDSPACE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            decimal DXCost = decimal.Parse(CommonHelp.GetConfig("DXCost", "1"));
            //已发送短信总数量
            msg.Result = new SZHL_DXGLB().GetEntities(d => d.ComId == UserInfo.User.ComId.Value).Count();
            msg.Result1 = (int)(UserInfo.QYinfo.AccountMoney.Value / DXCost);
            msg.Result2 = UserInfo.QYinfo.QySpace / 10000000000;
            string strSql = string.Format("SELECT isnull(sum(CAST( FileSize  as DECIMAL(18,2))),0) from  FT_File where ComId=" + UserInfo.User.ComId);
            object obj = new FT_FileB().ExsSclarSql(strSql);
            decimal Size = 0;
            string fileSize = obj.ToString();
            if (fileSize.Length < 4)
            {
                Size = decimal.Parse(fileSize);
                msg.Result4 = "kb";
            }
            if (fileSize.Length >= 4 && fileSize.Length <= 8)
            {
                Size = Math.Round(decimal.Parse(fileSize) / 10000, 2);
                msg.Result4 = "M";
            }
            if (fileSize.Length > 8)
            {
                Size = Math.Round(decimal.Parse(fileSize) / 100000000, 2);
                msg.Result4 = "G";
            }
            msg.Result3 = Size;
        }
        #endregion

        #region  扩展字段
        public void GETEXTENDFIELD(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = "";
            string pdid = context["PDID"] != null ? context["PDID"].ToString() : "0";
            DataTable dt = new JH_Auth_ExtendModeB().GetDTByCommand(string.Format("select * from JH_Auth_ExtendMode where ComId='{0}' and TableName='{1}' " + strWhere, UserInfo.User.ComId, P1));
            msg.Result = dt;
        }
        public void ADDEXTENDFIELD(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_ExtendMode extMode = JsonConvert.DeserializeObject<JH_Auth_ExtendMode>(P1);
            if (extMode.ID == 0) //add
            {
                extMode.ComId = UserInfo.User.ComId;
                extMode.CRUser = UserInfo.User.UserName;
                extMode.CRDate = DateTime.Now;
                new JH_Auth_ExtendModeB().Insert(extMode);
            }
            else //edit
            {
                new JH_Auth_ExtendModeB().Update(extMode);
            }

            msg.Result = extMode;

        }
        public void DELEXTENDFIELD(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            new JH_Auth_ExtendModeB().Delete(p => p.ID.ToString() == P1);
        }
        public void GETEXTDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            if (P1 == "YGGL")
            {
                JH_Auth_User user = new JH_Auth_UserB().GetEntity(d => d.UserName == P2 && d.ComId == UserInfo.User.ComId);
                if (user != null)
                {
                    P2 = user.ID.ToString();
                }
                else
                {
                    P2 = "";
                }
            }
            string strWhere = "";
            string pdid = context["PDID"] != null ? context["PDID"].ToString() : "0";
            DataTable dt = new JH_Auth_ExtendModeB().GetDTByCommand(string.Format("select j.ComId, j.ID, j.TableName, j.TableFiledColumn, j.TableFiledName, j.TableFileType, j.DefaultOption, j.DefaultValue, j.IsRequire, d.ExtendModeID, d.ID AS ExtID, d.DataID, d.ExtendDataValue from JH_Auth_ExtendMode j left join JH_Auth_ExtendData d on j.ComId=d.ComId and j.ID=d.ExtendModeID and d.DataID='{2}' where j.ComId='{0}' and j.TableName='{1}' " + strWhere, UserInfo.User.ComId, P1, P2));

            msg.Result = dt;
        }
        public void UPDATEEXTDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int dataid = Int32.Parse(P2);
            string ExtData = context["ExtData"] != null ? context["ExtData"].ToString() : "";


            if (!string.IsNullOrEmpty(ExtData))
            {
                List<ExtDataModel> ext = JsonConvert.DeserializeObject<List<ExtDataModel>>(ExtData);
                if (ext.Count > 0)
                {
                    foreach (var v in ext)
                    {
                        var extModel = new JH_Auth_ExtendDataB().GetEntity(p => p.ID == v.ExtID);
                        if (extModel == null)
                        {
                            JH_Auth_ExtendData jext = new JH_Auth_ExtendData();
                            jext.ComId = v.ComId;
                            jext.DataID = jext.DataID == null ? dataid : jext.DataID;
                            jext.TableName = v.TableName;
                            jext.ExtendModeID = v.ID;
                            jext.ExtendDataValue = v.ExtendDataValue;
                            jext.CRUser = UserInfo.User.UserName;
                            jext.CRDate = DateTime.Now;
                            jext.CRUserName = UserInfo.User.UserRealName;
                            jext.BranchNo = UserInfo.BranchInfo.DeptCode;
                            jext.BranchName = UserInfo.BranchInfo.DeptName;
                            new JH_Auth_ExtendDataB().Insert(jext);
                        }
                        else
                        {
                            extModel.ExtendDataValue = v.ExtendDataValue;
                            new JH_Auth_ExtendDataB().Update(extModel);
                        }
                    }
                }

            }

        }


        #endregion








        #region 草稿管理
        //获取草稿
        public void GETDRAFT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var dt = new SZHL_DRAFTB().Db.Queryable<SZHL_DRAFT>().Where(d => d.CRUser == UserInfo.User.UserName && d.FormCode == P1 && d.DataID == null).WhereIF(!string.IsNullOrEmpty(P2), d => d.FormID == P2).OrderBy(d => d.CRTime, OrderByType.Desc).Take(5).ToList();
            msg.Result = dt;
        }
        public void GETDRAFTMODEL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            new SZHL_DRAFTB().GetEntity(p => p.ID == ID && p.ComId == UserInfo.User.ComId && p.CRUser == UserInfo.User.CRUser);
        }

        public void SAVEDRAFT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_DRAFT tt = JsonConvert.DeserializeObject<SZHL_DRAFT>(P1);
            tt.ID = 0;
            if (tt.ID == 0)
            {
                tt.ComId = UserInfo.User.ComId;
                tt.CRUser = UserInfo.User.UserName;
                tt.CRTime = DateTime.Now;
                new SZHL_DRAFTB().Insert(tt);
            }

            msg.Result = tt;
        }
        public void DELDRAFT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            new SZHL_DRAFTB().Delete(p => p.ID == ID && p.ComId == UserInfo.User.ComId && p.CRUser == UserInfo.User.UserName);
        }
        #endregion






        #region 常用菜单设置
        /// <summary>
        /// 第五版的自定义显示菜单和左边菜单
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETINDEXMENUNEW(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dtModel = new JH_Auth_ModelB().GETMenuList(UserInfo, P1);
            dtModel.Columns.Add("ISSY", Type.GetType("System.Int32"));
            dtModel.Columns.Add("FunData", typeof(DataTable));
            if (dtModel != null && dtModel.Rows.Count > 0)
            {  //获取用户设置首页显示APP
                List<string> userCustom = new JH_Auth_UserCustomDataB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.UserName == UserInfo.User.UserName && d.DataType == P1 && d.DataContent1 == "Y").Select(d => d.DataContent).ToList();

                foreach (DataRow row in dtModel.Rows)
                {
                    if (userCustom.Count > 0)
                    {
                        row["ISSY"] = 0;
                        if (row["UserAPPID"].ToString() != "")
                        {
                            row["ISSY"] = 1;
                        }
                    }
                    else
                    {

                        row["ISSY"] = 1;
                    }
                    row["FunData"] = new JH_Auth_RoleB().GetModelFun(UserInfo.User.ComId.Value, UserInfo.UserRoleCode, row["ID"].ToString());

                }
            }
            msg.Result = dtModel;
            msg.Result2 = UserInfo.User.isSupAdmin;

        }
        #endregion




        #region 应用管理
        public void GETYYDATA(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = "";
            if (P1 != "")
            {
                strWhere = " AND (ModelName like '%" + P1 + "%'OR ModelType like '%" + P1 + "%') ";
            }
            string strSQL = "SELECT * FROM JH_Auth_Model WHERE 1=1" + strWhere + " ORDER BY ModelType";
            DataTable dt = new JH_Auth_ModelB().GetDTByCommand(strSQL);
            msg.Result = dt;
        }


        public void DELYY(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int intYYID = int.Parse(P1);
            new JH_Auth_ModelB().Delete(d => d.ID == intYYID);
            new JH_Auth_FunctionB().Delete(d => d.ModelID == intYYID);
        }

        public void GETYY(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int intYYID = int.Parse(P1);
            JH_Auth_Model model = new JH_Auth_ModelB().GetEntity(d => d.ID == intYYID);
            msg.Result = model;
            msg.Result1 = new JH_Auth_FunctionB().GetEntities(d => d.ModelID == intYYID);
            msg.Result2 = new JH_Auth_CommonB().GetEntities(d => d.ModelCode == model.ModelCode);//移动应用菜单

        }




        public void ADDYY(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Model MODELO = JsonConvert.DeserializeObject<JH_Auth_Model>(P1);

            JH_Auth_Model MODEL = new JH_Auth_Model();
            MODEL.ModelCode = MODELO.ModelCode;
            MODEL.PModelCode = MODELO.PModelCode;
            MODEL.ModelType = MODELO.ModelType;
            MODEL.ModelName = MODELO.ModelName;
            MODEL.ORDERID = MODELO.ORDERID;
            MODEL.WXUrl = MODELO.WXUrl;
            MODEL.ModelStatus = "0";
            MODEL.IsSQ = "1";
            MODEL.CRDate = DateTime.Now;
            MODEL.CRUser = UserInfo.User.UserName;
            MODEL.ComId = 0;
            MODEL.AppType = "1";
            MODEL.IsSys = 1;
            MODEL.IsKJFS = 0;
            new JH_Auth_ModelB().Insert(MODEL);

        }
        public void SAVEYY(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Model MODEL = JsonConvert.DeserializeObject<JH_Auth_Model>(P1);
            new JH_Auth_ModelB().Update(MODEL);


            List<JH_Auth_Function> FUNLIST = JsonConvert.DeserializeObject<List<JH_Auth_Function>>(P2);
            FUNLIST.ForEach(d => d.CRDate = DateTime.Now);
            FUNLIST.ForEach(d => d.CRUser = UserInfo.User.UserName);
            FUNLIST.ForEach(d => d.ComId = 0);
            FUNLIST.ForEach(d => d.ModelID = MODEL.ID);
            FUNLIST.ForEach(d => d.ActionData = "[]");
            foreach (JH_Auth_Function item in FUNLIST)
            {
                if (item.ID == 0)
                {
                    //新增
                    new JH_Auth_FunctionB().Insert(item);
                }
                else
                {
                    new JH_Auth_FunctionB().Update(item);
                    //更新
                }
            }

            string delid = context["DIDS"] != null ? context["DIDS"].ToString() : "";
            if (delid.Trim(',') != "")
            {
                new JH_Auth_FunctionB().ExsSclarSql("delete FROM JH_Auth_Function where id in ('" + delid.ToFormatLike(',') + "')");
                //删除
            }
            string MENUData = context["MENU"] != null ? context["MENU"].ToString() : "";
            if (MENUData.Trim(',') != "")
            {
                new JH_Auth_CommonB().Delete(d => d.ModelCode == MODEL.ModelCode);
                List<JH_Auth_Common> MENUS = JsonConvert.DeserializeObject<List<JH_Auth_Common>>(MENUData);
                MENUS.ForEach(d => d.CRDate = DateTime.Now);
                MENUS.ForEach(d => d.CRUser = UserInfo.User.UserName);
                MENUS.ForEach(d => d.ModelCode = MODEL.ModelCode);
                new JH_Auth_CommonB().Insert(MENUS);

            }


        }

        #endregion










        #region 导入用户
        public void SAVEIMPORTUSER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string branchMsg = "", branchErrorMsg = "", userMsg = "";
            int i = 0, j = 0;
            DataTable dt = new DataTable();
            dt = JsonConvert.DeserializeObject<DataTable>(P1);
            dt.Columns.Add("BranchCode");
            JH_Auth_Branch branchroot = new JH_Auth_BranchB().GetEntity(d => d.ComId == UserInfo.User.ComId && d.DeptRoot == -1);


            foreach (DataRow row in dt.Rows)
            {
                int bRootid = branchroot.DeptCode;
                string branchName = row[4].ToString();
                if (branchName != "")
                {
                    string[] branchNames = branchName.Split('/');
                    string strBranch = branchNames[0];
                    JH_Auth_Branch branchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptName == strBranch && d.ComId == UserInfo.User.ComId);
                    if (branchModel == null)
                    {
                        branchModel = new JH_Auth_Branch();
                        branchModel.DeptName = branchNames[0];
                        branchModel.DeptDesc = branchNames[0];
                        branchModel.ComId = UserInfo.User.ComId;
                        branchModel.DeptRoot = bRootid;
                        branchModel.CRDate = DateTime.Now;
                        branchModel.CRUser = UserInfo.User.UserName;
                        new JH_Auth_BranchB().Insert(branchModel);
                        branchModel.Remark1 = new JH_Auth_BranchB().GetBranchNo(UserInfo.User.ComId.Value, branchModel.DeptRoot) + branchModel.DeptCode;
                        new JH_Auth_BranchB().Update(branchModel);
                    }
                }
            }


            int rowIndex = 0;
            foreach (DataRow row in dt.Rows)
            {
                rowIndex++;
                string branchName = row[4].ToString();
                if (branchName != "")
                {
                    string[] branchNames = branchName.Split('/');
                    string strPBranch = branchNames[0];

                    JH_Auth_Branch PbranchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptName == strPBranch && d.ComId == UserInfo.User.ComId);
                    int bRootid = PbranchModel.DeptCode;
                    for (int l = 1; l < branchNames.Length; l++)
                    {
                        string strBranch = branchNames[1];
                        JH_Auth_Branch branchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptName == strBranch && d.DeptRoot == PbranchModel.DeptCode && d.ComId == UserInfo.User.ComId);
                        if (branchModel != null)
                        {
                            bRootid = branchModel.DeptCode;
                            if (l == branchNames.Length - 1)
                            {
                                row["BranchCode"] = branchModel.DeptCode;
                            }
                        }
                        else
                        {
                            branchModel = new JH_Auth_Branch();
                            branchModel.DeptName = strBranch;
                            branchModel.DeptDesc = strBranch;
                            branchModel.ComId = UserInfo.User.ComId;
                            branchModel.DeptRoot = bRootid;
                            branchModel.CRDate = DateTime.Now;
                            branchModel.CRUser = UserInfo.User.UserName;
                            new JH_Auth_BranchB().Insert(branchModel);
                            branchModel.Remark1 = new JH_Auth_BranchB().GetBranchNo(UserInfo.User.ComId.Value, branchModel.DeptRoot) + branchModel.DeptCode;
                            new JH_Auth_BranchB().Update(branchModel);
                            try
                            {
                                bRootid = branchModel.DeptCode;
                                if (l == branchNames.Length - 1)
                                {
                                    row["BranchCode"] = branchModel.DeptCode;
                                }
                                i++;
                                branchMsg += "新增部门“" + strBranch + "”成功<br/>";
                            }
                            catch (Exception ex)
                            {

                                branchErrorMsg += "部门：" + strBranch + "失败 " + msg.ErrorMsg + "<br/>";
                            }
                        }
                    }
                    string userName = row[2].ToString();
                    JH_Auth_User userModel = new JH_Auth_UserB().GetEntity(d => d.UserName == userName && d.ComId == UserInfo.User.ComId);
                    if (userModel == null)
                    {
                        JH_Auth_User userNew = new JH_Auth_User();
                        if (row["BranchCode"].ToString() != "")
                        {
                            int tempcode = int.Parse(row["BranchCode"].ToString());
                            JH_Auth_Branch branchTemp = new JH_Auth_BranchB().GetEntity(d => d.DeptCode == tempcode && d.ComId == UserInfo.User.ComId);

                            userNew.BranchCode = branchTemp.DeptCode;
                            userNew.remark = branchTemp.Remark1.Split('-')[0];
                        }
                        else
                        {
                            userNew.BranchCode = bRootid;
                        }
                        userNew.ComId = UserInfo.User.ComId;
                        userNew.IsUse = "Y";
                        userNew.mailbox = row[3].ToString();
                        userNew.mobphone = row[2].ToString();
                        userNew.RoomCode = row[7].ToString();
                        userNew.Sex = row[1].ToString();
                        userNew.telphone = row[9].ToString();
                        DateTime result;
                        if (DateTime.TryParse(row[10].ToString(), out result))
                        {
                            userNew.Birthday = result;
                        }

                        userNew.UserGW = row[6].ToString();
                        userNew.UserName = row[2].ToString();
                        userNew.UserRealName = row[0].ToString();
                        userNew.zhiwu = row[5].ToString() == "" ? "员工" : row[5].ToString();
                        userNew.UserPass = CommonHelp.GetMD5(P2);
                        userNew.CRDate = DateTime.Now;
                        userNew.CRUser = UserInfo.User.UserName;

                        if (!string.IsNullOrEmpty(row[8].ToString()))
                        {
                            int orderNum = 0;
                            int.TryParse(row[8].ToString(), out orderNum);
                            userNew.UserOrder = orderNum;

                        }
                        try
                        {
                            msg.ErrorMsg = "";
                            if (string.IsNullOrEmpty(userNew.UserName))
                            {
                                msg.ErrorMsg = "用户名必填";
                            }
                            //Regex regexPhone = new Regex("^0?1[3|4|5|8|7][0-9]\\d{8}$");
                            //if (!regexPhone.IsMatch(userNew.UserName))
                            //{
                            //    msg.ErrorMsg = "用户名必须为手机号";
                            //}
                            if (string.IsNullOrEmpty(userNew.mobphone))
                            {
                                msg.ErrorMsg = "手机号必填";
                            }
                            //if (!regexPhone.IsMatch(userNew.mobphone))
                            //{
                            //    msg.ErrorMsg = "手机号填写不正确";
                            //}
                            Regex regexOrder = new Regex("^[0-9]*$");
                            if (userNew.UserOrder != null && !regexOrder.IsMatch(userNew.UserOrder.ToString()))
                            {
                                msg.ErrorMsg = "序号必须是数字";
                            }
                            if (msg.ErrorMsg != "")
                            {
                                userMsg += "第" + rowIndex + "行" + msg.ErrorMsg + "<br/>";
                            }
                            if (msg.ErrorMsg == "")
                            {
                                new JH_Auth_UserB().Insert(userNew);
                                JH_Auth_Role role = new JH_Auth_RoleB().GetEntity(d => d.RoleName == userNew.zhiwu && d.ComId == UserInfo.User.ComId);
                                if (role == null)
                                {
                                    role = new JH_Auth_Role();
                                    role.PRoleCode = 0;
                                    role.RoleName = userNew.zhiwu;
                                    role.RoleDec = userNew.zhiwu;
                                    role.IsUse = "Y";
                                    role.isSysRole = "N";
                                    role.leve = 0;
                                    role.ComId = UserInfo.User.ComId;
                                    role.DisplayOrder = 0;
                                    new JH_Auth_RoleB().Insert(role);
                                }

                                var insertObjs = new List<JH_Auth_UserRole>();
                                var s9 = new JH_Auth_UserRoleB().Insert(new JH_Auth_UserRole() { UserName = userNew.UserName, ComId = UserInfo.User.ComId, RoleCode = role.RoleCode });
                                string isFS = context["issend"] != null ? context["issend"].ToString() : "";
                                if (isFS.ToLower() == "true")
                                {
                                    string content = string.Format("尊敬的" + userNew.UserName + "用户您好：你已被添加到" + UserInfo.QYinfo.QYName + ",账号：" + userNew.mobphone + "，密码" + P2 + ",登录请访问" + UserInfo.QYinfo.WXUrl);
                                    new SZHL_DXGLB().SendSMS(userNew.mobphone, content, userNew.ComId.Value);
                                }
                                j++;
                            }
                        }
                        catch (Exception ex)
                        {
                            userMsg += "第" + rowIndex + "行" + msg.ErrorMsg + "<br/>";
                        }

                    }
                    else
                    {

                        userMsg += "第" + rowIndex + "行" + "用户“" + row[2].ToString() + "”已存在<br/>";
                    }
                }
                else
                {
                    branchErrorMsg += "第" + rowIndex + "行所在部门必填<br/>";
                }

            }
            msg.Result = branchErrorMsg + "<br/>" + userMsg;
            msg.Result1 = "新增部门" + i + "个,新增用户" + j + "个<br/>" + branchMsg + (branchMsg == "" ? "" : "<br/>");
        }



        #endregion

        #region 获取系统首页用户数量信息
        public void GETUSERCOUNT(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("SELECT COUNT(0) TotalUser,isnull(sum(case when isgz=1 then 1 else 0 end ),0) gzCount,isnull(sum(case when isgz=4 then 1 else 0 end ),0) wgzCount,isnull(sum(case when IsUse!='Y' then 1 else 0 end ),0) wjhCount from JH_Auth_User where ComId={0}", UserInfo.User.ComId);
            msg.Result = new JH_Auth_UserB().GetDTByCommand(strSql);
            msg.Result1 = UserInfo.QYinfo.IsUseWX;
        }
        #endregion

        /// <summary>
        /// 同步通讯录
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">初始化密码</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>


        #region 企业号相关
        /// <summary>
        /// 将系统的组织架构同步到微信中去
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void TBBRANCHUSER(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            //判断是否启用微信后，启用部门需要同步添加微信部门
            if (UserInfo.QYinfo.IsUseWX == "Y")
            {

                #region 同步部门

                //系统部门
                List<JH_Auth_Branch> branchList = new JH_Auth_BranchB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.WXBMCode == null).ToList();

                WXHelp wx = new WXHelp(UserInfo.QYinfo);
                //微信部门
                GetDepartmentListResult bmlist = wx.WX_GetBranchList("");
                foreach (JH_Auth_Branch branch in branchList)
                {
                    List<DepartmentList> departList = bmlist.department.Where(d => d.name == branch.DeptName).ToList();
                    WorkJsonResult result = null;
                    if (departList.Count() > 0)
                    {
                        branch.WXBMCode = int.Parse(departList[0].id.ToString());
                        result = wx.WX_UpdateBranch(branch);
                    }
                    else
                    {

                        int branchWxCode = int.Parse(wx.WX_CreateBranchTB(branch).ToString());
                        branch.WXBMCode = branchWxCode;
                    }
                    new JH_Auth_BranchB().Update(branch);
                }

                #endregion

                #region 同步人员
                JH_Auth_Branch branchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptRoot == -1 && d.ComId == UserInfo.User.ComId);

                GetDepartmentMemberInfoResult yg = wx.WX_GetDepartmentMemberInfo(branchModel.WXBMCode.Value);
                List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.UserName != "administrator").ToList();
                foreach (JH_Auth_User user in userList)
                {
                    if (yg.userlist.Where(d => d.name == user.UserName || d.mobile == user.mobphone).Count() > 0)
                    {
                        wx.WX_UpdateUser(user);
                    }
                    else
                    {

                        wx.WX_CreateUser(user);
                    }
                }
                #endregion
            }
        }



        /// <summary>
        /// 从企业微信同步到系统里
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void TBTXL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {

                int bmcount = 0;
                int rycount = 0;
                if (P1 == "")
                {
                    msg.ErrorMsg = "请输入初始密码";
                    return;
                }
                WXHelp wx = new WXHelp(UserInfo.QYinfo);
                #region 更新部门
                GetDepartmentListResult bmlist = wx.WX_GetBranchList("");
                foreach (var wxbm in bmlist.department.OrderBy(d => d.parentid))
                {
                    var bm = new JH_Auth_BranchB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.WXBMCode == wxbm.id);
                    if (bm == null)
                    {
                        #region 新增部门
                        JH_Auth_Branch jab = new JH_Auth_Branch();
                        jab.WXBMCode = int.Parse(wxbm.id.ToString());
                        jab.ComId = UserInfo.User.ComId;
                        jab.DeptName = wxbm.name;
                        jab.DeptDesc = wxbm.name;
                        jab.DeptShort = int.Parse(wxbm.order.ToString());

                        if (wxbm.parentid == 0)//如果是跟部门,设置其跟部门为-1
                        {
                            jab.DeptRoot = -1;
                        }
                        else
                        {
                            var bm1 = new JH_Auth_BranchB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.WXBMCode == wxbm.parentid);
                            jab.DeptRoot = bm1.DeptCode;
                            jab.Remark1 = new JH_Auth_BranchB().GetBranchNo(UserInfo.User.ComId.Value, jab.DeptRoot);
                        }


                        new JH_Auth_BranchB().Insert(jab);
                        jab.Remark1 = new JH_Auth_BranchB().GetBranchNo(UserInfo.User.ComId.Value, jab.DeptRoot) + jab.DeptCode;
                        new JH_Auth_BranchB().Update(jab);


                        bmcount = bmcount + 1;
                        #endregion
                    }
                    else
                    {
                        //同步部门时放弃更新现有部门

                    }
                }
                #endregion

                #region 更新人员
                JH_Auth_Branch branchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptRoot == -1 && d.ComId == UserInfo.User.ComId);

                GetDepartmentMemberInfoResult yg = wx.WX_GetDepartmentMemberInfo(branchModel.WXBMCode.Value);
                foreach (var u in yg.userlist)
                {
                    var user = new JH_Auth_UserB().GetUserByUserName(UserInfo.QYinfo.ComId, u.userid);
                    if (user == null)
                    {
                        #region 新增人员
                        JH_Auth_User jau = new JH_Auth_User();
                        jau.ComId = UserInfo.User.ComId;
                        jau.UserName = u.userid;
                        jau.UserPass = CommonHelp.GetMD5(P1);
                        jau.UserRealName = u.name;
                        jau.Sex = u.gender == 1 ? "男" : "女";
                        if (u.department.Length > 0)
                        {
                            int id = int.Parse(u.department[0].ToString());
                            var bm1 = new JH_Auth_BranchB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.WXBMCode == id);
                            jau.BranchCode = bm1.DeptCode;
                            jau.remark = bm1.Remark1.Split('-')[0];//用户得部门路径

                        }
                        jau.mailbox = u.email;
                        jau.mobphone = u.mobile;
                        jau.zhiwu = string.IsNullOrEmpty(u.position) ? "员工" : u.position;
                        jau.IsUse = "Y";

                        if (u.status == 1 || u.status == 4)
                        {
                            jau.isgz = u.status.ToString();
                        }
                        jau.txurl = u.avatar;

                        new JH_Auth_UserB().Insert(jau);

                        rycount = rycount + 1;
                        #endregion

                        //为所有人增加普通员工的权限
                        JH_Auth_Role rdefault = new JH_Auth_RoleB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.isSysRole == "Y" && p.RoleName == "员工");//找到默认角色
                        if (rdefault != null)
                        {
                            JH_Auth_UserRole jaurdefault = new JH_Auth_UserRole();
                            jaurdefault.ComId = UserInfo.User.ComId;
                            jaurdefault.RoleCode = rdefault.RoleCode;
                            jaurdefault.UserName = jau.UserName;
                            new JH_Auth_UserRoleB().Insert(jaurdefault);
                        }


                    }
                    else
                    {
                        //同步人员时放弃更新现有人员
                        #region 更新人员
                        user.UserRealName = u.name;
                        if (u.department.Length > 0)
                        {
                            int id = int.Parse(u.department[0].ToString());
                            var bm1 = new JH_Auth_BranchB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.WXBMCode == id);
                            user.BranchCode = bm1.DeptCode;
                        }
                        user.mailbox = u.email;
                        user.mobphone = u.mobile;
                        user.zhiwu = string.IsNullOrEmpty(u.position) ? "员工" : u.position;
                        user.Sex = u.gender == 1 ? "男" : "女";
                        if (u.status == 1 || u.status == 4)
                        {
                            user.IsUse = "Y";
                            user.isgz = u.status.ToString();
                        }
                        else if (u.status == 2)
                        {
                            user.IsUse = "N";
                        }
                        user.txurl = u.avatar;

                        new JH_Auth_UserB().Update(user);
                        #endregion
                    }

                    #region 更新角色(职务)
                    if (!string.IsNullOrEmpty(u.position))
                    {
                        var r = new JH_Auth_RoleB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.RoleName == u.position);

                        if (r == null)
                        {
                            JH_Auth_Role jar = new JH_Auth_Role();
                            jar.ComId = UserInfo.User.ComId;
                            jar.RoleName = u.position;
                            jar.RoleDec = u.position;
                            jar.PRoleCode = 0;
                            jar.isSysRole = "N";
                            jar.IsUse = "Y";
                            jar.leve = 0;
                            jar.DisplayOrder = 0;

                            new JH_Auth_RoleB().Insert(jar);

                            JH_Auth_UserRole jaur = new JH_Auth_UserRole();
                            jaur.ComId = UserInfo.User.ComId;
                            jaur.RoleCode = jar.RoleCode;
                            jaur.UserName = u.userid;
                            new JH_Auth_UserRoleB().Insert(jaur);


                        }
                        else
                        {

                        }
                    }
                    #endregion
                }
                #endregion



                msg.Result1 = bmcount;
                msg.Result2 = rycount;


            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.ToString();
            }
        }

        //同步关注状态
        public void TBGZSTATUS(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {

                JH_Auth_Branch branchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptRoot == -1 && d.ComId == UserInfo.User.ComId);

                #region 同步用户关注状态
                WXHelp wx = new WXHelp(UserInfo.QYinfo);
                GetDepartmentMemberInfoResult yg = wx.WX_GetDepartmentMemberInfo(branchModel.WXBMCode.Value);

                if (yg != null && yg.userlist != null)
                {
                    foreach (var u in yg.userlist)
                    {

                        JH_Auth_User user = new JH_Auth_UserB().GetEntity(d => d.ComId == UserInfo.User.ComId && d.UserName == u.userid);

                        if (user != null && u != null && (u.status == 1 || u.status == 4))
                        {
                            user.isgz = u.status.ToString();
                            new JH_Auth_UserB().Update(user);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
            #endregion

        }

        public void YZCOMPANYQYH(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_QY company = new JH_Auth_QY();
            company = JsonConvert.DeserializeObject<JH_Auth_QY>(P1);


            if (string.IsNullOrEmpty(company.corpSecret) || string.IsNullOrEmpty(company.corpId))
            {
                msg.ErrorMsg = "初始化企业号信息失败,corpId,corpSecret 不能为空";
                return;
            }
            if (!new JH_Auth_QYB().Update(company))
            {
                msg.ErrorMsg = "初始化企业号信息失败";
                return;
            }

        }




        /// <summary>
        /// 获取具有手机端的应用列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWXAPP(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new JH_Auth_ModelB().GetEntities(d => !string.IsNullOrEmpty(d.WXUrl)).OrderBy(d => d.ORDERID);
        }


        /// <summary>
        /// 获取当前企业号拥有的IP,只返回和可信域名相同的应用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETQYAPP(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int id = Int32.Parse(P1);
            var model = new JH_Auth_ModelB().GetEntity(p => p.ID == id);
            msg.Result1 = model;//系统应用数据


            #region 获取应用默认菜单
            DataTable dt = new JH_Auth_CommonB().GetDTByCommand(" select * from JH_Auth_Common where ModelCode='" + model.ModelCode + "' and type='1' order by Sort");
            #endregion

            msg.Result2 = dt;

            //主页型应用的URL
            if (model.AppType == "2")
            {
                msg.Result3 = UserInfo.QYinfo.WXUrl.TrimEnd('/') + "/View_Mobile/UI/UI_COMMON.html?funcode=" + model.ModelCode + "&corpId=" + UserInfo.QYinfo.corpId; ;
            }
        }

        /// <summary>
        /// 保存应用Token和EncodingAESKey
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SAVEMODEL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Model model = JsonConvert.DeserializeObject<JH_Auth_Model>(P1);
            if (model.ID != 0)
            {
                if (string.IsNullOrEmpty(model.AppID))
                {
                    msg.ErrorMsg = "至少选择一个企业号应用才能绑定";
                    return;
                }

                if (model.AppType == "1" && (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.EncodingAESKey)))
                {
                    msg.ErrorMsg = "Token、EncodingAESKey、企业号应用不能为空";
                }
                else
                {
                    new JH_Auth_ModelB().Update(model);
                }
            }
            else
            {
                msg.ErrorMsg = "绑定失败";
            }
        }

        /// <summary>
        /// 创建应用菜单
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void CREATEMENU(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int id = Int32.Parse(P1);
                var model = new JH_Auth_ModelB().GetEntity(p => p.ID == id);
                if (model != null)
                {
                    if (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.EncodingAESKey) || string.IsNullOrEmpty(model.AppID))
                    {
                        msg.ErrorMsg = "Token、EncodingAESKey、企业号应用不能为空";
                    }
                    else
                    {
                        WXHelp WX = new WXHelp(UserInfo.QYinfo);
                        List<Senparc.Weixin.Work.Entities.Menu.BaseButton> lm = new List<Senparc.Weixin.Work.Entities.Menu.BaseButton>();
                        WorkJsonResult rel = WX.WX_WxCreateMenuNew(Int32.Parse(model.AppID), model.ModelCode, ref lm);
                        if (rel.errmsg != "ok")
                        {
                            msg.ErrorMsg = "创建菜单失败";
                        }
                    }
                }
                else
                {
                    msg.ErrorMsg = "当前应用不存在";
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = "创建菜单失败";
            }
        }

        /// <summary>
        /// 解除应用绑定
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void FIREMODEL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int id = Int32.Parse(P1);
                var model = new JH_Auth_ModelB().GetEntity(p => p.ID == id);
                if (model != null)
                {
                    //WXHelp WX = new WXHelp(UserInfo.QYinfo);
                    //WX.WX_DelMenu(Int32.Parse( model.AppID));

                    model.AppID = "";
                    model.Token = "";
                    model.EncodingAESKey = "";

                    new JH_Auth_ModelB().Update(model);
                }
                else
                {
                    msg.ErrorMsg = "当前应用不存在";
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = "解除绑定失败";
            }
        }




        /// <summary>
        /// @用的查询用户数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETUSERSBYKEY(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            DataTable dt1 = new JH_Auth_UserB().GetDTByCommand("SELECT  UserName,UserRealName,C.DeptName, B.DeptName as DeptName1   FROM JH_Auth_User a LEFT  JOIN  JH_Auth_Branch B on A.BranchCode=B.DeptCode  INNER   JOIN   JH_Auth_Branch C on b.DeptRoot=c.DeptCode WHERE   a.isTX='N' AND UserRealName LIKE '%" + P1 + "%'").SplitDataTable(1,5);
            DataTable dt2 = new JH_Auth_UserB().GetDTByCommand("SELECT  B.deptName,A.deptName  AS deptName1,A.DeptCode FROM JH_Auth_Branch A INNER JOIN   JH_Auth_Branch B on A.DeptRoot=B.DeptCode  WHERE A.deptName LIKE '%" + P1 + "%' OR B.deptName LIKE '%" + P1 + "%'");

            dt1.AddColum("PNAME", '/', "DeptName", "DeptName1");
            dt2.AddColum("PNAME", '/', "deptName", "deptName1");

            msg.Result = dt1;
            msg.Result1 = dt2;


        }


        #endregion



    }

    public class WXUserBR
    {
        public int DeptCode { get; set; }
        public string DeptName { get; set; }
        public dynamic DeptUser { get; set; }
        public int DeptUserNum { get; set; }
        public List<WXUserBR> SubDept { get; set; }
    }

    public class ExtDataModel
    {
        public int ComId { get; set; }
        public int ID { get; set; }
        public string TableName { get; set; }
        public string TableFiledColumn { get; set; }
        public string TableFiledName { get; set; }
        public string TableFileType { get; set; }
        public string DefaultOption { get; set; }
        public string DefaultValue { get; set; }
        public string IsRequire { get; set; }
        public int? ExtendModeID { get; set; }
        public int? ExtID { get; set; }
        public int? DataID { get; set; }
        public string ExtendDataValue { get; set; }
    }

}