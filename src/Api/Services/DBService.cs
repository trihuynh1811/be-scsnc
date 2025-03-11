using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;

namespace Api.Services
{
    public class DBService: IDBService
    {
        readonly ApplicationDbContext _context;

        public DBService()
        {
            _context = new ApplicationDbContext();
        }

        public void CreateDropDb(IConfiguration configuration)
        {
            try
            {
                if (bool.Parse(configuration["DbOptions:create-drop"]!))
                {
                    _context.Database.EnsureDeleted();
                    _context.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }

        }
        public void CreateDb(IConfiguration configuration)
        {
            var databaseCreator = _context.Database.GetService<IRelationalDatabaseCreator>();
            if (!databaseCreator.Exists())
            {
                _context.Database.EnsureCreated();
            }
            else
            {
                Console.WriteLine("db already existed");
                Debug.WriteLine("db already existed");
            }

        }

    }
}
