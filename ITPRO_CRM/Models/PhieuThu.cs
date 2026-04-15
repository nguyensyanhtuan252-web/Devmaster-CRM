using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITPRO_CRM.Models
{
    public class PhieuThu
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Mã phiếu")]
        public string? MaPhieu { get; set; } // VD: PT001

        // Liên kết Học viên (Bắt buộc)
        [Required]
        public int HocVienId { get; set; }
        [ForeignKey("HocVienId")]
        public virtual HocVien? HocVien { get; set; }

        // Liên kết Lớp học (Có thể null nếu thu tiền lẻ, nhưng tốt nhất nên có)
        [Display(Name = "Lớp học")]
        public int? LopHocId { get; set; } // Đổi thành nullable (int?) để linh hoạt hơn
        [ForeignKey("LopHocId")]
        public virtual LopHoc? LopHoc { get; set; }

        [Display(Name = "Số tiền thu")]
        [DisplayFormat(DataFormatString = "{0:0,0} đ")]
        [Column(TypeName = "decimal(18, 0)")] // Định dạng SQL để lưu tiền chính xác
        public decimal SoTien { get; set; }

        // 👇 ĐÃ SỬA: Đổi từ 'NgayThu' thành 'NgayThu' để khớp với HomeController
        [Display(Name = "Ngày thu")]
        [DataType(DataType.Date)]
        public DateTime NgayThu { get; set; } = DateTime.Now;

        [Display(Name = "Nội dung")]
        public string? NoiDung { get; set; }

        [Display(Name = "Người thu")]
        public string? NguoiThu { get; set; }

        // 0=Tiền mặt, 1=Chuyển khoản
        [Display(Name = "Hình thức")]
        public int HinhThuc { get; set; } = 1;
    }
}