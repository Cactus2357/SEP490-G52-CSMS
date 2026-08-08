using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_G52_CSMS.Models.Sales
{
    /// <summary>
    /// BẢNG NGUYÊN LIỆU TRONG KHO TỔNG (UC56/UC57)
    /// </summary>
    [Table("materials")]
    public class Material
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("material_id")]
        public int MaterialId { get; set; }

        [Required]
        [Column("material_code")]
        [StringLength(20)]
        public string MaterialCode { get; set; } = string.Empty;

        [Required]
        [Column("material_name")]
        [StringLength(100)]
        public string MaterialName { get; set; } = string.Empty;

        [Required]
        [Column("material_kind")]
        [StringLength(30)]
        public string MaterialKind { get; set; } = string.Empty; // "Dạng đặc" (Solid) hoặc "Dạng lỏng" (Liquid)

        [Required]
        [Column("category")]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Loại nguyên liệu (VD: Thô, Thành phẩm)

        [Required]
        [Column("supplier")]
        [StringLength(100)]
        public string Supplier { get; set; } = string.Empty; // Nhà cung cấp

        [Required]
        [Column("unit_price")]
        public decimal UnitPrice { get; set; } // Giá hàng tính theo đơn vị lưu kho

        [Column("origin")]
        [StringLength(100)]
        public string? Origin { get; set; } // Nơi trồng (Tùy chọn)

        [Required]
        [Column("storage_unit")]
        [StringLength(20)]
        public string StorageUnit { get; set; } = string.Empty; // Đơn vị lưu kho (kg, lít)
    }
}
