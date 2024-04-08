# CodeBuggy

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![GitHub stars](https://img.shields.io/github/stars/chrisdallat/CodeBuggy.svg)](https://github.com/chrisdallat/CodeBuggy/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/chrisdallat/CodeBuggy.svg)](https://github.com/chrisdallat/CodeBuggy/network)
[![GitHub issues](https://img.shields.io/github/issues/chrisdallat/CodeBuggy.svg)](https://github.com/chrisdallat/CodeBuggy/issues)

A bug tracking/ticketing software development and collaboration web application

## Authors

This project was created and maintained by the following individuals:

- [Chris Dallat](https://github.com/chrisdallat) - Software Developer
- [Khalil Masree](https://github.com/khalilmasri) - Software Developer

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [License](#license)

## Installation

### Docker container [for access to our hosted database]
**NOTE:** This is only for limited review purposes, if not invited proceed to next installation section.

Requires connection string `appsettings.json` - Can be requested from us.

**NOTE:** Add `appsettings.json` to the project root directory before executing the following commands 
```
docker build -t codebuggy .
```
```
docker run -d --name codebuggy_docker -p 54322:80 codebuggy:latest
```

Use following URL in browser
```
localhost:54322
``` 

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed
- [pgAdmin](https://www.pgadmin.org/download/pgadmin-4-macos/) installed
- [Visual Studio](https://visualstudio.microsoft.com/downloads/) (optional, but recommended for development)


### Clone the Repository
```
git clone https://github.com/chrisdallat/CodeBuggy.git
```

### Postgres Setup

#### MacOS

install postgres on MacOS with:
```
brew install postgresql@15
```

start the postgres services with suffix of version:
```
brew services start postgresql@15
```

Setup postgres superuser:
```
psql postgres
```
If psql command not working:
```
brew link postgresql@15 --force
```

[Documentation for setup in pgAdmin on MacOS](https://www.devart.com/dbforge/postgresql/how-to-install-postgresql-on-macos/) (Recommended through pgAdmin)

[Documentation for terminal setup on MacOS](https://www.sqlshack.com/setting-up-a-postgresql-database-on-mac/)

Create a database instance with preferred credentials.<br>
Recommended database name: CodeBuggyDB<br>
Recommended port: 5432<br>

### Application Setup

Create a file in the root directory of the Application named `appsettings.json` and add the following code, subtituting the relevant settings for your server.

```
{
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "AllowedHosts": "*",
    "ConnectionStrings": {
      "CodeBuggyDB": "Host=YOUR-HOST-IP; Database=YOUR-DB-NAME; Username=YOUR-DB-USERNAME; Password=YOUR-DB-PASSWORD;"
    }
  }
```

Then in `Startup.cs` line 16 substitute `CodeBuggyDB` for your database name. (see below)
```
var connectionString = _configuration.GetConnectionString("CodeBuggyDB") ?? throw new InvalidOperationException("Connection string 'CodeBuggyDB' not found.");
```


### Final Setup

#### MacOS 

Migrate and update the database with following terminal commands:
```
dotnet ef migrations add codebuggy

dotnet ef database update
```

After the database has initialised and updated run the application:
```
dotnet run
```
You can now navigate to the provided link to use the application.

## Usage

Below are some annotated screenshots of the various interfaces in our application.
#### Home Page Landing with clickable and scrollable Tutorial for new users.
<img src="./wwwroot/Images/Tutorial/step1.png" width="600" />

#### Add a New Project or Access an Existing project with Access Code.
<img src="./wwwroot/Images/Tutorial/step2.png" width="600" />

#### Select New Or Existing Project popup
<img src="./wwwroot/Images/Tutorial/step3.png" width="600" />

#### Confirmation of new project. (For Existing you will be asked to input Name and Access Code)
<img src="./wwwroot/Images/Tutorial/step4.png" width="600" />

#### Project List Interface
<img src="./wwwroot/Images/Tutorial/step6.png" width="600" />

#### Project Board Interface
<img src="./wwwroot/Images/Tutorial/step7.png" width="600" />

#### Ticket Details Popup
<img src="./wwwroot/Images/Tutorial/step8.png" width="600" />

#### Pupulated Project Board, tickets can be dragged and dropped to columns or clicked on to open.
<img src="./wwwroot/Images/Tutorial/step10.png" width="600" />

#### Reporting Graph Views
<img src="./wwwroot/Images/Tutorial/step9.png" width="600" />

## License

Copyright (c) 2024

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
