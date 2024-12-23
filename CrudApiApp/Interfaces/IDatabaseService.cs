using System.Data.SqlClient;

namespace CrudApiApp
{
    public interface IDatabaseService
    {
        SqlConnection GetConnection();
    }
}
