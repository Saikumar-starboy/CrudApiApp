using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace CrudApiApp.Controllers
{
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public EmployeesController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet]
        [Route("api/GetAllEmployees")]
        public async Task<IActionResult> GetAll()
        {
            using var connection = _databaseService.GetConnection();
            var employees = await connection.QueryAsync<Employee>("EXEC GetAllEmployees");
            return Ok(employees);
        }

        [HttpGet]
        [Route("api/GetEmployeeById/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            using var connection = _databaseService.GetConnection();
            var employee = await connection.QuerySingleOrDefaultAsync<Employee>("EXEC GetEmployeeById @id", new { id });
            if (employee == null)
                return NotFound();
            return Ok(employee);
        }

        [HttpPost]
        [Route("api/AddEmployee")]
        public async Task<IActionResult> Create([FromBody] Employee employee)
        {
            if (employee == null || string.IsNullOrWhiteSpace(employee.FirstName) || string.IsNullOrWhiteSpace(employee.LastName))
            {
                return BadRequest("Invalid employee data.");
            }

            using var connection = _databaseService.GetConnection();
            var id = await connection.ExecuteScalarAsync<int>("EXEC AddEmployee @FirstName, @LastName, @Email", new { employee.FirstName, employee.LastName, employee.Email });
            employee.EmployeeId = id;

            return CreatedAtAction(nameof(Get), new { id = employee.EmployeeId }, employee);
        }

        [HttpPut]
        [Route("api/UpdateEmployee/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Employee employee)
        {
            if (employee == null || id <= 0 || string.IsNullOrWhiteSpace(employee.FirstName) || string.IsNullOrWhiteSpace(employee.LastName))
            {
                return BadRequest("Invalid employee data.");
            }

            using var connection = _databaseService.GetConnection();
            var affectedRows = await connection.ExecuteAsync("EXEC UpdateEmployee @EmployeeId, @FirstName, @LastName, @Email",
                new { EmployeeId = id, employee.FirstName, employee.LastName, employee.Email });

            if (affectedRows == 0)
                return NotFound();

            return NoContent();
        }

        [HttpDelete]
        [Route("api/DeleteEmployee/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid employee ID.");
            }

            using var connection = _databaseService.GetConnection();
            var affectedRows = await connection.ExecuteAsync("EXEC DeleteEmployee @Id", new { Id = id });

            if (affectedRows == 0)
                return NotFound();

            return NoContent();
        }
    }
}
