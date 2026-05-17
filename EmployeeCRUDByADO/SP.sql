-- insert SP 
CREATE PROCEDURE sp_InsertEmployee
(
    @Name NVARCHAR(100),
    @City NVARCHAR(100),
    @Salary DECIMAL(18,2)
)
AS
BEGIN
    INSERT INTO Employee(Name, City, Salary)
    VALUES(@Name, @City, @Salary)
END

-- update SP 
CREATE PROCEDURE sp_UpdateEmployee
(
    @Id INT,
    @Name NVARCHAR(100),
    @City NVARCHAR(100),
    @Salary DECIMAL(18,2)
)
AS
BEGIN
    UPDATE Employee
    SET
        Name = @Name,
        City = @City,
        Salary = @Salary
    WHERE Id = @Id
END

-- deleete SP 
CREATE PROCEDURE sp_DeleteEmployee
(
    @Id INT
)
AS
BEGIN
    DELETE FROM Employee
    WHERE Id = @Id
END

-- get all emplployee sp 

CREATE PROCEDURE sp_GetEmployees
AS
BEGIN
    SELECT * FROM Employee
END

-- get by id SP 

CREATE PROCEDURE sp_GetEmployeeById
(
    @Id INT
)
AS
BEGIN
    SELECT * FROM Employee
    WHERE Id = @Id
END