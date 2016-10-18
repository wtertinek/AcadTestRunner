function Update-File ($targetFileName, $tempalteFileName, $variableName, $value)
{
  if (Test-Path $targetFileName)
  {
    $srcFileName = $targetFileName
  }
  else
  {
    $srcFileName = $tempalteFileName
  }
  
  (Get-Content -Encoding UTF8 $srcFileName) | ForEach-Object { $_ -replace $variableName, $value } | Set-Content -Encoding UTF8 $targetFileName
}

$acadRootDir = Read-Host 'Please enter your AutoCAD installation folder'
$addinRootDir = Read-Host 'Please enter the AcadTestRunner installation folder'

if (-Not (Test-Path $acadRootDir))
{
  Write-Host $acadRootDir 'does not exist'
}
elseif (-Not (Test-Path $addinRootDir))
{
  Write-Host $addinRootDir 'does not exist'
}
else
{
  if (Test-Path ..\AcadTestRunner\AcadTestRunner.csproj.user)
  {
    Remove-Item ..\AcadTestRunner\AcadTestRunner.csproj.user
  }
  if (Test-Path ..\AcadTestRunner\AcadTestRunner.dll.config)
  {
    Remove-Item ..\AcadTestRunner\AcadTestRunner.dll.config
  }  
  
  Update-File ..\AcadTestRunner\AcadTestRunner.csproj.user "Template.csproj.user"  "{AcadRootDir}" "$acadRootDir"
  Update-File ..\AcadTestRunner\AcadTestRunner.dll.config "AcadTestRunner.dll.config" "{AcadRootDir}" "$acadRootDir"
  Update-File ..\AcadTestRunner\AcadTestRunner.dll.config "AcadTestRunner.dll.config" "{AddinRootDir}" "$addinRootDir"
  
  Write-Host ''
  Write-Host 'AutoCAD reference path set to ' $acadRootDir
  Write-Host 'AcadTestRunner installation directory is ' $addinRootDir
}
