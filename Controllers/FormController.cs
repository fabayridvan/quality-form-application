using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using QuailtyForm.ViewModels;
using System.Collections.Generic;
using System.Xml.Linq;

namespace QuailtyForm.Controllers
{
    public class FormController : Controller
    {
        private readonly IConfiguration _configuration;

        public FormController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string projectAndCompany, string formName, string blockAndFloor, string approvalStatus, int qualityControlDefId, int projectBlockDefDId, int categoriesId)
        {
            string userName = HttpContext.Session.GetString("User");

            ViewData["ProjectAndCompany"] = projectAndCompany;
            ViewData["FormName"] = formName;
            ViewData["BlockAndFloor"] = blockAndFloor;
            ViewData["ApprovalStatus"] = approvalStatus;
            ViewData["QualityControlDefId"] = qualityControlDefId;
            ViewData["ProjectBlockDefDId"] = projectBlockDefDId;

            var recipeDescs = GetRecipeDesc(userName, formName, categoriesId);

            if (recipeDescs.Count == 0)
            {
                return View(new FormDetailsViewModel());
            }

            int recipeDefinitionId = recipeDescs[0].RecipeDefinitionId;
            var questions = GetQuestion(userName, formName, categoriesId, recipeDefinitionId);

            var formDetails = new FormDetailsViewModel
            {
                RecipeDescs = recipeDescs,
                Questions = questions
            };

            return View(formDetails);
        }

        private List<TableViewModel> GetAllParametersIds(string userName, int qualityControlDefId, int projectBlockDefDId)
        {
            var table = new List<TableViewModel>();

            string connectionString = _configuration.GetConnectionString("OracleDbConnection");
            //connectionString = 

            using (var connection = new OracleConnection(connectionString))
            {
                string query = @"SELECT  GC.CO_ID, -- FİRMA ID
                                        PM.PROJECT_M_ID, -- PROJE ID
                                        US.US_ID, -- KULLANICI ID
                                        PBD.PROJECT_BLOCK_DEF_ID, -- BLOK ID
                                        PDD.PROJECT_BLOCK_DEF_D_ID,
                                        QCF.QUALITY_CONTROL_DEF_ID
                                  FROM ZZZT_QUALITY_CONTROL_DEF  QCF
                                       LEFT JOIN FIND_CO_PROJECT_M CPM
                                           ON CPM.CO_PROJECT_M_ID = QCF.CO_PROJECT_M_ID
                                       LEFT JOIN FIND_PROJECT_M PM ON PM.PROJECT_M_ID = CPM.PROJECT_M_ID
                                       LEFT JOIN ZZZT_QUALITY_CONTROL_DEF_D CDD
                                           ON CDD.QUALITY_CONTROL_DEF_ID = QCF.QUALITY_CONTROL_DEF_ID
                                       LEFT JOIN ZZZT_PROJECT_BLOCK_DEF PBD
                                           ON PBD.PROJECT_BLOCK_DEF_ID = CDD.PROJECT_BLOCK_DEF_ID
                                       LEFT JOIN ZZZT_PROJECT_BLOCK_DEF_D PDD
                                           ON PDD.PROJECT_BLOCK_DEF_ID = PBD.PROJECT_BLOCK_DEF_ID
                                       LEFT JOIN GNLD_COMPANY GC ON GC.CO_ID = QCF.CO_ID
                                       LEFT JOIN ZZZT_FORM_BASED_AUTOHORIZATION FBA
                                           ON QCF.QUALITY_CONTROL_DEF_ID = FBA.QUALITY_CONTROL_DEF_ID
                                       LEFT JOIN USERS US ON FBA.US_ID = US.US_ID
                                       LEFT JOIN ZZZT_QUALITY_FORMS_ANSWERS QFA
                                           ON QFA.QUALITY_CONTROL_DEF_ID = QCF.QUALITY_CONTROL_DEF_ID
                                 WHERE US.US_USERNAME = :userName AND QCF.QUALITY_CONTROL_DEF_ID = :qualityControlDefId AND PDD.PROJECT_BLOCK_DEF_D_ID = :projectBlockDefDId";


                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("userName", userName));
                    command.Parameters.Add(new OracleParameter("qualityControlDefId", qualityControlDefId));
                    command.Parameters.Add(new OracleParameter("projectBlockDefDId", projectBlockDefDId));

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tableViewModel = new TableViewModel
                            {
                                CoId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                ProjectMId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                UsId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                ProjectBlockDefId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                ProjectBlockDefDId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                QualityControlDefId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                            };
                            table.Add(tableViewModel);
                        }
                    }
                }
            }
            return table;
        }

        private List<RecipeViewModel> GetRecipeDesc(string userName, string formName, int categoriesId)
        {
            var recipeDescs = new List<RecipeViewModel>();

            string connectionString = _configuration.GetConnectionString("OracleDbConnection");
            //connectionString = 

            using (var connection = new OracleConnection(connectionString))
            {
                string query = @"SELECT RD.RECIPE_DEFINITION_ID,
                                    RD.RECIPE_DESC
                          FROM ZZZT_QUALITY_CONTROL_DEF  QCD
                               LEFT JOIN ZZZT_FORM_BASED_AUTOHORIZATION FBA
                                   ON FBA.QUALITY_CONTROL_DEF_ID = QCD.QUALITY_CONTROL_DEF_ID
                               LEFT JOIN USERS US ON US.US_ID = FBA.US_ID
                               LEFT JOIN GNLD_CATEGORIES GT
                                   ON FBA.ZZ_CATEGORIES_ID = GT.CATEGORIES_ID
                               LEFT JOIN ZZZT_AUTOHORIZATION_D AD
                                   ON AD.FORM_BASED_AUTOHORIZATION_ID = FBA.FORM_BASED_AUTOHORIZATION_ID
                               LEFT JOIN ZZZT_RECIPE_DEFINITION RD
                                   ON RD.RECIPE_DEFINITION_ID = AD.ZZ_RECIPE_DEFINITION_ID
                               LEFT JOIN CRMD_SURVEY CS ON CS.SURVEY_ID = QCD.SURVEY_ID
                         WHERE QCD.QUALITY_CONTROL_NAME = :formName
                               AND US.US_USERNAME = :userName
                               AND FBA.ZZ_CATEGORIES_ID = :categoriesId";

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("formName", formName));
                    command.Parameters.Add(new OracleParameter("userName", userName));
                    command.Parameters.Add(new OracleParameter("categoriesId", categoriesId));

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var recipeViewModel = new RecipeViewModel
                            {
                                RecipeDefinitionId = reader.GetInt32(0),
                                RecipeDesc = reader.GetString(1)
                            };
                            recipeDescs.Add(recipeViewModel);
                        }
                    }
                }
            }

            return recipeDescs;
        }



        private List<string> GetQuestion(string userName, string formName, int categoriesId, int recipeDefinitionId)
        {
            var questions = new List<string>();

            string connectionString = _configuration.GetConnectionString("OracleDbConnection");
            //connectionString = 

            using (var connection = new OracleConnection(connectionString))
            {
                string query = @"SELECT CSQ.QUESTION
                          FROM ZZZT_QUALITY_CONTROL_DEF  QCD
                               LEFT JOIN ZZZT_FORM_BASED_AUTOHORIZATION FBA
                                   ON FBA.QUALITY_CONTROL_DEF_ID = QCD.QUALITY_CONTROL_DEF_ID
                               LEFT JOIN USERS US ON US.US_ID = FBA.US_ID
                               LEFT JOIN GNLD_CATEGORIES GT
                                   ON FBA.ZZ_CATEGORIES_ID = GT.CATEGORIES_ID
                               LEFT JOIN ZZZT_AUTOHORIZATION_D AD
                                   ON AD.FORM_BASED_AUTOHORIZATION_ID = FBA.FORM_BASED_AUTOHORIZATION_ID
                               LEFT JOIN ZZZT_RECIPE_DEFINITION RD
                                   ON RD.RECIPE_DEFINITION_ID = AD.ZZ_RECIPE_DEFINITION_ID
                               LEFT JOIN CRMD_SURVEY CS ON CS.SURVEY_ID = QCD.SURVEY_ID
                               LEFT JOIN CRMD_SURVEY_QUESTION CSQ
                                   ON CSQ.SURVEY_ID = CS.SURVEY_ID
                         WHERE QCD.QUALITY_CONTROL_NAME = :formName
                               AND US.US_USERNAME = :userName
                               AND CSQ.ZZ_CATEGORIES_ID = :categoriesId
                               AND CSQ.RECIPE_DEFINITION_ID = :recipeDefinitionId";

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("formName", formName));
                    command.Parameters.Add(new OracleParameter("userName", userName));
                    command.Parameters.Add(new OracleParameter("categoriesId", categoriesId));
                    command.Parameters.Add(new OracleParameter("recipeDefinitionId", recipeDefinitionId));

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            questions.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return questions;
        }


        [HttpPost]
        public IActionResult SubmitApproval(string approval, string comments, int qualityControlDefId, int projectBlockDefDId)
        {
            string userName = HttpContext.Session.GetString("User");
            var parameterIds = GetAllParametersIds(userName, qualityControlDefId, projectBlockDefDId);

            try
            {
                InsertApprovalRecord(parameterIds, approval, comments);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Hata işlemi
                Console.WriteLine($"An error occurred: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
        private void InsertApprovalRecord(List<TableViewModel> parameterIds, string approvalStatus, string comments)
        {
            string connectionString = _configuration.GetConnectionString("OracleDbConnection");

            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();

                foreach (var parameter in parameterIds)
                {
                    string selectQuery = @"SELECT COUNT(*) FROM ZZZT_QUALITY_FORMS_ANSWERS
                                    WHERE SURVEY_ID = :surveyId
                                    AND FLOOR_ID = :floorId
                                    AND US_ID = :usId";

                    using (var selectCommand = new OracleCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.Add(new OracleParameter("surveyId", parameter.QualityControlDefId));
                        selectCommand.Parameters.Add(new OracleParameter("floorId", parameter.ProjectBlockDefDId));
                        selectCommand.Parameters.Add(new OracleParameter("usId", parameter.UsId));

                        int recordCount = Convert.ToInt32(selectCommand.ExecuteScalar());

                        if (recordCount > 0)
                        {
                            string updateQuery = @"UPDATE ZZZT_QUALITY_FORMS_ANSWERS
                                           SET DESCRIPTION = :description, COMMENTS = :comments
                                           WHERE SURVEY_ID = :surveyId
                                           AND FLOOR_ID = :floorId";

                            using (var updateCommand = new OracleCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.Add(new OracleParameter("description", approvalStatus));
                                updateCommand.Parameters.Add(new OracleParameter("comments", comments));
                                updateCommand.Parameters.Add(new OracleParameter("surveyId", parameter.QualityControlDefId));
                                updateCommand.Parameters.Add(new OracleParameter("floorId", parameter.ProjectBlockDefDId));

                                updateCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string insertQuery = @"INSERT INTO ZZZT_QUALITY_FORMS_ANSWERS (QUESTION_ID,
                                    SURVEY_ID,
                                    FLOOR_ID,
                                    DESCRIPTION,
                                    US_ID,
                                    PROJECT_ID,
                                    CO_ID,
                                    QUALITY_CONTROL_DEF_ID,
                                    PROJECT_BLOCK_DEF_ID,
                                    COMMENTS,
                                    CREATE_DATE)
                             VALUES (:questionId,
                                     :surveyId,
                                     :floorId,
                                     :description,
                                     :usId,
                                     :projectMId,
                                     :coId,
                                     :qualityControlDefId,
                                     :projectBlockDefId,
                                     :comments,
                                      SYSDATE)";

                            using (var insertCommand = new OracleCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.Add(new OracleParameter("questionId", parameter.QuestionId));
                                insertCommand.Parameters.Add(new OracleParameter("surveyId", parameter.QualityControlDefId));
                                insertCommand.Parameters.Add(new OracleParameter("floorId", parameter.ProjectBlockDefDId));
                                insertCommand.Parameters.Add(new OracleParameter("description", approvalStatus));
                                insertCommand.Parameters.Add(new OracleParameter("usId", parameter.UsId));
                                insertCommand.Parameters.Add(new OracleParameter("projectMId", parameter.ProjectMId));
                                insertCommand.Parameters.Add(new OracleParameter("coId", parameter.CoId));
                                insertCommand.Parameters.Add(new OracleParameter("qualityControlDefId", parameter.QualityControlDefId));
                                insertCommand.Parameters.Add(new OracleParameter("projectBlockDefId", parameter.ProjectBlockDefId));
                                insertCommand.Parameters.Add(new OracleParameter("comments", comments));

                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
}
