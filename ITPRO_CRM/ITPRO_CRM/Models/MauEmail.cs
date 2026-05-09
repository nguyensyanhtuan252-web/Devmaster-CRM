using System;
using System.ComponentModel.DataAnnotations;

namespace ITPRO_CRM.Models
{
    public class MauEmail
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên mẫu")]
        [Display(Name = "Tên gợi nhớ")]
        public string TenMau { get; set; } // Ví dụ: Mau_Nhac_Hoc_Phi

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề email")]
        [Display(Name = "Tiêu đề Email")]
        public string TieuDe { get; set; } // Ví dụ: [Devmaster] Thông báo học phí tháng {{Thang}}

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        [Display(Name = "Nội dung (HTML)")]
        public string NoiDung { get; set; } // Chứa code HTML và các biến như {{TenHocVien}}

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Loại mẫu")]
        public string LoaiMau { get; set; } // Học tập, Tài chính, Marketing...
    }
}