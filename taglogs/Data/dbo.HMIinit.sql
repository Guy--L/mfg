USE [taglogs]
GO
/****** Object:  Table [dbo].[HMI]    Script Date: 4/10/2015 10:27:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HMI](
	[HMIId] [int] IDENTITY(1,1) NOT NULL,
	[ChannelId] [int] NOT NULL,
	[ChartId] [int] NOT NULL,
	[RequestPending] [bit] NOT NULL,
	[RequestComplete] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[HMIId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Operation]    Script Date: 4/10/2015 10:27:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Operation](
	[OperationId] [int] NOT NULL,
	[HMIId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[Stamp] [datetime] NOT NULL,
	[ApproverId] [int] NOT NULL,
	[Notes] [varchar](200) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[OperationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING ON
GO
SET IDENTITY_INSERT [dbo].[HMI] ON 

INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (2, 1, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (3, 2, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (4, 3, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (5, 4, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (6, 5, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (7, 6, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (8, 7, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (9, 8, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (10, 9, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (11, 10, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (12, 11, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (13, 12, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (14, 13, 0, 0, 0)
INSERT [dbo].[HMI] ([HMIId], [ChannelId], [ChartId], [RequestPending], [RequestComplete]) VALUES (15, 14, 0, 0, 0)
SET IDENTITY_INSERT [dbo].[HMI] OFF
ALTER TABLE [dbo].[HMI]  WITH CHECK ADD  CONSTRAINT [FK_HMI_ToChannel] FOREIGN KEY([ChannelId])
REFERENCES [dbo].[Channel] ([ChannelId])
GO
ALTER TABLE [dbo].[HMI] CHECK CONSTRAINT [FK_HMI_ToChannel]
GO
