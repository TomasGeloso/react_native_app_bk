namespace react_native_app_bk.Models.Sample.Dtos
{
    public class SampleDto
    {
        public string Sample_Number { get; set; } = null!;
        public int Sample_Type_Id { get; set; }
        public int Material_Id { get; set; }
        public string Dimentions { get; set; } = null!;
        public int Test_Specimen_Id { get; set; }
        public string? Observations { get; set; }
    }
}
