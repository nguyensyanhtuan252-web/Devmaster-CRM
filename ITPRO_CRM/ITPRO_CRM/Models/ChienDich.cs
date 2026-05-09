using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ITPRO_CRM.Models
{
    public class ChienDich
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên chiến dịch không được để trống")]
        [Display(Name = "Tên chiến dịch")]
        public string TenChienDich { get; set; } // VD: Tuyển sinh Hè 2026

        [Display(Name = "Loại chiến dịch")]
        public string LoaiChienDich { get; set; } // Facebook Ads, Google Ads, Hội thảo...

        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime NgayKetThuc { get; set; }

        [Display(Name = "Ngân sách (VNĐ)")]
        [DisplayFormat(DataFormatString = "{0:0,0}")]
        public decimal NganSach { get; set; } // Tiền chi cho quảng cáo
        [Display(Name = "Ngân sách dự kiến")]
        public decimal NganSachDuKien { get; set; } = 0;

        [Display(Name = "Doanh thu kỳ vọng")]
        [DisplayFormat(DataFormatString = "{0:0,0}")]
        public decimal DoanhThuKyVong { get; set; }

        [Display(Name = "Trạng thái")]
        public bool DangHoatDong { get; set; } = true; // True: Đang chạy, False: Tạm dừng
        

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        // Mối quan hệ: 1 Chiến dịch đem về Nhiều Học viên
        public virtual ICollection<HocVien>? HocViens { get; set; }
    }
}