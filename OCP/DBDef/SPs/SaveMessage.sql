IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[SaveMessage]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE dbo.SaveMessage
END
GO

CREATE PROCEDURE SaveMessage(
@EmailAddress nvarchar(320),
@MessageType nvarchar(50),
@Message nvarchar(max)
) AS
BEGIN

	INSERT INTO dbo.Messages
		(EmailAddress, MessageType, [Message], [Date])
		VALUES
		(@EmailAddress, @MessageType, @Message, GETUTCDATE())

END
GO
