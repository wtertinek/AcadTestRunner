function Update-File ($targetFileName, $srcFileName)
{  
  (Get-Content -Encoding UTF8 $srcFileName) | ForEach-Object { $_ -replace "{AcadRootDir}", "$acadRootDir" } | Set-Content -Encoding UTF8 $targetFileName
}

$acadRootDir = Read-Host 'Please enter your AutoCAD installation folder'

if (Test-Path $acadRootDir)
{
  if (Test-Path ..\AcadTestRunner\AcadTestRunner.csproj.user)
  {
    Remove-Item ..\AcadTestRunner\AcadTestRunner.csproj.user
  }
  if (Test-Path ..\AcadTestRunner\AcadTestRunner.dll.config)
  {
    Remove-Item ..\AcadTestRunner\AcadTestRunner.dll.config
  }  
  
  Update-File ..\AcadTestRunner\AcadTestRunner.csproj.user "Template.csproj.user"  
  Update-File ..\AcadTestRunner\AcadTestRunner.dll.config "AcadTestRunner.dll.config"
  
  Write-Host 'AutoCAD reference path set to' $acadRootDir
}
else
{
  Write-Host $acadRootDir 'does not exist'
}
