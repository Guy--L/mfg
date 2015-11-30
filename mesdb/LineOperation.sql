CREATE TABLE [dbo].[LineOperation] (
    [INDAY]         DECIMAL (5) NOT NULL,
    [INUNIT]        VARCHAR (1) NOT NULL,
    [INLINE]        DECIMAL (1) NOT NULL,
    [INSHFT]        DECIMAL (1) NOT NULL,
    [STCODE]        VARCHAR (1) NOT NULL,
    [INTIME]        DECIMAL (4) NOT NULL,
    [RSCODE]        VARCHAR (2) NOT NULL,
    [INPRD]         VARCHAR (6) NOT NULL,
    [stamp]         DATETIME    NULL,
    [LineId]        INT         NOT NULL,
    [ProductCodeId] INT         NULL
);
