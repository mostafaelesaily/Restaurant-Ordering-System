using Data_Access_Layer.Data;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Domain_Layer.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TablesRepo : MainGenaricRepo<Tables, int>, ITableRepo
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Tables> _tables;

        public TablesRepo(AppDbContext context) : base(context)
        {
            _context = context;
            _tables = context.Set<Tables>();
        }

        public IQueryable<Tables> Search_Tables(string searchKey)
        {
            return _tables
                .AsNoTracking()
                .Where(t =>
                    (t.QrCode != null && EF.Functions.Like(t.QrCode, $"%{searchKey}%")) ||
                    EF.Functions.Like(t.TableNumber.ToString(), $"%{searchKey}%"));
        }

        public IQueryable<Tables> GetAvailableTables(DateTime date)
        {
            var reservedTableIds = _context.Set<Reservations>()
                .AsNoTracking()
                .Where(r =>
                    r.ReservationDate.Date == date.Date &&
                    r.Status != ReservationStatus.Cancelled)
                .Select(r => r.tableId);

            return _tables
                .AsNoTracking()
                .Where(t => !reservedTableIds.Contains(t.Id));
        }
    }
}