using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Sales
{
    [Table("material_categories")]
    public class MaterialCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [Column("category_name")]
        [StringLength(50)]
        public string CategoryName { get; set; } = string.Empty; // Tên loại NgL (e.g. Cà phê, Sữa)

        [Column("description")]
        [StringLength(200)]
        public string? Description { get; set; } // Mô tả ngắn
    }
}
