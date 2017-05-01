# Troubleshooting

The following are some errors or problems you might run into while going through this exercise.

## The specified port is in use

When debugging the project, IIS Express may refuse to start with the following error.

![](readme-images/troubleshooting/port-in-use.png)

To fix this, use Task Manager to locate the process with the process ID reported and shut it down.

## SurveyService requires SQL Server 2012 Express LocalDB

Visual Studio may present an error indicating that SQL Server 2012 Express LocalDB is required.

![](readme-images/troubleshooting/local-db.png)

To fix this, visit the [Microsoft Download Center](https://www.microsoft.com/en-us/download/details.aspx?id=29062) and install SqlLocalDB.msi.

