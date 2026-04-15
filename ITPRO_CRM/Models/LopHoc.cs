using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
namespace ITPRO_CRM.Models
{
    public class LopHoc
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên lớp không được để trống")]
        [Display(Name = "Tên lớp học")]
        public string TenLop { get; set; } // VD: JAVA-WEB-K15

        [Display(Name = "Ngày khai giảng")]
        [DataType(DataType.Date)]
        public DateTime NgayKhaiGiang { get; set; }

        [Display(Name = "Ngày kết thúc (Dự kiến)")]
        [DataType(DataType.Date)]
        public DateTime? NgayKetThuc { get; set; }

        [Display(Name = "Lịch học")]
        public string LichHoc { get; set; } // VD: T2-T4-T6 (19:30 - 21:30)

        [Display(Name = "Học phí")]
        [DisplayFormat(DataFormatString = "{0:0,0}")]
        public decimal HocPhi { get; set; }

        [Display(Name = "Sĩ số tối đa")]
        public int SiSoToiDa { get; set; } = 20;

        // Trường tính toán (Không lưu trong DB, tự đếm số học viên)
        [NotMapped]
        public int SiSoHienTai => HocViens?.Count ?? 0;

        // Trạng thái: 0=Sắp mở, 1=Đang học, 2=Kết thúc
        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } = 0;

        // Liên kết: 1 Lớp có nhiều Học viên
        public virtual ICollection<HocVien>? HocViens { get; set; }
        // Thêm dòng này để Lớp học biết nó có danh sách điểm danh
        public virtual ICollection<DiemDanh>? DiemDanhs { get; set; }

        public bool CoLichHocHomNay
        {
            get
            {
                if (string.IsNullOrEmpty(LichHoc)) return false;

                // Ép tất cả về chữ HOA để dễ so sánh (tránh lỗi người dùng gõ "t2" thay vì "T2")
                string lichHocUpper = LichHoc.ToUpper();

                // Khai báo 2 định dạng: Đầy đủ (Thứ 2) và Viết tắt (T2)
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

                // Báo true nếu chuỗi có chứa chữ "THỨ 2" HOẶC chứa chữ "T2"
                return lichHocUpper.Contains(thuFull) || lichHocUpper.Contains(thuShort);
            }
        }
    }

}