CREATE TABLE [dbo].[LineStatus] (
    [INUNT]         VARCHAR (1)  NOT NULL,
    [INLIN]         VARCHAR (1)  NOT NULL,
    [INDAY]         NUMERIC (4)  NOT NULL,
    [INPRD]         VARCHAR (6)  NOT NULL,
    [CARTN]         VARCHAR (4)  NOT NULL,
    [INSID]         VARCHAR (4)  NOT NULL,
    [INLSQ]         NUMERIC (3)  NOT NULL,
    [INLST]         VARCHAR (16) NOT NULL,
    [INREL]         VARCHAR (10) NOT NULL,
    [INBSP]         NUMERIC (4)  NOT NULL,
    [INSAM]         NUMERIC (3)  NOT NULL,
    [LineId]        INT          NOT NULL,
    [Status]        CHAR (1)     NOT NULL,
    [Reason]        VARCHAR (2)  NOT NULL,
    [Stamp]         DATETIME     NOT NULL,
    [ProductCodeId] INT          NULL
);

