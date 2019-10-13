using System;
using System.Collections.Generic;
using System.Text;

namespace QJY.API.API
{
    class Class1
    {

      


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
            string strMsgType = context.Request["MsgType"] ?? "";
            string strMsgLYID = context.Request["MsgLYID"] ?? "";
            string strPoints = context.Request["Points"] ?? "0";
            string strfjID = context.Request["fjID"] ?? "";
            string strTLID = context.Request["TLID"] ?? "";


            if (!string.IsNullOrEmpty(P1) && APIHelp.TestWB(P1) != "0")
            {
                msg.ErrorMsg = "您得发言涉及违规内容,请完善后再发";
                return;
            }


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
            if (Model.MSGType == "GZBG" || Model.MSGType == "RWGL" || Model.MSGType == "TSSQ")
            {
                int modelId = int.Parse(Model.MSGTLYID);
                string CRUser = "";
                string Content = UserInfo.User.UserRealName + "评论了您的";


                if (CRUser != UserInfo.User.UserName)
                {
                    SZHL_TXSX CSTX = new SZHL_TXSX();
                    CSTX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    CSTX.APIName = "XTGL";
                    CSTX.ComId = UserInfo.User.ComId;
                    CSTX.FunName = "SENDPLMSG";
                    CSTX.CRUserRealName = UserInfo.User.UserRealName;
                    CSTX.MsgID = modelId.ToString();

                    CSTX.TXContent = Content;
                    CSTX.ISCS = "N";
                    CSTX.TXUser = CRUser;
                    CSTX.TXMode = Model.MSGType;
                    CSTX.CRUser = UserInfo.User.UserName;

                    TXSX.TXSXAPI.AddALERT(CSTX); //时间为发送时间 
                }
            }

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
            int.TryParse(context["p"] == null ? "0" : context["p"].ToString(), out page);
            int.TryParse(context["pagecount"] == null ? "8" : context["pagecount"].ToString(), out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new JH_Auth_TLB().GetDataPager(" JH_Auth_TL LEFT JOIN  SZHL_TSSQ ON  JH_Auth_TL.MSGTLYID=SZHL_TSSQ.ID ", " JH_Auth_TL.*,SZHL_TSSQ.HTNR ", pagecount, page, " JH_Auth_TL.CRDate desc", strWhere, ref total);
            msg.Result = dt;
            msg.Result1 = total;
        }

        #endregion
     


        public void UPLOADFILE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int fid = 3;
                if (!string.IsNullOrEmpty(P1))
                {
                    fid = Int32.Parse(P1);
                }

                List<FT_File> ls = new List<FT_File>();
                for (int i = 0; i < context.Request.Files.Count; i++)
                {
                    HttpPostedFile uploadFile = context.Request.Files[i];


                    string originalName = uploadFile.FileName;



                    string[] temp = uploadFile.FileName.Split('.');

                    //保存图片

                    string filename = System.Guid.NewGuid() + "." + temp[temp.Length - 1].ToLower();
                    string md5 = new CommonHelp().SaveFile(UserInfo.QYinfo, filename, uploadFile);

                    //MP4上传阿里云
                    if (Path.GetExtension(originalName).ToLower() == ".mp4")
                    {
                        md5 = md5.Replace("\"", "").Split(',')[0];
                        AliyunHelp.UploadToOSS(md5, "mp4", uploadFile.InputStream);
                    }

                    FT_File newfile = new FT_File();
                    newfile.ComId = UserInfo.User.ComId;
                    newfile.Name = uploadFile.FileName.Substring(0, uploadFile.FileName.LastIndexOf('.'));
                    newfile.FileMD5 = md5.Replace("\"", "").Split(',')[0];
                    newfile.zyid = md5.Split(',').Length == 2 ? md5.Split(',')[1] : md5.Split(',')[0];
                    newfile.FileSize = uploadFile.InputStream.Length.ToString();
                    newfile.FileVersin = 0;
                    newfile.CRDate = DateTime.Now;
                    newfile.CRUser = UserInfo.User.UserName;
                    newfile.UPDDate = DateTime.Now;
                    newfile.UPUser = UserInfo.User.UserName;
                    newfile.FileExtendName = temp[temp.Length - 1].ToLower();
                    newfile.FolderID = fid;
                    if (new List<string>() { "txt", "html", "doc", "mp4", "flv", "ogg", "jpg", "gif", "png", "bmp", "jpeg" }.Contains(newfile.FileExtendName.ToLower()))
                    {
                        newfile.ISYL = "Y";
                    }
                    if (new List<string>() { "pdf", "doc", "docx", "ppt", "pptx", "xls", "xlsx" }.Contains(newfile.FileExtendName.ToLower()))
                    {
                        newfile.ISYL = "Y";
                        //newfile.YLUrl = UserInfo.QYinfo.FileServerUrl + "/document/YL/" + newfile.zyid;
                        newfile.YLUrl = "https://www.txywpx.com/ViewV5/Base/doc.html?zyid=" + newfile.zyid;
                    }

                    if (P2 != "")
                    {
                        newfile.Name = P2.Substring(0, P2.LastIndexOf('.')); //文件名
                    }

                    new FT_FileB().Insert(newfile);

                    int filesize = 0;
                    int.TryParse(newfile.FileSize, out filesize);
                    new FT_FileB().AddSpace(UserInfo.User.ComId.Value, filesize);
                    //msg.Result = newfile;
                    ls.Add(newfile);
                }
                msg.Result = ls;
            }
            catch (Exception e)
            {
                msg.ErrorMsg = "上传图片";
            }
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void UPLOADFILEV1(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int fid = 3;
                if (!string.IsNullOrEmpty(P1))
                {
                    fid = Int32.Parse(P1);
                }
                CommonHelp.WriteLOG("文件数量" + context.Request.Files.Count);

                List<FT_File> ls = new List<FT_File>();
                for (int i = 0; i < context.Request.Files.Count; i++)
                {
                    HttpPostedFile uploadFile = context.Request.Files[i];
                    string originalName = uploadFile.FileName;
                    string[] temp = uploadFile.FileName.Split('.');

                    //保存图片

                    string filename = System.Guid.NewGuid() + "." + temp[temp.Length - 1].ToLower();
                    string md5 = new CommonHelp().SaveFile(UserInfo.QYinfo, filename, uploadFile);

                    string json = "[{filename:'" + uploadFile.FileName + "',md5:'" + md5.Split(',')[0] + "',zyid:'" + md5.Split(',')[1] + "',filesize:'" + uploadFile.InputStream.Length.ToString() + "'}]";
                    QYWDManage qywd = new QYWDManage();

                    //MP4上传阿里云
                    //if (Path.GetExtension(originalName).ToLower() == ".mp4")
                    //{
                    //    md5 = md5.Replace("\"", "").Split(',')[0];
                    //    AliyunHelp.UploadToOSS(md5, "mp4", uploadFile.InputStream);
                    //}

                    CommonHelp.WriteLOG("调用参数" + fid.ToString());
                    qywd.ADDFILE(context, msg, json, fid.ToString(), UserInfo);

                }
            }
            catch (Exception e)
            {
                msg.ErrorMsg = "上传图片";
            }
        }

        /// <summary>
        /// 上传文件（文档中心）
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void UPLOADFILES(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                HttpPostedFile uploadFile = context.Request.Files["upFile"];
                string originalName = uploadFile.FileName;
                string[] temp = uploadFile.FileName.Split('.');

                //保存图片
                string filename = System.Guid.NewGuid() + "." + temp[temp.Length - 1].ToLower();
                string md5 = new CommonHelp().SaveFile(UserInfo.QYinfo, filename, uploadFile);
                string json = "[{filename:'" + uploadFile.FileName + "',md5:" + md5 + ",filesize:'" + uploadFile.InputStream.Length.ToString() + "'}]";

                QYWDManage qywd = new QYWDManage();
                qywd.ADDFILE(context, msg, json, P1, UserInfo);

            }
            catch (Exception e)
            {
                msg.ErrorMsg = "上传图片";
            }
        }



        #region excel转换为table

        /// <summary>
        /// excel转换为table
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void EXCELTOTABLE(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                DataTable dt = new DataTable();
                HttpPostedFile _upfile = context.Request.Files["upFile"];
                string headrow = context.Request["headrow"] ?? "0";//头部开始行下标
                if (_upfile == null)
                {
                    msg.ErrorMsg = "请选择要上传的文件 ";
                }
                else
                {
                    string fileName = _upfile.FileName;/*获取文件名： C:\Documents and Settings\Administrator\桌面\123.jpg*/
                    string suffix = fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower();/*获取后缀名并转为小写： jpg*/
                    int bytes = _upfile.ContentLength;//获取文件的字节大小   
                    if (suffix == "xls" || suffix == "xlsx")
                    {
                        IWorkbook workbook = null;

                        Stream stream = _upfile.InputStream;

                        if (suffix == "xlsx") // 2007版本
                        { 
                            workbook = new XSSFWorkbook(stream);
                        }
                        else if (suffix == "xls") // 2003版本
                        {
                            workbook = new HSSFWorkbook(stream);
                        }

                        //获取excel的第一个sheet
                        ISheet sheet = workbook.GetSheetAt(0);

                        //获取sheet的第一行
                        IRow headerRow = sheet.GetRow(int.Parse(headrow));

                        //一行最后一个方格的编号 即总的列数
                        int cellCount = headerRow.LastCellNum;
                        //最后一列的标号  即总的行数
                        int rowCount = sheet.LastRowNum;
                        if (rowCount <= int.Parse(headrow))
                        {
                            msg.ErrorMsg = "文件中无数据! ";
                        }
                        else
                        {

                            List<JH_Auth_ExtendDataB.IMPORTYZ> yz = new List<JH_Auth_ExtendDataB.IMPORTYZ>();
                            yz = new JH_Auth_ExtendDataB().GetTable(P1, UserInfo.QYinfo.ComId);//获取字段
                            string str1 = string.Empty;//验证字段是否包含列名
                            //列名
                            for (int i = 0; i < cellCount; i++)
                            {
                                string strlm = headerRow.GetCell(i).ToString().Trim();
                                if (yz.Count > 0)
                                {
                                    #region 字段是否包含列名验证
                                    var l = yz.Where(p => p.Name == strlm);//验证字段是否包含列名
                                    if (l.Count() == 0)
                                    {
                                        if (string.IsNullOrEmpty(str1))
                                        {
                                            str1 = "文件中的【" + strlm + "】";
                                        }
                                        else
                                        {
                                            str1 = str1 + "、【" + strlm + "】";
                                        }

                                        strlm = strlm + "<span style='color:red;'>不会导入</span>";
                                    }
                                    #endregion
                                }
                                dt.Columns.Add(strlm);//添加列名
                            }

                            if (!string.IsNullOrEmpty(str1))
                            {
                                str1 = str1 + " 不属于当前导入的字段!<br>";
                            }

                            dt.Columns.Add("status", Type.GetType("System.String"));

                            string str2 = string.Empty;//验证必填字段是否存在

                            #region 必填字段在文件中存不存在验证
                            foreach (var v in yz.Where(p => p.IsNull == 1))
                            {
                                if (!dt.Columns.Contains(v.Name))
                                {
                                    if (string.IsNullOrEmpty(str2))
                                    {
                                        str2 = "当前导入的必填字段：【" + v.Name + "】";
                                    }
                                    else
                                    {
                                        str2 = str2 + "、【" + v.Name + "】";
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(str2))
                            {
                                str2 = str2 + " 在文件中不存在!<br>";
                            }
                            #endregion

                            string str3 = string.Empty;//验证必填字段是否有值
                            string str4 = string.Empty;//验证字段是否重复
                            string str5 = string.Empty;//验证字段是否存在

                            for (int i = (sheet.FirstRowNum + int.Parse(headrow) + 1); i <= sheet.LastRowNum; i++)
                            {
                                string str31 = string.Empty;
                                string str41 = string.Empty;
                                string str42 = string.Empty;
                                string str51 = string.Empty;

                                DataRow dr = dt.NewRow();
                                bool bl = false;
                                IRow row = sheet.GetRow(i);

                                dr["status"] = "0";

                                for (int j = row.FirstCellNum; j < cellCount; j++)
                                {
                                    string strsj = row.GetCell(j) != null ? row.GetCell(j).ToString().Trim() : "";
                                    if (strsj != "")
                                    {
                                        bl = true;
                                    }

                                    foreach (var v in yz.Where(p => p.Name == headerRow.GetCell(j).ToString().Trim()))
                                    {
                                        if (strsj == "")
                                        {
                                            #region 必填字段验证
                                            if (v.IsNull == 1)
                                            {
                                                //strsj = "<span style='color:red;'>必填</span>";

                                                if (string.IsNullOrEmpty(str31))
                                                {
                                                    str31 = "第" + (i + 1) + "行的必填字段：【" + v.Name + "】";
                                                }
                                                else
                                                {
                                                    str31 = str31 + "、【" + v.Name + "】";
                                                }
                                                dr["status"] = "2";
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region 长度验证
                                            if (v.Length != 0)
                                            {
                                                if (Encoding.Default.GetBytes(strsj).Length > v.Length)
                                                {
                                                    strsj = strsj + "<span style='color:red;'>长度不能超过" + v.Length + "</span>";
                                                    dr["status"] = "2";
                                                }
                                            }
                                            #endregion
                                            #region 重复验证
                                            if (!string.IsNullOrEmpty(v.IsRepeat))
                                            {
                                                #region 与现有数据比较是否重复
                                                string[] strRS = v.IsRepeat.Split('|');

                                                var cf = new JH_Auth_UserB().GetDTByCommand("select * from " + strRS[0] + " where " + strRS[1] + "= '" + strsj + "' and ComId='" + UserInfo.QYinfo.ComId + "'");
                                                if (cf.Rows.Count > 0)
                                                {
                                                    if (string.IsNullOrEmpty(str41))
                                                    {
                                                        str41 = "第" + (i + 1) + "行的字段：【" + v.Name + "】" + strsj;
                                                    }
                                                    else
                                                    {
                                                        str41 = str41 + "、【" + v.Name + "】:" + strsj;
                                                    }
                                                    dr["status"] = "2";
                                                }
                                                #endregion
                                                #region 与Excel中数据比较是否重复
                                                DataRow[] drs = dt.Select(headerRow.GetCell(j).ToString().Trim() + "='" + strsj + "'");
                                                if (drs.Length > 0)
                                                {
                                                    if (string.IsNullOrEmpty(str42))
                                                    {
                                                        str42 = "第" + (i + 1) + "行的字段：【" + v.Name + "】" + strsj;
                                                    }
                                                    else
                                                    {
                                                        str42 = str42 + "、【" + v.Name + "】" + strsj;
                                                    }
                                                    dr["status"] = "2";
                                                }
                                                #endregion
                                            }
                                            #endregion
                                            #region 存在验证
                                            if (!string.IsNullOrEmpty(v.IsExist))
                                            {
                                                string[] strES = v.IsExist.Split('|');

                                                var cz = new JH_Auth_UserB().GetDTByCommand("select * from " + strES[0] + " where " + strES[1] + "= '" + strsj + "' and ComId='" + UserInfo.QYinfo.ComId + "'");
                                                if (cz.Rows.Count == 0)
                                                {
                                                    if (string.IsNullOrEmpty(str51))
                                                    {
                                                        str51 = "第" + (i + 1) + "行的字段：【" + v.Name + "】" + strsj;
                                                    }
                                                    else
                                                    {
                                                        str51 = str51 + "、【" + v.Name + "】" + strsj;
                                                    }
                                                    dr["status"] = "2";
                                                }
                                            }
                                            #endregion
                                        }
                                    }

                                    dr[j] = strsj;
                                }
                                if (!string.IsNullOrEmpty(str31))
                                {
                                    str31 = str31 + " 不能为空！<br>";
                                    str3 = str3 + str31;
                                }
                                if (!string.IsNullOrEmpty(str41))
                                {
                                    str41 = str41 + " 已经存在！<br>";
                                    str4 = str4 + str41;
                                }
                                if (!string.IsNullOrEmpty(str42))
                                {
                                    str42 = str42 + " 在文件中已经存在！<br>";
                                    str4 = str4 + str42;
                                }
                                if (!string.IsNullOrEmpty(str51))
                                {
                                    str51 = str51 + " 不存在！<br>";
                                    str5 = str5 + str51;
                                }

                                if (bl)
                                {
                                    dt.Rows.Add(dr);
                                }
                            }
                            if (string.IsNullOrEmpty(str2) && string.IsNullOrEmpty(str3) && string.IsNullOrEmpty(str4) && string.IsNullOrEmpty(str5))
                            {
                                msg.Result = dt;
                            }

                            msg.Result1 = str1 + str2 + str3 + str4 + str5;
                        }

                        sheet = null;
                        workbook = null;
                    }
                    else
                    {
                        msg.ErrorMsg = "请上传excel文件 ";
                    }
                }
            }
            catch (Exception ex)
            {
                //msg.ErrorMsg = ex.ToString();
                msg.ErrorMsg = "导入失败！";
            }
        }

        #endregion

        #region 导出模板excel

        /// <summary>
        /// 导出模板excel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void EXPORTTOEXCEL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                List<JH_Auth_ExtendDataB.IMPORTYZ> yz = new List<JH_Auth_ExtendDataB.IMPORTYZ>();
                yz = new JH_Auth_ExtendDataB().GetTable(P1, UserInfo.QYinfo.ComId);//获取字段
                if (yz.Count > 0)
                {
                    HSSFWorkbook workbook = new HSSFWorkbook();
                    ISheet sheet = workbook.CreateSheet("Sheet1");

                    ICellStyle HeadercellStyle = workbook.CreateCellStyle();
                    HeadercellStyle.BorderBottom = BorderStyle.Thin;
                    HeadercellStyle.BorderLeft = BorderStyle.Thin;
                    HeadercellStyle.BorderRight = BorderStyle.Thin;
                    HeadercellStyle.BorderTop = BorderStyle.Thin;
                    HeadercellStyle.Alignment = HorizontalAlignment.Center;
                    HeadercellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SkyBlue.Index;
                    HeadercellStyle.FillPattern = FillPattern.SolidForeground;
                    HeadercellStyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.SkyBlue.Index;

                    //字体
                    NPOI.SS.UserModel.IFont headerfont = workbook.CreateFont();
                    headerfont.Boldweight = (short)FontBoldWeight.Bold;
                    headerfont.FontHeightInPoints = 12;
                    HeadercellStyle.SetFont(headerfont);


                    //用column name 作为列名
                    int icolIndex = 0;
                    IRow headerRow = sheet.CreateRow(0);
                    foreach (var l in yz)
                    {
                        ICell cell = headerRow.CreateCell(icolIndex);
                        cell.SetCellValue(l.Name);
                        cell.CellStyle = HeadercellStyle;
                        icolIndex++;
                    }

                    ICellStyle cellStyle = workbook.CreateCellStyle();

                    //为避免日期格式被Excel自动替换，所以设定 format 为 『@』 表示一率当成text來看
                    cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
                    cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;


                    NPOI.SS.UserModel.IFont cellfont = workbook.CreateFont();
                    cellfont.Boldweight = (short)FontBoldWeight.Normal;
                    cellStyle.SetFont(cellfont);

                    string strDataJson = new JH_Auth_ExtendDataB().GetExcelData(P1);
                    if (strDataJson != "")
                    {
                        string[] strs = strDataJson.Split(',');

                        //建立内容行

                        int iCellIndex = 0;

                        IRow DataRow = sheet.CreateRow(1);
                        for (int i = 0; i < strs.Length; i++)
                        {

                            ICell cell = DataRow.CreateCell(iCellIndex);
                            cell.SetCellValue(strs[i]);
                            cell.CellStyle = cellStyle;
                            iCellIndex++;
                        }
                    }

                    //自适应列宽度
                    for (int i = 0; i < icolIndex; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        workbook.Write(ms);

                        object curContext = object.Current;

                        string strName = string.Empty;


                        // 设置编码和附件格式
                        curContext.Response.ContentType = "application/vnd.ms-excel";
                        curContext.Response.ContentEncoding = Encoding.UTF8;
                        curContext.Response.Charset = "";
                        curContext.Response.AppendHeader("Content-Disposition",
                            "attachment;filename=" + HttpUtility.UrlEncode("CRM_" + strName + "_模板文件.xls", Encoding.UTF8));

                        curContext.Response.BinaryWrite(ms.GetBuffer());
                        curContext.Response.End();

                        workbook = null;
                        ms.Close();
                        ms.Dispose();
                    }
                }

            }
            catch
            {
                msg.ErrorMsg = "导入失败！";
            }
        }

        /// <summary>
        /// 下载模板excel(弃用)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DOWNLOADEXCEL(JObject context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strName = string.Empty;
                if (P1 == "KHGL")
                {
                    strName = "CRM_客户_导入模板.xls";
                }
                else if (P1 == "KHLXR")
                {
                    strName = "CRM_客户联系人_导入模板.xls";
                }
                else if (P1 == "HTGL")
                {
                    strName = "CRM_合同_导入模板.xls";
                }
                object curContext = object.Current;
                string headrow = context.Request["headrow"] ?? "0";//头部开始行下标
                string path = curContext.Server.MapPath(@"/ViewV5/base/" + strName);
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                string suffix = path.Substring(path.LastIndexOf(".") + 1).ToLower();

                IWorkbook workbook = null;

                if (suffix == "xlsx") // 2007版本
                {
                    workbook = new XSSFWorkbook(file);
                }
                else if (suffix == "xls") // 2003版本
                {
                    workbook = new HSSFWorkbook(file);
                }
                ISheet sheet = workbook.GetSheetAt(0);

                IRow headerRow = sheet.GetRow(int.Parse(headrow));
                IRow oneRow = sheet.GetRow(int.Parse(headrow) + 1);

                int icolIndex = headerRow.Cells.Count;

                DataTable dtExtColumn = new JH_Auth_ExtendModeB().GetExtColumnAll(UserInfo.QYinfo.ComId, P1);
                foreach (DataRow drExt in dtExtColumn.Rows)
                {
                    ICell cell = headerRow.CreateCell(icolIndex);
                    cell.SetCellValue(drExt["TableFiledName"].ToString());
                    cell.CellStyle = headerRow.Cells[icolIndex - 1].CellStyle;

                    ICell onecell = oneRow.CreateCell(icolIndex);
                    onecell.SetCellValue("");
                    onecell.CellStyle = oneRow.Cells[icolIndex - 1].CellStyle;

                    icolIndex++;
                }

                //自适应列宽度
                for (int i = 0; i < icolIndex; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                if (P1 == "KHGL")
                {
                    //表头样式
                    ICellStyle HeadercellStyle = workbook.CreateCellStyle();
                    HeadercellStyle.BorderBottom = BorderStyle.Thin;
                    HeadercellStyle.BorderLeft = BorderStyle.Thin;
                    HeadercellStyle.BorderRight = BorderStyle.Thin;
                    HeadercellStyle.BorderTop = BorderStyle.Thin;
                    HeadercellStyle.Alignment = HorizontalAlignment.Center;

                    //字体
                    NPOI.SS.UserModel.IFont headerfont = workbook.CreateFont();
                    headerfont.Boldweight = (short)FontBoldWeight.Bold;
                    headerfont.FontHeightInPoints = 12;
                    HeadercellStyle.SetFont(headerfont);

                    //单元格样式
                    ICellStyle cellStyle = workbook.CreateCellStyle();

                    //为避免日期格式被Excel自动替换，所以设定 format 为 『@』 表示一率当成text來看
                    cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
                    cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;


                    NPOI.SS.UserModel.IFont cellfont = workbook.CreateFont();
                    cellfont.Boldweight = (short)FontBoldWeight.Normal;
                    headerfont.FontHeightInPoints = 10;
                    cellStyle.SetFont(cellfont);

                    for (int i = 10; i < 15; i++)
                    {
                        string strZTName = string.Empty;
                        if (i == 10) { strZTName = "客户类型"; }
                        if (i == 11) { strZTName = "跟进状态"; }
                        if (i == 12) { strZTName = "客户来源"; }
                        if (i == 13) { strZTName = "所属行业"; }
                        if (i == 14) { strZTName = "人员规模"; }
                        ISheet sheet1 = workbook.CreateSheet(strZTName);
                        IRow headerRow1 = sheet1.CreateRow(0);
                        ICell cell1 = headerRow1.CreateCell(0);
                        cell1.SetCellValue(strZTName);
                        cell1.CellStyle = HeadercellStyle;

                        int rowindex1 = 1;

                        foreach (var l in new JH_Auth_ZiDianB().GetEntities(p => p.ComId == UserInfo.QYinfo.ComId && p.Class == i))
                        {
                            IRow DataRow = sheet1.CreateRow(rowindex1);
                            ICell cell = DataRow.CreateCell(0);
                            cell.SetCellValue(l.TypeName);
                            cell.CellStyle = cellStyle;
                            rowindex1++;
                        }

                        sheet1.AutoSizeColumn(0);
                    }
                }
                if (P1 == "HTGL")
                {
                    //表头样式
                    ICellStyle HeadercellStyle = workbook.CreateCellStyle();
                    HeadercellStyle.BorderBottom = BorderStyle.Thin;
                    HeadercellStyle.BorderLeft = BorderStyle.Thin;
                    HeadercellStyle.BorderRight = BorderStyle.Thin;
                    HeadercellStyle.BorderTop = BorderStyle.Thin;
                    HeadercellStyle.Alignment = HorizontalAlignment.Center;

                    //字体
                    NPOI.SS.UserModel.IFont headerfont = workbook.CreateFont();
                    headerfont.Boldweight = (short)FontBoldWeight.Bold;
                    headerfont.FontHeightInPoints = 12;
                    HeadercellStyle.SetFont(headerfont);

                    //单元格样式
                    ICellStyle cellStyle = workbook.CreateCellStyle();

                    //为避免日期格式被Excel自动替换，所以设定 format 为 『@』 表示一率当成text來看
                    cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
                    cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;


                    NPOI.SS.UserModel.IFont cellfont = workbook.CreateFont();
                    cellfont.Boldweight = (short)FontBoldWeight.Normal;
                    headerfont.FontHeightInPoints = 10;
                    cellStyle.SetFont(cellfont);

                    for (int i = 16; i < 18; i++)
                    {
                        string strZTName = string.Empty;
                        if (i == 16) { strZTName = "合同类型"; }
                        if (i == 17) { strZTName = "付款方式"; }
                        ISheet sheet1 = workbook.CreateSheet(strZTName);
                        IRow headerRow1 = sheet1.CreateRow(0);
                        ICell cell1 = headerRow1.CreateCell(0);
                        cell1.SetCellValue(strZTName);
                        cell1.CellStyle = HeadercellStyle;

                        int rowindex1 = 1;

                        foreach (var l in new JH_Auth_ZiDianB().GetEntities(p => p.ComId == UserInfo.QYinfo.ComId && p.Class == i))
                        {
                            IRow DataRow = sheet1.CreateRow(rowindex1);
                            ICell cell = DataRow.CreateCell(0);
                            cell.SetCellValue(l.TypeName);
                            cell.CellStyle = cellStyle;
                            rowindex1++;
                        }

                        sheet1.AutoSizeColumn(0);
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    workbook.Write(ms);

                    //object curContext = object.Current;

                    // 设置编码和附件格式
                    curContext.Response.ContentType = "application/vnd.ms-excel";
                    curContext.Response.ContentEncoding = Encoding.UTF8;
                    curContext.Response.Charset = "";
                    curContext.Response.AppendHeader("Content-Disposition",
                        "attachment;filename=" + HttpUtility.UrlEncode(strName, Encoding.UTF8));

                    curContext.Response.BinaryWrite(ms.GetBuffer());
                    curContext.Response.End();

                    workbook = null;
                    ms.Close();
                    ms.Dispose();
                }
            }
            catch
            {
                msg.ErrorMsg = "下载失败！";
            }
        }

        #endregion
    }
}
