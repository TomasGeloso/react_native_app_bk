using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace react_native_app_bk.Models.Sample
{

    public class Sample
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Sample_Number { get; set; } = null!;

        public int? Sample_Type_Id { get; set; }
        [ForeignKey("Sample_Type_Id")]
        public SampleType? Sample_Type { get; set; }

        public int? Material_Id { get; set; }
        [ForeignKey("Material_Id")]
        public Material? Material { get; set; }

        [MaxLength(100)]
        public string? Dimentions { get; set; }

        public int? Test_Specimen_Type_Id { get; set; }
        [ForeignKey("Test_Specimen_Type_Id")]
        public TestSpecimenType? Test_Specimen_Type { get; set; }

        public string? Observations { get; set; }

        [Required]
        public DateTime Date_Received { get; set; }
    }
    public class SampleType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
