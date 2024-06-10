using QuailtyForm.Models;

namespace QuailtyForm.ViewModels
{
    public class UserViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class TableViewModel
    {
        public int QualityControlDefId { get; set; } 
        public int ProjectBlockDefDId { get; set; }
        public int ProjectBlockDefId { get; set; }
        public int ProjectMId { get; set; }

        public int QuestionId { get; set; }

        public int ProjectId { get; set; }
        public int CoId { get; set; }
        public int UsId { get; set; }
        //public int ID { get; set; }
        public string ProjectAndCompany { get; set; }
        public string FormName { get; set; }
        public string BlockAndFloor { get; set; }
        public string ApprovalStatus { get; set; }

        public string BlockCode { get; set; }
        public string FloorCode { get; set; }
        public string Description { get; set; }
        public string ProjectCode { get; set; }

        //public int RecipeId { get; set; }
        //public string RecipeDesc { get; set; }
        public string Question { get; set; }

        public int CategoriesId { get; set; }
    }
    public class RecipeViewModel
    {
        public int RecipeDefinitionId { get; set; }
        public string RecipeDesc { get; set; }
    }
    //public class FormIndexViewModel
    //{
    //    public List<TableViewModel> ParameterIds { get; set; }
    //    public List<TableViewModel> FormDetails { get; set; }
    //    public string ApprovalStatus { get; set; } // Onay durumu
    //    public int QualityControlDefId { get; set; } // Gerekli diğer bilgiler
    //    public int ProjectBlockDefDId { get; set; }
    //}

    public class FormDetailsViewModel
    {
        public List<RecipeViewModel> RecipeDescs { get; set; } = new List<RecipeViewModel>();
        public List<string> Questions { get; set; } = new List<string>();
    }

    public class MainPageViewModel
    {
        public List<UserViewModel> Users { get; set; }
        public List<TableViewModel> Tables { get; set; }
        public string FilterName { get; set; }
        public string FilterEmail { get; set; }
        public string FilterProject { get; set; }
        public string FilterFormName { get; set; }
        public string FilterBlockAndFloor { get; set; }
        public string FilterApprovalStatus { get; set; }
    }
    public class FormPageViewModel
    {
        public List<TableViewModel> ParameterIds { get; set; }
        public List<TableViewModel> FormDetails { get; set; }
    }
    public class CombinedViewModel
    {
        public MainPageViewModel MainPage { get; set; }
        public FormPageViewModel FormPage { get; set; }
        public ComplexFormViewModel ComplexForm { get; set; }

        public class ComplexFormViewModel
        {
            public List<Company> Companies { get; set; }
            public List<Project> Projects { get; set; }
            public List<Category1> Categories1 { get; set; }
            public List<Category1> Categories2 { get; set; }
            public List<Category1> Categories3 { get; set; }
            public List<Category1> Categories4 { get; set; }
        }
    }

}
