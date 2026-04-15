using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITPRO_CRM.Models
{
    public class HocVien
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ tên học viên")]
        public string HoTen { get; set; }
        // 0: Tiềm năng (Mới), 1: Cơ hội (Đang tư vấn), 2: Học viên (Đã đóng tiền), 3: Hủy
        [Display(Name = "Mã học viên")]
        public string? MaHocVien { get; set; }
        [Display(Name = "Trạng thái CRM")]
        public int TrangThai { get; set; } = 0; // Mặc định tạo mới là Tiềm năng (0)
        [Display(Name = "Link Facebook")]
        public string? FacebookLink { get; set; }

        [Display(Name = "Link Zalo")]
        public string? ZaloLink { get; set; }

        // 👇 2. NHÓM HỌC VẤN & CÔNG VIỆC
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
        public string GioiTinh { get; set; } // Nam/Nữ

        [Display(Name = "Số điện thoại HV")]
        public string? SoDienThoai { get; set; } // Có thể null nếu học viên nhỏ tuổi

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        // --- CÁC TRƯỜNG MỚI (EDUCATION CRM) ---

        [Display(Name = "Họ tên Phụ huynh")]
        public string? TenPhuHuynh { get; set; } // Quan trọng để liên hệ đóng tiền

        [Display(Name = "SĐT Phụ huynh")]
        public string? SdtPhuHuynh { get; set; }

        [Display(Name = "Trình độ hiện tại")]
        public string? TrinhDoHienTai { get; set; } // VD: Mất gốc, Beginner, IELTS 5.0

        [Display(Name = "Mục tiêu học tập")]
        public string? MucTieu { get; set; } // VD: Thi Đại học, Du học, Giao tiếp

        [Display(Name = "Nguồn khách hàng")]
        public string? NguonGoc { get; set; } // Facebook, Google, Giới thiệu

        

        // --- TRẠNG THÁI QUY TRÌNH TUYỂN SINH (PIPELINE) ---
        // 0: Mới (Lead)
        // 1: Đang tư vấn
        // 2: Hẹn Test đầu vào
        // 3: Chờ xếp lớp (Đã chốt)
        // 4: Đang học (Đã đóng phí)
        // 5: Bảo lưu / Dừng


        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Khóa ngoại
        public int? NhanVienId { get; set; } // Sale phụ trách
        [ForeignKey("NhanVienId")]
        public virtual NhanVien? NhanVien { get; set; }

        public int? LopHocId { get; set; } // Lớp chính thức
        [ForeignKey("LopHocId")]
        public virtual LopHoc? LopHoc { get; set; }
        public virtual ICollection<LichSuTuVan>? LichSuTuVans { get; set; }
        public int? ChienDichId { get; set; }
        [ForeignKey("ChienDichId")]
        public virtual ChienDich? ChienDich { get; set; }
        [Display(Name = "Mục tiêu học tập")]
        public string? MucTieuHocTap { get; set; }
        public DateTime? NgayHen { get; set; }
        public string? NoiDungHen { get; set; }
        public virtual ICollection<PhieuThu> PhieuThus { get; set; } = new List<PhieuThu>();
        public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();


    }
}