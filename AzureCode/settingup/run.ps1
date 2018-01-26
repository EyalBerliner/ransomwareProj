# POST method: $req
$requestBody = Get-Content $req -Raw | ConvertFrom-Json
$username = $requestBody.username
$pass = $requestBody.password
#$path = "D:\home\site\wwwroot\settingup"
$path = "C:\Users\sefi"
if ($username -eq "") {
	Out-File -Encoding Ascii -FilePath $res -inputObject "Missing usernmae"
	return 
}

if ($pass -eq "") {
	Out-File -Encoding Ascii -FilePath $res -inputObject "Missing password"
	return 
}

if ($username -ne "sefi@sefieyaloutlook.onmicrosoft.com") {
	Write-Output "wrong username"
	return
}

######### createResources ############
$secure = echo $pass | ConvertTo-SecureString -AsPlainText -Force
$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $username, $secure
Login-AzureRmAccount -Credential $cred
$file = "azureprofile.json"
Save-AzureRmContext -Path "$path\$file"
$Location = 'westeurope'
$ResourceGroupName = -join ((65..90) + (97..122) | Get-Random -Count 5 | % {[char]$_})
$StorageName = -join ((65..90) + (97..122) | Get-Random -Count 5 | % {[char]$_})
$ResourceGroupName = $ResourceGroupName.ToLower()
$StorageName = $StorageName.ToLower()
$ContainerName = "public"
$StorageType = "Standard_GRS"
#Select Azure Subscription (TODO: There can be more then one, need to handle this)
$subscription = Get-AzureRmSubscription
$subscription = $subscription[0]
Set-AzureRmContext -SubscriptionId $subscription.SubscriptionId -TenantId $subscription.TenantId
# Resource Group
New-AzureRmResourceGroup -Name $ResourceGroupName -Location $Location
# Storage
$StorageAccount = New-AzureRmStorageAccount -ResourceGroupName $ResourceGroupName -Name $StorageName -Type $StorageType -Location $Location
Set-AzureRmCurrentStorageAccount -Name $StorageName -ResourceGroupName $ResourceGroupName
New-AzureStorageContainer -Name $ContainerName -Permission Blob
Set-AzureStorageBlobContent -Container $ContainerName -File "$path\$file"
######### createResources ############

######### createVM ############
## Network
$InterfaceName = "ServerInterface06"
$Subnet1Name = "Subnet1"
$VNetName = "VNet09"
$VNetAddressPrefix = "10.0.0.0/16"
$VNetSubnetAddressPrefix = "10.0.0.0/24"

## Compute
$VMName = "VirtualMachine12"
$ComputerName = "Server22"
$VMSize = "Standard_A2"
$OSDiskName = $VMName + "OSDisk"

 # Network
$PIp = New-AzureRmPublicIpAddress -Name $InterfaceName -ResourceGroupName $ResourceGroupName -Location $Location -AllocationMethod Dynamic
$SubnetConfig = New-AzureRmVirtualNetworkSubnetConfig -Name $Subnet1Name -AddressPrefix $VNetSubnetAddressPrefix
$VNet = New-AzureRmVirtualNetwork -Name $VNetName -ResourceGroupName $ResourceGroupName -Location $Location -AddressPrefix $VNetAddressPrefix -Subnet $SubnetConfig
$Interface = New-AzureRmNetworkInterface -Name $InterfaceName -ResourceGroupName $ResourceGroupName -Location $Location -SubnetId $VNet.Subnets[0].Id -PublicIpAddressId $PIp.Id
$user = "sefieyal"
$secpasswd = ConvertTo-SecureString "SE236499236499##" -AsPlainText -Force #doesn't matter, because user can reset the password anyway
$Credential = New-Object System.Management.Automation.PSCredential ($user, $secpasswd)
$VirtualMachine = New-AzureRmVMConfig -VMName $VMName -VMSize $VMSize
$VirtualMachine = Set-AzureRmVMOperatingSystem -VM $VirtualMachine -Windows -ComputerName $ComputerName -Credential $Credential -ProvisionVMAgent -EnableAutoUpdate
$VirtualMachine = Set-AzureRmVMSourceImage -VM $VirtualMachine -PublisherName MicrosoftWindowsServer -Offer WindowsServer -Skus 2016-Datacenter -Version "latest"
$VirtualMachine = Add-AzureRmVMNetworkInterface -VM $VirtualMachine -Id $Interface.Id
$OSDiskUri = $StorageAccount.PrimaryEndpoints.Blob.ToString() + "vhds/" + $OSDiskName + ".vhd"
$VirtualMachine = Set-AzureRmVMOSDisk -VM $VirtualMachine -Name $OSDiskName -VhdUri $OSDiskUri -CreateOption FromImage
## Create the VM in Azure
New-AzureRmVM -ResourceGroupName $ResourceGroupName -Location $Location -VM $VirtualMachine
######### createVM ############


###DEBUG: set for standalone running###
<#
$username = "sefi@sefieyaloutlook.onmicrosoft.com"
$pass = "SE236499se"
$secure = echo $pass | ConvertTo-SecureString -AsPlainText -Force
$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $username, $secure
Login-AzureRmAccount -Credential $cred
$ContainerName = "public"
$ResourceGroupName = "cwchq" #"cguhk"
$StorageName = "rullm" #"kzyou"
$VMName = "VirtualMachine12"
$Location = 'westeurope'
$subscription = Get-AzureRmSubscription
Set-AzureRmContext -SubscriptionId $subscription.SubscriptionId -TenantId $subscription.TenantId
Set-AzureRmCurrentStorageAccount -Name $StorageName -ResourceGroupName $ResourceGroupName
#>
###DEBUG###

######### runScriptOnVM ############
$ScriptExtensionName = 'WindowsUpdate'
$file = "init.ps1"
$args = "-StorageAccountName $StorageName"
$DefaultExtension = "BGInfo"

$url2init = "https://runonvm.blob.core.windows.net/public/init.ps1"
Invoke-WebRequest -Uri $url2init -OutFile "$path\$file"
Set-AzureStorageBlobContent -Container $ContainerName -File "$path\$file" -Force

Try
{
	Remove-AzureRmVMCustomScriptExtension -ResourceGroupName $ResourceGroupName `
    -VMName $VMName `
    -Name $ScriptExtensionName `
    -Force
}
Catch { }
Try
{
	Remove-AzureRmVMCustomScriptExtension -ResourceGroupName $ResourceGroupName `
    -VMName $VMName `
    -Name $DefaultExtension `
    -Force
}
Catch { }

$key = Get-AzureRmStorageAccountKey -ResourceGroupName $ResourceGroupName -AccountName $StorageName
$key = $key[0].Value

Set-AzureRmVMCustomScriptExtension -ResourceGroupName $ResourceGroupName `
    -VMName $VMName `
    -StorageAccountName $StorageName `
    -ContainerName $ContainerName `
    -FileName $file `
    -Run "$file $args"`
    -Name $ScriptExtensionName `
    -Location $Location `
	-StorageAccountKey $key
######### runScriptOnVM ############

Out-File -Encoding Ascii -FilePath $res -inputObject "Done"