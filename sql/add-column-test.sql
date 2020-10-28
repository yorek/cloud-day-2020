drop table if exists dbo.todos_test;

select * into dbo.todos_test from dbo.todos;

with cte as
(
	select top (1000000) n = row_number() over (order by a.[object_id]) from sys.[all_columns] a, sys.[all_columns] b 
)
insert into
	dbo.[todos_test]
select
	n as id,
	'Todo Test ' + cast(n as nvarchar(10)) as todo,
	0 as completed 
from
	cte
;

select top (100) * from dbo.[todos_test];

alter table dbo.[todos_test] 
add extension nvarchar(max) null 
	constraint ck_isjson check (isjson(extension) = 1) 

alter table dbo.[todos_test]
drop constraint ck_isjson

alter table dbo.[todos_test] 
drop column extension

--delete from dbo.todos


select * from dbo.[todos]
