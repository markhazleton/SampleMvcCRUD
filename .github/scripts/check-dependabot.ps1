# Script to update dependabot.yml with the missing HttpClientFactory configuration

# Read the current content and decode it
$currentContent = gh api repos/markhazleton/SampleMvcCRUD/contents/.github/dependabot.yml | ConvertFrom-Json
$decodedContent = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($currentContent.content))

# Check if HttpClientFactory is already in the config
if ($decodedContent -match "Mwh.Sample.HttpClientFactory") {
    Write-Host "HttpClientFactory configuration already exists in dependabot.yml" -ForegroundColor Green
} else {
    Write-Host "HttpClientFactory configuration is missing from dependabot.yml" -ForegroundColor Yellow
    Write-Host "The configuration needs to be added manually via a PR or direct edit"
    Write-Host ""
    Write-Host "Add the following configuration after the '/Mwh.Sample.Repository' section:" -ForegroundColor Cyan
    Write-Host @"

  # NuGet dependencies for HttpClientFactory project
  - package-ecosystem: "nuget"
    directory: "/Mwh.Sample.HttpClientFactory"
    schedule:
      interval: "weekly"
      day: "tuesday"
      time: "09:00"
      timezone: "America/New_York"
    labels:
      - "dependencies"
      - "nuget"
    commit-message:
      prefix: "deps"
      include: "scope"
"@
}

Write-Host ""
Write-Host "Current dependabot.yml content:" -ForegroundColor Cyan
Write-Host $decodedContent
