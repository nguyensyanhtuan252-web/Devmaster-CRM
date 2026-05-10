using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITPRO_CRM.Models
{
    public class ThongBao
    {
        [Key]
        public int Id { get; set; }

        public int NhanVienId { get; set; } // ID người nhận thông báo

        [StringLength(255)]
        public string TieuDe { get; set; } // VD: "Có khách hàng mới từ Chiến dịch X"

        public string NoiDung { get; set; } // Nội dung chi tiết

        public string LinkUrl { get; set; } // Link để nhảy đến (VD: "/HocViens/Details/5")

        public bool DaDoc { get; set; } = false; // Trạng thái đọc (Chưa đọc = false)

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("NhanVienId")]
        public virtual NhanVien NhanVien { get; set; }
    }
}