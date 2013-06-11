IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[UpsertUser]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE dbo.UpsertUser
END
GO

CREATE PROCEDURE UpsertUser(
@EmailAddress nvarchar(500)
) AS
BEGIN

	DECLARE @UserId bigint = NULL

	SELECT @UserId = UserId 
		FROM dbo.Users u
		WHERE u.EmailAddress = @EmailAddress

	IF @UserId IS NULL
	BEGIN

		INSERT INTO USERS
			(EmailAddress, DateAdded)
			VALUES
			(@EmailAddress, GETUTCDATE())

		SET @UserId = SCOPE_IDENTITY()

	END

	SELECT @UserId AS UserId

END
GO
