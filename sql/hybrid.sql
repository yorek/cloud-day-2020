drop table if exists [dbo].[todo_hybrid];
create table [dbo].[todo_hybrid]
(
	[id] [int] not null,
	[todo] [nvarchar](100) not null,
	[completed] [tinyint] not null,
	[extension] nvarchar(max) null,
)
go
alter table [dbo].[todo_hybrid] add constraint pk__todo_hybrid primary key clustered ([id] asc) with (optimize_for_sequential_key = on)
go
alter table [dbo].[todo_hybrid] add constraint df__todo_hybrid__id default (next value for [global_sequence]) for [id]
go
alter table [dbo].[todo_hybrid] add constraint df__todo_hybrid__completed default ((0)) for [completed]
go
alter table [dbo].[todo_hybrid] add constraint ck__todo_hybrid__other check (isjson([extension]) = 1)
go





