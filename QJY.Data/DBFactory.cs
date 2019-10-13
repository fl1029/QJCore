using SqlSugar;
using System.Collections.Generic;
using System.Data;

namespace QJY.Data
{
    public class DBFactory
    {
        private SqlSugarClient db;

        public DBFactory()
        {
        }
        public static string DBType = Appsettings.app("DBType");//获取连接字符串
        public DBFactory(string ConnectionString)
        {
            db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConnectionString,        //必填, 数据库连接字符串
                DbType = DBType == "0" ? SqlSugar.DbType.MySql : SqlSugar.DbType.SqlServer,         //必填, 数据库类型
                IsAutoCloseConnection = true,               //默认false, 时候知道关闭数据库连接, 设置为true无需使用using或者Close操作
                InitKeyType = InitKeyType.SystemTable       //默认SystemTable, 字段信息读取, 如：该属性是不是主键，是不是标识列等等信息
            });

        }


        public DBFactory(string DBType, string DBIP, string Port, string DBName, string Schema, string DBUser, string DBPwd)
        {
            string connectionString = "";
            var DbType = SqlSugar.DbType.MySql;
            if (DBType.ToLower() == "sqlserver")
            {
                if (string.IsNullOrEmpty(Port))
                {
                    Port = "1433";
                }
                DbType = SqlSugar.DbType.SqlServer;
                connectionString = string.Format("Data Source ={0},{4};Initial Catalog ={1};User Id ={2};Password={3};", DBIP, DBName, DBUser, DBPwd, Port);
            }
            else if (DBType.ToLower() == "mysql")
            {
                DbType = SqlSugar.DbType.MySql;
                if (string.IsNullOrEmpty(Port))
                {
                    Port = "3306";
                }
                connectionString = string.Format("server={0};userid={1};password={2};database={3};PORT={4};", DBIP, DBUser, DBPwd, DBName, Port);

            }
            db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = connectionString,        //必填, 数据库连接字符串
                DbType = DbType,         //必填, 数据库类型
                IsAutoCloseConnection = true,               //默认false, 时候知道关闭数据库连接, 设置为true无需使用using或者Close操作
                InitKeyType = InitKeyType.SystemTable       //默认SystemTable, 字段信息读取, 如：该属性是不是主键，是不是标识列等等信息
            });

        }



        //测试连接
        public bool TestConn()
        {
            try
            {
                if (db.Ado.Connection.State != ConnectionState.Open)
                {
                    db.Ado.Connection.Open();
                }
                if (db.Ado.Connection.State == ConnectionState.Open)
                {
                    db.Ado.Connection.Close();
                    return true;
                }
            }
            catch { }

            return false;
        }

        public void CModel()
        {
            db.DbFirst.CreateClassFile("D:\\Demo\\1");
        }
        public DataTable GetTables()
        {
            DataTable data = db.Ado.GetDataTable("select name ,type_desc from sys.objects where type_desc in ('USER_TABLE','VIEW') ORDER BY name");
            return data;
        }


        public SqlSugarClient GetDBClient()
        {
            return db;
        }


        public DataTable GetSQL(string strSQL)
        {
            DataTable data = db.Ado.GetDataTable(strSQL);
            return data;

        }

        /// <summary>
        /// 获取SQL模型数据
        /// </summary>
        /// <param name="DS"></param>
        /// <param name="wd"></param>
        /// <param name="dl"></param>
        /// <param name="strTableFiled"></param>
        /// <param name="strDataCount"></param>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        public DataTable GetYBData(BI_DB_Set DS, string wd, string dl, string strDataCount, string strWhere, string Order)
        {
            DataTable dt = new DataTable();
            string strSQL = "";
            string strSQLCount = "";

            string strMYSQLCount = "";
            if (strDataCount != "0" && DBType == "0")
            {
                strMYSQLCount = " LIMIT " + strDataCount;
            }
            if (strDataCount != "0" && DBType == "1")
            {
                strSQLCount = " TOP " + strDataCount;
            }
            string strOrder = "";
            if (Order != "")
            {
                strOrder = " ORDER BY " + Order;
            }

            if (wd == "")
            {
                strSQL = string.Format("select  {0}  {1} from ({2}) as DATESET  WHERE 1=1  {3}  {4} {5}", strSQLCount, dl, DS.DSQL, strWhere, strOrder, strMYSQLCount);

            }
            else if (dl == "")
            {
                strSQL = string.Format("select  {0}  {1} from ({2}) as DATESET  WHERE 1=1  {3}  {4} {5}", strSQLCount, wd, DS.DSQL, strWhere, strOrder, strMYSQLCount);

            }
            else
            {
                strSQL = string.Format("select  {0}  {1},{2} from ({3}) as DATESET  WHERE 1=1  {4}  group by {1} {5} {6}", strSQLCount, wd, dl, DS.DSQL, strWhere, strOrder, strMYSQLCount);
            }
            dt = db.Ado.GetDataTable(strSQL);


            return dt;
        }



        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="DS"></param>
        /// <param name="strTable"></param>
        /// <returns></returns>
        public int InserData(Dictionary<string, object> DS, string strTable)
        {
            var t66 = db.Insertable(DS).AS(strTable).ExecuteReturnIdentity();
            return int.Parse(t66.ToString());

        }

        public int UpdateData(Dictionary<string, object> DS, string strTable)
        {
            var returndata = db.Updateable(DS).AS(strTable).ExecuteCommand();
            return int.Parse(returndata.ToString());

        }




    }
}

