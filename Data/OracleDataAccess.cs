using QuailtyForm.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using QuailtyForm.ViewModels;

namespace QuailtyForm.Data
{
    public class OracleDataAccess
    {
        private string ConnectionString { get; set; }

        public OracleDataAccess(string connectionString)
        {
            ConnectionString = connectionString;
            //ConnectionString = 
        }

        public void ExecuteQuery(string query, OracleParameter[] parameters)
        {
            using (OracleConnection connection = new OracleConnection(ConnectionString))
            {
                connection.Open();

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    command.ExecuteNonQuery();
                }
            }
        }


        public List<Company> GetCompany()
        {
            List<Company> company = new List<Company>();

            using (OracleConnection con = new OracleConnection(ConnectionString))
            {
                con.Open();
                string query = string.Format(@"SELECT DISTINCT
                                                   QC.QUALITY_CONTROL_DEF_ID,
                                                   QC.QUALITY_CONTROL_NAME,
                                                   QC.SURVEY_ID
                                              FROM ZZZT_QUALITY_CONTROL_DEF  QC
                                                   LEFT JOIN ZZZT_QUALITY_CONTROL_DEF_D QCD
                                                       ON QC.QUALITY_CONTROL_DEF_ID = QCD.QUALITY_CONTROL_DEF_ID");
                //string query = string.Format(@"SELECT QC.QUALITY_CONTROL_DEF_ID,
                //                               QC.QUALITY_CONTROL_NAME,
                //                               QC.SURVEY_ID,
                //                               QCD.PROJECT_BLOCK_DEF_ID
                //                          FROM ZZZT_QUALITY_CONTROL_DEF  QC
                //                               LEFT JOIN ZZZT_QUALITY_CONTROL_DEF_D QCD
                //                                   ON QC.QUALITY_CONTROL_DEF_ID = QCD.QUALITY_CONTROL_DEF_ID");

                //OracleCommand cmd = new OracleCommand("SELECT QC.QUALITY_CONTROL_DEF_ID,QC.QUALITY_CONTROL_NAME,QC.SURVEY_ID,QCD.PROJECT_BLOCK_DEF_ID FROM ZZZT_QUALITY_CONTROL_DEF QC LEFT JOIN ZZZT_QUALITY_CONTROL_DEF_D  QCD ON QC.QUALITY_CONTROL_DEF_ID = QCD.QUALITY_CONTROL_DEF_ID", con);
                OracleCommand cmd = new OracleCommand(query, con);
                cmd.CommandType = CommandType.Text;

                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    company.Add(new Company
                    {
                        Id = Convert.ToInt32(reader["QUALITY_CONTROL_DEF_ID"]),
                        ControlName = reader["QUALITY_CONTROL_NAME"].ToString(),
                        SurveyId = Convert.ToInt32(reader["SURVEY_ID"])
                        //ProjectBlockDefId = Convert.ToInt32(reader["PROJECT_BLOCK_DEF_ID"])
                    });
                }
            }

            return company;
        }

        public List<Project> GetProject(int qualityControlDefId)
        {
            List<Project> project = new List<Project>();

            using (OracleConnection con = new OracleConnection(ConnectionString))
            {

                con.Open();

                string query = string.Format(@"SELECT T.BLOCK_CODE,
                                                     D.FLOOR_CODE,
                                                     T.PROJECT_BLOCK_DEF_ID,
                                                     D.PROJECT_BLOCK_DEF_D_ID
                                                FROM ZZZT_PROJECT_BLOCK_DEF T
                                                     LEFT JOIN ZZZT_PROJECT_BLOCK_DEF_D D
                                                         ON D.PROJECT_BLOCK_DEF_ID = T.PROJECT_BLOCK_DEF_ID
                                                     LEFT JOIN ZZZT_QUALITY_CONTROL_DEF_D QCD
                                                         ON T.PROJECT_BLOCK_DEF_ID = QCD.PROJECT_BLOCK_DEF_ID
                                                     LEFT JOIN ZZZT_QUALITY_CONTROL_DEF QC
                                                         ON QC.QUALITY_CONTROL_DEF_ID = QCD.QUALITY_CONTROL_DEF_ID
                                               WHERE QC.QUALITY_CONTROL_DEF_ID = :qualityControlDefId
                                            ORDER BY BLOCK_CODE", con);

                //string query = string.Format(@"SELECT T.BLOCK_CODE,
                //                            D.FLOOR_CODE,
                //                            T.PROJECT_BLOCK_DEF_ID,
                //                            D.PROJECT_BLOCK_DEF_D_ID
                //                       FROM ZZZT_PROJECT_BLOCK_DEF  T
                //                            LEFT JOIN ZZZT_PROJECT_BLOCK_DEF_D D
                //                                ON D.PROJECT_BLOCK_DEF_ID = T.PROJECT_BLOCK_DEF_ID
                //                      WHERE T.PROJECT_BLOCK_DEF_ID = :projectBlockDefId", con);



                //OracleCommand cmd = new OracleCommand("SELECT T.BLOCK_CODE,D.FLOOR_CODE,T.PROJECT_BLOCK_DEF_ID,D.PROJECT_BLOCK_DEF_D_ID FROM ZZZT_PROJECT_BLOCK_DEF T LEFT JOIN  ZZZT_PROJECT_BLOCK_DEF_D D ON  D.PROJECT_BLOCK_DEF_ID=T.PROJECT_BLOCK_DEF_ID where T.PROJECT_BLOCK_DEF_ID = :projectBlockDefId", con);
                OracleCommand cmd = new OracleCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new OracleParameter("qualityControlDefId", qualityControlDefId));

                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    project.Add(new Project
                    {
                        Id = Convert.ToInt32(reader["PROJECT_BLOCK_DEF_ID"]),
                        BlockDefDId = Convert.ToInt32(reader["PROJECT_BLOCK_DEF_D_ID"]),
                        FloorCode = reader["FLOOR_CODE"].ToString(),
                        BlockCode = reader["BLOCK_CODE"].ToString()
                    });
                }
            }

            return project;
        }


        public List<Category1> GetCategory1()
        {
            List<Category1> category = new List<Category1>();

            using (OracleConnection con = new OracleConnection(ConnectionString))
            {
                con.Open();
                string query = string.Format(@"SELECT CATEGORIES_ID, DESCRIPTION
                                         FROM gnld_categories
                                        WHERE categories_page = 12 AND STEP = 2 AND ZZ_NOT_ADD_SURVEY IS NULL", con);

                //OracleCommand cmd = new OracleCommand("SELECT CATEGORIES_ID, DESCRIPTION FROM gnld_categories where categories_page = 12 and STEP=2 ", con);
                OracleCommand cmd = new OracleCommand(query, con);
                cmd.CommandType = CommandType.Text;

                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    category.Add(new Category1
                    {
                        Id = Convert.ToInt32(reader["CATEGORIES_ID"]),
                        Description = reader["DESCRIPTION"].ToString()
                    });
                }
            }

            return category;
        }

        public List<Category1> GetCategory2(int parentId)
        {
            List<Category1> category = new List<Category1>();

            using (OracleConnection con = new OracleConnection(ConnectionString))
            {
                con.Open();
                string query = string.Format(@"SELECT CATEGORIES_ID, DESCRIPTION
                                          FROM gnld_categories
                                         WHERE     categories_page = 12
                                               AND STEP = 3
                                               AND PARENT_CAT_ID IS NOT NULL
                                               AND PARENT_CAT_ID = :parentId
                                               AND ZZ_NOT_ADD_SURVEY IS NULL", con);

                //string query = "SELECT CATEGORIES_ID, DESCRIPTION FROM gnld_categories WHERE categories_page = 12 AND STEP=3 AND (PARENT_CAT_ID = :parentId or nvl(parent_cat_id,0) = 0)";
                OracleCommand cmd = new OracleCommand(query, con);
                cmd.CommandType = CommandType.Text;

                // parentId parametresini sorgunuza ekleyin
                cmd.Parameters.Add(new OracleParameter("parentId", parentId));

                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    category.Add(new Category1
                    {
                        Id = Convert.ToInt32(reader["CATEGORIES_ID"]),
                        Description = reader["DESCRIPTION"].ToString()
                    });
                }
            }
            return category;
        }

        public List<Category1> GetCategory3(int parentId)
        {
            List<Category1> category = new List<Category1>();

            using (OracleConnection con = new OracleConnection(ConnectionString))
            {
                con.Open();
                string query = string.Format(@"SELECT CATEGORIES_ID, DESCRIPTION
                                          FROM gnld_categories
                                         WHERE     categories_page = 12
                                               AND STEP = 4
                                               AND PARENT_CAT_ID IS NOT NULL
                                               AND PARENT_CAT_ID <> 0
                                               AND PARENT_CAT_ID = :parentId
                                               AND ZZ_NOT_ADD_SURVEY IS NULL", con);
                //string query = "SELECT CATEGORIES_ID, DESCRIPTION FROM gnld_categories WHERE categories_page = 12 AND STEP=4 AND (PARENT_CAT_ID = :parentId or nvl(parent_cat_id,0) = 0)";
                OracleCommand cmd = new OracleCommand(query, con);
                cmd.CommandType = CommandType.Text;

                // parentId parametresini sorgunuza ekleyin
                cmd.Parameters.Add(new OracleParameter("parentId", parentId));

                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    category.Add(new Category1
                    {
                        Id = Convert.ToInt32(reader["CATEGORIES_ID"]),
                        Description = reader["DESCRIPTION"].ToString()
                    });
                }
            }
            return category;
        }

        public List<Category1> GetCategory4(int parentId)
        {
            List<Category1> category = new List<Category1>();

            using (OracleConnection con = new OracleConnection(ConnectionString))
            {
                con.Open();
                //string query = "SELECT CATEGORIES_ID, DESCRIPTION FROM gnld_categories WHERE categories_page = 12 AND STEP=5 AND (PARENT_CAT_ID = :parentId or nvl(parent_cat_id,0) = 0)";
                //string query = "SELECT CATEGORIES_ID, DESCRIPTION FROM gnld_categories WHERE categories_page = 12 AND STEP=5 AND ISPASSIVE = 0";
                string query = string.Format(@"SELECT CATEGORIES_ID, DESCRIPTION
                                               FROM gnld_categories
                                               WHERE     categories_page = 12
                                               AND STEP = 5
                                               AND PARENT_CAT_ID IS NOT NULL
                                               AND PARENT_CAT_ID <> 0
                                               AND PARENT_CAT_ID = :parentId
                                               AND ZZ_NOT_ADD_SURVEY IS NULL", con);
                OracleCommand cmd = new OracleCommand(query, con);
                cmd.CommandType = CommandType.Text;

                // parentId parametresini sorgunuza ekleyin
                cmd.Parameters.Add(new OracleParameter("parentId", parentId));

                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    category.Add(new Category1
                    {
                        Id = Convert.ToInt32(reader["CATEGORIES_ID"]),
                        Description = reader["DESCRIPTION"].ToString()
                    });
                }
            }
            return category;
        }
        public List<Question> GetQuestion(int category1Id, int category2Id, int category3Id, int category4Id)
        {
            List<Question> questions = new List<Question>();

            using (OracleConnection con = new OracleConnection(ConnectionString))
            {
                con.Open();
                string query = "SELECT CSQ.SURVEY_QUESTION_ID,QUESTION,ZZ_CATEGORIES_ID,ZZ_CATEGORIES_ID2,ZZ_CATEGORIES_ID3,ZZ_CATEGORIES_ID4 FROM CRMD_SURVEY CS LEFT JOIN CRMD_SURVEY_QUESTION    CSQ  ON  CS.SURVEY_ID=CSQ.SURVEY_ID LEFT JOIN CRMD_SURVEY_QUESTION_D  CSQD    ON   CSQD.SURVEY_QUESTION_ID=CSQ.SURVEY_QUESTION_ID where ZZ_CATEGORIES_ID = :category1Id AND ZZ_CATEGORIES_ID2 = :category2Id AND ZZ_CATEGORIES_ID3 = :category3Id AND ZZ_CATEGORIES_ID4 = :category4Id";
                OracleCommand cmd = new OracleCommand(query, con);
                cmd.Parameters.Add(new OracleParameter("category1Id", category1Id));
                cmd.Parameters.Add(new OracleParameter("category2Id", category2Id));
                cmd.Parameters.Add(new OracleParameter("category3Id", category3Id));
                cmd.Parameters.Add(new OracleParameter("category4Id", category4Id));

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        questions.Add(new Question
                        {
                            Id = Convert.ToInt32(reader["SURVEY_QUESTION_ID"]),
                            Question_Text = reader["QUESTION"].ToString(),
                            Categori1Id = Convert.ToInt32(reader["ZZ_CATEGORIES_ID"]),
                            Categori2Id = Convert.ToInt32(reader["ZZ_CATEGORIES_ID2"]),
                            Categori3Id = Convert.ToInt32(reader["ZZ_CATEGORIES_ID3"]),
                            Categori4Id = Convert.ToInt32(reader["ZZ_CATEGORIES_ID4"])
                        });
                    }
                }
            }
            return questions;
        }
    }
}
