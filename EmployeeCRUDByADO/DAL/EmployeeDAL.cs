using EmployeeCRUDByADO.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeCRUDByADO.DAL
{
    public class EmployeeDAL
    {
        private readonly string _connectionString;

        public EmployeeDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Get All Employees
        public List<Employee> GetEmployees()
        {
            List<Employee> employees = new List<Employee>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Employee";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    employees.Add(new Employee
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        City = reader["City"].ToString(),
                        Salary = Convert.ToDecimal(reader["Salary"])
                    });
                }
            }

            return employees;
        }

        // Add Employee
        public void AddEmployee(Employee emp)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Employee(Name, City, Salary) VALUES(@Name,@City,@Salary)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Name", emp.Name);
                cmd.Parameters.AddWithValue("@City", emp.City);
                cmd.Parameters.AddWithValue("@Salary", emp.Salary);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Get Employee By Id
        public Employee GetEmployeeById(int id)
        {
            Employee emp = new Employee();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Employee WHERE Id=@Id";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    emp.Id = Convert.ToInt32(reader["Id"]);
                    emp.Name = reader["Name"].ToString();
                    emp.City = reader["City"].ToString();
                    emp.Salary = Convert.ToDecimal(reader["Salary"]);
                }
            }

            return emp;
        }

        // Update Employee
        public void UpdateEmployee(Employee emp)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Employee SET Name=@Name, City=@City, Salary=@Salary WHERE Id=@Id";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Id", emp.Id);
                cmd.Parameters.AddWithValue("@Name", emp.Name);
                cmd.Parameters.AddWithValue("@City", emp.City);
                cmd.Parameters.AddWithValue("@Salary", emp.Salary);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        // Delete Employee
        public void DeleteEmployee(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Employee WHERE Id=@Id";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }
    }
}
