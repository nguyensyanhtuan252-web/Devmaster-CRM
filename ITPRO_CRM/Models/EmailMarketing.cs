using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITPRO_CRM.Models
{
    public class EmailMarketing
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Tiêu đề Email")]
        [Required(ErrorMessage = "Phải nhập tiêu đề")]
        public string TieuDe { get; set; } = string.Empty;

        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Phải nhập nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Display(Name = "Gửi tới")]
        public string? DoiTuongGui { get; set; } // VD: "Tất cả", "Học viên cũ"...

        [Display(Name = "Ngày gửi")]
        public DateTime NgayGui { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        public bool DaGui { get; set; } = false; // True = Đã gửi thành công
    }
}