IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[SaveEvent]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE dbo.SaveEvent
END
GO

CREATE PROCEDURE SaveEvent(
@Event varchar(20)
) AS
BEGIN

	INSERT INTO dbo.Events
		([Event], [Time])
		VALUES
		(@Event, GETUTCDATE())

END
GO
