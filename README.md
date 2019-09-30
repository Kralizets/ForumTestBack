# ForumTestBack
Dapper, base sql-query.
Query for create table:

create table Forum (
	id INT PRIMARY KEY IDENTITY (1, 1),
	[ThemeName] [varchar] (1000) NULL,
	[ChangeDate] [datetime] NULL
)
go

insert into Forum (ThemeName, ChangeDate)
values 					  
('name1', '20190220 10:35:10 AM'),
('name2', '20190420 10:35:10 AM'),
('name3', '20190620 10:35:10 AM'),
('name4', '20190820 10:35:10 AM'),
('name5', '20190315 10:35:10 AM'),
('name6', '20190515 10:35:10 AM'),
('name7', '20190715 10:35:10 AM')
go
