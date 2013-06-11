IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[SaveCurrentExchangeRates]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE dbo.SaveCurrentExchangeRates
END
GO

CREATE PROCEDURE SaveCurrentExchangeRates(
@AUD float,
@CAD float,
@EUR float,
@USD float)
AS
BEGIN

	INSERT INTO dbo.ExchangeRates
		(AUD, CAD, EUR, USD, [Date])
		VALUES
		(@AUD, @CAD, @EUR, @USD, CONVERT(date, GETUTCDATE()))

END
GO
