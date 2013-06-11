IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[LoadCurrentExchangeRates]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE dbo.LoadCurrentExchangeRates
END
GO

CREATE PROCEDURE LoadCurrentExchangeRates
AS
BEGIN

	SELECT er.AUD, er.CAD, er.EUR, er.USD, er.[Date]
		FROM dbo.ExchangeRates er
		WHERE er.[Date] = CONVERT(date, GETUTCDATE())

END
GO
