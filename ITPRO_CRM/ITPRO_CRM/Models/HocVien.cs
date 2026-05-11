using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ITPRO_CRM.Models
{
    public class HocVien
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ tên học viên")]
        public string HoTen { get; set; }

        [Display(Name = "Mã học viên")]
        public string? MaHocVien { get; set; }

        [Display(Name = "Trạng thái CRM")]
        public int TrangThai { get; set; } = 0;

        [Display(Name = "Link Facebook")]
        public string? FacebookLink { get; set; }

        [Display(Name = "Link Zalo")]
        public string? ZaloLink { get; set; }

        [Display(Name = "Trường học / Đại học")]
        public string? TruongHoc { get; set; }

        [Display(Name = "Ngành học / Chuyên môn")]
        public string? NganhHoc { get; set; }

        [Display(Name = "Nơi làm việc hiện tại")]
        public string? CongViecHienTai { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Display(Name = "Số điện thoại HV")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Họ tên Phụ huynh")]
        public string? TenPhuHuynh { get; set; }

        [Display(Name = "SĐT Phụ huynh")]
        public string? SdtPhuHuynh { get; set; }

        [Display(Name = "Trình độ hiện tại")]
        public string? TrinhDoHienTai { get; set; }

        [Display(Name = "Mục tiêu học tập")]
        public string? MucTieu { get; set; }

        [Display(Name = "Nguồn khách hàng")]
        public string? NguonGoc { get; set; }

        public DateTime? NgayTao { get; set; }

        // --- CÁC MỐI QUAN HỆ (KHÓA NGOẠI) ---

        public int? NhanVienId { get; set; }
        [ForeignKey("NhanVienId")]
        public virtual NhanVien? NhanVien { get; set; }

        public int? LopHocId { get; set; }
        [ForeignKey("LopHocId")]
        public virtual LopHoc? LopHoc { get; set; }

        public virtual ICollection<LichSuTuVan>? LichSuTuVans { get; set; }

        // Gộp chung khai báo Chiến dịch vào một chỗ cho sạch code
        [Display(Name = "Chiến dịch nguồn")]
        public int? ChienDichId { get; set; }
        [ForeignKey("ChienDichId")]
        public virtual ChienDich? ChienDich { get; set; }

        [Display(Name = "Mục tiêu học tập cụ thể")]
        public string? MucTieuHocTap { get; set; }

        public DateTime? NgayHen { get; set; }
        public string? NoiDungHen { get; set; }

        public virtual ICollection<PhieuThu> PhieuThus { get; set; } = new List<PhieuThu>();
        public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();
    }
}