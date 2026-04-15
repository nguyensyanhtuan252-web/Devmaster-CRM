using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITPRO_CRM.Models
{
    [Table("KhoaHoc")]
    public class KhoaHoc
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên khóa học không được để trống")]
        [Display(Name = "Tên Khóa Học")]
        public string TenKhoaHoc { get; set; }

        [Display(Name = "Học Phí")]
        public decimal? HocPhi { get; set; }

        [Display(Name = "Số Buổi")]
        public int? SoBuoi { get; set; }

        [Display(Name = "Mô Tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Ảnh Minh Họa")]
        public string? HinhAnh { get; set; }

        [Display(Name = "Trạng Thái")]
        public bool TrangThai { get; set; } = true; // Mặc định là Đang tuyển sinh
    }
}