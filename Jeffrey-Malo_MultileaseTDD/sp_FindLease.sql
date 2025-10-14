-- Stored procedure to find a lease --

CREATE PROCEDURE sp_FindLease
        --parameters--
        @CustFirstName Varchar(30),
        @CustLastName Varchar(30)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN
        SELECT c.LeaseID, l.VIN FROM Customers c, Lease l WHERE FirstName = @CustFirstName AND LastName = @CustLastName AND
        c.LeaseID = l.LeaseID;
    END
END;