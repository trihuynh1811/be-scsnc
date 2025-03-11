namespace Api.Services
{
    public interface IDBService
    {
        void CreateDropDb(IConfiguration configuration);
        void CreateDb(IConfiguration configuration);
    }

}
