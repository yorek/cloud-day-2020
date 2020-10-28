delete from dbo.[todos_hybrid];
go

declare @t nvarchar(max) = '{
	"title": "test",
	"completed": 0,
	"other": {
		"order": 1,
		"createdOn": "2020-10-25 10:00:00"	
	}
}';

insert into 
	dbo.[todos_hybrid] (todo, completed, [extension])
select
	title,
	completed,
	[extension]
from
	openjson(@t) with 
	(
		title nvarchar(100) '$.title',
		completed bit '$.completed',
		[extension] nvarchar(max) '$.extension' as json
	)
go

declare @t2 nvarchar(max) = '{
	"title": "another test",
	"completed": 1,
	"other": {
		"order": 2,
		"createdOn": "2020-10-24 22:00:00"	
	}
}';

insert into 
	dbo.[todos_hybrid] (todo, completed, [extension])
select
	title,
	completed,
	[extension]
from
	openjson(@t2) with 
	(
		title nvarchar(100) '$.title',
		completed bit '$.completed',
		[extension] nvarchar(max) '$.extension' as json
	)
go

select * from dbo.[todos_hybrid]
go

select 
	id,
	todo as title,
	completed,
	json_query([extension]) as [extension]
from 
	dbo.[todos_hybrid]
for json auto