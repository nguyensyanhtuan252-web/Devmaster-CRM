using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;

namespace ITPRO_CRM.Controllers
{
    public class CauHinhsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public CauHinhsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // GET: CauHinhs
        // GET: CauHinhs
        public async Task<IActionResult> Index()
        {
            // Kiểm tra xem đã có cấu hình nào chưa
            var config = await _context.CauHinh.FirstOrDefaultAsync();

            // Nếu chưa có -> Tạo mới 1 cái mặc định rồi chuyển đến trang Sửa
            if (config == null)
            {
                config = new CauHinh
                {
                    TenTrungTam = "ITPRO ACADEMY",
                    DiaChi = "Hà Nội",
                    SoDienThoai = "0988888888"
                };
                _context.Add(config);
                await _context.SaveChangesAsync();
            }

            // Chuyển thẳng đến trang Edit (Sửa) thông tin số 1
            return RedirectToAction(nameof(Edit), new { id = config.Id });
        }

        // GET: CauHinhs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cauHinh = await _context.CauHinh
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cauHinh == null)
            {
                return NotFound();
            }

            return View(cauHinh);
        }

        // GET: CauHinhs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CauHinhs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenTrungTam,DiaChi,SoDienThoai")] CauHinh cauHinh)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cauHinh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cauHinh);
        }

        // GET: CauHinhs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cauHinh = await _context.CauHinh.FindAsync(id);
            if (cauHinh == null)
            {
                return NotFound();
            }
            return View(cauHinh);
        }

        // POST: CauHinhs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CauHinh cauHinh)
        {
            if (id != cauHinh.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cauHinh);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CauHinhExists(cauHinh.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cauHinh);
        }

        // GET: CauHinhs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cauHinh = await _context.CauHinh
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cauHinh == null)
            {
                return NotFound();
            }

            return View(cauHinh);
        }

        // POST: CauHinhs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cauHinh = await _context.CauHinh.FindAsync(id);
            if (cauHinh != null)
            {
                _context.CauHinh.Remove(cauHinh);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CauHinhExists(int id)
        {
            return _context.CauHinh.Any(e => e.Id == id);
        }
        // GET: CauHinhs/TaoDuLieuMau
        public async Task<IActionResult> TaoDuLieuMau()
        {
            // 1. Dọn dẹp dữ liệu cũ (Xóa theo thứ tự để tránh lỗi khóa ngoại)
            if (_context.HocVien.Any())
            {
                _context.PhieuThu.RemoveRange(_context.PhieuThu); // Xóa con trước
                _context.HocVien.RemoveRange(_context.HocVien);
                _context.LopHoc.RemoveRange(_context.LopHoc);     // Xóa cha sau
                _context.ChienDich.RemoveRange(_context.ChienDich);
                await _context.SaveChangesAsync();
            }

            // 2. Tạo Chiến dịch Marketing
            var cd1 = new ChienDich
            {
                TenChienDich = "Tuyển sinh Lập trình Viên K15",
                NgayBatDau = DateTime.Now.AddMonths(-2),
                NgayKetThuc = DateTime.Now.AddMonths(1),
                DangHoatDong = true,
                LoaiChienDich = "Quảng cáo Facebook"
            };

            var cd2 = new ChienDich
            {
                TenChienDich = "Học bổng Tài năng trẻ IT",
                NgayBatDau = DateTime.Now.AddMonths(-1),
                NgayKetThuc = DateTime.Now.AddMonths(1),
                DangHoatDong = true,
                LoaiChienDich = "Hội thảo Offline"
            };

            var cd3 = new ChienDich
            {
                TenChienDich = "Workshop: AI & ChatGPT",
                NgayBatDau = DateTime.Now.AddDays(-20),
                NgayKetThuc = DateTime.Now.AddDays(-15),
                DangHoatDong = false,
                LoaiChienDich = "Sự kiện ngắn hạn"
            };

            _context.ChienDich.AddRange(cd1, cd2, cd3);
            await _context.SaveChangesAsync();

            // 3. Tạo Lớp học (Đầy đủ Lịch học)
            var lop1 = new LopHoc { TenLop = "Lập trình C# .NET Core (K15)", HocPhi = 6000000, NgayKhaiGiang = DateTime.Now.AddDays(10), TrangThai = 1, LichHoc = "Tối 2-4-6" };
            var lop2 = new LopHoc { TenLop = "Fullstack Web ReactJS (K12)", HocPhi = 8500000, NgayKhaiGiang = DateTime.Now.AddMonths(-2), TrangThai = 2, LichHoc = "Tối 3-5-7" };
            var lop3 = new LopHoc { TenLop = "Data Analyst Cấp tốc (K08)", HocPhi = 5500000, NgayKhaiGiang = DateTime.Now.AddMonths(-4), TrangThai = 3, LichHoc = "Cuối tuần T7-CN" };
            var lop4 = new LopHoc { TenLop = "Tester Kiểm thử phần mềm", HocPhi = 4500000, NgayKhaiGiang = DateTime.Now.AddDays(20), TrangThai = 0, LichHoc = "Tối 2-4-6" };
            var lop5 = new LopHoc { TenLop = "Lập trình Java Spring Boot", HocPhi = 7000000, NgayKhaiGiang = DateTime.Now.AddMonths(-1), TrangThai = 2, LichHoc = "Tối 3-5-7" };

            _context.LopHoc.AddRange(lop1, lop2, lop3, lop4, lop5);
            await _context.SaveChangesAsync();

            // 4. Tạo Danh sách Học viên (Đầy đủ Giới tính, Nguồn gốc)
            var listHV = new List<HocVien>();
            string[] ho = { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng" };
            string[] tenDem = { "Văn", "Thị", "Đức", "Thành", "Minh", "Quốc", "Gia", "Hữu", "Ngọc", "Thanh" };
            string[] ten = { "Anh", "Bình", "Châu", "Dương", "Em", "Giang", "Hải", "Hùng", "Khánh", "Long", "Minh", "Nam", "Oanh", "Phúc", "Quang", "Sơn", "Tùng", "Uyên", "Vinh", "Yến" };

            Random rand = new Random();

            // Nhóm 1: Tiềm năng (Chưa xếp lớp)
            for (int i = 0; i < 15; i++)
            {
                string fullName = ho[rand.Next(ho.Length)] + " " + tenDem[rand.Next(tenDem.Length)] + " " + ten[rand.Next(ten.Length)];
                string gioiTinh = fullName.Contains("Thị") || fullName.Contains("Oanh") || fullName.Contains("Uyên") || fullName.Contains("Yến") ? "Nữ" : "Nam";

                listHV.Add(new HocVien
                {
                    HoTen = fullName,
                    Email = $"lead{i}@test.com",
                    SoDienThoai = "09" + rand.Next(10000000, 99999999),
                    TrangThai = 0,
                    ChienDichId = (i % 2 == 0) ? cd1.Id : cd2.Id,
                    DiaChi = "Hà Nội",
                    NgayTao = DateTime.Now.AddDays(-rand.Next(1, 60)),
                    GioiTinh = gioiTinh,
                    NguonGoc = "Facebook Ads"
                });
            }

            // Nhóm 2: Cơ hội (Chưa xếp lớp)
            for (int i = 0; i < 10; i++)
            {
                string fullName = ho[rand.Next(ho.Length)] + " " + tenDem[rand.Next(tenDem.Length)] + " " + ten[rand.Next(ten.Length)];
                string gioiTinh = fullName.Contains("Thị") || fullName.Contains("Oanh") || fullName.Contains("Uyên") || fullName.Contains("Yến") ? "Nữ" : "Nam";

                listHV.Add(new HocVien
                {
                    HoTen = fullName,
                    Email = $"opp{i}@test.com",
                    SoDienThoai = "08" + rand.Next(10000000, 99999999),
                    TrangThai = 1,
                    ChienDichId = cd1.Id,
                    DiaChi = "TP.HCM",
                    NgayTao = DateTime.Now.AddDays(-rand.Next(10, 90)),
                    GioiTinh = gioiTinh,
                    NguonGoc = "Website"
                });
            }

            // Nhóm 3: Học viên chính thức (Đã có Lớp)
            for (int i = 0; i < 10; i++)
            {
                string fullName = ho[rand.Next(ho.Length)] + " " + tenDem[rand.Next(tenDem.Length)] + " " + ten[rand.Next(ten.Length)];
                string gioiTinh = fullName.Contains("Thị") || fullName.Contains("Oanh") || fullName.Contains("Uyên") || fullName.Contains("Yến") ? "Nữ" : "Nam";

                // Chia đều vào lớp 2 và lớp 5
                int lopId = (i % 2 == 0) ? lop2.Id : lop5.Id;

                listHV.Add(new HocVien
                {
                    HoTen = fullName,
                    Email = $"student{i}@test.com",
                    SoDienThoai = "03" + rand.Next(10000000, 99999999),
                    TrangThai = 2,
                    LopHocId = lopId,
                    DiaChi = "Đà Nẵng",
                    NgayTao = DateTime.Now.AddMonths(-rand.Next(1, 6)),
                    GioiTinh = gioiTinh,
                    NguonGoc = "Người quen giới thiệu"
                });
            }

            _context.HocVien.AddRange(listHV);
            await _context.SaveChangesAsync();

            // 5. Tạo Phiếu thu (SỬA LỖI Ở ĐÂY: Thêm LopHocId)
            var students = listHV.Where(h => h.TrangThai == 2).ToList();
            var phieuThus = new List<PhieuThu>();

            foreach (var st in students)
            {
                // Kiểm tra chắc chắn học viên có lớp thì mới tạo phiếu
                if (st.LopHocId != null)
                {
                    phieuThus.Add(new PhieuThu
                    {
                        HocVienId = st.Id,
                        // 👇 QUAN TRỌNG: Thêm dòng này để sửa lỗi FK_PhieuThu_LopHoc
                        LopHocId = st.LopHocId.Value,
                        SoTien = rand.Next(2, 5) * 1000000,
                        NgayThu = DateTime.Now.AddMonths(-rand.Next(0, 5)).AddDays(-rand.Next(1, 28)),
                        NoiDung = "Đóng học phí đợt 1",
                        NguoiThu = "Admin"
                    });

                    if (rand.Next(0, 2) == 1)
                    {
                        phieuThus.Add(new PhieuThu
                        {
                            HocVienId = st.Id,
                            // 👇 QUAN TRỌNG: Thêm dòng này nữa
                            LopHocId = st.LopHocId.Value,
                            SoTien = rand.Next(1, 3) * 1000000,
                            NgayThu = DateTime.Now.AddDays(-rand.Next(1, 10)),
                            NoiDung = "Đóng học phí đợt 2 (Hoàn tất)",
                            NguoiThu = "Admin"
                        });
                    }
                }
            }

            _context.PhieuThu.AddRange(phieuThus);
            await _context.SaveChangesAsync();

            return Content($"✅ ĐÃ TẠO XONG: Dữ liệu đã fix lỗi khóa ngoại (Phiếu thu đã có Lớp học)!");
        }
    }
}
