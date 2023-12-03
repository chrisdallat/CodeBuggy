# CodeBuggy
A bug tracking/ticketing software development and collaboration tool build with asp.net framework


### Postgres Setup Docs (MacOS for now)

install postgres on MacOS with:
```
brew install postgresql@15
```

download pgAdmin for dealing with the database
```
https://www.pgadmin.org/download/pgadmin-4-macos/
```

start and stop the postgres services with suffix of version:
```
brew services start postgresql@15
brew services stop postgresql@15
```

info on setting up postgres in terminal mac:
```
https://www.devart.com/dbforge/postgresql/how-to-install-postgresql-on-macos/
https://www.sqlshack.com/setting-up-a-postgresql-database-on-mac/
```

if psql command not working:
```
brew link postgresql@15 --force
```

open pgAdmin4 
create a new server (CodeBuggyServer)
port is set as 5432

