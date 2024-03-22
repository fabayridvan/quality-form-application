namespace QuailtyForm.Models
{
    public class SurveyModel
    {
        public int SurveyId { get; set; }
        public int QualityControlDefId { get; set; }
        public int FloorId { get; set; }

        public List<QuestionAnswer>? Questions { get; set; }

    }
    public class QuestionAnswer
    {
        public int QuestionId { get; set; }
        public string? Answer { get; set; }
    }
}
