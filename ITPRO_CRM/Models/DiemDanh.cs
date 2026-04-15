using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITPRO_CRM.Models
{
    public class DiemDanh
    {
        [Key]
        public int Id { get; set; }

        // Liên kết với Lớp học (Biết điểm danh cho lớp nào)
        public int LopHocId { get; set; }
        [ForeignKey("LopHocId")]
        public virtual LopHoc? LopHoc { get; set; }

        // Liên kết với Học viên (Biết ai đi học)
        public int HocVienId { get; set; }
        [ForeignKey("HocVienId")]
        public virtual HocVien? HocVien { get; set; }

        [Display(Name = "Ngày điểm danh")]
        [DataType(DataType.Date)]
        public DateTime NgayDiemDanh { get; set; } = DateTime.Now;

        // Quy ước: 1 = Có mặt, 0 = Vắng, 2 = Muộn/Có phép
        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } = 1;

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}