@echo off
:: %1 第一个参数变量 %~1 去掉变量中字符串的双引号
:: 项目路径
set ProjectDir=%~1
:: 项目名称
set ItemFileNam=%~2

::字符串拼接

:: 打包文件头路径
set "Sender=%ProjectDir%%ItemFileNam%.csproj"

::nupkg包
set "NupkgPath=*.nupkg"

::Nuget发布的账号密码,中间用英文冒号隔开(account:password)
set ApiKey=localhostnuget
::发布模式 Release/Debug
set PublishMode=Debug
::Nuget发布地址
set SourceUrl= http://www.localhostnuget.com/nuget/

::删除nuget包
del %NupkgPath% /F /Q

::生成程序包
nuget pack %Sender% -Build -Prop Configuration=%PublishMode%
::上传包
nuget push %NupkgPath% -Source %SourceUrl% -ApiKey %ApiKey%
::删除nuget包
del %NupkgPath% /F /Q

pause
