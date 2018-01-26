#Param(
#  [string]$accountName,
#  [string]$accountKey  
#)

#debug
$accountName="runonvm"
$accountkey = "ELAU5PswSsvQzTsgP8vwh5ZYK9QTKNWZwffgfLx5fQFrGA+AV7IogwBAobmTCdFtJt+4TySGmm85teR2o9372w=="

$connctionString1="DefaultEndpointsProtocol=https;AccountName="
$connectionString2=";AccountKey="
$connectionString3 = ";EndpointSuffix=core.windows.net"

$connectionString = $connctionString1 + $accountName + $connectionString2 + $accountKey + $connectionString3

$RptKeyFound=0;

$xml = [xml](get-content "app.config");              # Create the XML Object and open the app.config file 
$root = $xml.get_DocumentElement();                     # Get the root element of the file

foreach( $item in $root.appSettings.add)                  # loop through the child items in appsettings 
{ 
  if($item.key –eq “ReportingServer”)                       # If the desired element already exists 
    { 
      $item.value = $accountName;                          # Update the value attribute 
      $RptKeyFound=1;                                             # Set the found flag 
    } 
}

if($RptKeyFound -eq 0)                                                   # If the desired element does not exist 
{ 
    $newEl=$xml.CreateElement("add");                               # Create a new Element 
    $nameAtt1=$xml.CreateAttribute("key");                         # Create a new attribute “key” 
    $nameAtt1.psbase.value="StorageConnectionString";                    # Set the value of “key” attribute 
    $newEl.SetAttributeNode($nameAtt1);                              # Attach the “key” attribute 
    $nameAtt2=$xml.CreateAttribute("value");                       # Create “value” attribute  
    $nameAtt2.psbase.value=$connectionString;                       # Set the value of “value” attribute 
    $newEl.SetAttributeNode($nameAtt2);                               # Attach the “value” attribute 
    $xml.configuration["appSettings"].AppendChild($newEl);    # Add the newly created element to the right position

}

$xml.Save("$pwd\app.config")                                                # Save the app.config file