# Alvo: versões que desejas uniformizar
$efVersion = "8.0.13"
$pomeloVersion = "8.0.3"

# Aponta para todos os .csproj do projeto
$csprojFiles = Get-ChildItem -Recurse -Filter *.csproj

foreach ($file in $csprojFiles) {
    Write-Host "⏳ Atualizando $($file.FullName)"

    (Get-Content $file.FullName) `
        -replace '<PackageReference Include="Microsoft.EntityFrameworkCore(\.Relational)?" Version=".*?"', "<PackageReference Include=`"Microsoft.EntityFrameworkCore`$1`" Version=`"$efVersion`"" `
        -replace '<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version=".*?"', "<PackageReference Include=`"Pomelo.EntityFrameworkCore.MySql`" Version=`"$pomeloVersion`"" `
    | Set-Content $file.FullName

    Write-Host "✅ Corrigido: $($file.Name)"
}

Write-Host "`n🎉 Atualização completa. Corre agora:"
Write-Host "  dotnet clean"
Write-Host "  dotnet restore"
Write-Host "  dotnet build"
