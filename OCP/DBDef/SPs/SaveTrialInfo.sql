IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[SaveTrialInfo]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE dbo.SaveTrialInfo
END
GO

CREATE PROCEDURE SaveTrialInfo(
@Guid uniqueidentifier,
@UtcDate datetime,
@LocalDate datetime,
@Event varchar(20)
) AS
BEGIN

	INSERT INTO Trials
		([Guid], UtcDate, LocalDate, [Event])
		VALUES
		(@Guid, @UtcDate, @LocalDate, @Event)

END
GO
