using System;

namespace ITPRO_CRM.Models
{
    public class DotThanhToan
    {
        public int Id { get; set; }
        public int HocVienId { get; set; }
        public virtual HocVien HocVien { get; set; }

        public string TenDot { get; set; } // VD: "Đợt 1 (Đóng 60%)"
        public decimal SoTienPhaiThu { get; set; }
        public decimal SoTienDaThu { get; set; }
        public DateTime HanThanhToan { get; set; }

        // Trạng thái: 0: Chưa đóng, 1: Đóng thiếu, 2: Đã hoàn thành, 3: Quá hạn
        public int TrangThai { get; set; }
    }
}