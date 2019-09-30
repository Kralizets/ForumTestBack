using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace ForumBack
{
    public class Program
    {
        static void Main(string[] args)
        {
            int currentPage = 2;
            int countElemetsOnPage = 2;
            string sortTypeAsc = "asc";
            string sortTypeDesc = "desc";

            int updateId = 10;

            var updateModel = new ForumModel
            {
                ThemeName = "UpdateName",
                ChangeDate = ForumLogic.GetDataById(updateId).ChangeDate
            };

            var insertModel = new ForumModel
            {
                ThemeName = "InsertModel",
                ChangeDate = new DateTime(2018, 12, 12)
            };

            int deleteId = 11;

            var allCount = ForumLogic.GetCountAllRecords();
            
            var testDataAsc = ForumLogic.GetDataForCurrentPage(currentPage, countElemetsOnPage, allCount, sortTypeAsc);
            var testDataDesc = ForumLogic.GetDataForCurrentPage(currentPage, countElemetsOnPage, allCount, sortTypeDesc);

            ForumLogic.UpdateDataById(updateId, updateModel);
            ForumLogic.InsertData(insertModel);
            ForumLogic.InsertData(insertModel);
            ForumLogic.DeleteDataById(deleteId);

            var newAllCount = ForumLogic.GetCountAllRecords();
        }
    }

    public class ForumModel
    {
        public int id { get; set; }
        public string ThemeName { get; set; }
        public DateTime ChangeDate { get; set; }
    }

    public static class ForumLogic
    {
        private static string conn = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        public static int GetCountAllRecords()
        {
            string query = @"select count(*) from Forum with(nolock)";

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                return db.Query<int>(query).SingleOrDefault();
            }
        }

        public static List<ForumModel> GetDataForCurrentPage(int currentPage, int countElemetsOnPage, int allCount, string sortType)
        {
            int skipElements = (currentPage - 1) * countElemetsOnPage;

            if (allCount < (skipElements + 1))
            {
                return new List<ForumModel>();
            }

            string query =
                @"select id, ThemeName, ChangeDate from Forum with(nolock)
                order by ChangeDate " + sortType +
                @" offset @skipElements rows
                FETCH NEXT @takeElements rows only";

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                return db.Query<ForumModel>(query, 
                    new { skipElements, @takeElements = countElemetsOnPage })
                    .ToList();
            }
        }

        //Есть вероятность, что имена будут совпадать в случае нескольких юзеров
        public static ForumModel GetDataById(int id)
        {
            string query = @"select * from Forum with(nolock) where id = @id";

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                return db.Query<ForumModel>(query, new { id }).SingleOrDefault();
            }
        }
        
        public static void UpdateDataById(int id, ForumModel updateModel)
        {
            string query = @"update Forum set ThemeName = @ThemeName, ChangeDate = @ChangeDate where id = @id";

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                db.Execute(query, new { updateModel.ThemeName, updateModel.ChangeDate, id });
            }
        }

        public static void InsertData(ForumModel insertModel)
        {
            string query = @"insert into Forum (ThemeName, ChangeDate) values (@ThemeName, @ChangeDate)";

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                db.Execute(query, new { insertModel.ThemeName, insertModel.ChangeDate });
            }
        }

        //В некоторых случаях целесообразно не удалять данные полностью, а, например, выставлять им флаг: @isDeleted (например, для восстановления удаленных профилей)
        public static void DeleteDataById(int id)
        {
            string query = @"delete from Forum where id = @id";

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                db.Execute(query, new { id });
            }
        }
    }
}
