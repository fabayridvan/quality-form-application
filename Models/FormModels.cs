namespace QuailtyForm.Models
{
    public class Company
    {
        public int Id { get; set; }
        //public string? Adi { get; set; }
        //public string? CoCode { get; set; }
        public string? ControlName { get; set; }
        public int SurveyId { get; set; }
        public int ProjectBlockDefId { get; set; }
    }
    public class Category1
    {
        public int Id { get; set; }
        public string? Description { get; set; }
    }
    public class Project
    {
        public int Id { get; set; } // project_block_def_id
        public int BlockDefDId { get; set; } // project_block_def_d_id
        public string? FloorCode { get; set; }
        public string? BlockCode { get; set; }
        public string DisplayText => $"{BlockCode} - {FloorCode}";

    }
    public class Question
    {
        public int Id { get; set; }
        public string? Question_Text { get; set; }
        public int Categori1Id { get; set; }
        public int Categori2Id { get; set; }
        public int Categori3Id { get; set; }
        public int Categori4Id { get; set; }
    }
}
