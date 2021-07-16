# Run using right-click 'Run with PowerShell'

trap {
    Write-Host -Fore Red $_
    "Press any key to exit."
    $null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

function Get-WeekOfYear {
    param([datetime]$date)
    $day = [System.Globalization.CultureInfo]::InvariantCulture.Calendar.GetDayOfWeek($date)
    if ($day -ge [System.DayOfWeek]::Monday -and $day -le [System.DayOfWeek]::Wednesday)
    {
        $date = $date.AddDays(3);
    }

    [System.Globalization.CultureInfo]::InvariantCulture.Calendar.GetWeekOfYear($date, [System.Globalization.CalendarWeekRule]::FirstFourDayWeek, [System.DayOfWeek]::Monday)
}

$year = Get-Date -UFormat %y
$week = Get-WeekOfYear -date (Get-Date)
$day = [int](Get-Date).DayOfWeek

# http://stackoverflow.com/a/3879077
$staged = & "${env:ProgramFiles(x86)}\Git\cmd\git.exe" "diff-index" "--cached" "--name-status" "-r" "--ignore-submodules" "HEAD" "--"
if ($staged) {
    throw "There are staged changes. Commit them first or unstage and reset all changes before building.`n$($staged -join "`n")"
}

$diffs = & "${env:ProgramFiles(x86)}\Git\cmd\git.exe" "diff-files" "--name-status" "-r" "--ignore-submodules" "--"
if ($diffs) {
    throw "There are local changes. Reset all changes before building.`n$($diffs -join "`n")"
}

# http://stackoverflow.com/a/1593246
$currentTag = & "${env:ProgramFiles(x86)}\Git\cmd\git.exe" "name-rev" "--name-only" "--tags" "HEAD"
if ($currentTag -eq "^undefined$") {
    throw "No tag found. Create version tag for current commit, then try again."
}

if (-not ($currentTag -match "^(?<major>\d+)\.(?<minor>\d+)\.(?<revision>\d+)\.(?<build>\d+)$")) {
    throw "Unable to find version tag or tag is not according to expected format (YY.W.DAYxx.0). Create version tag for current commit, then try again.`n" +
    "Example: $($year).$($week).$($day)01.0 - Year 20$($year), week $week, day $day, first build of day (01). Trailing 0 is not significant."
}

$expectedTag = "^$year\.$week\.$day[0-9]{2}\.\d$"

if (-not ($currentTag -match $expectedTag)) {
    $title = "Verify version tag."
    $message = "Is the version $currentTag really correct? Expected version should match $expectedTag"
    $defaultChoice = 1
} else {
    $title = "Build install files."
    $message = "Is the version $currentTag correct?"
    $defaultChoice = 0
}

$yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Builds the install files."
$no = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Aborts the script."
$options = [System.Management.Automation.Host.ChoiceDescription[]]($yes, $no)
$result = $host.ui.PromptForChoice($title, $message, $options, $defaultChoice)

switch ($result)
    {
        0 {"You selected Yes."}
        1 { return }
    }

# build
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" build.msbuild /target:All /property:Version=$currentTag

# reset changed files
$filesToReset = Get-ChildItem -Recurse -Filter AssemblyInfo.cs

foreach ($file in $filesToReset) {
    "Restoring $($file.FullName)"
    & "${env:ProgramFiles(x86)}\Git\cmd\git.exe" "checkout" "--" $file.FullName
}

# push tag
$yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Push the tag."
$no = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Exit script."
$options = [System.Management.Automation.Host.ChoiceDescription[]]($yes, $no)
$result = $host.ui.PromptForChoice("Push the tag", "Push the tag to remote?", $options, 1)

switch ($result)
    {
        0 {
            & "${env:ProgramFiles(x86)}\Git\cmd\git.exe" "push" "--tags"
        }
        1 { return }
    }
