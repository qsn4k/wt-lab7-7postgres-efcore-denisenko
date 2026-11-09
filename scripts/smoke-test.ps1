param(
    [string]$BaseUrl = "http://localhost:10000"
)

$ErrorActionPreference = "Stop"
$BaseUrl = $BaseUrl.TrimEnd('/')
$passed = 0

function Assert-Status([string]$Name, [scriptblock]$Action, [int]$ExpectedStatus = 200) {
    try {
        & $Action | Out-Null
        Write-Host "[PASS] $Name" -ForegroundColor Green
        $script:passed++
    }
    catch {
        $response = $_.Exception.Response
        $actual = if ($response) { [int]$response.StatusCode } else { $_.Exception.Message }
        if ($actual -eq $ExpectedStatus) {
            Write-Host "[PASS] $Name ($ExpectedStatus)" -ForegroundColor Green
            $script:passed++
        }
        else {
            throw "[FAIL] $Name. Ожидался статус $ExpectedStatus; получено: $actual"
        }
    }
}

Write-Host "Проверка Tournament Hub API: $BaseUrl" -ForegroundColor Cyan
Assert-Status "GET /" { Invoke-WebRequest "$BaseUrl/" -UseBasicParsing }
Assert-Status "GET /api/info" { Invoke-RestMethod "$BaseUrl/api/info" }
$players = Invoke-RestMethod "$BaseUrl/api/players"
if ($players.Count -lt 1) { throw "[FAIL] В БД не найдены начальные записи." }
Write-Host "[PASS] GET /api/players ($($players.Count) записей)" -ForegroundColor Green
$passed++
Assert-Status "GET /api/players/999999 -> 404" { Invoke-WebRequest "$BaseUrl/api/players/999999" -UseBasicParsing } 404
Assert-Status "LINQ search" { Invoke-RestMethod "$BaseUrl/api/players/search?search=cyber" }
Assert-Status "LINQ top" { Invoke-RestMethod "$BaseUrl/api/players/top?count=3" }
Assert-Status "LINQ filter" { Invoke-RestMethod "$BaseUrl/api/players/filter?minRating=1000&minWins=1" }
Assert-Status "LINQ group statistics" { Invoke-RestMethod "$BaseUrl/api/players/statistics/by-game" }

$stamp = Get-Date -Format "HHmmss"
$body = @{
    nickname = "Smoke$stamp"
    game = "Valorant"
    teamName = "Smoke Test"
    description = "Описание тестового профиля"
    rating = 1234
    wins = 10
    losses = 4
} | ConvertTo-Json
$created = Invoke-RestMethod "$BaseUrl/api/players" -Method Post -ContentType "application/json" -Body $body
if (-not $created.id) { throw "[FAIL] POST не вернул ID." }
Write-Host "[PASS] POST /api/players (ID=$($created.id))" -ForegroundColor Green
$passed++

$updatedBody = @{
    nickname = "SmokeUpdated$stamp"
    game = "Valorant"
    teamName = "Smoke Team 2"
    description = "Обновленное описание профиля"
    rating = 1400
    wins = 12
    losses = 4
} | ConvertTo-Json
$updated = Invoke-RestMethod "$BaseUrl/api/players/$($created.id)" -Method Put -ContentType "application/json" -Body $updatedBody
if ($updated.rating -ne 1400) { throw "[FAIL] PUT не сохранил рейтинг." }
Write-Host "[PASS] PUT /api/players/{id}" -ForegroundColor Green
$passed++

Invoke-WebRequest "$BaseUrl/api/players/$($created.id)" -Method Delete -UseBasicParsing | Out-Null
Assert-Status "GET deleted player -> 404" { Invoke-WebRequest "$BaseUrl/api/players/$($created.id)" -UseBasicParsing } 404
Write-Host "`nУспешно пройдено проверок: $passed" -ForegroundColor Cyan
