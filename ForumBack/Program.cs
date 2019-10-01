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

            //ForumLogic.UpdateDataById(updateId, updateModel);
            //ForumLogic.InsertData(insertModel);
            //ForumLogic.InsertData(insertModel);
            //ForumLogic.DeleteDataById(deleteId);

            var newAllCount = ForumLogic.GetCountAllRecords();

            int currentPage2 = 3;
            int countElemetsOnPage2 = 4;

            var testDataAsc2 = ForumLogic.GetDataByIds(currentPage2, countElemetsOnPage2, newAllCount, sortTypeAsc);
            var testDataDesc2 = ForumLogic.GetDataByIds(currentPage2, countElemetsOnPage2, newAllCount, sortTypeDesc);

            var slqTestAsc1 = ForumLogic.GetDataForCurrentPageSql(currentPage2, countElemetsOnPage2, newAllCount, sortTypeAsc, sortTypeDesc);
            var slqTestDesc1 = ForumLogic.GetDataForCurrentPageSql(currentPage2, countElemetsOnPage2, newAllCount, sortTypeDesc, sortTypeAsc);
            
            var testDataAsc3 = ForumLogic.GetDataForCurrentPageLinq(currentPage2, countElemetsOnPage2, newAllCount, sortTypeAsc);
            var testDataDesc3 = ForumLogic.GetDataForCurrentPageLinq(currentPage2, countElemetsOnPage2, newAllCount, sortTypeDesc);
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
                order by ChangeDate " + sortType + @", id " + sortType +
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

        public static List<ForumModel> GetDataByIds(int currentPage, int countElemetsOnPage, int allCount, string sortType)
        {
            int skipElements = (currentPage - 1) * countElemetsOnPage;

            if (allCount < (skipElements + 1))
            {
                return new List<ForumModel>();
            }

            int[] ids = GetIds(skipElements, countElemetsOnPage, sortType);
            string query =
                @"select id, ThemeName, ChangeDate from Forum 
                with(nolock) where id in @ids";

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                return db.Query<ForumModel>(query, new { ids }).ToList();
            }
        }

        private static int[] GetIds(int skipElements, int takeElements, string sortType)
        {
            string query =
                @"select id from Forum with(nolock)
                order by ChangeDate " + sortType + @", id " + sortType;

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                return db.Query<int>(query).Skip(skipElements).Take(takeElements).ToArray();
            }
        }

        #region SQL and LINQ without 'offset' and ('skip()' and 'take()')

        public static List<ForumModel> GetDataForCurrentPageSql(int currentPage, int countElemetsOnPage, int allCount, string sortType1, string sortType2)
        {
            int skipElements = (currentPage - 1) * countElemetsOnPage;

            if (allCount < (skipElements + 1))
            {
                return new List<ForumModel>();
            }

            skipElements += countElemetsOnPage;

            int range = allCount - skipElements;
            int takeElements = range < 0 ? (-1) * range : countElemetsOnPage;

            string query =
                @"select * from (
                    select top(@takeElements) * from (
                        select top(@skipElements) * from Forum with(nolock)
                        order by ChangeDate " + sortType1 + @", id " + sortType1 +
                @"  ) Forum
                    order by ChangeDate " + sortType2 + @", id " + sortType2 + @") as result
                order by ChangeDate " + sortType2 + @", id " + sortType2;

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                return db.Query<ForumModel>(query, new { skipElements, takeElements }).ToList();
            }
        }

        public static List<ForumModel> GetDataForCurrentPageLinq(int currentPage, int countElemetsOnPage, int allCount, string sortType)
        {
            int skipElements = (currentPage - 1) * countElemetsOnPage;

            if (allCount < (skipElements + 1))
            {
                return new List<ForumModel>();
            }

            int[] ids = GetIdsWithoutSkipTake(skipElements, countElemetsOnPage, sortType);
            string query =
                @"select id, ThemeName, ChangeDate from Forum 
                with(nolock) where id in @ids";

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                var items = db.Query<ForumModel>(query, new { ids });
                var t1 = items.Where((item, index) => index > skipElements && index < (skipElements + countElemetsOnPage)).ToList();

                return db.Query<ForumModel>(query, new { ids }).ToList();
            }
        }

        private static int[] GetIdsWithoutSkipTake(int skipElements, int takeElements, string sortType)
        {
            string query =
                @"select id from Forum with(nolock)
                order by ChangeDate " + sortType + @", id " + sortType;

            using (IDbConnection db = new SqlConnection(conn))
            {
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }

                return db.Query<int>(query).Where((item, index) => index > (skipElements - 1) && index < (skipElements + takeElements)).ToArray();
            }
        }

        #endregion
    }
}
