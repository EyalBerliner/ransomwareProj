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

Import-AzureRmContext -Path $file
$name = ""
$key = ""
$subscriptions = Get-AzureRmSubscription
foreach ($sub in $subscriptions) {

  Set-AzureRmContext -SubscriptionId $sub.SubscriptionId -TenantId $sub.TenantId
  $resources = Get-AzureRmResource
  $output_creds = New-Object System.Collections.ArrayList

  $y = ""
  foreach ($resource in $resources) {

     If ($resource.ResourceType -eq "Microsoft.Storage/storageAccounts") {

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

# Now get the C# code and run it.

#create App.config file

Invoke-Expression "./createAppConfig.ps1" $name $key