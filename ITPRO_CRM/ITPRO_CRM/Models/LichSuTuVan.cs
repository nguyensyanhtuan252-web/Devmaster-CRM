using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITPRO_CRM.Models
{
    [Table("LichSuTuVan")]
    public class LichSuTuVan
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Hình thức")]
        public string HinhThuc { get; set; } // VD: Gọi điện, Gặp mặt, Zalo...

        [Display(Name = "Nội dung trao đổi")]
        [Required(ErrorMessage = "Phải nhập nội dung")]
        public string NoiDung { get; set; }

        [Display(Name = "Kết quả/Trạng thái")]
        public string KetQua { get; set; } // VD: Thuê bao, Hẹn gọi lại, Đã chốt...

        public DateTime NgayTuVan { get; set; } = DateTime.Now;

        // Liên kết với Học viên
        public int HocVienId { get; set; }
        [ForeignKey("HocVienId")]
        public virtual HocVien? HocVien { get; set; }

        // Lưu ai là người tư vấn (Optional)
        public string? NguoiTuVan { get; set; }

    }
}