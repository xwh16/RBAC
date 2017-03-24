using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using RBAC.lib.Model;
using System.Collections.ObjectModel;

namespace RBAC
{
    public class RoleBasedAccessControlSystem
    {
        public delegate void TransitionMessage(string msg);
        public event TransitionMessage handler;

        public RoleBasedAccessControlSystem(bool isAdmin)
        {
            ds = new DataSet();
            UserTable = GetUesrList();
            RoleTable = GetRoleList();
            PermissionTable = GetPermissionList();
            UserRole = GetUserRoleList();
            PermissionRole = GetRolePermissionList();
            RoleRelation = GetRoleRelationList();
            ExclusionTable = GetExclusionList();
            if (activeUser != null)
            {
                // 获取用户的所有角色
                FetchUserRoles();
                active_permissions = new ObservableCollection<PermissionActivation>();
                active_roles = new ObservableCollection<RoleActivation>();
            }
        }

        public void Update()
        {
            UserTable = GetUesrList();
            RoleTable = GetRoleList();
            PermissionTable = GetPermissionList();
            UserRole = GetUserRoleList();
            PermissionRole = GetRolePermissionList();
            RoleRelation = GetRoleRelationList();
            if (activeUser != null)
            {
                // 获取用户的所有角色
                FetchUserRoles();
            }
        }

        #region 用于刷新管理员界面的关系表
        public List<UserRoleModel> getURAView()
        {
            var query = from table in UserRole.AsEnumerable()
                    join mark1 in UserTable.AsEnumerable()
                    on table.Field<int>("uid") equals mark1.Field<int>("uid")
                    join mark2 in RoleTable.AsEnumerable()
                    on table.Field<int>("rid") equals mark2.Field<int>("rid")
                    select new UserRoleModel(
                        table.Field<int>("id"),
                        table.Field<int>("uid"),
                        mark1.Field<string>("name"),
                        table.Field<int>("rid"),
                        mark2.Field<string>("name")
                        );
            return query.ToList();
        }

        public List<PermissionRoleModel> getPRAView()
        {
            var query = from table in PermissionRole.AsEnumerable()
                        join mark1 in RoleTable.AsEnumerable()
                        on table.Field<int>("rid") equals mark1.Field<int>("rid")
                        join mark2 in PermissionTable.AsEnumerable()
                        on table.Field<int>("pid") equals mark2.Field<int>("pid")
                        select new PermissionRoleModel(
                            table.Field<int>("id"),
                            table.Field<int>("rid"),
                            mark1.Field<string>("name"),
                            table.Field<int>("pid"),
                            mark2.Field<string>("name")
                            );
            return query.ToList();
        }

        public List<RoleRelationModel> getRRAView()
        {
            var query = from table in RoleRelation.AsEnumerable()
                        join mark1 in RoleTable.AsEnumerable()
                        on table.Field<int>("parent") equals mark1.Field<int>("rid")
                        join mark2 in RoleTable.AsEnumerable()
                        on table.Field<int>("child") equals mark2.Field<int>("rid")
                        select new RoleRelationModel(
                            table.Field<int>("id"),
                            table.Field<int>("parent"),
                            mark1.Field<string>("name"),
                            table.Field<int>("child"),
                            mark2.Field<string>("name")
                            );
            return query.ToList();
        }

        public List<UserModel> getRoleUsers(int rid)
        {
            var query = from table in UserRole.AsEnumerable()
                        where table.Field<int>("rid") == rid
                        join mark1 in UserTable.AsEnumerable()
                        on table.Field<int>("uid") equals mark1.Field<int>("uid")
                        select new UserModel(
                            table.Field<int>("uid"),
                            mark1.Field<string>("name")
                            );
            return query.ToList();
        }

        public List<PermissionModel> getRolePermissions(int rid)
        {
            var query = from table in PermissionRole.AsEnumerable()
                        where table.Field<int>("rid") == rid
                        join mark1 in PermissionTable.AsEnumerable()
                        on table.Field<int>("pid") equals mark1.Field<int>("pid")
                        select new PermissionModel(
                            table.Field<int>("pid"),
                            mark1.Field<string>("name")
                            );
            return query.ToList();
        }
        #endregion

        #region ADO.NET 数据表（属性）
        internal DataSet ds;
        public DataTable UserTable, RoleTable, PermissionTable;
        public DataTable UserRole, PermissionRole, RoleRelation;
        internal static string connstring = @"server=localhost;userid=root;password=asdfg;database=RBAC";
        #endregion

        #region 前端登录用户相关
        /// <summary>
        /// 当前活动用户
        /// </summary>
        public static UserModel activeUser = null;
        /// <summary>
        /// 用户的所有角色
        /// </summary>
        public List<RoleModel> userRoles;
        /// <summary>
        /// 用户激活的所有权限
        /// </summary>
        public ObservableCollection<PermissionActivation> active_permissions;
        /// <summary>
        /// 用户激活的所有角色
        /// </summary>
        public ObservableCollection<RoleActivation> active_roles;

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="user"></param>
        public static void Login(UserModel user)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 匹配登录数据
                string c = "SELECT * FROM user WHERE name = @param1 AND password = @param2";
                MySqlCommand cmd = new MySqlCommand(c, conn);
                cmd.Parameters.AddWithValue("@param1", user.name);
                cmd.Parameters.AddWithValue("@param2", user.password);

                MySqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    // 用户数据匹配成功
                    user.uid = Convert.ToInt32(rdr["uid"]);
                    activeUser = user;
                }
                else
                    throw new Exception("用户名或密码错误");
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception("登录失败\n" + "错误信息:" + ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }

        /// <summary>
        /// 用户注销
        /// </summary>
        public void Logout()
        {
            activeUser = null;
        }

        /// <summary>
        /// 返回用户被指派的所有角色
        /// </summary>
        /// <returns></returns>
        public List<RoleModel> FetchUserRoles()
        {
            IEnumerable<RoleModel> query = from table in UserRole.AsEnumerable()
                                            where table.Field<int>("uid") == activeUser.uid
                                            join foreign in RoleTable.AsEnumerable()
                                            on table.Field<int>("rid") equals foreign.Field<int>("rid")
                                            select
                                            new RoleModel(
                                            table.Field<int>("rid"),
                                            foreign.Field<string>("name")
                                            );
            userRoles = query.ToList();
            return userRoles;
        }

        /// <summary>
        /// 激活角色role并自动激活所有的下层角色
        /// </summary>
        /// <param name="role"></param>
        public void Acitvate(RoleModel role)
        {
            if (userRoles.Find(item => item.rid == role.rid) == null)
                throw new Exception("激活角色不是用户被直接分配的角色.");

            // 动态互斥检测
            foreach (RoleActivation item in active_roles)
            {
                if (CheckExclusion(role, new RoleModel(item.rid)) == true)
                    throw new Exception("与已激活角色: " + item.rolename +" 互斥");
            }
            

            // 对根节点的操作
            bool ret = ActivateRole(role, false);
            ActivatePermission(role, false);
            if (ret == true)
            {
                // 角色已激活
                return;
            }

            Queue<RoleModel> pending = new Queue<RoleModel>();    // 等待继承激活的节点
            pending.Enqueue(role);

            do
            {
                RoleModel current = pending.Dequeue();
                List<RoleModel> children = getChild(current);
                foreach (RoleModel child in children)
                {
                    ret = ActivateRole(child, true);
                    if (ret == true)
                    {
                        // 角色已激活
                        continue;
                    }
                    ActivatePermission(child, true);
                    pending.Enqueue(child);
                }
            } while (pending.Count > 0);

        }
        private bool ActivateRole(RoleModel role, bool isInherit)
        {
            RoleActivation query = active_roles.ToList().Find(item => item.rid == role.rid);
            if (query == null)
            {
                // 角色尚未获得
                RoleActivation activate =
                    new RoleActivation(role.rid, role.name, isInherit);
                active_roles.Add(activate);
                return false;
            }
            else
            {
                if (isInherit == false)
                    query.isHerit = false;

                // 角色已激活
                return true;
            }
        }
        private void ActivatePermission(RoleModel role, bool isInherit)
        {
            // isInherit = 1 表示继承权限
            // isInherit = 0 表示激活权限
            List<PermissionModel> permissions = getRolePermission(role);
            foreach (PermissionModel permission in permissions)
            {
                PermissionActivation query = active_permissions.ToList().Find(item => item.pid == permission.pid);
                if (query == null)
                {
                    // 权限尚未获得
                    PermissionActivation activate =
                        new PermissionActivation(permission.pid, permission.name, isInherit, role.rid, role.name);
                    active_permissions.Add(activate);
                }
                else
                {
                    // 权限已激活
                    if (isInherit == false)
                        query.isHerit = false;
                }
            }
        }
        #endregion

        #region user
        /// <summary>
        /// 获取所有用户列表
        /// </summary>
        public DataTable GetUesrList()
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 获取所有用户
                string c = "SELECT * FROM user";
                MySqlDataAdapter user_adapter = new MySqlDataAdapter(c, conn);
                user_adapter.FillAsync(ds, "user");
                ds.Tables["user"].PrimaryKey = new DataColumn[] { ds.Tables["user"].Columns["uid"] };
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
            return ds.Tables["user"];
        }
        /// <summary>
        /// 用户插入
        /// </summary>
        /// <param name="user">用户模型,id字段可为空</param>
        /// <param name="info">用户说明</param>
        public void AddUser(UserModel user, string info)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 生成新的DataRow对象
                DataRow row = UserTable.NewRow();
                if (UserTable.Rows.Count == 0)
                    // 表中没有数据
                    row["uid"] = 1;
                else 
                    // UID在最新的一列基础上+1
                    row["uid"] = Convert.ToInt32(UserTable.Rows[UserTable.Rows.Count-1]["uid"])+1;
                row["name"] = user.name;
                row["password"] = user.password;
                row["info"] = info;
                // 将新用户添加到DataSet当中
                UserTable.Rows.Add(row);

                // 应用更改
                string c = "SELECT * FROM user";
                MySqlDataAdapter user_adapter = new MySqlDataAdapter(c, conn);
                // 自动构造数据库操作语句
                MySqlCommandBuilder builder = new MySqlCommandBuilder(user_adapter);
                user_adapter.Update(UserTable);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        /// <summary>
        /// 用户删除
        /// </summary>
        /// <param name="user">用户模型,只需填充id字段</param>
        public void DeleteUser(UserModel user)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                DataRow row = UserTable.Rows.Find(Convert.ToInt32(user.uid));
                row.Delete();

                string c = "SELECT * FROM user";
                MySqlDataAdapter user_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(user_adapter);
                user_adapter.Update(UserTable);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        #endregion

        #region role
        /// <summary>
        /// 获取所有用户列表
        /// </summary>
        public DataTable GetRoleList()
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 获取所有用户
                string c = "SELECT * FROM role";
                MySqlDataAdapter role_adapter = new MySqlDataAdapter(c, conn);
                role_adapter.FillAsync(ds, "role");
                ds.Tables["role"].PrimaryKey = new DataColumn[] { ds.Tables["role"].Columns["rid"] };
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
            return ds.Tables["role"];
        }
        /// <summary>
        /// 角色插入
        /// </summary>
        /// <param name="role">角色模型</param>
        /// <param name="info">角色信息</param>
        public void AddRole(RoleModel role, string info)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                DataRow row = RoleTable.NewRow();
                if (RoleTable.Rows.Count == 0)
                    row["rid"] = 1;
                else
                    row["rid"] = Convert.ToInt32(RoleTable.Rows[RoleTable.Rows.Count - 1]["rid"]) + 1;
                row["name"] = role.name;
                row["info"] = info;
                RoleTable.Rows.Add(row);

                string c = "SELECT * FROM role";
                MySqlDataAdapter role_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(role_adapter);
                role_adapter.Update(RoleTable);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        /// <summary>
        /// 角色删除
        /// 节点的删除导致RH关系树中可能出现连接悬空
        /// 需要对RH/UR/PR关系进行修复
        /// 1. 在parent与child之间建立一对一联系
        /// 2. 当前节点的User与Permission全部下放到child
        /// </summary>
        /// <param name="role"></param>
        public void DeleteRole(RoleModel role)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 复杂事务处理 **大头**
                try
                {
                    reconnection(role);
                }
                catch(Exception ex)
                {
                    Debug.Write(ex.Message);
                }

                DataRow[] rows = RoleTable.Select("rid = " + role.rid.ToString());
                foreach (DataRow row in rows)
                    row.Delete();

                string c = "SELECT * FROM role";
                MySqlDataAdapter role_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(role_adapter);
                role_adapter.Update(RoleTable);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        /// <summary>
        /// 角色删除的预处理
        /// </summary>
        /// <param name="user"></param>
        /// <param name="conn"></param>
        internal void reconnection(RoleModel role)
        {
            List<RoleModel> child = getChild(role);
            List<RoleModel> parent = getParent(role);

            foreach (RoleModel _parent in parent)
            {
                foreach (RoleModel _child in child)
                {
                    // 在parent与child之前建立连接
                    AssignRoleRelation(_parent, _child);
                }
            }

            List<UserModel> users = getRoleUser(role);
            foreach (UserModel _user in users)
            {
                foreach (RoleModel _child in child)
                {
                    // 将role的user下放到child
                    AssignUserRole(_user, _child);
                }
            }

            List<PermissionModel> permissions = getRolePermission(role);
            foreach (PermissionModel _permission in permissions)
            {
                foreach (RoleModel _parent in parent)
                {
                    // 将role的permission上递到parent
                    AssignRolePermission(_parent, _permission);
                }
            }


        }
        #region internal methods
        /// <summary>
        /// 获取角色的所有子角色
        /// </summary>
        /// <param name="role"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        internal List<RoleModel> getChild(RoleModel role)
        {
            IEnumerable<RoleModel> query = from table in RoleRelation.AsEnumerable()
                                            where table.Field<int>("parent") == role.rid
                                            join foreign in RoleTable.AsEnumerable()
                                            on table.Field<int>("child") equals foreign.Field<int>("rid")
                                            select
                                            new RoleModel(
                                            table.Field<int>("child"),
                                            foreign.Field<string>("name")
                                            );
            return query.ToList();
        }
        /// <summary>
        /// 获取角色的所有父角色
        /// </summary>
        /// <param name="role"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        internal List<RoleModel> getParent(RoleModel role)
        {
            IEnumerable<RoleModel> query = from table in RoleRelation.AsEnumerable()
                                            where table.Field<int>("child") == role.rid
                                            join foreign in RoleTable.AsEnumerable()
                                            on table.Field<int>("child") equals foreign.Field<int>("rid")
                                            select
                                            new RoleModel(
                                            table.Field<int>("parent"),
                                            foreign.Field<string>("name")
                                            );
            return query.ToList();
        }
        /// <summary>
        /// 获取角色的所有用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        internal List<UserModel> getRoleUser(RoleModel role)
        {
            IEnumerable<UserModel> query = from table in UserRole.AsEnumerable()
                                            where table.Field<int>("rid") == role.rid
                                            join foreign in UserTable.AsEnumerable()
                                            on table.Field<int>("uid") equals foreign.Field<int>("uid")
                                            select
                                            new UserModel(
                                            table.Field<int>("uid"),
                                            foreign.Field<string>("name")
                                            );
            return query.ToList();
        }
        /// <summary>
        /// 获取角色的所有权限
        /// </summary>
        /// <returns></returns>
        internal List<PermissionModel> getRolePermission(RoleModel role)
        {
            IEnumerable<PermissionModel> query = from table in PermissionRole.AsEnumerable()
                                            where table.Field<int>("rid") == role.rid
                                            join foreign in PermissionTable.AsEnumerable()
                                            on table.Field<int>("pid") equals foreign.Field<int>("pid")
                                            select
                                            new PermissionModel(
                                            table.Field<int>("pid"),
                                            foreign.Field<string>("name")
                                            );
            return query.ToList();
        }
        #endregion
        #endregion

        #region Permission
        /// <summary>
        /// 获取所有权限列表
        /// </summary>
        public DataTable GetPermissionList()
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 获取所有用户
                string c = "SELECT * FROM Permission";
                MySqlDataAdapter Permission_adapter = new MySqlDataAdapter(c, conn);
                Permission_adapter.FillAsync(ds, "Permission");
                ds.Tables["Permission"].PrimaryKey = new DataColumn[] { ds.Tables["Permission"].Columns["pid"] };
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
            return ds.Tables["Permission"];
        }
        /// <summary>
        /// 权限插入
        /// </summary>
        /// <param name="Permission"></param>
        /// <param name="info"></param>
        public void AddPermission(PermissionModel Permission, string info)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                DataRow row = PermissionTable.NewRow();
                if (PermissionTable.Rows.Count == 0)
                    row["pid"] = 1;
                else
                    row["pid"] = Convert.ToInt32(PermissionTable.Rows[PermissionTable.Rows.Count - 1]["pid"]) + 1;
                row["name"] = Permission.name;
                row["info"] = info;
                PermissionTable.Rows.Add(row);

                string c = "SELECT * FROM Permission";
                MySqlDataAdapter Permission_adaper = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(Permission_adaper);
                Permission_adaper.Update(PermissionTable);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        /// <summary>
        /// 权限删除
        /// </summary>
        /// <param name="Permission"></param>
        public void DeletePermission(PermissionModel Permission)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                DataRow row = PermissionTable.Rows.Find(Convert.ToInt32(Permission.pid));
                row.Delete();

                string c = "SELECT * FROM Permission";
                MySqlDataAdapter Permission_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(Permission_adapter);
                Permission_adapter.Update(PermissionTable);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        #endregion

        #region ura
        /// <summary>
        /// 获取所有用户-角色分配
        /// </summary>
        /// <returns></returns>
        public DataTable GetUserRoleList()
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                string c = "SELECT * FROM ura";
                MySqlDataAdapter ura_adpator = new MySqlDataAdapter(c, conn);
                ura_adpator.FillAsync(ds, "ura");
                ds.Tables["ura"].PrimaryKey = new DataColumn[] { ds.Tables["ura"].Columns["id"] };
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
            return ds.Tables["ura"];
        }
        /// <summary>
        /// 分配用户角色
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        public void AssignUserRole(UserModel user, RoleModel role)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 预先查询要添加的用户-角色关系以避免重复分配
                var query = from table in UserRole.AsEnumerable()
                             where table.Field<int>("uid") == user.uid
                             && table.Field<int>("rid") == role.rid
                             select table;
                if (query.ToList().Count > 0)
                    throw new Exception("Duplicate User-Role relationship detected.");

                // 向DataSet中的数据表插入数据行
                DataRow row = UserRole.NewRow();
                if (UserRole.Rows.Count == 0)
                    row["id"] = 1;
                else
                    row["id"] = Convert.ToInt32(UserRole.Rows[UserRole.Rows.Count - 1]["id"]) + 1;
                row["uid"] = user.uid;
                row["rid"] = role.rid;
                UserRole.Rows.Add(row);

                // 使用DataAdapter更新数据库
                string c = "SELECT * FROM ura";
                MySqlDataAdapter ura_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(ura_adapter);
                ura_adapter.Update(UserRole);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        /// <summary>
        /// 撤销用户-角色关系
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        public void RevokeUserRole(UserModel user, RoleModel role)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 查询将要撤销的用户-角色关系是否存在
                IEnumerable<DataRow> query = from table in UserRole.AsEnumerable()
                             where table.Field<int>("uid") == user.uid
                             && table.Field<int>("rid") == role.rid
                             select table;
                if (query.ToList().Count == 0)
                    throw new Exception("用户-角色关系不存在");

                // 删除查询到的用户-角色关系项
                foreach (DataRow rowitem in query)
                {
                    rowitem.Delete();
                }

                // 使用DataAdapter更新数据库
                string c = "SELECT * FROM ura";
                MySqlDataAdapter ura_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(ura_adapter);
                ura_adapter.Update(UserRole);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        #endregion

        #region pra
        /// <summary>
        /// 获取所有角色-权限分配
        /// </summary>
        /// <returns></returns>
        public DataTable GetRolePermissionList()
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                string c = "SELECT * FROM pra";
                MySqlDataAdapter ura_adpator = new MySqlDataAdapter(c, conn);
                ura_adpator.FillAsync(ds, "pra");
                ds.Tables["pra"].PrimaryKey = new DataColumn[] { ds.Tables["pra"].Columns["id"] };
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
            return ds.Tables["pra"];
        }
        /// <summary>
        /// 分配角色权限
        /// </summary>
        /// <param name="role"></param>
        /// <param name="Permission"></param>
        public void AssignRolePermission(RoleModel role, PermissionModel Permission)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 预先查询要添加的角色-权限关系以避免重复分配
                var query = from table in PermissionRole.AsEnumerable()
                             where table.Field<int>("rid") == role.rid
                             && table.Field<int>("pid") == Permission.pid
                             select table;
                if (query.ToList().Count > 0)
                    throw new Exception("Duplicate Permission-Role relationship detected.");

                // 向DataSet中的数据表插入数据行
                DataRow row = PermissionRole.NewRow();
                if (PermissionRole.Rows.Count == 0)
                    row["id"] = 1;
                else
                    row["id"] = Convert.ToInt32(PermissionRole.Rows[PermissionRole.Rows.Count - 1]["id"]) + 1;
                row["rid"] = role.rid;
                row["pid"] = Permission.pid;
                PermissionRole.Rows.Add(row);

                // 使用DataAdapter更新数据库
                string c = "SELECT * FROM pra";
                MySqlDataAdapter pra_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(pra_adapter);
                pra_adapter.Update(PermissionRole);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        /// <summary>
        /// 撤销角色-权限关系
        /// </summary>
        /// <param name="role"></param>
        /// <param name="Permission"></param>
        public void RevokeRolePermission(RoleModel role, PermissionModel Permission)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 查询将要撤销的权限-角色关系是否存在
                IEnumerable<DataRow> query = from table in PermissionRole.AsEnumerable()
                                              where table.Field<int>("pid") == Permission.pid
                                              && table.Field<int>("rid") == role.rid
                                              select table;
                if (query.ToList().Count == 0)
                    throw new Exception("权限-角色关系不存在");

                // 删除查询到的权限-角色关系项
                foreach (DataRow rowitem in query)
                {
                    rowitem.Delete();
                }

                // 使用DataAdapter更新数据库
                string c = "SELECT * FROM pra";
                MySqlDataAdapter pra_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(pra_adapter);
                pra_adapter.Update(PermissionRole);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        #endregion

        #region rra
        /// <summary>
        /// 获取所有角色-角色层次关系
        /// </summary>
        /// <returns></returns>
        public DataTable GetRoleRelationList()
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                string c = "SELECT * FROM rra";
                MySqlDataAdapter ura_adpator = new MySqlDataAdapter(c, conn);
                ura_adpator.FillAsync(ds, "rra");
                ds.Tables["rra"].PrimaryKey = new DataColumn[] { ds.Tables["rra"].Columns["id"] };
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
            return ds.Tables["rra"];
        }
        /// <summary>
        /// 分配角色层次关系
        /// </summary>
        /// <param name="role"></param>
        /// <param name="child"></param>
        public void AssignRoleRelation(RoleModel role, RoleModel child)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                if (role.rid == child.rid)
                    throw new Exception("角色1-2重复");

                // 预先查询要添加的角色-角色关系以避免重复分配
                var query = from table in RoleRelation.AsEnumerable()
                             where table.Field<int>("parent") == role.rid
                             && table.Field<int>("child") == child.rid
                             select table;
                if (query.ToList().Count > 0)
                    throw new Exception("重复的角色关系.");
                // 检测角色-角色层次关系环路
                if (CheckLoop(role, child) == true)
                    throw new Exception("检测到角色环路");

                // 向DataSet中的数据表插入数据行
                DataRow row = RoleRelation.NewRow();
                if (RoleRelation.Rows.Count == 0)
                    row["id"] = 1;
                else
                    row["id"] = Convert.ToInt32(RoleRelation.Rows[RoleRelation.Rows.Count - 1]["id"]) + 1;
                row["parent"] = role.rid;
                row["child"] = child.rid;
                RoleRelation.Rows.Add(row);

                // 使用DataAdapter更新数据库
                string c = "SELECT * FROM rra";
                MySqlDataAdapter rra_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(rra_adapter);
                rra_adapter.Update(RoleRelation);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        /// <summary>
        /// 删除下级子角色
        /// </summary>
        /// <param name="node"></param>
        /// <param name="Permission"></param>
        public void RevokeRoleChild(RoleModel node, RoleModel child)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 查询将要撤销的角色-角色关系是否存在
                IEnumerable<DataRow> query = from table in RoleRelation.AsEnumerable()
                                              where table.Field<int>("child") == child.rid
                                              && table.Field<int>("parent") == node.rid
                                              select table;
                if (query.ToList().Count == 0)
                    throw new Exception("角色-角色关系不存在");

                // 删除查询到的角色-角色关系项
                foreach (DataRow rowitem in query)
                {
                    rowitem.Delete();
                }

                // 使用DataAdapter更新数据库
                string c = "SELECT * FROM rra";
                MySqlDataAdapter rra_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(rra_adapter);
                rra_adapter.Update(RoleRelation);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        /// <summary>
        /// 删除上级父角色
        /// </summary>
        /// <param name="node"></param>
        /// <param name="Permission"></param>
        public void RevokeRoleParent(RoleModel node, RoleModel parent)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 查询将要撤销的角色-角色关系是否存在
                IEnumerable<DataRow> query = from table in RoleRelation.AsEnumerable()
                                              where table.Field<int>("parent") == parent.rid
                                              && table.Field<int>("child") == node.rid
                                              select table;
                if (query.ToList().Count == 0)
                    throw new Exception("角色-角色关系不存在");

                // 删除查询到的角色-角色关系项
                foreach (DataRow rowitem in query)
                {
                    rowitem.Delete();
                }

                // 使用DataAdapter更新数据库
                string c = "SELECT * FROM rra";
                MySqlDataAdapter rra_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(rra_adapter);
                rra_adapter.Update(RoleRelation);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        #region internal methods
        /// <summary>
        /// 检查是否有从dest到source的环路径
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        internal bool CheckLoop(RoleModel source, RoleModel dest)
        {
            RoleModel current = null;

            List<RoleModel> visited = new List<RoleModel>();
            Queue<RoleModel> trace = new Queue<RoleModel>();
            trace.Enqueue(dest);
            do
            {
                // 获取当前源节点
                current = trace.Dequeue();
                // 获取当前源节点的所有子节点
                List<RoleModel> childs = getChild(current);
                foreach(RoleModel child in childs)
                {
                    if (child.rid == source.rid)
                    {
                        // 找到从dest出发到source的路径
                        return true;
                    }
                    else
                    {
                        if (visited.Find(item => item.rid == child.rid) == null)
                        {
                            // child节点尚未遍历到,
                            // 将当前节点的后继节点添加到列表中
                            visited.Add(child);
                            trace.Enqueue(child);
                        }

                    }
                }
            } while (trace.Count > 0);
            // 没有检测到环路
            return false;
        }
        #endregion
        #endregion

        #region exclusion
        public DataTable ExclusionTable;
        /// <summary>
        /// 获取互斥设置表
        /// </summary>
        /// <returns></returns>
        public DataTable GetExclusionList()
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 获取所有互斥项
                string c = "SELECT * FROM exclusion";
                MySqlDataAdapter exclusion_adapter = new MySqlDataAdapter(c, conn);
                exclusion_adapter.FillAsync(ds, "exclusion");
                ds.Tables["exclusion"].PrimaryKey = new DataColumn[] { ds.Tables["exclusion"].Columns["id"] };
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
            return ds.Tables["exclusion"];
        }
        /// <summary>
        /// 添加互斥规则
        /// </summary>
        /// <param name="exc"></param>
        /// <param name="info"></param>
        public void AddExclusion(ExclusionModel exc)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();


                if (exc.rid1 == exc.rid2)
                    throw new Exception("不可与自身互斥");

                // 预先查询要添加的互斥关系以避免重复分配
                //  由于是无向边因此需交换查询
                var query = from table in ExclusionTable.AsEnumerable()
                            where (table.Field<int>("rid1") == exc.rid1
                            && table.Field<int>("rid2") == exc.rid2)
                            || (table.Field<int>("rid1") == exc.rid2
                            && table.Field<int>("rid2") == exc.rid1)
                            select table;
                if (query.ToList().Count > 0)
                    throw new Exception("重复的互斥规则.");

                DataRow row = ExclusionTable.NewRow();
                if (ExclusionTable.Rows.Count == 0)
                    row["id"] = 1;
                else
                    row["id"] = Convert.ToInt32(ExclusionTable.Rows[ExclusionTable.Rows.Count - 1]["id"]) + 1;

                row["rid1"] = exc.rid1;
                row["rid2"] = exc.rid2;
                ExclusionTable.Rows.Add(row);

                string c = "SELECT * FROM exclusion";
                MySqlDataAdapter exec_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(exec_adapter);
                exec_adapter.Update(ExclusionTable);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        /// <summary>
        /// 删除互斥规则
        /// </summary>
        /// <param name="exc"></param>
        public void DeleteExclusion(ExclusionModel exc)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connstring);
                conn.Open();

                // 预先查询要添加的互斥关系以避免重复分配
                IEnumerable<DataRow> query = from table in ExclusionTable.AsEnumerable()
                            where table.Field<int>("id") == exc.id
                            select table;
                if (query.ToList().Count == 0)
                    throw new Exception("互斥规则不存在.");

                // 删除查询到的互斥规则
                foreach (DataRow rowitem in query)
                {
                    rowitem.Delete();
                }

                string c = "SELECT * FROM exclusion";
                MySqlDataAdapter exclusion_adapter = new MySqlDataAdapter(c, conn);
                MySqlCommandBuilder builder = new MySqlCommandBuilder(exclusion_adapter);
                exclusion_adapter.Update(ExclusionTable);
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Error: {0}", ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }
        }
        /// <summary>
        /// 检查角色是否互斥
        /// </summary>
        /// <param name="role1"></param>
        /// <param name="role2"></param>
        /// <returns></returns>
        public bool CheckExclusion(RoleModel role1, RoleModel role2)
        {
            var query = from table in ExclusionTable.AsEnumerable()
                        where table.Field<int>("rid1") == role1.rid && table.Field<int>("rid2") == role2.rid
                        || table.Field<int>("rid2") == role1.rid && table.Field<int>("rid1") == role2.rid
                        select table;
            if (query.ToList().Count == 0)
                return false;    // 不互斥
            else
                return true;   // 互斥
        }
        #endregion

    }

    public class TreeNode
    {
        public RoleBasedAccessControlSystem access;

        public TreeNode(RoleBasedAccessControlSystem _access, int _Id, string _Name)
        {
            Id = _Id;
            Name = _Name;

            access = _access;
            Children = new List<TreeNode>();

            // 获取子节点
            List<RoleModel> children = access.getChild(new RoleModel(Id));
            foreach (RoleModel role in children.ToList())
            {
                // 生成子节点
                TreeNode item = new TreeNode(access, role.rid, role.name);
                Children.Add(item);
            }

        }
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<TreeNode> Children { get; set; }
    }

    public class TreeViewModel
    {
        public RoleBasedAccessControlSystem access;

        private readonly List<TreeNode> treeList;

        public List<TreeNode> TreeList
        {
            get { return treeList; }
        }

        // 生成所有角色的树模型
        public TreeViewModel(RoleBasedAccessControlSystem _access)
        {
            access = _access;

            treeList = new List<TreeNode>();

            DataTable roles = access.GetRoleList();
            foreach (DataRow row in roles.AsEnumerable())
            {
                // 获取父节点
                if (access.getParent(new RoleModel(Convert.ToInt32(row["rid"]))).Count == 0)
                {
                    // 1级节点
                    // 生成节点
                    TreeNode item = new TreeNode(access, Convert.ToInt32(row["rid"]), row["name"].ToString());
                    treeList.Add(item);
                }
            }
        }

        // 生成登录用户角色的树模型
        public TreeViewModel(RoleBasedAccessControlSystem _access, int i)
        {
            bool mark = false;

            access = _access;

            treeList = new List<TreeNode>();

            // 用户的所有角色
            List<RoleModel> roles = access.userRoles;
            foreach (RoleModel row in roles.AsEnumerable())
            {
                // 获取父节点
                List<RoleModel> match = access.getParent(new RoleModel(row.rid));
                // 如果所有父节点均不在roles中则它为局部的父节点
                foreach (RoleModel item in match)
                {
                    if (roles.Find(one => one.rid == item.rid) != null)
                    {
                        mark = true;   // 找到了roles中的父节点
                        break;
                    }
                }
                if (!mark)
                {
                    // 1级节点
                    // 生成节点
                    TreeNode item = new TreeNode(access, row.rid, row.name);
                    treeList.Add(item);
                }
            }
        }
    }
}
