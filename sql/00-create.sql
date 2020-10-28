create schema [web];
go

create user [todo-backend] with password = 'Super_Str0ng*P@ZZword!'
go

alter role [db_datareader] add member [todo-backend];
go

grant execute on schema::[web] to [todo-backend]
go

create sequence dbo.[global_sequence]
as int
start with 1
increment by 1;
go

