delete from dbo.[todo_json];
go

declare @t nvarchar(max) = '{
	"title": "test",
	"completed": 0,
	"order": 1,
	"createdOn": "2020-10-25 10:00:00"	
}';

declare @t2 nvarchar(max) = '{
	"title": "another test",
	"completed": 1,
	"order": 2,
	"createdOn": "2020-10-24 22:00:00"		
}';

exec [web].[post_todo_json] @t2

select json_query(@t2, '$[0]')

insert into 
	dbo.[todo_json] (todo)
values
	(@t), (@t2)
;	

select * from dbo.[todo_json]
go

exec web.get_todo_json

declare @t3 nvarchar(max) = '[{
	"title": "one",
	"completed": 1,
	"order": 2,
	"createdOn": "2020-10-24 22:00:00"		
},
{
	"title": "two",
	"completed": 1,
	"order": 2,
	"createdOn": "2020-10-24 22:00:00"		
}]';
exec [web].[post_todo_json] @t3

select [value] from openjson(@t3) where [type] = 5

select * from dbo.[todo_json]
delete from dbo.[todo_json]


alter table dbo.[todo_json]
add [Title] as json_value([todo], '$.title') persisted
