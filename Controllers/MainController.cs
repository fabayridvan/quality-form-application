using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol.Plugins;
using Oracle.ManagedDataAccess.Client;
using QuailtyForm.Data;
using QuailtyForm.ViewModels;
using System.Collections;
using System.Collections.Generic;

namespace QuailtyForm.Controllers
{
    public class MainController : Controller
    {
        private readonly IConfiguration _configuration;

        public MainController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string projectAndCompany, string formName, string blockAndFloor, string approvalStatus)
        {
            string username = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Login");
            }

            if (string.IsNullOrEmpty(formName))
            {
                Console.WriteLine("Form Name is null or empty.");
            }

            ViewData["FilterProject"] = projectAndCompany;
            ViewData["FilterFormName"] = formName;
            ViewData["FilterBlockAndFloor"] = blockAndFloor;
            ViewData["FilterApprovalStatus"] = approvalStatus;

            ViewData["ProjectAndCompany"] = GetProjectAndCompany(username) ?? new List<string>();
            ViewData["FormNameData"] = GetFormname(projectAndCompany) ?? new List<string>();
            ViewData["BlockAndFloorData"] = GetBlockandFloor(formName) ?? new List<string>();
            ViewData["ApprovalStatus"] = GetApprovalStatus() ?? new List<string>();

            var mainPageViewModel = new MainPageViewModel
            {
                Tables = GetTableList(username, projectAndCompany, formName, blockAndFloor, approvalStatus),
                FilterProject = projectAndCompany,
                FilterFormName = formName,
                FilterBlockAndFloor = blockAndFloor,
                FilterApprovalStatus = approvalStatus
            };

            var combinedViewModel = new CombinedViewModel
            {
                MainPage = mainPageViewModel
            };

            return View("Main", combinedViewModel);
        }

        private List<TableViewModel> GetTableList(string username, string projectAndCompanyFilter, string formNameFilter, string blockAndFloorFilter, string approvalStatusFilter)
        {
            var table = new List<TableViewModel>();

            string connectionString = _configuration.GetConnectionString("OracleDbConnection");
            //connectionString = 

            using (var connection = new OracleConnection(connectionString))
            {
                string query = @"SELECT (GC.CO_CODE || '-' || PM.PROJECT_CODE),
                                       QCF.QUALITY_CONTROL_NAME,
                                       (PBD.BLOCK_CODE || '-' || PDD.FLOOR_CODE),
                                       QFA.DESCRIPTION,
                                       QCF.QUALITY_CONTROL_DEF_ID,
                                       PDD.PROJECT_BLOCK_DEF_D_ID,
                                       FBA.ZZ_CATEGORIES_ID
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
                                           ON     QFA.QUALITY_CONTROL_DEF_ID = QCF.QUALITY_CONTROL_DEF_ID and QFA.US_ID = US.US_ID
                                              AND PDD.PROJECT_BLOCK_DEF_D_ID = QFA.FLOOR_ID
                               WHERE US.US_USERNAME = :username";

                if (!string.IsNullOrEmpty(projectAndCompanyFilter))
                {
                    query += " AND (GC.CO_CODE || '-' || PM.PROJECT_CODE) LIKE :projectAndCompany";
                }
                if (!string.IsNullOrEmpty(formNameFilter))
                {
                    query += " AND QCF.QUALITY_CONTROL_NAME LIKE :formName";
                }
                if (!string.IsNullOrEmpty(blockAndFloorFilter))
                {
                    query += " AND (PBD.BLOCK_CODE || '-' || PDD.FLOOR_CODE) LIKE :blockAndFloor";
                }
                if (!string.IsNullOrEmpty(approvalStatusFilter))
                {
                    if(approvalStatusFilter == "Onaylanmış")
                    {
                        query += " AND QFA.DESCRIPTION = 'accept'";
                    }
                    else if(approvalStatusFilter == "Onaylanmamış")
                    {
                        query += " AND QFA.DESCRIPTION = 'reject'";
                    }
                }

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("username", username));
                    if (!string.IsNullOrEmpty(projectAndCompanyFilter))
                    {
                        command.Parameters.Add(new OracleParameter("projectAndCompany", "%" + projectAndCompanyFilter + "%"));
                    }
                    if (!string.IsNullOrEmpty(formNameFilter))
                    {
                        command.Parameters.Add(new OracleParameter("formName", "%" + formNameFilter + "%"));
                    }
                    if (!string.IsNullOrEmpty(blockAndFloorFilter))
                    {
                        command.Parameters.Add(new OracleParameter("blockAndFloor", "%" + blockAndFloorFilter + "%"));
                    }
                    
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tables = new TableViewModel
                            {
                                ProjectAndCompany = reader.IsDBNull(0) ? null : reader.GetString(0),
                                FormName = reader.IsDBNull(1) ? null : reader.GetString(1),
                                BlockAndFloor = reader.IsDBNull(2) ? null : reader.GetString(2),
                                ApprovalStatus = reader.IsDBNull(3) ? null : reader.GetString(3),
                                QualityControlDefId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                ProjectBlockDefDId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                CategoriesId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
                            };
                            table.Add(tables);
                        }
                    }
                }
            }

            return table;
        }
        private List<string> GetProjectAndCompany(string username) {

            var projectAndCompany = new List<string>();

            string connectionString = _configuration.GetConnectionString("OracleDbConnection");
            //connectionString = 

            using (var connection = new OracleConnection(connectionString))
            {
                string query = @"SELECT GC.CO_CODE,FPM.PROJECT_CODE
                                      FROM ZZZT_QUALITY_CONTROL_DEF  QCD
                                           LEFT JOIN ZZZT_FORM_BASED_AUTOHORIZATION FBA
                                               ON FBA.QUALITY_CONTROL_DEF_ID = QCD.QUALITY_CONTROL_DEF_ID
                                           LEFT JOIN USERS US ON US.US_ID = FBA.US_ID
                                           LEFT JOIN GNLD_COMPANY GC ON GC.CO_ID = QCD.CO_ID
                                           INNER JOIN FIND_CO_PROJECT_M FCP
                                               ON QCD.CO_PROJECT_M_ID = FCP.CO_PROJECT_M_ID
                                           LEFT JOIN FIND_PROJECT_M FPM ON FCP.PROJECT_M_ID = FPM.PROJECT_M_ID
                                     WHERE US.US_USERNAME = :username";
                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("username", username));
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                string combinedResult = reader.GetString(0) + "-" + reader.GetString(1);
                                projectAndCompany.Add(combinedResult);
                            }
                        }
                    }
                       
                }
            }
                return projectAndCompany;
        }
        [HttpGet]
        public JsonResult GetFormName(string projectAndCompany)
        {
            var formName = GetFormname(projectAndCompany);
            return Json(formName);
        }
        private List<string> GetFormname(string projectAndCompany)
        {
            var formName = new List<string>();

            string connectionString = _configuration.GetConnectionString("OracleDbConnection");
            //connectionString = 

            using (var connection = new OracleConnection(connectionString))
            {
                string query = @"SELECT DISTINCT (QCF.QUALITY_CONTROL_NAME)
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
                                     WHERE (GC.CO_CODE || '-' || PM.PROJECT_CODE) = :projectAndCompany";

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("projectAndCompany", projectAndCompany));
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                formName.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            return formName;
        }
        [HttpGet]
        public JsonResult GetBlockAndFloor(string formName)
        {
            var blockAndFloor = GetBlockandFloor(formName);
            return Json(blockAndFloor);
        }
        private List<string> GetBlockandFloor(string formName)
        {
            var blockAndFloor = new List<string>();

            string connectionString = _configuration.GetConnectionString("OracleDbConnection");
            //connectionString = 

            using (var connection = new OracleConnection(connectionString))
            {
                string query = @"SELECT QPB.BLOCK_CODE,QPBB.FLOOR_CODE
                                      FROM ZZZT_QUALITY_CONTROL_DEF  QCD
                                           LEFT JOIN ZZZT_QUALITY_CONTROL_DEF_D QCDD
                                               ON QCDD.QUALITY_CONTROL_DEF_ID = QCD.QUALITY_CONTROL_DEF_ID
                                           LEFT JOIN ZZZT_PROJECT_BLOCK_DEF QPB
                                               ON QPB.PROJECT_BLOCK_DEF_ID = QCDD.PROJECT_BLOCK_DEF_ID
                                           LEFT JOIN ZZZT_PROJECT_BLOCK_DEF_D QPBB
                                               ON QPBB.PROJECT_BLOCK_DEF_ID = QPB.PROJECT_BLOCK_DEF_ID
                                     WHERE QCD.QUALITY_CONTROL_NAME = :formName";
                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("formName", formName));
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                //string combinedResult = reader.GetString(0) + "-" + reader.GetString(1);
                                string combinedResult = (reader.IsDBNull(0) ? null : reader.GetString(0)) + "-" + (reader.IsDBNull(1) ? null : reader.GetString(1));

                                blockAndFloor.Add(combinedResult);
                            }
                        }
                    }
                }
            }
            return blockAndFloor;
        }
        private List<string> GetApprovalStatus()
        {
            var approvalStatus = new List<string>();

            string connectionString = _configuration.GetConnectionString("OracleDbConnection");
            //connectionString = 

            using (var connection = new OracleConnection(connectionString))
            {
                string query = @"SELECT DISTINCT DESCRIPTION FROM ZZZT_QUALITY_FORMS_ANSWERS ORDER BY DESCRIPTION";
                using (var command = new OracleCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var status = reader.GetString(0);
                            if (status == "accept")
                            {
                                approvalStatus.Add("Onaylanmış");
                            }
                            else if (status == "reject")
                            {
                                approvalStatus.Add("Onaylanmamış");
                            }
                        }
                    }
                }
            }
            return approvalStatus;
        }
    }
}
