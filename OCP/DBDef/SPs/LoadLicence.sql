IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[LoadLicence]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE dbo.LoadLicence
END
GO

CREATE PROCEDURE LoadLicence(
@Guid uniqueidentifier
) AS
BEGIN

	SELECT l.NoOfLicences, l.Licence
		FROM dbo.Licences l
		WHERE l.[Guid] = @Guid

END
GO
