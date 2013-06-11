IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[SaveLicence]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE dbo.SaveLicence
END
GO

CREATE PROCEDURE SaveLicence(
@UserId bigint,
@Licence varchar(500),
@Guid uniqueidentifier,
@NoOfLicences int,
@ValidModules varchar(500),
@Price bigint
) AS
BEGIN

	INSERT INTO Licences
		(UserId, Licence, DateCreated, [Guid], NoOfLicences, ValidModules, Price)
		VALUES
		(@UserId, @Licence, GETUTCDATE(), @Guid, @NoOfLicences, @ValidModules, @Price)

END
GO
