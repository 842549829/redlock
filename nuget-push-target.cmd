@echo off
:: %1 ��һ���������� %~1 ȥ���������ַ�����˫����
:: ��Ŀ·��
::set ProjectDir=%~1
:: ��Ŀ����
::set ItemFileNam=%~2

::�ַ���ƴ��

:: ����ļ�ͷ·��
::set "Sender=%ProjectDir%%ItemFileNam%.csproj"

::nupkg��
::set "NupkgPath=*.nupkg"

::Nuget�������˺�����,�м���Ӣ��ð�Ÿ���(account:password)
set ApiKey=localhostnuget
::����ģʽ Release/Debug
set PublishMode=Debug
::Nuget������ַ
set SourceUrl= http://www.localhostnuget.com/nuget/

::ɾ��nuget��
::del %NupkgPath% /F /Q

::���ɳ����
nuget pack E:\code\core\ConsoleApp1\ConsoleApp1.csproj -Build -Prop Configuration=%PublishMode%  -OutputDirectory E:\code\core\ConsoleApp1
::�ϴ���
::nuget push %NupkgPath% -Source %SourceUrl% -ApiKey %ApiKey%
::ɾ��nuget��
::del %NupkgPath% /F /Q

pause
