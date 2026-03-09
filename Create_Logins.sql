create table Logins
(
	LoginID int not null identity(1000,1),
	UserName nvarchar(50) not null,
	Password nvarchar(50) collate SQL_Latin1_General_CP1_CS_AS not null,
	constraint [PK_Login] primary key clustered (LoginID asc)
	with (statistics_norecompute = off, ignore_dup_key = off) on [primary]
) on [primary]
go
