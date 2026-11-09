$ErrorActionPreference = "Stop"

# Создаёт историю в два этапа: сначала WT-7 с тремя миграциями,
# затем оставляет AddDescription незакоммиченным для второго Deploy.
# Запускать один раз из корня проекта в чистой папке без существующего .git.

$name = (& git config user.name 2>$null)
$email = (& git config user.email 2>$null)
if ([string]::IsNullOrWhiteSpace($name) -or [string]::IsNullOrWhiteSpace($email)) {
    Write-Host 'Сначала укажите Git identity:' -ForegroundColor Yellow
    Write-Host '  git config --global user.name "Ваше имя"'
    Write-Host '  git config --global user.email "ваш-email@example.com"'
    exit 1
}
if (Test-Path ".git") { throw "Папка .git уже существует. Скрипт предназначен для нового репозитория." }

$files = @(
    "Models/Player.cs",
    "Models/PlayerRequest.cs",
    "Data/AppDbContext.cs",
    "Program.cs",
    "Migrations/AppDbContextModelSnapshot.cs",
    "README.md",
    "DEPLOYMENT_CHECKLIST.md",
    "scripts/check-db.sql",
    "scripts/smoke-test.ps1",
    "requests.http"
)
$backup = @{}
foreach ($file in $files) { $backup[$file] = [System.IO.File]::ReadAllText((Join-Path (Get-Location) $file)) }
$newMigrations = @(
    "Migrations/20261107130000_AddDescription.cs",
    "Migrations/20261107130000_AddDescription.Designer.cs"
)
$removedMigrations = @{}
foreach ($file in $newMigrations) {
    $path = Join-Path (Get-Location) $file
    $removedMigrations[$file] = [System.IO.File]::ReadAllText($path)
}

function Write-Utf8([string]$Path, [string]$Content) {
    [System.IO.File]::WriteAllText((Join-Path (Get-Location) $Path), $Content, [System.Text.UTF8Encoding]::new($false))
}

try {
    # Временно приводим исходники к состоянию первого Deploy без Description.
    $player = $backup["Models/Player.cs"] -replace '(?s)\s*/// <summary>Краткое описание профиля игрока \(до 500 символов\)\.</summary>\s*public string Description \{ get; set; \} = string.Empty;\s*', "`r`n"
    $request = $backup["Models/PlayerRequest.cs"] -replace '(?s)\s*\[StringLength\(500, ErrorMessage = "Описание не должно превышать 500 символов\."\)\]\s*public string Description \{ get; set; \} = string.Empty;\s*', "`r`n"
    $context = $backup["Data/AppDbContext.cs"] -replace '(?m)^\s*player\.Property\(item => item\.Description\).*\r?\n', ''
    $context = $context -replace ', Description = ""', ''
    $program = $backup["Program.cs"] -replace '(?m)^\s*Description = request\.Description\?\.Trim\(\) \?\? string\.Empty,\r?\n', ''
    $program = $program -replace '(?m)^\s*player\.Description = request\.Description\?\.Trim\(\) \?\? string\.Empty;\r?\n', ''
    $snapshot = $backup["Migrations/AppDbContextModelSnapshot.cs"]
    $snapshot = $snapshot -replace '(?s)\s*b\.Property<string>\("Description"\).*?\.HasDefaultValue\(""\);\s*', "`r`n"
    $snapshot = $snapshot -replace ', Description = ""', ''
    $readme = $backup["README.md"]
    $readme = $readme -replace 'четыре миграции \(`InitialCreate`, `AddTeamName`, `SeedPlayers`, `AddDescription`\)', 'три миграции (`InitialCreate`, `AddTeamName`, `SeedPlayers`)'
    $readme = $readme -replace 'следующая миграция добавляет новое свойство `TeamName`, а после первой публикации отдельная миграция `AddDescription` расширяет схему ещё одним полем\. Это демонстрирует полный цикл повторного Deploy\.', 'следующая миграция добавляет новое свойство `TeamName` и демонстрирует эволюцию модели без ручного изменения SQL-схемы.'
    $readme = $readme -replace '(?m)^- `AddDescription`.*\r?\n', ''
    $readme = $readme -replace '(?m)^\s*"description": "Профиль игрока Tournament Hub",\r?\n', ''
    $readme = $readme -replace 'Для демонстрации повторного Deploy используйте двухэтапную публикацию\..*?Не добавляйте незакоммиченные изменения до первого Deploy\.', 'После первой публикации можно добавить новое поле, создать миграцию, отправить изменения и проверить повторный Deploy.'
    $checklist = $backup["DEPLOYMENT_CHECKLIST.md"] -replace ', `AddDescription`', ''
    $checklist = $checklist -replace ', `Description`', ''
    $checklist = $checklist -replace '(?m)^- \[ \] После первого Deploy.*\r?\n', ''
    $checklist = $checklist -replace '(?m)^- \[ \] `__EFMigrationsHistory` содержит четвёртую запись.*\r?\n', ''
    $sql = $backup["scripts/check-db.sql"] -replace ', "Description"', ''
    $smoke = $backup["scripts/smoke-test.ps1"] -replace '(?m)^\s*description = ".*"\r?\n', ''
    $http = $backup["requests.http"] -replace '(?m)^\s*"description": ".*",\r?\n', ''

    Write-Utf8 "Models/Player.cs" $player
    Write-Utf8 "Models/PlayerRequest.cs" $request
    Write-Utf8 "Data/AppDbContext.cs" $context
    Write-Utf8 "Program.cs" $program
    Write-Utf8 "Migrations/AppDbContextModelSnapshot.cs" $snapshot
    Write-Utf8 "README.md" $readme
    Write-Utf8 "DEPLOYMENT_CHECKLIST.md" $checklist
    Write-Utf8 "scripts/check-db.sql" $sql
    Write-Utf8 "scripts/smoke-test.ps1" $smoke
    Write-Utf8 "requests.http" $http
    foreach ($file in $newMigrations) { Remove-Item (Join-Path (Get-Location) $file) -Force }

    git init
    if ($LASTEXITCODE -ne 0) { throw "git init завершился с ошибкой." }
    git branch -M main
    if ($LASTEXITCODE -ne 0) { throw "Не удалось создать ветку main." }

    git add .gitignore .dockerignore *.csproj *.slnx appsettings.json appsettings.Development.example.json Properties Models Data
    if ($LASTEXITCODE -ne 0) { throw "Не удалось подготовить первый коммит." }
    git commit -m "Configure EF Core and Player data model"
    if ($LASTEXITCODE -ne 0) { throw "Первый commit завершился с ошибкой." }

    git add Program.cs Migrations requests.http
    if ($LASTEXITCODE -ne 0) { throw "Не удалось подготовить второй коммит." }
    git commit -m "Add PostgreSQL migrations and REST endpoints"
    if ($LASTEXITCODE -ne 0) { throw "Второй commit завершился с ошибкой." }

    git add Dockerfile README.md DEPLOYMENT_CHECKLIST.md scripts .config
    if ($LASTEXITCODE -ne 0) { throw "Не удалось подготовить третий коммит." }
    git commit -m "Prepare Docker deployment and verification"
    if ($LASTEXITCODE -ne 0) { throw "Третий commit завершился с ошибкой." }
}
finally {
    # Восстанавливаем финальную модель и оставляем её незакоммиченной.
    foreach ($file in $files) { Write-Utf8 $file $backup[$file] }
    foreach ($file in $newMigrations) { Write-Utf8 $file $removedMigrations[$file] }
}

Write-Host "Базовая версия с тремя миграциями закоммичена." -ForegroundColor Green
Write-Host "Проверьте git log --oneline, затем добавьте origin и отправьте main на GitHub." -ForegroundColor Cyan
Write-Host "Сначала дождитесь первого успешного Deploy на Render." -ForegroundColor Yellow
Write-Host "После первого Deploy выполните: git add .; git commit -m 'Add player description migration'; git push" -ForegroundColor Cyan
