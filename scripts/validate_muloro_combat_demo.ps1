$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$scenePath = Join-Path $repoRoot "Assets/MuloroCombatDemo/Scenes/PortfolioCombatDemo.unity"
$buildSettingsPath = Join-Path $repoRoot "ProjectSettings/EditorBuildSettings.asset"
$portfolioScriptsPath = Join-Path $repoRoot "Assets/MuloroCombatDemo/Scripts/Portfolio"
$portfolioBossScriptPath = Join-Path $portfolioScriptsPath "PortfolioOfflineBoss.cs"
$manifestPath = Join-Path $repoRoot "Packages/manifest.json"

$characterManagerGuid = "eda93a426def7024c8deb9e3b8af8e85"
$singlePlayerBootstrapGuid = "7ad0e1c95b8c4fb2a831c6cb16f3729b"
$offlinePlayerGuid = "8f55b4e28c244b77a264f306d0b7f8c1"
$offlineBossGuid = "94f0c5e46df04fafa4f47942cc705bb9"
$offlinePlayerSpriteGuid = "3b9952febfc558648bb1d8e7ad95de5e"
$missingPinkSpriteGuid = "d132066afc95e8541af2d20260317160"
$belphegorAnimatorControllerGuid = "b369c0fa7383a4815bd8bb68005be036"
$demoSceneGuid = "8c9cfa26abfee488c85f1582747f6a02"
$gxgLobbyGuid = "6d01996c9dc0ab54faf60c103971aad9"
$gxgLobbyStageManagerFileId = "2831559919466458734"
$mainCameraCinemachineBrainFileId = "519420033"

if (!(Test-Path $scenePath)) {
    throw "Missing demo scene: $scenePath"
}

$scene = Get-Content -Raw -Path $scenePath
$buildSettings = Get-Content -Raw -Path $buildSettingsPath
$manifest = Get-Content -Raw -Path $manifestPath

if ($scene -match $characterManagerGuid) {
    throw "Muloro demo scene still references multiplayer CharacterManager ($characterManagerGuid)."
}

foreach ($guid in @($singlePlayerBootstrapGuid, $offlinePlayerGuid, $offlineBossGuid)) {
    if ($scene -notmatch $guid) {
        throw "Muloro demo scene is missing required offline demo script guid: $guid"
    }
}

if ($scene -notmatch $offlinePlayerSpriteGuid) {
    throw "Muloro offline player is not using the available Pink idle sprite asset."
}

if ($scene -match $missingPinkSpriteGuid) {
    throw "Muloro demo scene still references a missing Pink sprite asset ($missingPinkSpriteGuid)."
}

if ($scene -notmatch $belphegorAnimatorControllerGuid) {
    throw "Muloro offline Belphegor does not reference the gameplay animation controller."
}

$portfolioBossScript = Get-Content -Raw -Path $portfolioBossScriptPath
if ($portfolioBossScript -notmatch 'WalkStateName\s*=\s*"walk"') {
    throw "Muloro offline Belphegor movement animation is not driven by the offline boss script."
}

if ($portfolioBossScript -notmatch 'public\s+void\s+PlaySound\s*\(\s*string\s+\w+\s*\)') {
    throw "Muloro offline Belphegor is missing the PlaySound(string) receiver required by imported AnimationEvents."
}

foreach ($requiredBtToken in @(
    "BossBehaviorTree",
    "SelectorNode",
    "SequenceNode",
    "Phase2BasicAttackWeight = 40",
    "Phase2ComboAttackWeight = 60",
    "Phase3BasicAttackWeight = 10",
    "Phase3SmashComboWeight = 40",
    "Phase3PunchStarFingerWeight = 30",
    "Phase3PunchDashComboWeight = 20",
    "ForwardDashDistanceThreshold = 5f",
    "BackDashDistanceThreshold = 1f",
    "RandomDashChance = 0.7f",
    'WalkTriggerName = "WALK"',
    'IdleTriggerName = "IDLE"',
    "PlayLocomotion(isMoving)",
    "_spriteRenderer.flipX = toTarget.x > 0f",
    "StarFingerFollowUpDamageDelay = 0.15f",
    "ShouldApplyAttackDamage(string animationName)",
    "animationName != StarFingerStateName",
    "animationName != Punch4ToStarFingerStateName",
    "GetAttackDamageDelay(animationName)"
)) {
    if ($portfolioBossScript -notmatch [regex]::Escape($requiredBtToken)) {
        throw "Muloro offline Belphegor is missing BT diagram behavior token: $requiredBtToken"
    }
}

foreach ($animationName in @("punch1", "punch2", "punch3", "punch4", "smash1", "smash2", "smash3", "starfinger")) {
    if ($scene -notmatch [regex]::Escape("- $animationName")) {
        throw "Muloro offline Belphegor attack animation is not wired: $animationName"
    }
}

$demoScenePath = "Assets/MuloroCombatDemo/Scenes/PortfolioCombatDemo.unity"
if ($buildSettings -notmatch [regex]::Escape($demoScenePath) -or $buildSettings -notmatch $demoSceneGuid) {
    throw "EditorBuildSettings does not include Muloro PortfolioCombatDemo."
}

foreach ($field in @("_networkPrefabs:", "_playerPrefab:", "_rpcHandlerPrefab:", "_allianceControllerPrefab:", "_spawnDemoMonsters:", "_monsterSpawns:")) {
    if ($scene -match [regex]::Escape($field)) {
        throw "Muloro demo scene still contains legacy network bootstrap field: $field"
    }
}

$gxgStageDisabledPattern = "(?s)target: \{fileID: $gxgLobbyStageManagerFileId, guid: $gxgLobbyGuid, type: 3\}\s+propertyPath: m_Enabled\s+value: 0"
if ($scene -notmatch $gxgStageDisabledPattern) {
    throw "GXGLobby StageManager is not disabled for the offline demo."
}

$gxgSpawnerOffPattern = "(?s)target: \{fileID: $gxgLobbyStageManagerFileId, guid: $gxgLobbyGuid, type: 3\}\s+propertyPath: _monsterSpawnPoints\.Array\.size\s+value: 0"
if ($scene -notmatch $gxgSpawnerOffPattern) {
    throw "GXGLobby monster spawning is not disabled for scene-placed offline actors."
}

$mainCameraBrainDisabledPattern = "(?s)--- !u!114 &$mainCameraCinemachineBrainFileId\s+MonoBehaviour:.*?m_Enabled: 0"
if ($scene -notmatch $mainCameraBrainDisabledPattern) {
    throw "Main Camera Cinemachine Brain is still enabled and can fight the offline camera follow."
}

$networkTerms = @(
    "Unity.Netcode",
    "NetworkManager",
    "NetworkObject",
    "UnityTransport",
    "StartHost",
    "NetworkPrefab"
)
$portfolioScripts = Get-ChildItem -Path $portfolioScriptsPath -Filter "*.cs" -File
foreach ($term in $networkTerms) {
    $match = Select-String -Path $portfolioScripts.FullName -Pattern ([regex]::Escape($term)) -Quiet
    if ($match) {
        throw "Muloro demo scripts still use network runtime term: $term"
    }
}

foreach ($packageName in @(
    "com.unity.2d.aseprite",
    "com.unity.inputsystem",
    "com.unity.render-pipelines.universal",
    "com.unity.feature.cinematic"
)) {
    if ($manifest -notmatch [regex]::Escape($packageName)) {
        throw "Missing package dependency required by Muloro demo: $packageName"
    }
}

Write-Host "Muloro combat demo validation passed."
