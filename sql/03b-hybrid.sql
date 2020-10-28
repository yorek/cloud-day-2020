delete from dbo.[todo_hybrid];
go

declare @t nvarchar(max) = '{
	"title": "test",
	"completed": 0,
	"extension": {
		"order": 1,
		"createdOn": "2020-10-25 10:00:00"	
	}
}';

insert into 
	dbo.[todo_hybrid] (todo, completed, [extension])
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
	"extension": {
		"order": 2,
		"createdOn": "2020-10-24 22:00:00"	
	}
}';

insert into 
	dbo.[todo_hybrid] (todo, completed, [extension])
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

select * from dbo.[todo_hybrid]
go

delete from dbo.[todo_hybrid] where id != 496

exec [web].[get_todo_hybrid] 
go

exec [web].[get_todo_hybrid] '{"id": 442}'
go

declare @t3 nvarchar(max) = '{
	"title": "another test",
	"completed": 1,
	"extension": {
		"order": 2,
		"createdOn": "2020-10-24 22:00:00"	
	}
}';

exec web.[post_todo_hybrid] @t3

select 
	json_query((select id, todo, completed from dbo.todo_hybrid as i where o.id = i.id for json auto, without_array_wrapper)) as todo,
	json_query(extension) as extension
from 
	dbo.[todo_hybrid] as o
where
	o.id = 441
go