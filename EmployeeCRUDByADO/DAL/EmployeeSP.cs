using EmployeeCRUDByADO.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EmployeeCRUDByADO.DAL
{
    public class EmployeeSP
    {
        private readonly string _connectionString;

        public EmployeeSP(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET ALL EMPLOYEES
        public List<Employee> GetEmployees()
        {
            List<Employee> employees = new List<Employee>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetEmployees", con);

                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Employee emp = new Employee();

                    emp.Id = Convert.ToInt32(reader["Id"]);
                    emp.Name = reader["Name"].ToString();
                    emp.City = reader["City"].ToString();
                    emp.Salary = Convert.ToDecimal(reader["Salary"]);

                    employees.Add(emp);
                }
            }

            return employees;
        }

        // INSERT EMPLOYEE
        public void AddEmployee(Employee emp)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_InsertEmployee", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", emp.Name);
                cmd.Parameters.AddWithValue("@City", emp.City);
                cmd.Parameters.AddWithValue("@Salary", emp.Salary);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        // GET EMPLOYEE BY ID
        public Employee GetEmployeeById(int id)
        {
            Employee emp = new Employee();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetEmployeeById", con);

                cmd.CommandType = CommandType.StoredProcedure;

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

        // UPDATE EMPLOYEE
        public void UpdateEmployee(Employee emp)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateEmployee", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", emp.Id);
                cmd.Parameters.AddWithValue("@Name", emp.Name);
                cmd.Parameters.AddWithValue("@City", emp.City);
                cmd.Parameters.AddWithValue("@Salary", emp.Salary);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        // DELETE EMPLOYEE
        public void DeleteEmployee(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteEmployee", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }
    }
}
