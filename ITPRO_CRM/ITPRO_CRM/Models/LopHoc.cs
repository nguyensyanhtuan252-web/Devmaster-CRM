using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITPRO_CRM.Models
{
    public class LopHoc
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên lớp không được để trống")]
        [Display(Name = "Tên lớp học")]
        [StringLength(100)]
        public string TenLop { get; set; } = string.Empty; // VD: JAVA-WEB-K15

        // ✅ MỚI: Liên kết với Khóa học (chương trình)
        [Display(Name = "Khóa học / Chương trình")]
        public int? KhoaHocId { get; set; }
        [ForeignKey("KhoaHocId")]
        public virtual KhoaHoc? KhoaHoc { get; set; }

        // ✅ MỚI: Liên kết với Giảng viên phụ trách
        [Display(Name = "Giảng viên phụ trách")]
        public int? GiangVienId { get; set; }
        [ForeignKey("GiangVienId")]
        public virtual GiangVien? GiangVien { get; set; }

        [Display(Name = "Ngày khai giảng")]
        [DataType(DataType.Date)]
        public DateTime NgayKhaiGiang { get; set; }

        [Display(Name = "Ngày kết thúc (Dự kiến)")]
        [DataType(DataType.Date)]
        public DateTime? NgayKetThuc { get; set; }

        [Display(Name = "Lịch học")]
        [StringLength(200)]
        public string? LichHoc { get; set; } // VD: T2-T4-T6 (19:30 - 21:30)

        // ✅ MỚI: Giờ bắt đầu / kết thúc buổi học
        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan? GioBatDau { get; set; }

        [Display(Name = "Giờ kết thúc")]
        public TimeSpan? GioKetThuc { get; set; }

        [Display(Name = "Học phí")]
        [DisplayFormat(DataFormatString = "{0:0,0}")]
        [Range(0, double.MaxValue, ErrorMessage = "Học phí không hợp lệ")]
        public decimal HocPhi { get; set; }

        [Display(Name = "Sĩ số tối đa")]
        [Range(1, 500)]
        public int SiSoToiDa { get; set; } = 20;

        // ✅ MỚI: Phòng học / Địa điểm
        [Display(Name = "Phòng học")]
        [StringLength(100)]
        public string? PhongHoc { get; set; }

        // ✅ MỚI: Mô tả ngắn về lớp
        [Display(Name = "Mô tả lớp học")]
        public string? MoTa { get; set; }

        // Trường tính toán (Không lưu trong DB, tự đếm số học viên)
        [NotMapped]
        public int SiSoHienTai => HocViens?.Count ?? 0;

        [NotMapped]
        public int SoChoConLai => SiSoToiDa - SiSoHienTai;

        [NotMapped]
        public bool DayLop => SiSoHienTai >= SiSoToiDa;

        // Trạng thái: 0=Sắp mở, 1=Đang học, 2=Kết thúc
        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } = 0;

        // ✅ MỚI: Ngày tạo bản ghi
        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<HocVien>? HocViens { get; set; }
        public virtual ICollection<DiemDanh>? DiemDanhs { get; set; }

        // --- Computed helpers ---
        [NotMapped]
        public string TrangThaiText => TrangThai switch
        {
            0 => "Sắp mở",
            1 => "Đang học",
            2 => "Đã kết thúc",
            _ => "Không xác định"
        };

        [NotMapped]
        public string TrangThaiBadgeClass => TrangThai switch
        {
            0 => "badge-warning",
            1 => "badge-success",
            2 => "badge-secondary",
            _ => "badge-secondary"
        };

        [NotMapped]
        public int PhanTramSiSo
        {
            get
            {
                if (SiSoToiDa <= 0) return 0;
                return Math.Min((SiSoHienTai * 100) / SiSoToiDa, 100);
            }
        }

        [NotMapped]
        public int SoNgayConLai
        {
            get
            {
                if (NgayKetThuc == null) return -1;
                return (int)(NgayKetThuc.Value - DateTime.Today).TotalDays;
            }
        }

        public bool CoLichHocHomNay
        {
            get
            {
                if (string.IsNullOrEmpty(LichHoc)) return false;
                string lichHocUpper = LichHoc.ToUpper();
                var (thuFull, thuShort) = DateTime.Now.DayOfWeek switch
                {
                    DayOfWeek.Monday => ("THỨ 2", "T2"),
                    DayOfWeek.Tuesday => ("THỨ 3", "T3"),
                    DayOfWeek.Wednesday => ("THỨ 4", "T4"),
                    DayOfWeek.Thursday => ("THỨ 5", "T5"),
                    DayOfWeek.Friday => ("THỨ 6", "T6"),
                    DayOfWeek.Saturday => ("THỨ 7", "T7"),
                    DayOfWeek.Sunday => ("CHỦ NHẬT", "CN"),
                    _ => ("", "")
                };
                return lichHocUpper.Contains(thuFull) || lichHocUpper.Contains(thuShort);
            }
        }
    }
}
