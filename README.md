
# Cloud Day 2020 Demos

Demos used in the Cloud Day 2020 Session: 

**Serverless Scalable Back-End API with Hybrid Data Models**

Azure SQL natively support to JSON is really a game changing feature as it allows both object model and relational model to happily live together, allowing application developer and database developer to use the best model - or even both - for their need. It also provide great performances and flexibility and helps to achieve great scalability and agility. In this session we'll see how one can create REST API with the language if its choice while leveraging JSON to communicate efficiently and comfortably with the database and to create hybrid data models, taking the best from relational and non-relational world

The demos shows how ToDoMVC backend API can be implemented using three different models, to simplify development and giving a developer all the needed flexibility to support a dynamic schema

## Pre-Requisites

To run this sample, you need to have [Azure Function Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash) and an Azure SQL database to use. If you need help to create an Azure SQL database, take a look here: [Running the samples](https://github.com/yorek/azure-sql-db-samples#running-the-samples)

## Samples

### Classic Model

Implemented using a table like the following:

```sql
create table [dbo].[todo]
(
	[id] [int] not null,
	[title] [nvarchar](100) not null,
	[completed] [tinyint] not null
)
```

and using JSON to simplify *a lot* the communication between REST API and Azure SQL. Model can be extended just by adding columns to the table. The JSON communication protocol will shield table schema changes to REST API.

### Hybrid Model

Implemented using a table like the following:

```sql
create table [dbo].[todo]
(
	[id] [int] not null,
	[title] [nvarchar](100) not null,
	[completed] [tinyint] not null,
	[extension] nvarchar(max) null, -- JSON
)
```

Schema can be extended by storing additional field as JSON into the `extensions` column. This allows for a good balance between flexibility and performance when querying / using well-know columns, like `title` or `completed`.

### Full JSON Model

Implemented using a table like the following:

```sql
create table [dbo].[todo]
(
	[id] [int] not null,
	[todo] nvarchar(max) null -- JSON
)
```

The entire object is stored as JSON object in the `todo` column. This allows extreme flexibility. Performance improvement can be obtained by creating indexable calculated columns on properties that are know to be mandatory. For example `title`:

```sql
alter table dbo.[todo_json]
add [Title] as json_value([todo], '$.title') persisted
go
```

## Best of Both Worlds

By using JSON as a transport protocol or even by using it to store atomic objects in the database, you can get the best from both world, relational and non-relational. If you are interested in performance analysis of the options describe in this repo, take a look a this article:

[JSON in your Azure SQL Database? Let’s benchmark some options!](https://devblogs.microsoft.com/azure-sql/json-in-your-azure-sql-database-lets-benchmark-some-options/)


## More Azure SQL Samples 

More samples around Azure SQL and how it can be used for developing modern applications can be found here:

https://github.com/yorek/azure-sql-db-samples 

and here

https://aka.ms/azure-sql-db-dev-samples 