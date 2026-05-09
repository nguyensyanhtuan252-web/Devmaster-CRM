using System.ComponentModel.DataAnnotations;

namespace ITPRO_CRM.Models
{
    public class CauHinh
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Tên trung tâm")]
        public string? TenTrungTam { get; set; } = "ITPRO ACADEMY";

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; } = "Hà Nội";

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }
    }
}
