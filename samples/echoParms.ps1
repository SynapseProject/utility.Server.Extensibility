param(
	[string]$p1,
	[string]$p2,
	[string]$p3
)

Write-Host $MyInvocation.MyCommand.Path
Write-Host "P1 = $p1"
Write-Host "P2 = $p2"
Write-Host "P3 = $p3"
hostname
whoami
