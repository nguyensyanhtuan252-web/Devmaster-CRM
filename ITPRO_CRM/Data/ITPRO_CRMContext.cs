using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Models; // Nhớ dòng này để nó hiểu Models

namespace ITPRO_CRM.Data
{
    public class ITPRO_CRMContext : DbContext
    {
        public ITPRO_CRMContext(DbContextOptions<ITPRO_CRMContext> options)
            : base(options)
        {
        }

        // Đăng ký các bảng với hệ thống
        public DbSet<ITPRO_CRM.Models.KhoaHoc> KhoaHoc { get; set; } = default!;
        public DbSet<ITPRO_CRM.Models.GiangVien> GiangVien { get; set; } = default!;
        public DbSet<ITPRO_CRM.Models.LopHoc> LopHoc { get; set; } = default!;
        public DbSet<DiemDanh> DiemDanh { get; set; }
        public DbSet<ITPRO_CRM.Models.HocVien> HocVien { get; set; } = default!;
        public DbSet<ITPRO_CRM.Models.LichSuTuVan> LichSuTuVan { get; set; } = default!;
        public DbSet<ITPRO_CRM.Models.NhanVien> NhanVien { get; set; } = default!;
        public DbSet<ITPRO_CRM.Models.ChienDich> ChienDich { get; set; } = default!;
        public DbSet<PhieuThu> PhieuThu { get; set; }
        public DbSet<CauHinh> CauHinh { get; set; }
        public DbSet<EmailMarketing> EmailMarketing { get; set; }
        
    }
}